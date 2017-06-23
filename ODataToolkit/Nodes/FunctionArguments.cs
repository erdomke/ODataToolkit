using ODataToolkit.Nodes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ODataToolkit.Nodes
{
  public class FunctionArguments : IEnumerable<ODataNode>
  {
    private ODataNode _parent;

    public int Count { get { return Math.Max(0, _parent.Children.Count - 1);  } }

    public ODataNode this[int i]
    {
      get { return GetValue(_parent.Children[i + 1]); }
    }
    public ODataNode this[string name]
    {
      get
      {
        var node = _parent.Children.Skip(1)
          .FirstOrDefault(n => n.Text == name);
        if (node == null)
          return null;
        var child = node.Children[0];
        if (child.Type == TokenType.Parameter)
        {
          var defn = _parent.Uri.QueryOption[child.Text];
          if (defn != null)
            return defn.Children[0];
        }
        return child;
      }
    }

    internal FunctionArguments(ODataNode parent)
    {
      _parent = parent;
    }

    private ODataNode GetValue(ODataNode arg)
    {
      if (arg.Type == TokenType.QueryAssign)
        return arg.Children[1];
      return arg;
    }

    public IEnumerable<KeyValuePair<string, ODataNode>> Named()
    {
      foreach (var child in _parent.Children.Skip(1)
          .Where(n => n.Type == TokenType.QueryAssign
            && n.Children.Count == 2))
      {
        yield return new KeyValuePair<string, ODataNode>(
          child.Children[0].Text,
          child.Children[1]
        );
      }
    }

    public IEnumerator<ODataNode> GetEnumerator()
    {
      foreach (var child in _parent.Children.Skip(1))
      {
        yield return GetValue(child);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
