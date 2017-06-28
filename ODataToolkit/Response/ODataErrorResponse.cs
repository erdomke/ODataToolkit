using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ODataToolkit
{
  internal class ODataErrorResponse : ODataResponse
  {
    private ResponseFormat _format;
    private Exception _ex;
    private bool _renderDetails;
    private string _language;

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

    public ODataErrorResponse(ODataUri uri, Exception ex, bool renderDetails, string language = "en-US") : base(uri)
    {
      _ex = ex;
      _renderDetails = renderDetails;
      _language = language;
    }

    public override void WriteXml(XmlWriter xml)
    {
      xml.WriteStartElement("error", ns_m);
      xml.WriteElementString("code", ns_m, "");
      xml.WriteStartElement("message", ns_m);
      xml.WriteAttributeString("xml", "lang", null, _language);
      xml.WriteValue(_ex.Message);
      xml.WriteEndElement(); // message
      if (_renderDetails)
      {
        xml.WriteStartElement("innererror", ns_m);
        xml.WriteElementString("message", ns_m, _ex.Message);
        xml.WriteElementString("type", ns_m, _ex.GetType().FullName);
        xml.WriteElementString("stacktrace", ns_m, _ex.StackTrace);
        xml.WriteEndElement();
      }
      xml.WriteEndElement();
    }

    public override void WriteJson(TextWriter writer)
    {
      using (var json = new JsonTextWriter(writer, false))
      {
        json.WriteStartObject();
        json.WritePropertyName("odata.error");
        json.WriteStartObject();
        json.WriteProperty("code", "");

        json.WritePropertyName("message");
        json.WriteStartObject();
        json.WriteProperty("lang", _language);
        json.WriteProperty("value", _ex.Message);
        json.WriteEndObject();

        if (_renderDetails)
        {
          json.WritePropertyName("innererror");
          json.WriteStartObject();
          json.WriteProperty("message", _ex.Message);
          json.WriteProperty("type", _ex.GetType().FullName);
          json.WriteProperty("stacktrace", _ex.StackTrace);
          json.WriteEndObject();
        }

        json.WriteEndObject();
        json.WriteEndObject();
      }
    }
  }
}
