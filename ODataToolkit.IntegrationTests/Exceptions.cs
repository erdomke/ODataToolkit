namespace ODataToolkit.IntegrationTests.Sql
{
  using System;
  using System.Data;
  using System.Linq;

  using ODataToolkit;

  using Machine.Specifications;

  public class When_using_skip_on_unordered_data : SqlPagingAndOrdering
  {
    private static Exception ex;

    private Because of = () => ex = Catch.Exception(() => result = testDb.ConcreteCollection.ExecuteOData("?$skip=1").ToList());

    private It should_throw_an_exception = () => ex.ShouldBeOfType<NotSupportedException>();
  }

  public class When_trying_to_order_by_complex_types : SqlPagingAndOrdering
  {
    private static Exception ex;

    private Because of = () => ex = Catch.Exception(() => complexResult = testDb.ComplexCollection.ExecuteOData("?$orderby=concrete").ToList());

    private It should_throw_an_exception = () => ex.ShouldBeOfType<ArgumentException>();
  }
}
