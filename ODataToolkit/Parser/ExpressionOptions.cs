using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ODataToolkit
{
  public class ExpressionOptions
  {
    public Expression<Func<object, string, object>> DynamicAccessor { get; set; }
    public Expression Expression { get; set; }
    public Type InputType { get; set; }
    public Expression Item { get; set; }
    public IQueryable Query { get; set; }

    public ExpressionOptions() { }
    public ExpressionOptions(ExpressionOptions clone)
    {
      this.Query = clone.Query;
      this.InputType = clone.InputType;
      this.Expression = clone.Expression;
      this.Item = clone.Item;
      this.DynamicAccessor = clone.DynamicAccessor;
    }

    public ExpressionOptions WithQuery(IQueryable query)
    {
      this.Query = query;
      return this;
    }
    public ExpressionOptions WithExpression(Expression expression)
    {
      this.Expression = expression;
      return this;
    }
    public ExpressionOptions WithItem(Expression item)
    {
      this.Item = item;
      return this;
    }

    public ExpressionOptions Clone()
    {
      return new ExpressionOptions(this);
    }
  }
}
