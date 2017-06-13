namespace LinqToQuerystring.TreeNodes.DataTypes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.TreeNodes.Base;

  public class NullNode : TreeNode
  {
    public NullNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return Expression.Constant(null);
    }
  }
}