namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class SkipNode : UnaryNode
  {
    public SkipNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return Expression.Call(typeof(Queryable), "Skip", new[] { options.Query.ElementType }, options.Query.Expression
        , this.ChildNode.BuildLinqExpression(options));
    }

    public override int CompareTo(ODataNode other)
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
