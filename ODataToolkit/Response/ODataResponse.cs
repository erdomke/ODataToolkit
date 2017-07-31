using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ODataToolkit
{
  /// <summary>
  /// Represents data for an HTTP response from an OData service
  /// </summary>
  public abstract class ODataResponse
  {
    protected const string ns_feed = "http://www.w3.org/2005/Atom";
    private string _callback;

    /// <summary>
    /// Create an OData response from a list of items
    /// </summary>
    public static ODataResponse FromList(ODataUri uri
      , IEdmNavigationSource path
      , IEnumerable<IEnumerable<KeyValuePair<string, object>>> records
      , int? totalCount)
    {
      try
      {
        return new ODataListResponse(uri, path, records.ToArray(), totalCount);
      }
      catch (Exception ex)
      {
        return FromException(uri, ex, false);
      }
    }

    /// <summary>
    /// Create an OData response from a single item
    /// </summary>
    public static ODataResponse FromItem(ODataUri uri
      , IEdmNavigationSource path
      , IEnumerable<KeyValuePair<string, object>> record)
    {
      try
      {
        return new ODataItemResponse(uri, path, record);
      }
      catch (Exception ex)
      {
        return FromException(uri, ex, false);
      }
    }

    /// <summary>
    /// Create an OData response for a service document
    /// </summary>
    public static ODataResponse FromModel_Service(ODataUri uri, IEdmModel model)
    {
      try
      {
        return new ODataServiceResponse(uri, model);
      }
      catch (Exception ex)
      {
        return FromException(uri, ex, false);
      }
    }

    /// <summary>
    /// Create an OData response for an EDMX document (at the $metadata path)
    /// </summary>
    public static ODataResponse FromModel_Edmx(ODataUri uri, IEdmModel model)
    {
      try
      {
        return new ODataEdmxResponse(uri, model);
      }
      catch (Exception ex)
      {
        return FromException(uri, ex, false);
      }
    }

    /// <summary>
    /// Create an OData response for a Swagger JSON document
    /// </summary>
    public static ODataResponse FromModel_Swagger(ODataUri uri, IEdmModel model, Version version)
    {
      try
      {
        return new SwaggerResponse(uri, model, version);
      }
      catch (Exception ex)
      {
        return FromException(uri, ex, false);
      }
    }

    /// <summary>
    /// Create an OData response for an exception
    /// </summary>
    public static ODataResponse FromException(ODataUri uri, Exception ex, bool renderDetails, string language = "en-US")
    {
      return new ODataErrorResponse(uri, ex, renderDetails, language);
    }

    protected string ns_d, ns_m;
    protected ODataUri _uri;
    protected Dictionary<string, string> _headers = new Dictionary<string, string>();

    /// <summary>
    /// Format to render the response in (XML or JSON)
    /// </summary>
    public abstract ResponseFormat Format { get; set; }

    /// <summary>
    /// Headers to include with the HTTP response
    /// </summary>
    public IDictionary<string, string> Headers { get { return _headers; } }

    public virtual int StatusCode { get { return 200; } }

    /// <summary>
    /// Write the response as either XML or JSON (based on the <see cref="Format"/> property)
    /// to the <see cref="Stream"/>
    /// </summary>
    public void WriteBytes(Stream stream)
    {
      if (Format == ResponseFormat.Xml)
      {
        var settings = new XmlWriterSettings()
        {
          Encoding = Encoding.UTF8
        };
        using (var xml = XmlWriter.Create(stream, settings))
        {
          WriteXml(xml);
        }
      }
      else
      {
        using (var writer = new StreamWriter(stream, Encoding.UTF8))
        {
          if (!string.IsNullOrEmpty(_callback))
          {
            writer.Write(_callback);
            writer.Write("(");
          }
          WriteJson(writer);
          if (!string.IsNullOrEmpty(_callback))
          {
            writer.Write(");");
          }
        }
      }
    }

    /// <summary>
    /// Write the response as XML to a <see cref="XmlWriter"/>
    /// </summary>
    public abstract void WriteXml(XmlWriter xml);

    /// <summary>
    /// Write the response as JSON to a <see cref="TextWriter"/>
    /// </summary>
    public abstract void WriteJson(TextWriter writer);

    protected ODataResponse(ODataUri uri)
    {
      _uri = uri;
      if (_uri.Version.OnlySupportsV2OrV3())
      {
        ns_d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        ns_m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
      }
      else
      {
        ns_d = "http://docs.oasis-open.org/odata/ns/data";
        ns_m = "http://docs.oasis-open.org/odata/ns/metadata";
      }

      var format = _uri.QueryOption["$format"].Child().Text;
      if (string.Equals(format, "xml", StringComparison.OrdinalIgnoreCase)
        || string.Equals(format, "atom", StringComparison.OrdinalIgnoreCase))
        Format = ResponseFormat.Xml;
      else if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
        Format = ResponseFormat.Json;
      else
        Format = ResponseFormat.Default;

      _callback = _uri.QueryOption["$callback"].Child().Text;
      if (Format == ResponseFormat.Json && !string.IsNullOrEmpty(_callback))
        _headers["Content-Type"] = "text/javascript;charset=utf-8";

      if (_uri.Version.SupportsV4())
        _headers["OData-Version"] = "4.0";
      else if (_uri.Version.SupportsV3())
        _headers["DataServiceVersion"] = "3.0";
      else
        _headers["DataServiceVersion"] = "2.0";
    }
  }
}
