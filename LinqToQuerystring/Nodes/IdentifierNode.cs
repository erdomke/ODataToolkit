namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class IdentifierNode : ODataNode
  {
    public IdentifierNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      if (base.Type == TokenType.Identifier)
      {
        var param = options.Item as ParameterExpression;
        if (param != null && param.Name == this.Text)
          return options.Item;
        if (options.DynamicAccessor != null)
          return Expression.Invoke(options.DynamicAccessor, new[] { options.Item, Expression.Constant(this.Text) });
        return Expression.Property(options.Item, this.Text);
      }
      else
      {
        var parent = options.Item;
        foreach (var child in this.Children)
        {
          parent = child.BuildLinqExpression(options.WithItem(parent));
        }
        return parent;
      }
    }
  }
}
