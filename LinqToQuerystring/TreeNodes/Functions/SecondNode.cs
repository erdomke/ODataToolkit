namespace LinqToQuerystring.TreeNodes.Functions
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Exceptions;
  using LinqToQuerystring.TreeNodes.Base;

  public class SecondNode : SingleChildNode
  {
    public SecondNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(IQueryable query, Type inputType, Expression expression, Expression item = null)
    {
      var childexpression = this.ChildNode.BuildLinqExpression(query, inputType, expression, item);

      if (!typeof(DateTime).IsAssignableFrom(childexpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(childexpression.Type))
      {
        throw new FunctionNotSupportedException(childexpression.Type, "second");
      }

      return Expression.Property(childexpression, "Second");
    }
  }
}