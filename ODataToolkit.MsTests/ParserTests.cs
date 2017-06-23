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
      Assert.AreEqual(2, tree.PathSegments.Count);
      Assert.AreEqual("service", tree.PathSegments[0].Text);
      Assert.AreEqual("ProductsByColor", tree.PathSegments[1].Text);
      Assert.AreEqual(TokenType.Call, tree.PathSegments[1].Type);
      var call = (CallNode)tree.PathSegments[1];
      Assert.AreEqual("'red'", call.Arguments["color"].Text);
     }
  }
}
