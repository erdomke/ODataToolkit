namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;

  using LinqToQuerystring.Nodes.Base;

  public class SelectNode : UnaryNode
  {
    public SelectNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var fixedexpr = Expression.Call(typeof(Queryable), "Cast", new[] { options.InputType }, options.Query.Expression);

      options.Query = options.Query.Provider.CreateQuery(fixedexpr);

      var parameter = options.Item ?? Expression.Parameter(options.InputType, "o");
      Expression childExpression = fixedexpr;

      MethodInfo addMethod = typeof(Dictionary<string, object>).GetMethod("Add");
      var elements = this.Children.Select(
          o => Expression.ElementInit(addMethod, Expression.Constant(o.Text)
          , Expression.Convert(o.BuildLinqExpression(options.Clone().WithExpression(childExpression).WithItem(parameter)), typeof(object))
          ));

      var newDictionary = Expression.New(typeof(Dictionary<string, object>));
      var init = Expression.ListInit(newDictionary, elements);

      var lambda = Expression.Lambda(init, new[] { parameter as ParameterExpression });
      return Expression.Call(typeof(Queryable), "Select", new[] { options.Query.ElementType, typeof(Dictionary<string, object>) }, options.Query.Expression, lambda);
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
