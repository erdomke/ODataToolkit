namespace LinqToQuerystring.TreeNodes.Base
{
  using System;

  public abstract class ExplicitOrderByBase : TreeNode
  {
    protected ExplicitOrderByBase(Token payload) : base(payload)
    {
      this.IsFirstChild = false;
    }

    public bool IsFirstChild { get; set; }
  }
}
