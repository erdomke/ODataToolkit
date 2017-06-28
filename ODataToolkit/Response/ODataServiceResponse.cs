using System;
using System.IO;
using System.Xml;

namespace ODataToolkit
{
  internal class ODataServiceResponse : ODataResponse
  {
    private IEdmModel _model;
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
        _headers["Content-Type"] = _format == ResponseFormat.Xml
          ? "application/xml;charset=utf-8"
          : "application/json;charset=utf-8";
      }
    }

    public ODataServiceResponse(ODataUri uri, IEdmModel model) : base(uri)
    {
      _model = model;
    }

    public override void WriteXml(XmlWriter xml)
    {
      var ns_atom = "http://www.w3.org/2005/Atom";
      var ns_app = "http://www.w3.org/2007/app";

      xml.WriteStartElement("service", ns_app);
      xml.WriteAttributeString("xml", "base", null, _uri.UriBuilder().Segment("").ToString());
      xml.WriteAttributeString("xmlns", "atom", null, ns_atom);
      if (_uri.Version.SupportsV4())
        xml.WriteAttributeString("m", "context", "http://docs.oasis-open.org/odata/ns/metadata", _uri.UriBuilder().Segment("$metadata").ToString());

      xml.WriteStartElement("workspace", ns_app);

      xml.WriteStartElement("title", ns_atom);
      if (_uri.Version.SupportsV4())
        xml.WriteAttributeString("type", "text");
      xml.WriteValue("Default");
      xml.WriteEndElement();

      foreach (var set in _model.EntityContainer.EntitySets())
      {
        xml.WriteStartElement("collection", ns_app);
        xml.WriteAttributeString("href", set.Name);
        xml.WriteStartElement("title", ns_atom);
        if (_uri.Version.SupportsV4())
          xml.WriteAttributeString("type", "text");
        xml.WriteValue(set.Name);
        xml.WriteEndElement();
        xml.WriteEndElement();
      }

      xml.WriteEndElement(); // workspace
      xml.WriteEndElement(); // service
    }

    public override void WriteJson(TextWriter writer)
    {
      using (var json = new JsonTextWriter(writer, false))
      {
        if (_uri.Version.SupportsV4() || _uri.Version.SupportsV3())
        {
          json.WriteStartObject();
          json.WriteProperty("@odata.context", _uri.UriBuilder().Segment("$metadata"));
          json.WritePropertyName("value");
          json.WriteStartArray();
          foreach (var set in _model.EntityContainer.EntitySets())
          {
            json.WriteStartObject();
            json.WriteProperty("name", set.Name);
            json.WriteProperty("kind", "EntitySet");
            json.WriteProperty("url", set.Name);
            json.WriteEndObject();
          }
          json.WriteEndArray();
          json.WriteEndObject();
        }
        else
        {
          json.WriteStartObject();
          json.WritePropertyName("d");
          json.WriteStartObject();
          json.WritePropertyName("EntitySets");
          json.WriteStartArray();
          foreach (var set in _model.EntityContainer.EntitySets())
          {
            json.WriteValue(set.Name);
          }
          json.WriteEndArray();
          json.WriteEndObject();
          json.WriteEndObject();
        }
      }
    }
  }
}
