namespace LinqToQuerystring.TreeNodes.Aggregates
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.TreeNodes.Base;

  public class AverageNode : TreeNode
  {
    public AverageNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      var property = this.Children.ElementAt(0).BuildLinqExpression(query, inputType, expression, item);
      return Expression.Call(typeof(Enumerable), "Average", null, property);
    }
  }
}
