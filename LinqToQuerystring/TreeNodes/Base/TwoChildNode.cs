namespace LinqToQuerystring.TreeNodes.Base
{
  using System;
  using System.Linq;

  public abstract class TwoChildNode : TreeNode
  {
    protected TwoChildNode(Token payload) : base(payload) { }

    public TreeNode LeftNode
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

    public TreeNode RightNode
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
  }
}