using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ODataToolkit
{
  [DebuggerDisplay("{Text} {Type}")]
  public class Token
  {
    public TokenType Type { get; set; }
    public string Text { get; set; }

    public Token(TokenType type, string value)
    {
      this.Type = type;
      this.Text = Uri.UnescapeDataString(value);
    }

    public object AsPrimitive()
    {
      switch (Type)
      {
        case TokenType.Date:
        case TokenType.TimeOfDay:
          if (Text.StartsWith("datetime'"))
            return DateTime.Parse(Text.Substring(9).TrimEnd('\''));
          return DateTime.Parse(Text);
        case TokenType.Decimal:
          return decimal.Parse(Text.TrimEnd(new char[] { 'M', 'm' }));
        case TokenType.Double:
          return double.Parse(Text.TrimEnd(new char[] { 'd', 'D' }));
        case TokenType.Duration:
          return System.Xml.XmlConvert.ToTimeSpan("PT13H20M");
        case TokenType.False:
          return false;
        case TokenType.Guid:
          if (Text.StartsWith("guid'"))
            return new Guid(Text.Substring(5, 36));
          return new Guid(Text);
        case TokenType.Integer:
          return int.Parse(Text);
        case TokenType.Long:
          return long.Parse(Text.TrimEnd('L'));
        case TokenType.NaN:
          return double.NaN;
        case TokenType.NegInfinity:
          return double.NegativeInfinity;
        case TokenType.Null:
          return null;
        case TokenType.PosInfinity:
          return double.PositiveInfinity;
        case TokenType.Single:
          return float.Parse(Text.TrimEnd(new char[] { 'f', 'F'}));
        case TokenType.String:
          return CleanString(Text);
        case TokenType.True:
          return true;
      }
      throw new InvalidOperationException();
    }

    private string CleanString(string value)
    {
      var buffer = new char[value.Length - 2];
      var o = 0;
      for (var i = 1; i < value.Length - 1; i++)
      {
        switch (value[i])
        {
          case '\\':
            switch (value[i+1])
            {
              case '\\':
                buffer[o++] = '\\';
                break;
              case 'b':
                buffer[o++] = '\b';
                break;
              case 't':
                buffer[o++] = '\t';
                break;
              case 'n':
                buffer[o++] = '\n';
                break;
              case 'f':
                buffer[o++] = '\f';
                break;
              case 'r':
                buffer[o++] = '\r';
                break;
              default:
                buffer[o++] = value[i + 1];
                break;
            }
            i++;
            break;
          case '\'':
            buffer[o++] = value[i];
            i++;
            break;
          default:
            buffer[o++] = value[i];
            break;
        }
      }
      return new string(buffer, 0, o);
    }
  }
}
