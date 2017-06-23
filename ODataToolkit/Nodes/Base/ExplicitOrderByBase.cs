namespace ODataToolkit.Nodes.Base
{
  using System;

  public abstract class ExplicitOrderByBase : ODataNode
  {
    protected ExplicitOrderByBase(Token payload) : base(payload)
    {
      this.IsFirstChild = false;
    }

    public bool IsFirstChild { get; set; }
  }
}
