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
  using System.Collections.Specialized;

  /// <summary>
  /// OData extension and utility methods.
  /// </summary>
  public static class OData
  {
    /// <summary>
    /// Get the negotiated OData version from the client headers
    /// </summary>
    public static ODataVersion VersionFromHeaders(IEnumerable<KeyValuePair<string, string>> headers)
    {
      var maxDataServiceVersion = headers
        .Where(k => string.Equals(k.Key, "MaxDataServiceVersion", StringComparison.OrdinalIgnoreCase))
        .Select(k => (int?)double.Parse(k.Value))
        .FirstOrDefault();
      var oDataMax = headers
        .Where(k => string.Equals(k.Key, "OData-MaxVersion", StringComparison.OrdinalIgnoreCase))
        .Select(k => (int?)double.Parse(k.Value))
        .FirstOrDefault();
      return VersionFromHeaders(maxDataServiceVersion, oDataMax);
    }

    /// <summary>
    /// Get the negotiated OData version from the client headers
    /// </summary>
    public static ODataVersion VersionFromHeaders(NameValueCollection headers)
    {
      var buffer = headers["MaxDataServiceVersion"];
      var maxDataServiceVersion = buffer == null ? null : (int?)double.Parse(buffer);
      buffer = headers["OData-MaxVersion"];
      var oDataMax = buffer == null ? null : (int?)double.Parse(buffer);
      return VersionFromHeaders(maxDataServiceVersion, oDataMax);
    }

    /// <summary>
    /// Get the negotiated OData version from the client headers
    /// </summary>
    public static ODataVersion VersionFromHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
      var maxDataServiceVersion = headers
        .Where(k => string.Equals(k.Key, "MaxDataServiceVersion", StringComparison.OrdinalIgnoreCase))
        .Select(k => (int?)double.Parse(k.Value.First()))
        .FirstOrDefault();
      var oDataMax = headers
        .Where(k => string.Equals(k.Key, "OData-MaxVersion", StringComparison.OrdinalIgnoreCase))
        .Select(k => (int?)double.Parse(k.Value.First()))
        .FirstOrDefault();
      return VersionFromHeaders(maxDataServiceVersion, oDataMax);
    }

    private static ODataVersion VersionFromHeaders(int? maxDataServiceVersion, int? oDataMax)
    {
      if (maxDataServiceVersion.HasValue && !oDataMax.HasValue)
      {
        if (maxDataServiceVersion.Value > 2)
          return ODataVersion.v2 | ODataVersion.v3;
        return ODataVersion.v2;
      }
      return ODataVersion.All;
    }
    
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
    public static ODataQueryable<T> ExecuteOData<T>(this IQueryable<T> query, string uri, ExecutionSettings settings = null, ODataVersion version = ODataVersion.All)
    {
      return ExecuteOData((IQueryable)query, uri, (settings ?? new ExecutionSettings()).WithType(typeof(T)), version).Cast<T>();
    }

    /// <summary>
    /// Execute the query represented by an OData URI against an <c>IQueryable</c> data source
    /// </summary>
    /// <param name="query"><c>IQueryable</c> data source</param>
    /// <param name="inputType">Model data type</param>
    /// <param name="uri">OData URI</param>
    /// <param name="dynamicAccessor">Expression for accessing fields on dynamic model objects</param>
    /// <param name="maxPageSize">Maximum page size (used to override values specified in the URL)</param>
    public static ODataQueryable<object> ExecuteOData(this IQueryable query, string uri, ExecutionSettings settings = null, ODataVersion version = ODataVersion.All)
    {
      if (uri == null)
        throw new ArgumentNullException("uri", "URI cannot be null");

      return Parse(uri, version).Execute(query, settings);
    }

    /// <summary>
    /// Return the single child of an ODataNode
    /// </summary>
    public static ODataNode Child(this ODataNode node)
    {
      if (node.Children.Count < 1)
        return PlaceholderNode.Instance;
      if (node.Children.Count == 1)
        return node.Children[0];
      throw new InvalidOperationException("OData node contains more than one child.");
    }
  }
}
