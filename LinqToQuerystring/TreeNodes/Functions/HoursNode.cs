namespace LinqToQuerystring.TreeNodes.Functions
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.Exceptions;
  using LinqToQuerystring.TreeNodes.Base;

  public class HoursNode : SingleChildNode
  {
    public HoursNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      var childexpression = this.ChildNode.BuildLinqExpression(query, inputType, expression, item);

      if (!typeof(DateTime).IsAssignableFrom(childexpression.Type))
      {
        throw new FunctionNotSupportedException(childexpression.Type, "hours");
      }

      return Expression.Property(childexpression, "Hour");
    }
  }
}