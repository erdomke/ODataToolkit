namespace LinqToQuerystring.TreeNodes.Aggregates
{
  using System;
  using System.Collections;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.TreeNodes.Base;

  public class MaxNode : TreeNode
  {
    public MaxNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      var property = this.Children.ElementAt(0).BuildLinqExpression(query, inputType, expression, item);

      var underlyingType = property.Type;
      if (typeof(IEnumerable).IsAssignableFrom(property.Type) && property.Type.GetGenericArguments().Any())
      {
        underlyingType = property.Type.GetGenericArguments()[0];
      }

      return Expression.Call(typeof(Enumerable), "Max", new[] { underlyingType }, property);
    }
  }
}
