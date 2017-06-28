using ODataToolkit.Csdl;
using ODataToolkit.Nodes;
using ODataToolkit.Nodes.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;

namespace ODataToolkit
{
  public class ODataUri : Uri
  {
    internal ODataUri(string uri) : base(uri, UriKind.RelativeOrAbsolute) { }

    private ODataQuery _query = new ODataQuery();
    private int _rootSegmentCount;
    private List<ODataNode> _segments = new List<ODataNode>();
    private ODataVersion _version;

    /// <summary>
    /// Interpreted path segments of the URL
    /// </summary>
    public IEnumerable<ODataNode> PathSegments { get { return _segments.Skip(_rootSegmentCount); } }
    /// <summary>
    /// OData query options
    /// </summary>
    public ODataQuery QueryOption { get { return _query; } }
    /// <summary>
    /// Version(s) of OData which are supported
    /// </summary>
    public ODataVersion Version
    {
      get { return _version; }
      internal set { _version = value; }
    }

    /// <summary>
    /// Clone the URL to create a new copy in memory
    /// </summary>
    public ODataUri Clone()
    {
      return OData.Parse(ToString(), _version).WithRootSegmentCount(_rootSegmentCount);
    }

    /// <summary>
    /// Execute the query represented by an OData URI against an <c>IQueryable</c> data source
    /// </summary>
    /// <param name="query"><c>IQueryable</c> data source</param>
    /// <param name="dynamicAccessor">Expression for accessing fields on dynamic model objects</param>
    public ODataQueryable<T> Execute<T>(IQueryable<T> query, ExecutionSettings settings = null)
    {
      return Execute((IQueryable)query, (settings ?? new ExecutionSettings()).WithType(typeof(T))).Cast<T>();
    }
    
    /// <summary>
    /// Execute the query represented by an OData URI against an <c>IQueryable</c> data source
    /// </summary>
    /// <param name="query"><c>IQueryable</c> data source</param>
    /// <param name="inputType">Model data type</param>
    /// <param name="dynamicAccessor">Expression for accessing fields on dynamic model objects</param>
    public ODataQueryable<object> Execute(IQueryable query, ExecutionSettings settings = null)
    {
      if (query == null)
        throw new ArgumentNullException("query", "Query cannot be null");

      settings = settings ?? ExecutionSettings.Empty;
      var queryResult = query;
      var constrainedQuery = query;
      var path = settings.GetEdmSource(this);
      var inputType = settings.GetInputType(query);
      var dynamicAccessor = settings.GetDynamicAccessor();
      var lookupByKey = false;

      var children = this.QueryOption.Where(n => !(n is IgnoredNode) && !(n is AliasNode)).ToList();

      // Try and do an ID lookup if applicable
      var functionNode = _segments.Skip(_rootSegmentCount).FirstOrDefault() as CallNode;
      if (functionNode != null)
      {
        var entity = ((IEdmCollectionType)path.Type).ElementType.ToStructuredType() as IEdmEntityType;
        if (entity != null && entity.Key().Count() == functionNode.Arguments.Count)
        {
          var keys = entity.Key().ToArray();
          var root = default(ODataNode);
          for (var i = 0; i < keys.Length; i++)
          {
            var newNode = ODataNode.Equals(ODataNode.Identifier(keys[i].Name), functionNode.Arguments[i]);
            if (root == null)
              root = newNode;
            else
              newNode = ODataNode.And(root, newNode);
          }

          children.RemoveByFilter(n => n is FilterNode);
          children.Add(ODataNode.Filter(root));
          lookupByKey = true;
        }
      }

      var maxPageSize = settings.GetMaxPageSize();
      if (maxPageSize.HasValue && (!QueryOption.Top.HasValue || maxPageSize.Value < QueryOption.Top.Value))
      {
        children.RemoveByFilter(n => n is TopNode);
        children.Add(ODataNode.Top(ODataNode.Literal(maxPageSize.Value)));
      }
      children.Sort();

      // These should always come first
      foreach (var node in children.Where(o => !(o is SelectNode)))
      {
        BuildQuery(node, inputType, dynamicAccessor, ref queryResult, ref constrainedQuery);
      }

      var selectNode = children.FirstOrDefault(o => o is SelectNode);
      if (selectNode != null)
      {
        constrainedQuery = ProjectQuery(constrainedQuery, selectNode, inputType, dynamicAccessor);
      }

      return new ODataQueryable<object>(constrainedQuery, this) { LookupByKey = lookupByKey };
    }

    internal IEdmEntitySet FindEntitySet(IEdmModel model)
    {
      var firstSegment = _segments.Skip(_rootSegmentCount).FirstOrDefault();
      return model == null || firstSegment == null ? null : model.EntityContainer.FindEntitySet(firstSegment.Text);
    }
    
