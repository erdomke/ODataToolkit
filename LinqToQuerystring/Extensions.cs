namespace LinqToQuerystring
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Linq.Expressions;
  using TreeNodes;
  using TreeNodes.Base;

  //    using Antlr.Runtime;
  //    using Antlr.Runtime.Tree;

  //    using LinqToQuerystring.TreeNodes;
  //    using LinqToQuerystring.TreeNodes.Base;

  public static class OData
  {
    public static readonly Expression<Func<object, string, object>> DictionaryAccessor 
      = (obj, key) => ((IDictionary<string, object>)obj)[key];

    public static IEnumerable<Token> Tokenize(string value)
    {
      var tokenizer = new Tokenizer(value);
      while (tokenizer.MoveNext())
        yield return tokenizer.Current;
    }

    public static ODataUri Parse(this IEnumerable<Token> tokens)
    {
      var parser = new Parser(tokens);
      parser.Process();
      return parser.Uri;
    }

    public static ODataUri Parse(string value)
    {
      if (string.IsNullOrEmpty(value))
        return new ODataUri("?");
      return Tokenize(value).Parse();
    }

    public static TResult ExecuteOData<T, TResult>(this IQueryable<T> query, string queryString = "", Expression<Func<object, string, object>> dynamicAccessor = null, int maxPageSize = -1)
    {
      return (TResult)ExecuteOData(query, typeof(T), queryString, dynamicAccessor, maxPageSize);
    }

    public static IQueryable<T> ExecuteOData<T>(this IQueryable<T> query, string queryString = "", Expression<Func<object, string, object>> dynamicAccessor = null, int maxPageSize = -1)
    {
      return (IQueryable<T>)ExecuteOData(query, typeof(T), queryString, dynamicAccessor, maxPageSize);
    }

    public static object ExecuteOData(this IQueryable query, Type inputType, string queryString = "", Expression<Func<object, string, object>> dynamicAccessor = null, int maxPageSize = -1)
    {
      var queryResult = query;
      var constrainedQuery = query;

      if (query == null)
        throw new ArgumentNullException("query", "Query cannot be null");
      if (queryString == null)
        throw new ArgumentNullException("queryString", "Query String cannot be null");

      var odataUri = OData.Parse(queryString);
      if (maxPageSize > 0)
        odataUri.QueryOption.Top = Math.Min(odataUri.QueryOption.Top ?? int.MaxValue, maxPageSize);

      var children = odataUri.QueryOption.Where(n => !(n is IgnoredNode)).ToList();
      children.Sort();

      // These should always come first
      foreach (var node in children.Where(o => !(o is SelectNode) && !(o is InlineCountNode)))
      {
        BuildQuery(node, inputType, dynamicAccessor, ref queryResult, ref constrainedQuery);
      }

      var selectNode = children.FirstOrDefault(o => o is SelectNode);
      if (selectNode != null)
      {
        constrainedQuery = ProjectQuery(constrainedQuery, selectNode, inputType, dynamicAccessor);
      }

      var inlineCountNode = children.FirstOrDefault(o => o is InlineCountNode);
      if (inlineCountNode != null)
      {
        return PackageResults(queryResult, constrainedQuery);
      }

      return constrainedQuery;
    }

    private static void BuildQuery(TreeNode node, Type inputType, Expression<Func<object, string, object>> dynamicAccessor, ref IQueryable queryResult, ref IQueryable constrainedQuery)
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

    private static IQueryable ProjectQuery(IQueryable constrainedQuery, TreeNode node, Type inputType, Expression<Func<object, string, object>> dynamicAccessor)
    {
      // TODO: Find a solution to the following:
      // Currently the only way to perform the SELECT part of the query is to call ToList and then project onto a dictionary. Two main problems:
      // 1. Linq to Entities does not support projection onto list initialisers with more than one value
      // 2. We cannot build an anonymous type using expression trees as there is compiler magic that must happen.
      // There is a solution involving reflection.emit, but is it worth it? Not sure...

      var result = constrainedQuery.GetEnumeratedQuery().AsQueryable();
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

    private static object PackageResults(IQueryable query, IQueryable constrainedQuery)
    {
      var result = query.GetEnumeratedQuery();
      return new Dictionary<string, object> { { "Count", result.Count() }, { "Results", constrainedQuery } };
    }

    public static IEnumerable<object> GetEnumeratedQuery(this IQueryable query)
    {
      return Iterate(query.GetEnumerator()).Cast<object>().ToList();
    }

    static IEnumerable Iterate(this IEnumerator iterator)
    {
      while (iterator.MoveNext())
        yield return iterator.Current;
    }
  }
}
