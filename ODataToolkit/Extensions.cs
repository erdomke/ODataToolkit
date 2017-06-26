namespace ODataToolkit
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
    /// <summary>
    /// Expression for retrieving a value from a Dictionary given the string name of the key
    /// </summary>
    public static readonly Expression<Func<object, string, object>> DictionaryAccessor
      = (obj, key) => ((IDictionary<string, object>)obj)[key];

    /// <summary>
    /// Break an OData URI into a flat stream of tokens
    /// </summary>
    /// <param name="uri">OData URI</param>
    /// <param name="version">Which version(s) to support</param>
    public static IEnumerable<Token> Tokenize(string uri, ODataVersion version = ODataVersion.All, bool decodeUri = true)
    {
      var tokenizer = new Tokenizer(uri, version, decodeUri);
      while (tokenizer.MoveNext())
        yield return tokenizer.Current;
    }

    /// <summary>
    /// Parse a stream of OData URI tokens into a URL
    /// </summary>
    /// <param name="tokens">OData URI tokens</param>
    /// <param name="version">Which version(s) to support</param>
    public static ODataUri Parse(this IEnumerable<Token> tokens, ODataVersion version = ODataVersion.All)
    {
      var parser = new Parser(tokens);
      parser.Process();
      var result = parser.Uri;
      result.Version = version;
      return result;
    }

    /// <summary>
    /// Parse an OData URI into a URL
    /// </summary>
    /// <param name="tokens">OData URI</param>
    /// <param name="version">Which version(s) to support</param>
    public static ODataUri Parse(string value, ODataVersion version = ODataVersion.All, bool decodeUri = true)
    {
      if (string.IsNullOrEmpty(value))
        return new ODataUri("?");
      return Tokenize(value, version, decodeUri).Parse(version);
    }

    /// <summary>
    /// Execute the query represented by an OData URI against an <c>IQueryable</c> data source
    /// </summary>
    /// <param name="query"><c>IQueryable</c> data source</param>
    /// <param name="uri">OData URI</param>
    /// <param name="dynamicAccessor">Expression for accessing fields on dynamic model objects</param>
    /// <param name="maxPageSize">Maximum page size (used to override values specified in the URL)</param>
    public static ODataQueryable<T> ExecuteOData<T>(this IQueryable<T> query, string uri = "", Expression<Func<object, string, object>> dynamicAccessor = null, int maxPageSize = -1)
    {
      return ExecuteOData(query, typeof(T), uri, dynamicAccessor, maxPageSize).Cast<T>();
    }

    /// <summary>
    /// Execute the query represented by an OData URI against an <c>IQueryable</c> data source
    /// </summary>
    /// <param name="query"><c>IQueryable</c> data source</param>
    /// <param name="inputType">Model data type</param>
    /// <param name="uri">OData URI</param>
    /// <param name="dynamicAccessor">Expression for accessing fields on dynamic model objects</param>
    /// <param name="maxPageSize">Maximum page size (used to override values specified in the URL)</param>
    public static ODataQueryable<object> ExecuteOData(this IQueryable query, Type inputType, string uri = "", Expression<Func<object, string, object>> dynamicAccessor = null, int maxPageSize = -1)
    {
      if (uri == null)
        throw new ArgumentNullException("queryString", "Query String cannot be null");

      var odataUri = OData.Parse(uri);
      if (maxPageSize > 0)
        odataUri.QueryOption.Top = Math.Min(odataUri.QueryOption.Top ?? int.MaxValue, maxPageSize);
      return odataUri.Execute(query, inputType, dynamicAccessor);
    }
  }
}
