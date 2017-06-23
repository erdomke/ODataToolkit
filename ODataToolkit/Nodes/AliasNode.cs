namespace ODataToolkit.Nodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using ODataToolkit.Nodes.Base;

  public class AliasNode : ODataNode
  {
    public AliasNode(Token payload) : base(payload) { }

    internal override ODataNode GetValueNode()
    {
      var defn = Uri.QueryOption[this.Text];
      if (defn == null)
        return this;
      var child = defn.Children.FirstOrDefault();
      if (child == null)
        return this;
      return child;
    }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      var child = GetValueNode();
      if (child != null)
      {
        return child.BuildLinqExpression(options);
      }

      return options.Item;
    }
  }
}
