namespace LinqToQuerystring.TreeNodes.DataTypes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.TreeNodes.Base;

  public class SingleNode : TreeNode
  {
    public SingleNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      return Expression.Constant(Convert.ToSingle(this.Text.Replace("f", string.Empty)));
    }
  }
}