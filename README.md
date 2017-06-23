# ODataToolkit

The C# library aims to be a platform-agnostic toolkit for developing OData web services.  It can  be 
used from Microsoft's Web API, Nancy, or the platform of your choice.  At it's core is an expression
parser which builds an abstract syntax tree from an OData URI and converts that tree to a LINQ 
expression that can be used with an `IQueryable` data source.

## Features

* No dependencies on external libraries
* Target multiple OData versions with support for OData v2, v3, and v4 syntax.  This includes support for
  * string, int32, bool, datetime, decimal, double, single, guid, long data types
  * nullable types & the null keyword
  * $top
  * $skip (must be used in conjunction with orderby in Linq to Entities)
  * $orderby:
    * simple types,
    * subproperties
    * complex types ( Linq to Objects only, via IComparable, )
  * $filter - simple properties & subproperties
  * $select - simple properties
  * Functions and Collection Aggregates (any and all with predicates)
  * Parameters
* Supports loosely-typed data structures
* Parses the entire URI allowing you to retrieve information about function calls or requests for
  an item by id.

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

## Installing via NuGet

[![NuGet version](https://badge.fury.io/nu/ODataToolkit.svg)](https://www.nuget.org/packages/ODataToolkit)

```
Install-Package ODataToolkit
```

## Support

The library supports the follwoing frameworks

* .NET Framework 3.5+
* (Coming soon) [.Net Standard 2.0](https://docs.microsoft.com/en-us/dotnet/articles/standard/library). 