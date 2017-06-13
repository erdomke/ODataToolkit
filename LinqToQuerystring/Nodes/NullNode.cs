namespace LinqToQuerystring.Nodes.DataTypes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.Nodes.Base;

  public class NullNode : ODataNode
  {
    public NullNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return Expression.Constant(null);
    }
  }
}