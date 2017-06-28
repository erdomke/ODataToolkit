using ODataToolkit.Csdl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ODataToolkit
{
  class ODataEdmxResponse : ODataResponse
  {
    private IEdmModel _model;
    private ResponseFormat _format;

    public override ResponseFormat Format
    {
      get { return _format; }
      set
      {
        if (value == ResponseFormat.Json)
          throw new NotSupportedException();
        _format = ResponseFormat.Xml;
        _headers["Content-Type"] = "application/xml;charset=utf-8";
      }
    }

    public ODataEdmxResponse(ODataUri uri, IEdmModel model) : base(uri)
    {
      _model = model;
      Format = ResponseFormat.Xml;
    }

    public override void WriteXml(XmlWriter xml)
    {
      var ns_edmx = _uri.Version.SupportsV4() ? "http://docs.oasis-open.org/odata/ns/edmx" : "http://schemas.microsoft.com/ado/2007/06/edmx";
      xml.WriteStartElement("edmx", "Edmx", ns_edmx);
      xml.WriteAttributeString("Version", _uri.Version.SupportsV4() ? "4.0" : "1.0");
      xml.WriteStartElement("edmx", "DataServices", ns_edmx);
      if (_uri.Version.OnlySupportsV2OrV3())
      {
        var ns_m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        var isVer3 = _uri.Version.SupportsV3();
        xml.WriteAttributeString("m", "DataServiceVersion", ns_m, isVer3 ? "3.0" : "2.0");
        if (isVer3)
          xml.WriteAttributeString("m", "MaxDataServiceVersion", ns_m, "3.0");
      }

      IEnumerable<Validation.EdmError> errors;
      _model.TryWriteSchema(xml, out errors);

      xml.WriteEndElement();
      xml.WriteEndElement();
    }

    public override void WriteJson(TextWriter writer)
    {
      throw new NotSupportedException();
    }
  }
}
