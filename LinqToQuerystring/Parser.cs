using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqToQuerystring.TreeNodes;
using LinqToQuerystring.TreeNodes.Comparisons;
using LinqToQuerystring.TreeNodes.Base;
using LinqToQuerystring.TreeNodes.DataTypes;
using LinqToQuerystring.TreeNodes.Aggregates;

namespace LinqToQuerystring
{
  public class Parser
  {
    private Stack<TreeNode> _output = new Stack<TreeNode>();
    private Stack<TreeNode> _operators = new Stack<TreeNode>();
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
      var queryNode = default(TreeNode);

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
              node = new FunctionNode(token);
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
            case TokenType.OpenParen:
              _operators.Push(node);
              break;
            case TokenType.CloseParen:
              while (_operators.Count > 0 && _operators.Peek().Type != TokenType.OpenParen)
                PopOperator();
              if (_operators.Count > 0 && _operators.Peek().Type == TokenType.OpenParen)
              {
                var op = _operators.Pop();
                if (op is FunctionNode || op is AnyNode || op is AllNode)
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

    private void CollectQueryTerms(TreeNode node)
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

    private void PopOperator()
    {
      var op = _operators.Pop();
      if (op is TwoChildNode || op.Type == TokenType.Amperstand
        || op.Type == TokenType.Navigation || op.Type == TokenType.Colon)
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
      else if (op is SingleChildNode || op.Type == TokenType.Question || op is AscNode || op is DescNode)
      {
        op.Children.Insert(0, _output.Pop());
        _output.Push(op);
      }
      else if (op.Type == TokenType.Equals)
      {
        var value = _output.Pop();
        if (_output.Peek().Type == TokenType.QueryName)
        {
          _output.Peek().Children.Add(value);
        }
        else
        {
          op.Children.Insert(0, _output.Pop());
          op.Children.Insert(0, _output.Pop());
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

    private TreeNode FromToken(Token token, TreeNode queryNode)
    {
      switch (token.Type)
      {
        case TokenType.Alias:
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
        case TokenType.Operator:
          switch (token.Text)
          {
            case "and":
              return new AndNode(token);
            case "or":
              return new OrNode(token);
            case "eq":
              return new EqualsNode(token);
            case "ne":
              return new NotEqualsNode(token);
            case "lt":
              return new LessThanNode(token);
            case "le":
              return new LessThanOrEqualNode(token);
            case "gt":
              return new GreaterThanNode(token);
            case "ge":
              return new GreaterThanOrEqualNode(token);
            case "not":
              return new NotNode(token);
            //case "has":
            //  return new Has(_inputType, token);
            //case "add":
            //case "sub":
            //case "mul":
            //case "div":
            //case "mod":
            default:
              return new IgnoredNode(token);
          }
        case TokenType.QueryName:
          switch (token.Text)
          {
            case "$filter":
              return new FilterNode(token);
            case "$select":
              return new SelectNode(token);
            case "$orderby":
              return new OrderByNode(token);
            //case "$expand":
            //  return new ExpandNode(token);
            case "$skip":
              return new SkipNode(token);
            case "$top":
              return new TopNode(token);
            case "$inlinecount":
              return new InlineCountNode(token);
            default:
              return new IgnoredNode(token);
          }
        default:
          return new IgnoredNode(token);
      }
    }

    private int GetPrecedence(TreeNode node)
    {
      // TODO: isof and cast

      if (node.Type == TokenType.Navigation || node.Type == TokenType.Period)
        return 170;
      else if (node.Type == TokenType.Operator && node.Text == "has")
        return 170;
      else if (node.Type == TokenType.Operator
        && (node.Text == "-" || node.Text == "not"))
        return 160;
      else if (node.Type == TokenType.Operator
        && (node.Text == "mul" || node.Text == "div"
         || node.Text == "mod"))
        return 150;
      else if (node.Type == TokenType.Operator
        && (node.Text == "add" || node.Text == "sub"))
        return 140;
      else if (node.Type == TokenType.Operator
        && (node.Text == "gt" || node.Text == "ge"
         || node.Text == "lt" || node.Text == "le"))
        return 130;
      else if (node.Type == TokenType.Operator
        && (node.Text == "eq" || node.Text == "ne"))
        return 120;
      else if (node.Type == TokenType.Operator && node.Text == "and")
        return 110;
      else if (node.Type == TokenType.Operator && node.Text == "or")
        return 100;
      else if (node is AscNode || node is DescNode)
        return 95;
      else if (node.Type == TokenType.Comma || node.Type == TokenType.Colon)
        return 90;
      else if (node.Type == TokenType.Equals)
        return 80;
      else if (node.Type == TokenType.Amperstand)
        return 70;
      else if (node.Type == TokenType.Question || node.Type == TokenType.PathSeparator)
        return 60;

      return 0;
    }
  }
}
