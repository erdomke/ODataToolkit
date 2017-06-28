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
    private readonly static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
    /// Headers to include with the HTTP response
    /// </summary>
    public IDictionary<string, string> Headers { get { return _headers; } }

    /// <summary>
    /// Format to render the response in (XML or JSON)
    /// </summary>
    public abstract ResponseFormat Format { get; set; }

    /// <summary>
    /// Write the response as either XML or JSON (based on the <see cref="Format"/> property)
    /// to the <see cref="Stream"/>
    /// </summary>
    public void WriteBytes(Stream stream)
    {
      if (Format == ResponseFormat.Xml)
      {
        using (var xml = XmlWriter.Create(stream))
        {
          WriteXml(xml);
        }
      }
      else
      {
        using (var writer = new StreamWriter(stream))
        {
          WriteJson(writer);
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

      if (_uri.Version.SupportsV4())
        _headers["OData-Version"] = "4.0";
      else if (_uri.Version.SupportsV3())
        _headers["DataServiceVersion"] = "3.0";
      else
        _headers["DataServiceVersion"] = "2.0";
    }

    private string GetItemUrl(IEdmNavigationSource path
      , IEnumerable<KeyValuePair<string, object>> record)
    {
      var entity = (IEdmEntityType)((IEdmCollectionType)path.Type).ElementType.ToStructuredType();
      var keys = entity.Key()
        .Select(k => record
          .Where(r => r.Key == k.Name)
          .Select(r => r.Value)
          .FirstOrDefault())
        .ToArray();
      if (keys.All(k => k != null))
        return _uri.UriBuilder().Segment(path.Name).Raw('(').Argument(keys).Raw(')').ToString();
      return null;
    }

    protected void WriteJsonItem(JsonTextWriter json
      , IEdmNavigationSource path
      , IEnumerable<KeyValuePair<string, object>> record
      , bool renderMetadata)
    {
      if (renderMetadata)
      {
        var entity = (IEdmEntityType)((IEdmCollectionType)path.Type).ElementType.ToStructuredType();
        if (_uri.Version.SupportsV4() || _uri.Version.SupportsV3())
        {
          json.WritePropertyName("@odata.context");
          json.WriteValue(_uri.UriBuilder().Segment("$metadata"));
        }
        else
        {
          json.WritePropertyName("__metadata");
          json.WriteStartObject();
          var url = GetItemUrl(path, record);
          if (!string.IsNullOrEmpty(url))
          {
            json.WritePropertyName("uri");
            json.WriteValue(url);
          }
          json.WritePropertyName("type");
          json.WriteValue(entity.FullTypeName());
          json.WriteEndObject();
        }
      }

      foreach (var prop in record)
      {
        json.WritePropertyName(prop.Key);
        RenderValue(json, prop.Value);
      }
    }

    private void RenderValue(JsonTextWriter json, object value)
    {
      if (value == null)
      {
        json.WriteValue(value);
      }
      else
      {
        var dict = value as IEnumerable<KeyValuePair<string, object>>;
        var arr = value as IEnumerable;
        DateTime date;
        if (dict != null)
        {
          json.WriteStartObject();
          foreach (var kvp in dict)
          {
            json.WritePropertyName(kvp.Key);
            RenderValue(json, kvp.Value);
          }
          json.WriteEndObject();
        }
        else if (arr != null && !(value is string))
        {
          json.WriteStartArray();
          foreach (var val in arr)
          {
            RenderValue(json, val);
          }
          json.WriteEndArray();
        }
        else if (TryGetUtcDate(value, out date))
        {
          if (_uri.Version.SupportsV4() || _uri.Version.SupportsV3())
            json.WriteValue(date.ToString("s") + "Z");
          else
            json.WriteRaw(@"""\/Date(")
              .WriteRaw(((long)(date - UnixEpoch).TotalMilliseconds).ToString())
              .WriteRaw(@")\/""");
        }
        else
        {
          json.WriteValue(value);
        }
      }
    }

    protected void WriteXmlItem(XmlWriter xml
      , IEdmNavigationSource path
      , IEnumerable<KeyValuePair<string, object>> record
      , bool renderMetadata)
    {
      xml.WriteStartElement("entry", ns_feed);
      if (renderMetadata)
      {
        xml.WriteAttributeString("xml", "base", null, _uri.UriBuilder().Segment("").ToString());
        xml.WriteAttributeString("xmlns", "d", null, ns_d);
        xml.WriteAttributeString("xmlns", "m", null, ns_m);
      }

      xml.WriteElementString("id", ns_feed, GetItemUrl(path, record));
      xml.WriteElementString("title", ns_feed, null);
      xml.WriteElementString("updated", ns_feed, DateTime.UtcNow.ToString("s") + "Z");
      xml.WriteStartElement("author", ns_feed);
      xml.WriteElementString("name", ns_feed, null);
      xml.WriteEndElement();

      xml.WriteStartElement("content", ns_feed);
      xml.WriteAttributeString("type", "application/xml");
      xml.WriteStartElement("properties", ns_m);
      var entity = (IEdmEntityType)((IEdmCollectionType)path.Type).ElementType.ToStructuredType();
      foreach (var prop in record)
      {
        xml.WriteStartElement(prop.Key, ns_d);
        var meta = entity.FindProperty(prop.Key);
        RenderValue(xml, prop.Value, meta.Type);
        xml.WriteEndElement();
      }
      xml.WriteEndElement();
      xml.WriteEndElement();

      xml.WriteEndElement();
    }

    private void RenderValue(XmlWriter xml, object value, IEdmTypeReference type)
    {
      if (value == null)
      {
        xml.WriteAttributeString("null", ns_m, "true");
      }
      else
      {
        if (!type.IsString())
        {
          var typeName = type.FullName();
          if (_uri.Version.SupportsV4() && typeName.StartsWith("Edm."))
            typeName = typeName.Substring(4);
          xml.WriteAttributeString("type", ns_m, typeName);
        }

        var arr = value is string ? null : value as IEnumerable;
        var dict = value as IDictionary<string, object>;
        DateTime date;
        if (arr != null)
        {
          var coll = (IEdmCollectionTypeReference)type;
          var elemType = coll.ElementType();
          foreach (var elem in arr)
          {
            xml.WriteStartElement("element", ns_d);
            if (!elemType.IsPrimitive())
              xml.WriteAttributeString("type", ns_m, elemType.FullName());
            RenderValue(xml, elem, elemType);
            xml.WriteEndElement();
          }
        }
        else if (dict != null)
        {
          var complex = (IEdmComplexTypeReference)type;
          foreach (var kvp in dict)
          {
            xml.WriteStartElement(kvp.Key, ns_d);
            RenderValue(xml, value, complex.FindProperty(kvp.Key).Type);
            xml.WriteEndElement();
          }
        }
        else if (TryGetUtcDate(value, out date))
        {
          xml.WriteValue(date.ToString("s") + "Z");
        }
        else
        {
          xml.WriteValue(value);
        }
      }
    }

    private bool TryGetUtcDate(object value, out DateTime date)
    {
      if (value is DateTime)
      {
        date = ((DateTime)value);
        if (date.Kind != DateTimeKind.Utc)
          date = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local, TimeZoneInfo.Utc);
        return true;
      }
      else if (value is DateTimeOffset)
      {
        var offset = ((DateTimeOffset)value).ToUniversalTime();
        date = offset.DateTime;
        return true;
      }
      date = DateTime.MinValue;
      return false;
    }
  }
}
