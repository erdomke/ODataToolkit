namespace ODataToolkit.UnitTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using ODataToolkit;

  using Machine.Specifications;

  public abstract class Dynamics
  {
    protected static List<Dictionary<string, object>> collection;

    protected static IQueryable<Dictionary<string, object>> result;

    private Establish context = () =>
        {
          var item1 = new Dictionary<string, object>();
          item1["Age"] = 23;
          item1["Name"] = "Karl";
          item1["Score"] = 0.1m;

          var item2 = new Dictionary<string, object>();
          item2["Age"] = 25;
          item2["Name"] = "Kathryn";
          item2["Score"] = 0.2m;

          var item3 = new Dictionary<string, object>();
          item3["Age"] = 28;
          item3["Name"] = "Pete";
          item3["Score"] = 0.3m;

          var item4 = new Dictionary<string, object>();
          item4["Age"] = 17;
          item4["Name"] = "Dominic";
          item4["Score"] = 0.4m;

          collection = new List<Dictionary<string, object>> { item1, item2, item3, item4 };
        };
  }
}
