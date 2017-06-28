using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ODataToolkit.Nodes.Base;

namespace ODataToolkit.Nodes
{
  public class PlaceholderNode : ODataNode
  {
    private PlaceholderNode() : base(new Token(TokenType.Whitespace, "")) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      return options.Expression;
    }

    private static PlaceholderNode _instance = new PlaceholderNode();
    public static PlaceholderNode Instance { get { return _instance; } }
  }
}
