namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class OrNode : TwoChildNode
  {
    public OrNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return Expression.OrElse(
          this.LeftNode.BuildLinqExpression(options),
          this.RightNode.BuildLinqExpression(options));
    }
  }
}
