namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class AscNode : ExplicitOrderByBase
  {
    public AscNode(Token payload) : base(payload) { }
    public AscNode(ODataNode node) : base(new Token(TokenType.Identifier, "asc"))
    {
      Children.Add(node);
    }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var parameter = options.Item ?? Expression.Parameter(options.InputType, "o");
      Expression childExpression = options.Expression;

      var temp = parameter;
      foreach (var child in this.Children)
      {
        childExpression = child.BuildLinqExpression(options.Clone().WithExpression(childExpression).WithItem(temp));
        temp = childExpression;
      }

      Debug.Assert(childExpression != null, "childExpression should never be null");

      var methodName = "OrderBy";
      //(query.Provider.GetType().Name.Contains("DbQueryProvider") || query.Provider.GetType().Name.Contains("MongoQueryProvider")) &&
      if (!this.IsFirstChild)
      {
        methodName = "ThenBy";
      }

      var lambda = Expression.Lambda(childExpression, new[] { parameter as ParameterExpression });
      return Expression.Call(typeof(Queryable), methodName, new[] { options.Query.ElementType, childExpression.Type }, options.Query.Expression, lambda);
    }
  }
}
