using LinqToQuerystring.Nodes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using LinqToQuerystring.Nodes;

namespace LinqToQuerystring
{
  public class ODataQuery : IList<ODataNode>
  {
    private List<ODataNode> _nodes = new List<ODataNode>();

    public int? Skip
    {
      get
      {
        return (int?)GetPrimative("$skip");
      }
      set
      {
        SetPrimative("$skip", value);
      }
    }
    public int? Top
    {
      get
      {
        return (int?)GetPrimative("$top");
      }
      set
      {
        SetPrimative("$top", value);
      }
    }

    private object GetPrimative(string key)
    {
      var node = this[key];
      if (node == null)
        return null;

      var literal = (LiteralNode)node.Children.Single();
      return literal.AsPrimitive();
    }
    private void SetPrimative(string key, object value)
    {
      if (value == null)
      {
        Remove(key);
        return;
      }

      var node = this[key];
      if (node == null)
      {
        node = new TopNode(new Token(TokenType.QueryName, key));
        this.Add(node);
      }

      node.Children.Clear();
      node.Children.Add(new LiteralNode(new Token(TokenType.Integer, value.ToString())));
    }

    public ODataNode this[int index]
    {
      get { return _nodes[index]; }
      set { _nodes[index] = value; }
    }

    public ODataNode this[string name]
    {
      get
      {
        return _nodes
          .LastOrDefault(n => string.Equals(n.Text, name, StringComparison.OrdinalIgnoreCase));
      }
    }

    public int Count { get { return _nodes.Count; } }

    public bool IsReadOnly { get { return false; } }

    public void Add(ODataNode item)
    {
      _nodes.Add(item);
    }

    public void Clear()
    {
      _nodes.Clear();
    }

    public bool Contains(ODataNode item)
    {
      return _nodes.Contains(item);
    }

    public void CopyTo(ODataNode[] array, int arrayIndex)
    {
      _nodes.CopyTo(array, arrayIndex);
    }

    public IEnumerator<ODataNode> GetEnumerator()
    {
      return _nodes.GetEnumerator();
    }

    public int IndexOf(ODataNode item)
    {
      return _nodes.IndexOf(item);
    }

    public void Insert(int index, ODataNode item)
    {
      _nodes.Insert(index, item);
    }

    public bool Remove(string key)
    {
      var origCount = _nodes.Count;
      var i = 0;
      while (i < _nodes.Count)
      {
        if (_nodes[i].Text == key)
          _nodes.RemoveAt(i);
        else
          i++;
      }
      return i < origCount;
    }

    public bool Remove(ODataNode item)
    {
      return _nodes.Remove(item);
    }

    public void RemoveAt(int index)
    {
      _nodes.RemoveAt(index);
    }

    public void Sort()
    {
      _nodes.Sort();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
