using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ODataToolkit.Csdl;
using System.Xml;
using System.Diagnostics;

namespace ODataToolkit
{
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
    
    static Module()
    {
      var model = new EdmModel();

      var product = model.AddEntityType("ODataTest", "Product");
      product.AddKeys(product.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));
      product.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
      product.AddStructuralProperty("Description", EdmPrimitiveTypeKind.String);
      product.AddStructuralProperty("ReleaseDate", EdmPrimitiveTypeKind.DateTimeOffset, false);
      product.AddStructuralProperty("DiscontinuedDate", EdmPrimitiveTypeKind.DateTimeOffset, true);
      product.AddStructuralProperty("Rating", EdmPrimitiveTypeKind.Int16, false);
      product.AddStructuralProperty("Price", EdmPrimitiveTypeKind.Double, false);

      var container = model.AddEntityContainer("ODataTest", "Service");
      container.AddEntitySet("Products", product);

      _model = model;

      _products = new Product[] {
        new Product()
        {
          Id = 0,
          Name = "Bread",
          Description = "Whole grain bread",
          ReleaseDate = DateTime.Parse("1992-01-01T00:00:00Z"),
          Rating = 4,
          Price = 2.5
        },
        new Product()
        {
          Id = 1,
          Name = "Milk",
          Description = "Low fat milk",
          ReleaseDate = DateTime.Parse("1995-10-01T00:00:00Z"),
          Rating = 3,
          Price = 3.5
        },
        new Product()
        {
          Id = 2,
          Name = "Vint soda",
          Description = "Americana Variety - Mix of 6 flavors",
          ReleaseDate = DateTime.Parse("2000-10-01T00:00:00Z"),
          Rating = 3,
          Price = 20.9
        },
        new Product()
        {
          Id = 3,
          Name = "Havina Cola",
          Description = "The Original Key Lime Cola",
          ReleaseDate = DateTime.Parse("2005-10-01T00:00:00Z"),
          DiscontinuedDate = DateTime.Parse("2006-10-01T00:00:00Z"),
          Rating = 3,
          Price = 19.9
        },
        new Product()
        {
          Id = 4,
          Name = "Fruit Punch",
          Description = "Mango flavor, 8.3 Ounce Cans (Pack of 24)",
          ReleaseDate = DateTime.Parse("2003-01-05T00:00:00Z"),
          Rating = 3,
          Price = 22.99
        },
        new Product()
        {
          Id = 5,
          Name = "Cranberry Juice",
          Description = "16-Ounce Plastic Bottles (Pack of 12)",
          ReleaseDate = DateTime.Parse("2006-08-04T00:00:00Z"),
          Rating = 3,
          Price = 22.8
        },
        new Product()
        {
          Id = 6,
          Name = "Pink Lemonade",
          Description = "36 Ounce Cans (Pack of 3)",
          ReleaseDate = DateTime.Parse("2006-11-05T00:00:00Z"),
          Rating = 3,
          Price = 18.8
        },
        new Product()
        {
          Id = 7,
          Name = "DVD Player",
          Description = "1080P Upconversion DVD Player",
          ReleaseDate = DateTime.Parse("2006-11-15T00:00:00Z"),
          Rating = 3,
          Price = 35.88
        },
        new Product()
        {
          Id = 8,
          Name = "LCD HDTV",
          Description = "42 inch 1080p LCD with Built-in Blu-ray Disc Player",
          ReleaseDate = DateTime.Parse("2008-05-08T00:00:00Z"),
          Rating = 3,
          Price = 1088.8
        }
      }.AsQueryable();

    }
  }
}
