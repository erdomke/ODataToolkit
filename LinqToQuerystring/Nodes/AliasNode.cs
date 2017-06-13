namespace LinqToQuerystring.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Nodes.Base;

  public class AliasNode : ODataNode
  {
    public AliasNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var child = this.Children.FirstOrDefault();
      if (child != null)
      {
        return child.BuildLinqExpression(options);
      }

      return options.Item;
    }
  }
}
