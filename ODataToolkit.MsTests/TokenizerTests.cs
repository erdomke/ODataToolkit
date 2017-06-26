using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ODataToolkit.MsTests
{
  [TestClass]
  public class TokenizerTests
  {
    private void VerifySequence(string value, params TokenType[] types)
    {
      var tokens = OData.Tokenize(value).ToArray();
      var actual = tokens.Select(t => t.Type).ToArray();
      CollectionAssert.AreEqual(types, actual);
    }

    [TestMethod]
    public void Tokens_SimpleUrl()
    {
      VerifySequence("http://host/service/Products"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_FunctionNoParam()
    {
      VerifySequence("http://host/service/Products/Model.MostExpensive()"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.OpenParen, TokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_FunctionOneParam()
    {
      VerifySequence("http://host/service/ProductsByCategoryId(categoryId=2)"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Identifier, TokenType.QueryAssign, TokenType.Integer, TokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_FunctionTwoParams()
    {
      VerifySequence("https://host/service/Orders(1)/Items(OrderID=1,ItemNo=2)"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Integer, TokenType.CloseParen
        , TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Identifier, TokenType.QueryAssign, TokenType.Integer, TokenType.Comma
        , TokenType.Identifier, TokenType.QueryAssign, TokenType.Integer, TokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_FunctionWithAlias()
    {
      VerifySequence("http://host/service/ProductsByColor(color=@color)?@color='red'"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Identifier, TokenType.QueryAssign, TokenType.Parameter, TokenType.CloseParen
        , TokenType.Question, TokenType.Parameter, TokenType.QueryAssign, TokenType.String);
    }

    [TestMethod]
    public void Tokens_SingleItem()
    {
      VerifySequence("http://host/service/Categories(1)"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Integer, TokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_SingleItemPath()
    {
      VerifySequence("http://host/service/Products(1)/Supplier"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Integer, TokenType.CloseParen
        , TokenType.PathSeparator, TokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_SingleItemFunction()
    {
      VerifySequence("http://host/service/Products(1)/Model.MostRecentOrder()"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Integer, TokenType.CloseParen
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.OpenParen, TokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_UnexpectedQueryString()
    {
      VerifySequence("http://host/service/ProductsByColor(color=@color)?@color='red'&callback=2func&random=3*stuff"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Identifier, TokenType.QueryAssign, TokenType.Parameter
        , TokenType.CloseParen, TokenType.Question, TokenType.Parameter, TokenType.QueryAssign
        , TokenType.String, TokenType.Amperstand, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Amperstand, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_TwoFunctions()
    {
      VerifySequence("http://host/service/Categories(ID=1)/Products(ID=1)"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Identifier, TokenType.QueryAssign, TokenType.Integer, TokenType.CloseParen
        , TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Identifier, TokenType.QueryAssign, TokenType.Integer, TokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_EscapeString01()
    {
      var url = "http://host/service/People('O''Neil')";
      VerifySequence(url
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.String, TokenType.CloseParen);
      var parts = OData.Tokenize(url).ToArray();
      Assert.AreEqual("O'Neil", parts[8].AsPrimitive());
    }

    [TestMethod]
    public void Tokens_EscapeString02()
    {
      var url = "http://host/service/People(%27O%27%27Neil%27)";
      VerifySequence(url
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.String, TokenType.CloseParen);
      var parts = OData.Tokenize(url).ToArray();
      Assert.AreEqual("O'Neil", parts[8].AsPrimitive());
    }

    [TestMethod]
    public void Tokens_EscapeString03()
    {
      var url = "http://host/service/People%28%27O%27%27Neil%27%29";
      VerifySequence(url
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.String, TokenType.CloseParen);
      var parts = OData.Tokenize(url).ToArray();
      Assert.AreEqual("O'Neil", parts[8].AsPrimitive());
    }

    [TestMethod]
    public void Tokens_EscapeString04()
    {
      var url = "http://host/service/Categories('Smartphone%2FTablet')";
      VerifySequence(url
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.OpenParen, TokenType.String, TokenType.CloseParen);
      var parts = OData.Tokenize(url).ToArray();
      Assert.AreEqual("Smartphone/Tablet", parts[8].AsPrimitive());
    }

    [TestMethod]
    public void Tokens_Escape()
    {
      var url = "?$callback=jQuery112304312923812233427_1494592722830&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(name)%2C%27c+b%27)";
      VerifySequence(url
        , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Amperstand, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Amperstand, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Amperstand, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Identifier, TokenType.OpenParen, TokenType.Identifier, TokenType.CloseParen
        , TokenType.Comma, TokenType.String, TokenType.CloseParen);
      var parts = OData.Tokenize(url).ToArray();
      Assert.AreEqual("c b", parts[parts.Length - 2].AsPrimitive());
    }


    [TestMethod]
    public void Tokens_FilterQuery01()
    {
      VerifySequence("http://host/service/Categories?$filter=Products/$count gt 0"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Navigation, TokenType.Identifier
        , TokenType.Whitespace, TokenType.GreaterThan, TokenType.Whitespace, TokenType.Integer);
    }

    [TestMethod]
    public void Tokens_FilterQuery02()
    {
      VerifySequence("http://host/service/$all/Model.Customer?$filter=contains(Name,'red')"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.PathSeparator, TokenType.Identifier
        , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.OpenParen, TokenType.Identifier, TokenType.Comma, TokenType.String, TokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_FilterQuery_BoolLogic()
    {
      VerifySequence("http://host/service/Products?$filter=Name eq 'Milk' and Price lt 2.55"
          , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
          , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
          , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
          , TokenType.Whitespace, TokenType.Equal, TokenType.Whitespace, TokenType.String
          , TokenType.Whitespace, TokenType.And, TokenType.Whitespace, TokenType.Identifier
          , TokenType.Whitespace, TokenType.LessThan, TokenType.Whitespace, TokenType.Double);
      VerifySequence("http://host/service/Products?$filter=Name eq 'Milk' or Price lt 2.55"
          , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
          , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
          , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
          , TokenType.Whitespace, TokenType.Equal, TokenType.Whitespace, TokenType.String
          , TokenType.Whitespace, TokenType.Or, TokenType.Whitespace, TokenType.Identifier
          , TokenType.Whitespace, TokenType.LessThan, TokenType.Whitespace, TokenType.Double);
    }

    [TestMethod]
    public void Tokens_FilterQuery_Operators()
    {
      VerifySequence("http://host/service/Products?$filter=Name eq 'Milk'"
          , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
          , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
          , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
          , TokenType.Whitespace, TokenType.Equal, TokenType.Whitespace, TokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name ne 'Milk'"
          , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
          , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
          , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
          , TokenType.Whitespace, TokenType.NotEqual, TokenType.Whitespace, TokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name gt 'Milk'"
          , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
          , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
          , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
          , TokenType.Whitespace, TokenType.GreaterThan, TokenType.Whitespace, TokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name ge 'Milk'"
          , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
          , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
          , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
          , TokenType.Whitespace, TokenType.GreaterThanOrEqual, TokenType.Whitespace, TokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name lt 'Milk'"
          , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
          , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
          , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
          , TokenType.Whitespace, TokenType.LessThan, TokenType.Whitespace, TokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name le 'Milk'"
          , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
          , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
          , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
          , TokenType.Whitespace, TokenType.LessThanOrEqual, TokenType.Whitespace, TokenType.String);
    }


    [TestMethod]
    public void Tokens_FilterQuery_DataTypes()
    {
      var urls = new Dictionary<string, TokenType>()
      {
        { "?$filter=NullValue eq null", TokenType.Null },
        { "?$filter=TrueValue eq true", TokenType.True },
        { "?$filter=FalseValue eq false", TokenType.False },
        { "?$filter=BinaryValue eq binary'T0RhdGE'", TokenType.Base64 },
        { "?$filter=BinaryValue eq X'ffa3cd'", TokenType.Binary },
        { "?$filter=IntegerValue lt -128", TokenType.Integer },
        { "?$filter=IntegerValue lt -128L", TokenType.Long },
        { "?$filter=DoubleValue ge 0.31415926535897931e1", TokenType.Double },
        { "?$filter=DoubleValue ge 0.31415926535897931M", TokenType.Decimal },
        { "?$filter=DoubleValue ge 0.31415926535897931d", TokenType.Double },
        { "?$filter=DoubleValue ge 0.314f", TokenType.Single },
        { "?$filter=SingleValue eq INF", TokenType.PosInfinity },
        { "?$filter=DecimalValue eq 34.95", TokenType.Double },
        { "?$filter=StringValue eq 'Say Hello,then go'", TokenType.String },
        { "?$filter=DateValue eq 2012-12-03", TokenType.Date },
        { "?$filter=DateValue eq datetime'2012-12-03'", TokenType.Date },
        { "?$filter=DateTimeOffsetValue eq 2012-12-03T07:16:23Z", TokenType.Date },
        { "?$filter=DateTimeOffsetValue eq datetimeoffset'2012-12-03T07:16:23Z'", TokenType.Date },
        { "?$filter=DurationValue eq duration'P12DT23H59M59.999999999999S'", TokenType.Duration },
        { "?$filter=DurationValue eq time'P12DT23H59M59.999999999999S'", TokenType.Duration },
        { "?$filter=TimeOfDayValue eq 07:59:59.999", TokenType.TimeOfDay },
        { "?$filter=GuidValue eq 01234567-89ab-cdef-0123-456789abcdef", TokenType.Guid },
        { "?$filter=GuidValue eq guid'01234567-89ab-cdef-0123-456789abcdef'", TokenType.Guid },
        { "?$filter=Int64Value eq 0", TokenType.Integer },
      };

      foreach (var url in urls)
      {
        Assert.AreEqual(url.Value, OData.Tokenize(url.Key).Last().Type);
      }
    }

    [TestMethod]
    public void Tokens_OrderBy01()
    {
      VerifySequence("http://host/service/Categories?$orderby=Products/$count"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Navigation, TokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_Expand01()
    {
      VerifySequence("http://host/service/Orders?$expand=Customer/Model.VipCustomer"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Navigation, TokenType.Identifier, TokenType.Period, TokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_Search01()
    {
      VerifySequence("http://host/service/$all?$search=red"
        , TokenType.Scheme, TokenType.PathSeparator, TokenType.Authority
        , TokenType.PathSeparator, TokenType.Identifier, TokenType.PathSeparator, TokenType.Identifier
        , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_Query()
    {
      VerifySequence("?$format=json&$filter=Name eq 'Apple'"
        , TokenType.Question, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Amperstand, TokenType.QueryName, TokenType.QueryAssign, TokenType.Identifier
        , TokenType.Whitespace , TokenType.Equal, TokenType.Whitespace, TokenType.String);
    }
  }
}
