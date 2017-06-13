namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class TopNode : UnaryNode
  {
    public TopNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return Expression.Call(
          typeof(Queryable),
          "Take",
          new[] { options.Query.ElementType },
          options.Query.Expression,
          this.ChildNode.BuildLinqExpression(options));
    }

    public override int CompareTo(ODataNode other)
    {
      if (other is TopNode)
      {
        return 0;
      }

      if (other is OrderByNode || other is FilterNode || other is SkipNode)
      {
        return 1;
      }

      return -1;
    }
  }
}
