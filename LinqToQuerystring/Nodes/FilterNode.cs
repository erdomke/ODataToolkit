namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class FilterNode : UnaryNode
  {
    public FilterNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var parameter = options.Item ?? Expression.Parameter(options.InputType, "o");
      var body = this.ChildNode.BuildLinqExpression(options.Clone().WithItem(parameter));
      if (!typeof(bool).IsAssignableFrom(body.Type))
        body = Expression.Coalesce(body, Expression.Constant(false));

      var lambda = Expression.Lambda(body, new[] { parameter as ParameterExpression });
      return Expression.Call(typeof(Queryable), "Where"
        , new[] { options.Query.ElementType }, options.Query.Expression, lambda);
    }

    public override int CompareTo(ODataNode other)
    {
      if (other is FilterNode)
      {
        return 0;
      }

      return -1;
    }
  }
}
