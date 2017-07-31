using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ODataToolkit
{
  internal abstract class ODataResponseBase : ODataResponse
  {
    private readonly static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public ODataResponseBase(ODataUri uri) : base(uri) { }
    
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
              .WriteRaw(@")\/""")
              .NeedsComma();
        }
        else
        {
          json.WriteValue(value);
        }
      }
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
        if (dict != null && type is IEdmComplexTypeReference)
        {
          var complex = (IEdmComplexTypeReference)type;
          foreach (var kvp in dict)
          {
            xml.WriteStartElement(kvp.Key, ns_d);
            RenderValue(xml, kvp.Value, complex.FindProperty(kvp.Key).Type);
            xml.WriteEndElement();
          }
        }
        else if (arr != null && type is IEdmCollectionTypeReference)
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
