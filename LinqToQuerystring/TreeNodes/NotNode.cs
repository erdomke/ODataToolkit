namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class NotNode : SingleChildNode
  {
    public NotNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var childExpression = this.ChildNode.BuildLinqExpression(options);
      if (!typeof(bool).IsAssignableFrom(childExpression.Type))
      {
        childExpression = Expression.Convert(childExpression, typeof(bool));
      }

      return Expression.Not(childExpression);
    }
  }
}