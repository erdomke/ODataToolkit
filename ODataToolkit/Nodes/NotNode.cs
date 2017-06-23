namespace ODataToolkit.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using ODataToolkit.Nodes.Base;

  public class NotNode : UnaryNode
  {
    public NotNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var childExpression = this.ChildNode.BuildLinqExpression(options);
      if (typeof(bool?).IsAssignableFrom(childExpression.Type))
      {
        childExpression = ApplyEnsuringNullablesHaveValues(Expression.Equal
          , childExpression
          , Expression.Constant(true));
      }
      else if (!typeof(bool).IsAssignableFrom(childExpression.Type))
      {
        childExpression = Expression.Convert(childExpression, typeof(bool));
      }

      return Expression.Not(childExpression);
    }
  }
}