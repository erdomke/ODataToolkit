using System;

namespace LinqToQuerystring.Nodes.Base
{
  using System.Linq;

  public abstract class QueryModifier : ODataNode
  {
    protected QueryModifier(Token payload) : base(payload) { }

    public override System.Linq.Expressions.Expression BuildLinqExpression(ExpressionOptions options)
    {
      throw new NotSupportedException(
         string.Format("{0} is just a placeholder and should be handled differently in Extensions.cs", this.GetType().Name));
    }

    public abstract IQueryable ModifyQuery(ExpressionOptions options);
  }
}
