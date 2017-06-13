namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class LiteralNode : TreeNode
  {
    public LiteralNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return Expression.Constant(payload.AsPrimitive());
    }

    public object AsPrimitive()
    {
      return payload.AsPrimitive();
    }
  }

}
