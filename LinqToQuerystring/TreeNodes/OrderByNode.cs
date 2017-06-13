namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class OrderByNode : QueryModifier
  {
    public OrderByNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      throw new NotSupportedException(
          "Orderby is just a placeholder and should be handled differently in Extensions.cs");
    }

    public override IQueryable ModifyQuery(ExpressionOptions options)
    {
      var queryresult = options.Query;
      var orderbyChildren = this.Children.Cast<ExplicitOrderByBase>();

      //if (!queryresult.Provider.GetType().Name.Contains("DbQueryProvider") && !queryresult.Provider.GetType().Name.Contains("MongoQueryProvider"))
      //{
      //  orderbyChildren = orderbyChildren.Reverse();
      //}

      var explicitOrderByNodes = orderbyChildren as IList<ExplicitOrderByBase> ?? orderbyChildren.ToList();
      explicitOrderByNodes.First().IsFirstChild = true;

      foreach (var child in explicitOrderByNodes)
      {
        var opt = options.Clone().WithQuery(queryresult).WithExpression(queryresult.Expression);
        queryresult = queryresult.Provider.CreateQuery(child.BuildLinqExpression(opt));
      }

      return queryresult;
    }

    public override int CompareTo(TreeNode other)
    {
      if (other is OrderByNode)
      {
        return 0;
      }

      if (other is FilterNode)
      {
        return 1;
      }

      return -1;
    }
  }
}