namespace LinqToQuerystring.TreeNodes.DataTypes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class GuidNode : TreeNode
  {
    public GuidNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      var guidText = this.Text.Replace("guid'", string.Empty).Replace("'", string.Empty);
      return Expression.Constant(new Guid(guidText));
    }
  }
}