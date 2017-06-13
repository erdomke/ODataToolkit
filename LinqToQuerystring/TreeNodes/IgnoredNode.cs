namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class IgnoredNode : TreeNode
  {
    public IgnoredNode(Token payload) : base(payload)
    {
    }
    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return options.Expression;
    }
  }
}
