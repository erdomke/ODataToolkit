using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace ODataToolkit.MsTests
{
  [TestClass]
  public class ConversionTests
  {
    List<Dictionary<string, object>> vals = new List<Dictionary<string, object>>()
    {
       new Dictionary<string, object>()
          {
            {"age", "7" },
            {"name", "bob" }
          },
      new Dictionary<string, object>()
          {
            {"age", "10" },
            {"name", "jane" }
          }
     };

    [TestMethod]
    public void ODataV2ConvertToJsonResponse()
    {
      var resp = GetResponseForVersion(ODataVersion.v2, vals);
      var json = BuildJson(resp);

      Assert.AreEqual(@"{""d"":{""results"":[{""__metadata"":{""uri"":""http://localhost/entityName()""," +
        @"""type"":""nsName.Name""},""age"":""7"",""name"":""bob""},{""__metadata"":{""uri"":""http://localhost/entityName()""," +
        @"""type"":""nsName.Name""},""age"":""10"",""name"":""jane""}],""__count"":5}}", json);
    }

    [TestMethod]
    public void ODataV4ConvertToJsonResponse()
    {
      var resp = GetResponseForVersion(ODataVersion.v4, vals);
      var json = BuildJson(resp);

      Assert.AreEqual(@"{""@odata.context"":""http://localhost/$metadata#entityName"",""@odata.count"":5," +
        @"""value"":[{""age"":""7"",""name"":""bob""},{""age"":""10"",""name"":""jane""}],""__count"":5}", json);
    }

    private ODataResponse GetResponseForVersion(ODataVersion v, List<Dictionary<string, object>> l)
    {
      var entityType = new EdmEntityType("nsName", "Name");
      var entitySet = new EdmEntitySet("entityName", entityType);
      var uri = "http://localhost";
      switch (v)
      {
        case ODataVersion.v2:
        case ODataVersion.v3:
          uri += "?$inlinecount=allpages"; break;
        case ODataVersion.v4:
          uri += "?$count=true"; break;
        default: return null;
      }

      return new ODataEnumerable<Dictionary<string, object>>(new[] { l[0], l[1] }, OData.Parse(uri, v))
        .WithDictionaryFactory(i => (Dictionary<string, object>)i)
        .CreateResponse(entitySet, 5);
    }

    private string BuildJson(ODataResponse resp)
    {
      using (var ms = new MemoryStream())
      using (var sw = new StreamWriter(ms))
      {
        resp.WriteJson(sw);
        sw.Flush();
        sw.BaseStream.Seek(0, SeekOrigin.Begin);

        using (var sr = new StreamReader(sw.BaseStream))
          return sr.ReadToEnd();
      }
    }
  }
}
