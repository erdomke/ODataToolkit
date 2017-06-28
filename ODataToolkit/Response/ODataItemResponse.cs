using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ODataToolkit
{
  internal class ODataItemResponse : ODataResponse
  {
    private IEdmNavigationSource _path;
    private IEnumerable<KeyValuePair<string, object>> _record;
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
          ? "application/atom+xml;charset=utf-8"
          : "application/json;charset=utf-8";
      }
    }

    public ODataItemResponse(ODataUri uri
      , IEdmNavigationSource path
      , IEnumerable<KeyValuePair<string, object>> record) : base(uri)
    {
      _path = path;
      _record = record;
    }

    public override void WriteXml(XmlWriter xml)
    {
      WriteXmlItem(xml, _path, _record, true);
    }
    public override void WriteJson(TextWriter writer)
    {
      using (var json = new JsonTextWriter(writer, false))
      {
        json.WriteStartObject();
        WriteJsonItem(json, _path, _record, true);
        json.WriteEndObject();
      }
    }
  }
}