    internal void AddSegment(ODataNode node)
    {
      _segments.Add(node);
    }
    internal void Initialize()
    {
      foreach (var segment in _segments)
        Initialize(segment);
      foreach (var part in _query)
        Initialize(part);
    }

    internal void Initialize(ODataNode node)
    {
      node.Uri = this;
      foreach (var child in node.Children)
      {
        Initialize(child);
      }
    }

    /// <summary>
    /// Returns an <see cref="ODataUriBuilder"/> for the path section of the URL
    /// (the portion after the root)
    /// </summary>
    public ODataUriBuilder PathBuilder()
    {
      return new ODataUriBuilder()
        .Segment(AbsolutePath.TrimStart('/').Split('/').Skip(_rootSegmentCount).ToArray());
    }

    /// <summary>
    /// Returns an <see cref="ODataUriBuilder"/> for creating new OData URLs
    /// </summary>
    /// <param name="outputAuthority">
    /// Whether to output the authority portion of the URL (e.g. http://services.odata.org/
    /// </param>
    public ODataUriBuilder UriBuilder(bool outputAuthority = true)
    {
      return new ODataUriBuilder()
        .Raw(outputAuthority ? GetLeftPart(UriPartial.Authority) : "/")
        .Segment(AbsolutePath.TrimStart('/').Split('/').Take(_rootSegmentCount).ToArray());
    }

    /// <summary>
    /// Indicates how many of the initial segments in the URL are part of the root (or base) path
    /// </summary>
    /// <param name="count">Number of segments</param>
    /// <remarks>
    /// Consider the URL http://host/services/_api/Products(0).  This has 2 root segments such that
    /// the root path is http://host/services/_api/.
    /// </remarks>
    public ODataUri WithRootSegmentCount(int count)
    {
      _rootSegmentCount = count;
      return this;
    }
    internal ODataUri WithVersion(ODataVersion version)
    {
      _version = version;
      return this;
    }

    private static void BuildQuery(ODataNode node, Type inputType, Expression<Func<object, string, object>> dynamicAccessor, ref IQueryable queryResult, ref IQueryable constrainedQuery)
    {
      var type = queryResult.Provider.GetType().Name;
      if (!(node is TopNode) && !(node is SkipNode))
      {
        var opts = new ExpressionOptions()
        {
          Query = queryResult,
          InputType = inputType,
          Expression = queryResult.Expression,
          DynamicAccessor = dynamicAccessor
        };
        var modifier = node as QueryModifier;
        if (modifier != null)
        {
          queryResult = modifier.ModifyQuery(opts);
        }
        else
        {
          queryResult = queryResult.Provider.CreateQuery(
              node.BuildLinqExpression(opts));
        }
      }

      var queryModifier = node as QueryModifier;
      if (queryModifier != null)
      {
        var opts = new ExpressionOptions()
        {
          Query = constrainedQuery,
          InputType = inputType,
          Expression = constrainedQuery.Expression,
          DynamicAccessor = dynamicAccessor
        };
        constrainedQuery = queryModifier.ModifyQuery(opts);
      }
      else
      {
        var opts = new ExpressionOptions()
        {
          Query = constrainedQuery,
          InputType = inputType,
          Expression = constrainedQuery.Expression,
          DynamicAccessor = dynamicAccessor
        };
        constrainedQuery =
            constrainedQuery.Provider.CreateQuery(
                node.BuildLinqExpression(opts));
      }
    }

    private static IQueryable ProjectQuery(IQueryable constrainedQuery, ODataNode node, Type inputType, Expression<Func<object, string, object>> dynamicAccessor)
    {
      // TODO: Find a solution to the following:
      // Currently the only way to perform the SELECT part of the query is to call ToList and then project onto a dictionary. Two main problems:
      // 1. Linq to Entities does not support projection onto list initialisers with more than one value
      // 2. We cannot build an anonymous type using expression trees as there is compiler magic that must happen.
      // There is a solution involving reflection.emit, but is it worth it? Not sure...

      //var result = GetEnumeratedQuery(constrainedQuery).AsQueryable();
      var opts = new ExpressionOptions()
      {
        Query = constrainedQuery,
        InputType = inputType,
        Expression = constrainedQuery.Expression,
        DynamicAccessor = dynamicAccessor
      };
      return
          constrainedQuery.Provider.CreateQuery(
              node.BuildLinqExpression(opts));

    }

    static IEnumerable<object> GetEnumeratedQuery(IQueryable query)
    {
      return Iterate(query.GetEnumerator()).Cast<object>().ToList();
    }

    static IEnumerable Iterate(IEnumerator iterator)
    {
      while (iterator.MoveNext())
        yield return iterator.Current;
    }
  }
}
