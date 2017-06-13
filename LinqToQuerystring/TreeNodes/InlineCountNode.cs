namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class InlineCountNode : SingleChildNode
  {
    public InlineCountNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      throw new NotSupportedException(
          "InlineCountNode is just a placeholder and should be handled differently in Extensions.cs");
    }

    public override int CompareTo(TreeNode other)
    {
      if (other is InlineCountNode)
      {
        return 0;
      }

      return 1;
    }
  }
}
