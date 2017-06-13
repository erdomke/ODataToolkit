namespace LinqToQuerystring.TreeNodes.DataTypes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.TreeNodes.Base;

  public class BoolNode : TreeNode
  {
    public BoolNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      return Expression.Constant(Convert.ToBoolean(this.Text));
    }
  }
}