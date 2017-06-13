namespace LinqToQuerystring.TreeNodes.DataTypes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.TreeNodes.Base;

  public class LongNode : TreeNode
  {
    public LongNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      return Expression.Constant(Convert.ToInt64(this.Text.Replace("L", string.Empty)));
    }
  }
}