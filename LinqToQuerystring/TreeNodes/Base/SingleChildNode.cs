namespace LinqToQuerystring.TreeNodes.Base
{
  using System;
  using System.Linq;

  public abstract class SingleChildNode : TreeNode
  {
    protected SingleChildNode(Token payload) : base(payload) { }

    public TreeNode ChildNode
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