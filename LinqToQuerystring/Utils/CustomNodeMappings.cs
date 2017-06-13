namespace LinqToQuerystring.Utils
{
  using System;
  using System.Collections.Generic;
  using System.Linq.Expressions;
  
  using LinqToQuerystring.Nodes;
  using LinqToQuerystring.Nodes.Base;

  public class CustomNodeMappings : Dictionary<TokenType, Func<Token, ODataNode>>
  {
    public ODataNode MapNode(ODataNode node, Expression expression)
    {
      if (this.ContainsKey(node.Type))
      {
        var mappedNode = this[node.Type](node.payload);
        foreach (var child in node.Children)
        {
          mappedNode.Children.Add(child);
        }
        return mappedNode;
      }

      return node;
    }
  }
}
