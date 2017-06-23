namespace ODataToolkit.Nodes
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;

  using ODataToolkit.Nodes.Base;

  public class SelectNode : UnaryNode
  {
    public SelectNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var fixedexpr = Expression.Call(typeof(Queryable), "Cast"
        , new[] { options.InputType }, options.Query.Expression);

      options.Query = options.Query.Provider.CreateQuery(fixedexpr);

      var parameter = options.Item ?? Expression.Parameter(options.InputType, "o");

      var opts = options.Clone().WithExpression(fixedexpr).WithItem(parameter);
      var children = this.Children
        .Select(o => new { Text = o.Text, Expr = o.BuildLinqExpression(opts) })
        .ToArray();
      if (children.Length > 50)
        throw new NotSupportedException("Select expressions with over 50 parameters are currently unsupported");

      // Create the result type
      var types = new Type[50];
      for (var i = 0; i < children.Length; i++)
      {
        types[i] = children[i].Expr.Type;
      }
      for (var i = children.Length; i < types.Length; i++)
      {
        types[i] = typeof(object);
      }
      var resultType = typeof(ProjectedRecord<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);

      // Create the initialization expression
      var bindings = children
        .Select((c, i) => Expression.Bind(resultType.GetProperty("Value" + i), c.Expr))
        .ToArray();
      var body = Expression.MemberInit(Expression.New(resultType), bindings);
      var lambda = Expression.Lambda(body, new[] { parameter as ParameterExpression });
      return Expression.Call(typeof(Queryable), "Select"
        , new[] { options.Query.ElementType, resultType }
        , options.Query.Expression, lambda);
    }

    public override int CompareTo(ODataNode other)
    {
      if (other is SelectNode)
      {
        return 0;
      }

      return 1;
    }
  }
}
