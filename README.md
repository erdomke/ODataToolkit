# ODataToolkit

The C# library aims to be a platform-agnostic toolkit for developing OData web services.  It can  be 
used from Microsoft's Web API, Nancy, or the platform of your choice.  At it's core is an expression
parser which builds an abstract syntax tree from an OData URI and converts that tree to a LINQ 
expression that can be used with an `IQueryable` data source.

## Features

* Allows you to use an OData URL to query an `IQueryable` data source.
* Allows you to generate OData-formatted responses from `IDictionary<string,object>` objects.
* Supports loosely-typed data structures, just specify the appropriate code using the 
  `WithDynamicAccessor` method on the `ExecutionSettings` class.
* Allows you to parse and/or tokenize OData URLs to so you can process them yourself (e.g. retrieve
  information about function calls or requests for an item by id.)
* No dependencies on external libraries
* Allows you to target multiple OData versions with support for OData v2, v3, and v4 syntax.  This 
  includes support for
  * Version-specific literal syntax
  * Version-specific JSON and XML responses
  * `$top` and `$skip` query parameters
  * `$orderby` for simple types, subproperties, and complex types (with Linq to Objects only, via 
    `IComparable`)
  * `$filter` for simple properties & subproperties
  * `$select` for simple properties
  * `$inlinecount` (v2, v3) and `$count` (v4)
  * `$expand` if the `IQueryable` has a method with the signature `IQueryable Include(string path)`
  * Functions and collection aggregates such as `any()` and `all()` with predicates
  * Parameters, for example `$filter=Name eq @value&$@value='thing'`

## Installing via NuGet

[![NuGet version](https://badge.fury.io/nu/ODataToolkit.svg)](https://www.nuget.org/packages/ODataToolkit)

```
Install-Package ODataToolkit
```

## Support

The library supports the follwoing frameworks

* .NET Framework 3.5+
* (Coming soon) [.Net Standard 2.0](https://docs.microsoft.com/en-us/dotnet/articles/standard/library). 
  
## Usage

Work directly with Linq to Object IQueryables:

```csharp
var collection = new List<Dummy>
{
   new Dummy("Apple", 5, new DateTime(2005, 01, 01), true),
   new Dummy("Custard", 3, new DateTime(2007, 01, 01), true),
   new Dummy("Banana", 2, new DateTime(2003, 01, 01), false),
   new Dummy("Eggs", 1, new DateTime(2000, 01, 01), true),
   new Dummy("Dogfood", 4, new DateTime(2009, 01, 01), false),
}.AsQueryable();

var ordered = collection.ExecuteOData("?$orderby=Complete,Age");
var paged = collection.ExecuteOData("?$skip=2$top=2");
```

Work with dynamic objects

```csharp
var item1 = new Dictionary<string, object>();
item1["Age"] = 25;
item1["Name"] = "Kathryn";

var item2 = new Dictionary<string, object>();
item2["Age"] = 28;
item2["Name"] = "Pete";

collection = new List<Dictionary<string, object>> { item1, item2 }.AsQueryable();

var ordered = collection.ExecuteOData("?$orderby=Age desc", OData.DictionaryAccessor);
// OR 
var ordered2 = collection.ExecuteOData("?$orderby=Age desc", (obj, key) => ((IDictionary<string, object>)obj)[key]);
```

Tested against Entity Framework:

```csharp
var query = this.unitOfWork.Data.Where(o => o.SomeRepoLevelFilter == x);
var extended = query.ExecuteOData("?$filter=Name eq @food and Complete eq true&@food='Eggs'");
```

Complete service example (with Nancy).  For the complete example, see the [source code](tree/master/ODataToolkit.Nancy)

```csharp
public class Module : NancyModule
{
  private static IEdmModel _model;               // The EDM module (definition not shown)
  private static IQueryable<Product> _products;  // The data source (definition not shown)

  /// <summary>Define the routes for each OData version</summary>
  public Module()
  {
    this.Get["_api/V4"] = _ => GetResponse(ODataVersion.All);
    this.Get["_api/V4/{path*}"] = _ => GetResponse(ODataVersion.All);
    this.Get["_api/V3"] = _ => GetResponse(ODataVersion.v3);
    this.Get["_api/V3/{path*}"] = _ => GetResponse(ODataVersion.v3);
    this.Get["_api/V2"] = _ => GetResponse(ODataVersion.v2);
    this.Get["_api/V2/{path*}"] = _ => GetResponse(ODataVersion.v2);
  }

  /// <summary>Create an OData response based on the URL</summary>
  private Response GetResponse(ODataVersion version)
  {
    // Parse the URL and specify the number of segments at the root of the 
    // path
    var uri = OData.Parse(Request.Url.ToString(), version)
        .WithRootSegmentCount(2);

    // Return response for the root of the path
    var path = uri.PathBuilder().ToString();
    if (string.IsNullOrEmpty(path))
      return GetResponse(ODataResponse.FromModel_Service(uri, _model));
    if (path.StartsWith("$metadata", StringComparison.OrdinalIgnoreCase))
      return GetResponse(ODataResponse.FromModel_Edmx(uri, _model));
    if (path.StartsWith("$swagger", StringComparison.OrdinalIgnoreCase))
      return GetResponse(ODataResponse.FromModel_Swagger(uri, _model, new Version(1, 0)));

    // Query the data source and return the appropriate response
    ODataResponse oResp;
    try
    {
      oResp = uri
        .Execute(_products, new ExecutionSettings().WithEdmModel(_model))
        .CreateResponse(_model, null);
    }
    catch (Exception ex)
    {
      oResp = ODataResponse.FromException(uri, ex, false);
    }
    return GetResponse(oResp);
  }

  /// <summary>Convert an OData response to a Nancy response</summary>
  private Response GetResponse(ODataResponse oResp)
  {
    var resp = new Response().WithStatusCode(HttpStatusCode.OK);
    foreach (var header in oResp.Headers)
    {
      resp.WithHeader(header.Key, header.Value);
    }
    resp.Contents = oResp.WriteBytes;
    return resp;
  }
}
```