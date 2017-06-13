namespace LinqToQuerystring.TreeNodes.DataTypes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using Antlr.Runtime;

  using LinqToQuerystring.TreeNodes.Base;

  public class DecimalNode : TreeNode
  {
    public DecimalNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      return Expression.Constant(Convert.ToDecimal(this.Text.Replace("m", string.Empty)));
    }
  }
}