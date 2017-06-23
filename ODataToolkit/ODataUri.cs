using ODataToolkit.Nodes;
using ODataToolkit.Nodes.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ODataToolkit
{
  public class ODataUri : Uri
  {
    public ODataUri(string uri) : base(uri, UriKind.RelativeOrAbsolute) { }

    private ODataQuery _query = new ODataQuery();
    private List<ODataNode> _segments = new List<ODataNode>();
    private ODataVersion _version;

    /// <summary>
    /// Interpreted path segments of the URL
    /// </summary>
    public IList<ODataNode> PathSegments { get { return _segments; } }
    /// <summary>
    /// OData query options
    /// </summary>
    public ODataQuery QueryOption { get { return _query; } }
    /// <summary>
    /// Version(s) of OData which are supported
    /// </summary>
    public ODataVersion Version { get { return _version; } }

    /// <summary>
    /// Execute the query represented by an OData URI against an <c>IQueryable</c> data source
    /// </summary>
    /// <param name="query"><c>IQueryable</c> data source</param>
    /// <param name="dynamicAccessor">Expression for accessing fields on dynamic model objects</param>
    public ODataQueryable<T> Execute<T>(IQueryable<T> query, Expression<Func<object, string, object>> dynamicAccessor = null)
    {
      return Execute(query, typeof(T), dynamicAccessor).Cast<T>();
    }

    /// <summary>
    /// Execute the query represented by an OData URI against an <c>IQueryable</c> data source
    /// </summary>
    /// <param name="query"><c>IQueryable</c> data source</param>
    /// <param name="inputType">Model data type</param>
    /// <param name="dynamicAccessor">Expression for accessing fields on dynamic model objects</param>
    public ODataQueryable<object> Execute(IQueryable query, Type inputType, Expression<Func<object, string, object>> dynamicAccessor = null)
    {
      var queryResult = query;
      var constrainedQuery = query;

      if (query == null)
        throw new ArgumentNullException("query", "Query cannot be null");

      var children = this.QueryOption.Where(n => !(n is IgnoredNode) && !(n is AliasNode)).ToList();
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

      return new ODataQueryable<object>(constrainedQuery, this);
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
