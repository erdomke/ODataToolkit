namespace LinqToQuerystring
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Linq.Expressions;
  using Nodes;
  using Nodes.Base;

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
      if (queryString == null)
        throw new ArgumentNullException("queryString", "Query String cannot be null");

      var odataUri = OData.Parse(queryString);
      if (maxPageSize > 0)
        odataUri.QueryOption.Top = Math.Min(odataUri.QueryOption.Top ?? int.MaxValue, maxPageSize);
      return odataUri.Execute(query, inputType, dynamicAccessor);
    }
  }
}
