using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODataParser
{
  [DebuggerDisplay("{Value} {Type}")]
  public class Token
  {
    public TokenType Type { get; set; }
    public string Value { get; set; }

    public Token(TokenType type, string value)
    {
      this.Type = type;
      this.Value = Uri.UnescapeDataString(value);
    }

    public object AsPrimitive()
    {
      switch (Type)
      {
        case TokenType.Date:
          return DateTime.Parse(Value);
        case TokenType.Double:
          return double.Parse(Value);
        case TokenType.False:
          return false;
        case TokenType.Guid:
          return new Guid(Value);
        case TokenType.Integer:
          return int.Parse(Value);
        case TokenType.NaN:
          return double.NaN;
        case TokenType.NegInfinity:
          return double.NegativeInfinity;
        case TokenType.Null:
          return null;
        case TokenType.PosInfinity:
          return double.PositiveInfinity;
        case TokenType.String:
          return Value.Trim('\'').Replace("''", "'");
        case TokenType.TimeOfDay:
          return DateTime.Parse(Value);
        case TokenType.True:
          return true;
      }
      throw new InvalidOperationException();
    }
  }
}
