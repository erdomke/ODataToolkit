using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqToQuerystring.Nodes;
using LinqToQuerystring.Nodes.Base;
using LinqToQuerystring.Nodes.DataTypes;
using LinqToQuerystring.Nodes.Aggregates;

namespace LinqToQuerystring
{
  public class Parser
  {
    private Stack<ODataNode> _output = new Stack<ODataNode>();
    private Stack<ODataNode> _operators = new Stack<ODataNode>();
    private IEnumerable<Token> _tokens;
    private StringBuilder _original = new StringBuilder();
    private ODataUri _uri;

    public ODataUri Uri { get { return _uri; } }

    internal Parser(IEnumerable<Token> tokens)
    {
      _tokens = tokens;
    }

    public void Process()
    {
      var lastType = TokenType.Scheme;
      var queryNode = default(ODataNode);

      foreach (var token in _tokens)
      {
        _original.Append(token.Text);
        var node = FromToken(token, queryNode);
        if (token.Type == TokenType.OpenParen && lastType == TokenType.Identifier)
        {
          var ident = _output.Pop();
          switch (ident.Text)
          {
            case "all":
              node = new AllNode(token);
              break;
            case "any":
              node = new AnyNode(token);
              break;
            default:
              node = new CallNode(token);
              break;
          }
          node.Children.Add(ident);
        }
        else if (token.Type == TokenType.QueryName)
        {
          queryNode = node;
        }

        if (GetPrecedence(node) > 0)
        {
          while (_operators.Count > 0 && GetPrecedence(node) < GetPrecedence(_operators.Peek()))
            PopOperator();
          _operators.Push(node);
        }
        else
        {
          switch (node.Type)
          {
            case TokenType.Call:
            case TokenType.OpenParen:
              _operators.Push(node);
              break;
            case TokenType.CloseParen:
              while (_operators.Count > 0
                && _operators.Peek().Type != TokenType.OpenParen
                && _operators.Peek().Type != TokenType.Call)
                PopOperator();
              if (_operators.Count > 0
                && (_operators.Peek().Type == TokenType.OpenParen
                  || _operators.Peek().Type == TokenType.Call))
              {
                var op = _operators.Pop();
                if (op is CallNode || op is AnyNode || op is AllNode)
                {
                  var child = _output.Pop();
                  if (child.Type == TokenType.Comma || child.Type == TokenType.Colon)
                  {
                    foreach (var c in child.Children)
                    {
                      op.Children.Add(c);
                    }
                  }
                  else
                  {
                    op.Children.Add(child);
                  }
                  _output.Push(op);
                }
              }
              break;
            case TokenType.Whitespace:
              // Skip
              break;
            default:
              _output.Push(node);
              break;
          }
        }
        lastType = token.Type;
      }

      while (_operators.Count > 0)
        PopOperator();

      _uri = new ODataUri(_original.ToString());
      Flatten();
    }

    public void Flatten()
    {
      // Flatten query terms
      var query = _output.Peek();
      if (query.Type == TokenType.Question)
      {
        query = query.Children.Last();
      }
      CollectQueryTerms(query);

      // Flatten path segments
      var path = _output.Peek();
      if (path.Type == TokenType.Question)
      {
        path = path.Children.First();
      }
      CollectSegments(path);

      // Restructure the OrderBy clause
      var orderBy = _uri.QueryOption["$orderBy"];
      if (orderBy != null && orderBy.Children.Count == 1)
      {
        if (orderBy.Children[0].Type == TokenType.Comma)
        {
          var comma = orderBy.Children[0];
          orderBy.Children.Clear();
          foreach (var child in comma.Children)
          {
            if (child is IdentifierNode)
              orderBy.Children.Add(new AscNode(child));
            else
              orderBy.Children.Add(child);
          }
        }
        else if (orderBy.Children[0] is IdentifierNode)
        {
          var ident = orderBy.Children[0];
          orderBy.Children.Clear();
          orderBy.Children.Add(new AscNode(ident));
        }
      }

      foreach (var node in _uri.QueryOption)
      {
        if (node.Children.Count == 1 && node.Children[0].Type == TokenType.Comma)
        {
          var comma = node.Children[0];
          node.Children.Clear();
          foreach (var child in comma.Children)
          {
            node.Children.Add(child);
          }
        }
      }
    }

    private void CollectQueryTerms(ODataNode node)
    {
      if (node.Type == TokenType.QueryName)
      {
        _uri.QueryOption.Add(node);
      }
      else
      {
        foreach (var child in node.Children)
        {
          CollectQueryTerms(child);
        }
      }
    }

    private void CollectSegments(ODataNode node)
    {
      if (node.Type == TokenType.PathSeparator)
      {
        foreach (var child in node.Children)
        {
          switch (child.Type)
          {
            case TokenType.Scheme:
            case TokenType.Authority:
            case TokenType.Port:
            case TokenType.Colon:
              break;
            case TokenType.PathSeparator:
              CollectSegments(child);
              break;
            default:
              _uri.PathSegments.Add(child);
              break;
          }
        }
      }
    }

