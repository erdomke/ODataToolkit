namespace ODataToolkit.Nodes
{
  using Base;
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  public class BinaryNode : ODataNode
  {
    public BinaryNode(Token payload) : base(payload) { }

    public ODataNode LeftNode
    {
      get
      {
        var leftNode = this.Children.ElementAtOrDefault(0);
        if (leftNode == null)
        {
          throw new InvalidOperationException(string.Format("No valid left node for {0}", this.GetType()));
        }

        return leftNode;
      }
    }

    public ODataNode RightNode
    {
      get
      {
        var rightNode = this.Children.ElementAtOrDefault(1);
        if (rightNode == null)
        {
          throw new InvalidOperationException(string.Format("No valid right node for {0}", this.GetType()));
        }

        return rightNode;
      }
    }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var left = this.LeftNode.BuildLinqExpression(options);
      var right = this.RightNode.BuildLinqExpression(options);

      switch (Type)
      {
        case TokenType.And:
          return Expression.AndAlso(left, right);
        case TokenType.Or:
          return Expression.OrElse(left, right);
        case TokenType.Equal:

          // Nasty workaround to avoid comparison of Aggregate functions to true or false which breaks Entity framework
          if (left.Type == typeof(bool) && right.Type == typeof(bool) && right is ConstantExpression)
          {
            if ((bool)(right as ConstantExpression).Value)
            {
              return left;
            }

            return Expression.Not(left);
          }

          if (right.Type == typeof(bool) && left.Type == typeof(bool)
              && left is ConstantExpression)
          {
            if ((bool)(left as ConstantExpression).Value)
            {
              return right;
            }

            return Expression.Not(right);
          }

          NormalizeTypes(ref left, ref right);

          return ApplyEnsuringNullablesHaveValues(Expression.Equal, left, right);
        case TokenType.GreaterThan:

          NormalizeTypes(ref left, ref right);

          return ApplyEnsuringNullablesHaveValues(Expression.GreaterThan, left, right);
        case TokenType.GreaterThanOrEqual:

          NormalizeTypes(ref left, ref right);

          return ApplyEnsuringNullablesHaveValues(Expression.GreaterThanOrEqual, left, right);
        case TokenType.LessThan:

          NormalizeTypes(ref left, ref right);

          return ApplyEnsuringNullablesHaveValues(Expression.LessThan, left, right);
        case TokenType.LessThanOrEqual:

          NormalizeTypes(ref left, ref right);

          return ApplyEnsuringNullablesHaveValues(Expression.LessThanOrEqual, left, right);
        case TokenType.NotEqual:

          NormalizeTypes(ref left, ref right);

          return ApplyWithNullAsValidAlternative(Expression.NotEqual, left, right);
        case TokenType.Add:
          return Expression.Add(left, right);
        case TokenType.Subtract:
          return Expression.Subtract(left, right);
        case TokenType.Multiply:
          return Expression.Multiply(left, right);
        case TokenType.Divide:
          return Expression.Divide(left, right);
        case TokenType.Modulo:
          return Expression.Modulo(left, right);
      }

      throw new NotSupportedException();
    }
  }
}
