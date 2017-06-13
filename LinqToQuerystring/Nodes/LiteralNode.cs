namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class LiteralNode : ODataNode
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
