using LinqToQuerystring.Nodes;
using LinqToQuerystring.Nodes.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToQuerystring
{
  public class ODataUri : Uri
  {
    public ODataUri(string uri) : base(uri, UriKind.RelativeOrAbsolute) { }

    private ODataQuery _query = new ODataQuery();
    private List<ODataNode> _segments = new List<ODataNode>();

    public ODataQuery QueryOption { get { return _query; } }
    public IList<ODataNode> PathSegments { get { return _segments; } }

    public TResult Execute<T, TResult>(IQueryable<T> query, Expression<Func<object, string, object>> dynamicAccessor = null)
    {
      return (TResult)Execute(query, typeof(T), dynamicAccessor);
    }

    public IQueryable<T> Execute<T>(IQueryable<T> query, Expression<Func<object, string, object>> dynamicAccessor = null)
    {
      return (IQueryable<T>)Execute(query, typeof(T), dynamicAccessor);
    }

    public object Execute(IQueryable query, Type inputType, Expression<Func<object, string, object>> dynamicAccessor = null)
    {
      var queryResult = query;
      var constrainedQuery = query;

      if (query == null)
        throw new ArgumentNullException("query", "Query cannot be null");

      var children = this.QueryOption.Where(n => !(n is IgnoredNode)).ToList();
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

      return constrainedQuery;
    }

    private static void BuildQuery(ODataNode node, Type inputType, Expression<Func<object, string, object>> dynamicAccessor, ref IQueryable queryResult, ref IQueryable constrainedQuery)
    {
      var type = queryResult.Provider.GetType().Name;

      var mappings = (!string.IsNullOrEmpty(type) && Configuration.CustomNodes.ContainsKey(type))
                         ? Configuration.CustomNodes[type]
                         : null;

      if (mappings != null)
      {
        node = mappings.MapNode(node, queryResult.Expression);
      }

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

      var result = GetEnumeratedQuery(constrainedQuery).AsQueryable();
      var opts = new ExpressionOptions()
      {
        Query = result,
        InputType = inputType,
        Expression = result.Expression,
        DynamicAccessor = dynamicAccessor
      };
      return
          result.Provider.CreateQuery<Dictionary<string, object>>(
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
