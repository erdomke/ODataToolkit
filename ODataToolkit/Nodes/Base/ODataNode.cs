namespace ODataToolkit.Nodes.Base
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;

  [DebuggerDisplay("{Text} {Type}")]
  public abstract class ODataNode : IComparable<ODataNode>
  {
    private List<ODataNode> _children = new List<ODataNode>();

    protected internal readonly Token _payload;

    internal ODataUri Uri { get; set; }
    public virtual TokenType Type { get { return _payload.Type; } }
    public virtual string Text { get { return _payload.Text; } }

    protected ODataNode(Token payload)
    {
      this._payload = payload;
    }

    /// <summary>
    /// This hacky property overwrites the base property which has a bug when using tree rewrites
    /// </summary>
    public IList<ODataNode> Children
    {
      get { return _children; }
    }

    public abstract Expression BuildLinqExpression(ExpressionOptions options);

    public virtual int CompareTo(ODataNode other)
    {
      return 0;
    }

    internal virtual ODataNode GetValueNode()
    {
      return this;
    }

    protected static void NormalizeTypes(ref Expression leftSide, ref Expression rightSide)
    {
      var rightSideIsConstant = rightSide is ConstantExpression;
      var leftSideIsConstant = leftSide is ConstantExpression;

      if (rightSideIsConstant && leftSideIsConstant)
      {
        return;
      }

      if (rightSideIsConstant)
      {
        // If we are comparing to an object try to cast it to the same type as the constant
        if (leftSide.Type == typeof(object))
        {
          leftSide = MapAndCast(leftSide, rightSide);
        }
        else
        {
          rightSide = MapAndCast(rightSide, leftSide);
        }
      }

      if (leftSideIsConstant)
      {
        // If we are comparing to an object try to cast it to the same type as the constant
        if (rightSide.Type == typeof(object))
        {
          rightSide = MapAndCast(rightSide, leftSide);
        }
        else
        {
          leftSide = MapAndCast(leftSide, rightSide);
        }
      }
    }

    private static Expression MapAndCast(Expression from, Expression to)
    {
      return CastIfNeeded(from, to.Type);
    }

    protected static Expression CastIfNeeded(Expression expression, Type type)
    {
      var converted = expression;
      if (!type.IsAssignableFrom(expression.Type))
        converted = Expression.Convert(expression, type);

      return converted;
    }

    protected static Expression ApplyEnsuringNullablesHaveValues(Func<Expression, Expression, Expression> produces, Expression leftExpression, Expression rightExpression)
    {
      var leftExpressionIsNullable = (Nullable.GetUnderlyingType(leftExpression.Type) != null);
      var rightExpressionIsNullable = (Nullable.GetUnderlyingType(rightExpression.Type) != null);

      if (leftExpressionIsNullable && !rightExpressionIsNullable)
      {
        return Expression.AndAlso(
            Expression.NotEqual(leftExpression, Expression.Constant(null)),
            produces(Expression.Property(leftExpression, "Value"), rightExpression));
      }

      if (rightExpressionIsNullable && !leftExpressionIsNullable)
      {
        return Expression.AndAlso(
            Expression.NotEqual(rightExpression, Expression.Constant(null)),
            produces(leftExpression, Expression.Property(rightExpression, "Value")));
      }

      return produces(leftExpression, rightExpression);
    }

    protected static Expression ApplyWithNullAsValidAlternative(Func<Expression, Expression, Expression> produces, Expression leftExpression, Expression rightExpression)
    {
      var leftExpressionIsNullable = (Nullable.GetUnderlyingType(leftExpression.Type) != null);
      var rightExpressionIsNullable = (Nullable.GetUnderlyingType(rightExpression.Type) != null);

      if (leftExpressionIsNullable && !rightExpressionIsNullable)
      {
        return Expression.OrElse(
            Expression.Equal(leftExpression, Expression.Constant(null)),
            produces(Expression.Property(leftExpression, "Value"), rightExpression));
      }

      if (rightExpressionIsNullable && !leftExpressionIsNullable)
      {
        return Expression.OrElse(
            Expression.Equal(rightExpression, Expression.Constant(null)),
            produces(leftExpression, Expression.Property(rightExpression, "Value")));
      }

      return produces(leftExpression, rightExpression);
    }

    public override string ToString()
    {
      return Text;
    }

    public static BinaryNode And(ODataNode left, ODataNode right)
    {
      var result = new BinaryNode(new Token(TokenType.And, "and"));
      result.Children.Add(left);
      result.Children.Add(right);
      return result;
    }
    public static BinaryNode Equals(ODataNode left, ODataNode right)
    {
      var result = new BinaryNode(new Token(TokenType.Equal, "eq"));
      result.Children.Add(left);
      result.Children.Add(right);
      return result;
    }
    public static FilterNode Filter(ODataNode expr)
    {
      var result = new FilterNode(new Token(TokenType.QueryName, "$filter"));
      result.Children.Add(expr);
      return result;
    }
    public static IdentifierNode Identifier(string ident)
    {
      return new IdentifierNode(new Token(TokenType.Identifier, ident));
    }
    public static LiteralNode Literal(object value)
    {
      return new LiteralNode(Token.FromPrimative(value));
    }
    public static BinaryNode Or(ODataNode left, ODataNode right)
    {
      var result = new BinaryNode(new Token(TokenType.Or, "or"));
      result.Children.Add(left);
      result.Children.Add(right);
      return result;
    }
    public static TopNode Top(ODataNode expr)
    {
      var result = new TopNode(new Token(TokenType.QueryName, "$top"));
      result.Children.Add(expr);
      return result;
    }
  }
}
