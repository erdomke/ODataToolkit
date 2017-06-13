namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class SkipNode : SingleChildNode
  {
    public SkipNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return Expression.Call(typeof(Queryable), "Skip", new[] { options.Query.ElementType }, options.Query.Expression
        , this.ChildNode.BuildLinqExpression(options));
    }

    public override int CompareTo(TreeNode other)
    {
      if (other is SkipNode)
      {
        return 0;
      }

      if (other is OrderByNode || other is FilterNode)
      {
        return 1;
      }

      return -1;
    }
  }
}
