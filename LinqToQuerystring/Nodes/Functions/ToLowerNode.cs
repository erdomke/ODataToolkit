namespace LinqToQuerystring.TreeNodes.Functions
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.TreeNodes.Base;

  public class ToLowerNode : SingleChildNode
  {
    public ToLowerNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      var childexpression = this.ChildNode.BuildLinqExpression(query, inputType, expression, item);

      if (!typeof(string).IsAssignableFrom(childexpression.Type))
      {
        childexpression = Expression.Convert(childexpression, typeof(string));
      }

      return Expression.Call(childexpression, "ToLower", null, null);
    }
  }
}