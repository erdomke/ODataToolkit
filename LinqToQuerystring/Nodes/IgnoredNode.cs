namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class IgnoredNode : ODataNode
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
