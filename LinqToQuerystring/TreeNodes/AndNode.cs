namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class AndNode : TwoChildNode
  {
    public AndNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return Expression.AndAlso(
          this.LeftNode.BuildLinqExpression(options),
          this.RightNode.BuildLinqExpression(options));
    }
  }
}
