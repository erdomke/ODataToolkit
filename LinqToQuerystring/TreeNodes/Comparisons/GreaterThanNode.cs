namespace LinqToQuerystring.TreeNodes.Comparisons
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class GreaterThanNode : TwoChildNode
  {
    public GreaterThanNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var leftExpression = this.LeftNode.BuildLinqExpression(options);
      var rightExpression = this.RightNode.BuildLinqExpression(options);

      NormalizeTypes(ref leftExpression, ref rightExpression);

      return ApplyEnsuringNullablesHaveValues(Expression.GreaterThan, leftExpression, rightExpression);
    }
  }
}
