namespace LinqToQuerystring.Nodes.Aggregates
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class AnyNode : ODataNode
  {
    public AnyNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var property = options.Item;
      var alias = this.Children[1].Text;
      var filter = this.Children[2];

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

      var parameter = Expression.Parameter(underlyingType, alias);

      var lambda = Expression.Lambda(
          filter.BuildLinqExpression(options.Clone().WithItem(parameter)), new[] { parameter });

      return Expression.Call(typeof(Enumerable), "Any", new[] { underlyingType }, property, lambda);
    }
  }
}
