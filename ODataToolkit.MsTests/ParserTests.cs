using ODataToolkit.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODataToolkit.MsTests
{
  [TestClass]
  public class ParserTests
  {
    [TestMethod]
    public void Parse_Query()
    {
      var tree = OData.Parse("?$format=json&$filter=Name eq 'Apple'");
      Assert.AreEqual(2, tree.QueryOption.Count);
      Assert.AreEqual("eq", tree.QueryOption["$filter"].Children.First().Text);

      tree = OData.Parse("$format=json&$filter=Name eq 'Apple'&$top=3");
      Assert.AreEqual(3, tree.QueryOption.Count);
    }

    [TestMethod]
    public void Parse_Function()
    {
      var tree = OData.Parse(@"?$filter=day(Date) eq 2");
      var initialNode = tree.QueryOption["$filter"].Children.First().Children.First();
      Assert.AreEqual(true, initialNode is CallNode);
      Assert.AreEqual(2, initialNode.Children.Count);
    }

    [TestMethod]
    public void Parse_FunctionMultiParam()
    {
      var tree = OData.Parse(@"?$filter=day(Date, 2) eq 2");
      var initialNode = tree.QueryOption["$filter"].Children.First().Children.First();
      Assert.AreEqual(true, initialNode is CallNode);
      Assert.AreEqual(3, initialNode.Children.Count);

      tree = OData.Parse(@"?$filter=day(Date, 2, 'string') eq 2");
      initialNode = tree.QueryOption["$filter"].Children.First().Children.First();
      Assert.AreEqual(true, initialNode is CallNode);
      Assert.AreEqual(4, initialNode.Children.Count);
    }

    [TestMethod]
    public void Parse_OrderBy()
    {
      var tree = OData.Parse(@"?$orderby=ReleaseDate asc, Rating desc, Another desc");
      var orderBy = tree.QueryOption["$orderby"];
      Assert.IsInstanceOfType(orderBy.Children[0], typeof(AscNode));
      Assert.IsInstanceOfType(orderBy.Children[1], typeof(DescNode));
      Assert.IsInstanceOfType(orderBy.Children[2], typeof(DescNode));

      tree = OData.Parse(@"?$orderby=ReleaseDate, Rating, Another desc");
      orderBy = tree.QueryOption["$orderby"];
      Assert.IsInstanceOfType(orderBy.Children[0], typeof(AscNode));
      Assert.IsInstanceOfType(orderBy.Children[1], typeof(AscNode));
      Assert.IsInstanceOfType(orderBy.Children[2], typeof(DescNode));
    }

    [TestMethod]
    public void Parse_NavigationProperty()
    {
      var tree = OData.Parse(@"?$orderby=Concrete/Complete,Title");
      var orderBy = tree.QueryOption["$orderby"];
      Assert.IsInstanceOfType(orderBy.Children[0], typeof(AscNode));
      Assert.AreEqual(2, orderBy.Children[0].Children[0].Children.Count);
      Assert.IsInstanceOfType(orderBy.Children[1], typeof(AscNode));
    }

    [TestMethod]
    public void Parse_Url()
    {
      var tree = OData.Parse("http://host/service/ProductsByColor(color=@color)?@color='red'&callback=2func&random=3*stuff");
      var segments = tree.PathSegments.ToArray();
      Assert.AreEqual(2, segments.Length);
      Assert.AreEqual("service", segments[0].Text);
      Assert.AreEqual("ProductsByColor", segments[1].Text);
      Assert.AreEqual(TokenType.Call, segments[1].Type);
      var call = (CallNode)segments[1];
      Assert.AreEqual("'red'", call.Arguments["color"].Text);
    }

    [TestMethod]
    public void Parse_AndClone()
    {
      var uri = OData.Parse("http://localhost:53645/_api/V4/Products?$filter=Id%20gt%203&$select=Id%2CName%2CDescription&$orderby=Description&$top=1000");
      Assert.AreEqual("http://localhost:53645/_api/V4/Products?$filter=Id gt 3&$select=Id,Name,Description&$orderby=Description&$top=1000", uri.ToString());
    }

    [TestMethod]
    public void Parse_VerifySegments()
    {
      var uri = OData.Parse("http://localhost:50359/QA/_api/int/v2/Lab_Test/?$callback=jQuery112405806855772931012_1498853440769&%24inlinecount=allpages&%24format=json&%24filter=(test_lab+eq+%27EMC%27+and+test_end+ge+datetime%272017-06-01T00%3A00%3A00%27+and+test_start+le+datetime%272017-07-01T00%3A00%3A00%27+and+(state+eq+%27Approved%27+or+state+eq+%27In+Process%27+or+state+eq+%27Not+Initiated%27+or+state+eq+%27Ready+to+Schedule%27+or+state+eq+%27Report+Review%27+or+state+eq+%27Testing+Complete%27))");
      Assert.AreEqual(5, uri.PathSegments.Count());
    }

    [TestMethod]
    public void Parse_VerifySegments_EmptyQuery()
    {
      var uri = OData.Parse("http://localhost:50359/DEV/_api/int/v2/Plan%20Template?callback=jQuery11240908378691521541_1501273153206&values%5B0%5D=&_=1501273153209");
      Assert.AreEqual(5, uri.PathSegments.Count());
    }

    [TestMethod]
    public void Parse_VerifySegments_EmptyFunctionArgs()
    {
      var uri = OData.Parse("http://localhost:50359/DEV/_api/int/v2/Active_Identities()?$callback=jQuery112406300001408986726_1501513316775&%24inlinecount=allpages&%24format=json&%24top=250");
      Assert.AreEqual(5, uri.PathSegments.Count());
    }
  }
}
