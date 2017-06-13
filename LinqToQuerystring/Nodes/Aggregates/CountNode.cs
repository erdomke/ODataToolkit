namespace LinqToQuerystring.Nodes.Aggregates
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class CountNode : ODataNode
  {
    public CountNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var property = this.Children.ElementAt(0).BuildLinqExpression(options);

      var underlyingType = property.Type;
      if (typeof(IEnumerable).IsAssignableFrom(property.Type) && property.Type.GetGenericArguments().Any())
      {
        underlyingType = property.Type.GetGenericArguments()[0];
      }
      else
      {
        //We will sometimes need to cater for special cases here, such as Enumerating BsonValues
        underlyingType = Configuration.EnumerableTypeMap(underlyingType);
        var enumerable = typeof(IEnumerable<>).MakeGenericType(underlyingType);
        property = Expression.Convert(property, enumerable);
      }

      return Expression.Call(typeof(Enumerable), "Count", new[] { underlyingType }, property);
    }
  }
}
