namespace ODataToolkit.Nodes.Base
{
  using System;
  using System.Linq;

  public abstract class UnaryNode : ODataNode
  {
    protected UnaryNode(Token payload) : base(payload) { }

    public ODataNode ChildNode
    {
      get
      {
        var childNode = this.Children.FirstOrDefault();
        if (childNode == null)
        {
          throw new InvalidOperationException(string.Format("No valid child for {0}", this.GetType()));
        }

        return childNode;
      }
    }
  }
}
