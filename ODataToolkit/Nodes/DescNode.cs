namespace ODataToolkit.Nodes
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;

  using ODataToolkit.Nodes.Base;

  public class DescNode : ExplicitOrderByBase
  {
    public DescNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var parameter = options.Item ?? Expression.Parameter(options.InputType, "o");
      Expression childExpression = options.Expression;

      var temp = parameter;

      foreach (var child in this.Children.Cast<ODataNode>())
      {
        childExpression = child.BuildLinqExpression(options.Clone().WithExpression(childExpression).WithItem(temp));
        temp = childExpression;
      }

      Debug.Assert(childExpression != null, "childExpression should never be null");

      var methodName = "OrderByDescending";
      if (!this.IsFirstChild)
        methodName = "ThenByDescending";

      var lambda = Expression.Lambda(childExpression, new[] { parameter as ParameterExpression });
      return Expression.Call(typeof(Queryable), methodName, new[] { options.Query.ElementType, childExpression.Type }, options.Query.Expression, lambda);
    }
  }
}
