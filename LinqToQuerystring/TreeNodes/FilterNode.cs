namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class FilterNode : SingleChildNode
  {
    public FilterNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var parameter = options.Item ?? Expression.Parameter(options.InputType, "o");
      var lambda = Expression.Lambda(
          this.ChildNode.BuildLinqExpression(options.Clone().WithItem(parameter))
          , new[] { parameter as ParameterExpression });

      return Expression.Call(typeof(Queryable), "Where", new[] { options.Query.ElementType }, options.Query.Expression, lambda);
    }

    public override int CompareTo(TreeNode other)
    {
      if (other is FilterNode)
      {
        return 0;
      }

      return -1;
    }
  }
}
