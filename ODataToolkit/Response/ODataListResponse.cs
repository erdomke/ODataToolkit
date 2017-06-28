using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ODataToolkit
{
  internal class ODataListResponse : ODataResponse
  {
    private IEdmNavigationSource _path;
    private IEnumerable<IEnumerable<KeyValuePair<string, object>>> _records;
    private int? _totalCount;
    private ResponseFormat _format;

    public override ResponseFormat Format
    {
      get { return _format; }
      set
      {
        if (value == ResponseFormat.Default)
          _format = _uri.Version.OnlySupportsV2OrV3() ? ResponseFormat.Xml : ResponseFormat.Json;
        else
          _format = value;
        if (_uri.Version.SupportsV4())
          _headers["Content-Type"] = _format == ResponseFormat.Xml
            ? "application/atom+xml;type=feed;charset=utf-8"
            : "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false;charset=utf-8";
        else if (_uri.Version.SupportsV3())
          _headers["Content-Type"] = _format == ResponseFormat.Xml
            ? "application/atom+xml;type=feed;charset=utf-8"
            : "application/json;odata.metadata=minimal;odata.streaming=false;charset=utf-8";
        else
          _headers["Content-Type"] = _format == ResponseFormat.Xml
            ? "application/atom+xml;charset=utf-8"
            : "application/json;charset=utf-8";
      }
    }

    public ODataListResponse(ODataUri uri
      , IEdmNavigationSource path
      , IEnumerable<IEnumerable<KeyValuePair<string, object>>> records
      , int? totalCount) : base(uri)
    {
      _path = path;
      _records = records;
      _totalCount = totalCount;
    }

    public override void WriteXml(XmlWriter xml)
    {
      xml.WriteStartElement("feed", ns_feed);
      xml.WriteAttributeString("xml", "base", null, _uri.UriBuilder().Segment("").ToString());
      xml.WriteAttributeString("xmlns", "d", null, ns_d);
      xml.WriteAttributeString("xmlns", "m", null, ns_m);
      if (!_uri.Version.OnlySupportsV2OrV3())
      {
        xml.WriteAttributeString("context", "http://docs.oasis-open.org/odata/ns/metadata", _uri.UriBuilder().Segment("$metadata").Raw('#').Raw(_path.Name).ToString());
      }

      if (ShowCount())
        xml.WriteElementString("count", ns_m, (_totalCount ?? _records.Count()).ToString());

      xml.WriteElementString("id", ns_feed, _uri.GetLeftPart(UriPartial.Path));
      xml.WriteStartElement("title", ns_feed);
      xml.WriteAttributeString("type", "text");
      xml.WriteValue(_path.Name);
      xml.WriteEndElement(); // title
      xml.WriteElementString("updated", ns_feed, DateTime.UtcNow.ToString("s") + "Z");
      xml.WriteStartElement("link", ns_feed);
      xml.WriteAttributeString("rel", "self");
      xml.WriteAttributeString("title", _path.Name);
      xml.WriteAttributeString("href", _uri.PathBuilder().ToString());
      xml.WriteEndElement(); // link

      foreach (var record in _records)
      {
        WriteXmlItem(xml, _path, record, false);
      }

      xml.WriteEndElement(); // feed
    }

    private bool ShowCount()
    {
      return (_uri.Version.OnlySupportsV2OrV3()
        && string.Equals(_uri.QueryOption["$inlinecount"].Child().Text, "allpages", StringComparison.OrdinalIgnoreCase))
      || (_uri.Version.SupportsV4()
        && string.Equals(_uri.QueryOption["$count"].Child().Text, "true", StringComparison.OrdinalIgnoreCase));
    }

    public override void WriteJson(TextWriter writer)
    {
      using (var json = new JsonTextWriter(writer, false))
      {
        var inlineCount = ShowCount();
        json.WriteStartObject();
        if (_uri.Version.SupportsV4() || _uri.Version.SupportsV3())
        {
          json.WriteProperty("@odata.context", _uri.UriBuilder().Segment("$metadata#").Raw(_path.Name));

          var top = _uri.QueryOption.Top;
          if (top.HasValue)
          {
            var skip = _uri.QueryOption.Skip ?? 0;
            var nextLink = _uri.Clone();
            nextLink.QueryOption.Skip = skip + top.Value;

            if (skip + top.Value < (_totalCount ?? (skip + _records.Count() + 1)))
            {
              json.WritePropertyName("@odata.nextLink");
              json.WriteValue(nextLink);
            }
          }

          if (inlineCount)
          {
            json.WriteProperty("@odata.context", _totalCount ?? _records.Count());
          }

          json.WritePropertyName("value");
        }
        else if (_uri.Version.SupportsV2())
        {
          json.WritePropertyName("d");

          if (inlineCount)
          {
            json.WriteStartObject();
            json.WritePropertyName("results");
          }
        }

        json.WriteStartArray();
        foreach (var record in _records)
        {
          json.WriteStartObject();
          WriteJsonItem(json, _path, record, _uri.Version.OnlySupportsV2OrV3());
          json.WriteEndObject();
        }
        json.WriteEndArray();

        if (inlineCount)
        {
          json.WritePropertyName("__count");
          json.WriteValue(_totalCount ?? _records.Count());

          json.WriteEndObject(); // Close the 'd' object
        }
        json.WriteEndObject();
      }
    }
  }
}