    private void PopOperator()
    {
      var op = _operators.Pop();
      if (op is BinaryNode || op.Type == TokenType.Amperstand
        || op.Type == TokenType.Navigation || op.Type == TokenType.Colon
        || op.Type == TokenType.PathSeparator)
      {
        var right = _output.Pop();
        var left = _output.Pop();

        if (right is CountNode)
        {
          right.Children.Insert(0, left);
          _output.Push(right);
        }
        else
        {
          op.Children.Add(left);
          op.Children.Add(right);
          _output.Push(op);
        }
      }
      else if (op is UnaryNode || op is AscNode || op is DescNode)
      {
        op.Children.Add(_output.Pop());
        _output.Push(op);
      }
      else if (op.Type == TokenType.Question)
      {
        op.Children.Add(_output.Pop());
        if (_output.Count > 0)
          op.Children.Insert(0, _output.Pop());
        _output.Push(op);
      }
      else if (op.Type == TokenType.QueryAssign)
      {
        var right = _output.Pop();
        var left = _output.Pop();

        if (left.Type == TokenType.QueryName)
        {
          left.Children.Add(right);
          _output.Push(left);
        }
        else
        {
          op.Children.Add(left);
          op.Children.Add(right);
          _output.Push(op);
        }
      }
      else if (op.Type == TokenType.Comma)
      {
        var right = _output.Pop();
        var left = _output.Pop();
        if (left.Type == TokenType.Comma)
        {
          left.Children.Add(right);
          _output.Push(left);
        }
        else if (right.Type == TokenType.Comma)
        {
          right.Children.Insert(0, left);
          _output.Push(right);
        }
        else
        {
          op.Children.Add(left);
          op.Children.Add(right);
          _output.Push(op);
        }
      }
      else
      {
        _output.Push(op);
      }
    }

    private ODataNode FromToken(Token token, ODataNode queryNode)
    {
      switch (token.Type)
      {
        case TokenType.Parameter:
          return new AliasNode(token);
        case TokenType.Base64:
        case TokenType.Binary:
        case TokenType.Date:
        case TokenType.Decimal:
        case TokenType.Double:
        case TokenType.Duration:
        case TokenType.False:
        case TokenType.Guid:
        case TokenType.Integer:
        case TokenType.Long:
        case TokenType.NaN:
        case TokenType.NegInfinity:
        case TokenType.PosInfinity:
        case TokenType.Single:
        case TokenType.String:
        case TokenType.True:
          return new LiteralNode(token);
        case TokenType.Null:
          return new NullNode(token);
        case TokenType.Identifier:
        case TokenType.Navigation:
          if (queryNode is OrderByNode && token.Text == "asc")
            return new AscNode(token);
          if (queryNode is OrderByNode && token.Text == "desc")
            return new DescNode(token);
          if (token.Text == "$count")
            return new CountNode(token);
          return new IdentifierNode(token);
        case TokenType.And:
        case TokenType.Or:
        case TokenType.Equal:
        case TokenType.NotEqual:
        case TokenType.LessThan:
        case TokenType.LessThanOrEqual:
        case TokenType.GreaterThan:
        case TokenType.GreaterThanOrEqual:
        case TokenType.Add:
        case TokenType.Subtract:
        case TokenType.Multiply:
        case TokenType.Divide:
        case TokenType.Modulo:
          return new BinaryNode(token);
        case TokenType.Not:
          return new NotNode(token);
        case TokenType.QueryName:
          switch (token.Text)
          {
            case "$filter":
              return new FilterNode(token);
            case "$select":
              return new SelectNode(token);
            case "$orderby":
              return new OrderByNode(token);
            case "$skip":
              return new SkipNode(token);
            case "$top":
              return new TopNode(token);
            default:
              return new IgnoredNode(token);
          }
        default:
          return new IgnoredNode(token);
      }
    }

    private int GetPrecedence(ODataNode node)
    {
      // TODO: isof and cast

      if (node.Type == TokenType.Navigation
        || node.Type == TokenType.Period)
        return 170;
      else if (node.Type == TokenType.Has)
        return 170;
      else if (node.Type == TokenType.Negate
        || node.Type == TokenType.Not)
        return 160;
      else if (node.Type == TokenType.Multiply
        || node.Type == TokenType.Divide
        || node.Type == TokenType.Modulo)
        return 150;
      else if (node.Type == TokenType.Add
        || node.Type == TokenType.Subtract)
        return 140;
      else if (node.Type == TokenType.GreaterThan
        || node.Type == TokenType.LessThan
        || node.Type == TokenType.GreaterThanOrEqual
        || node.Type == TokenType.LessThanOrEqual)
        return 130;
      else if (node.Type == TokenType.Equal
        || node.Type == TokenType.NotEqual)
        return 120;
      else if (node.Type == TokenType.And)
        return 110;
      else if (node.Type == TokenType.Or)
        return 100;
      else if (node is AscNode || node is DescNode)
        return 95;
      else if (node.Type == TokenType.Comma || node.Type == TokenType.Colon)
        return 90;
      else if (node.Type == TokenType.QueryAssign)
        return 80;
      else if (node.Type == TokenType.Amperstand)
        return 70;
      else if (node.Type == TokenType.PathSeparator)
        return 60;
      else if (node.Type == TokenType.Question)
        return 50;

      return 0;
    }
  }
}
