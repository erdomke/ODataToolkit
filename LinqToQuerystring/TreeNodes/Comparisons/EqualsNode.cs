namespace LinqToQuerystring.TreeNodes.Comparisons
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.TreeNodes.Base;

  public class EqualsNode : TwoChildNode
  {
    public EqualsNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var leftExpression = this.LeftNode.BuildLinqExpression(options);
      var rightExpression = this.RightNode.BuildLinqExpression(options);

      // Nasty workaround to avoid comparison of Aggregate functions to true or false which breaks Entity framework
      if (leftExpression.Type == typeof(bool) && rightExpression.Type == typeof(bool) && rightExpression is ConstantExpression)
      {
        if ((bool)(rightExpression as ConstantExpression).Value)
        {
          return leftExpression;
        }

        return Expression.Not(leftExpression);
      }

      if (rightExpression.Type == typeof(bool) && leftExpression.Type == typeof(bool)
          && leftExpression is ConstantExpression)
      {
        if ((bool)(leftExpression as ConstantExpression).Value)
        {
          return rightExpression;
        }

        return Expression.Not(rightExpression);
      }

      NormalizeTypes(ref leftExpression, ref rightExpression);

      return ApplyEnsuringNullablesHaveValues(Expression.Equal, leftExpression, rightExpression);
    }
  }
}
