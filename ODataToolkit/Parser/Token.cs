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
    public TokenType Type { get; internal set; }
    public string Text { get; internal set; }

    private Token() { }
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
          return System.Xml.XmlConvert.ToTimeSpan(Text);
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

    public static Token FromPrimative(object value, ODataVersion version = ODataVersion.All)
    {
      var writer = new StringBuilder();
      var result = new Token();

      if (value == null || value is DBNull)
      {
        writer.Append("null");
        result.Type = TokenType.Null;
      }
      else if (value is byte[])
      {
        if (version.SupportsV4())
        {
          writer.Append("binary'");
          var str = Convert.ToBase64String((byte[])value);
          writer.Append(str.Replace('+', '-').Replace('/', '_'));
          writer.Append("'");
        }
        else
        {
          writer.Append("X'");
          foreach (var b in (byte[])value)
          {
            writer.Append(b.ToString("X2"));
          }
          writer.Append("'");
        }
        result.Type = TokenType.Binary;
      }
      else if (value is bool)
      {
        if ((bool)value)
        {
          writer.Append("true");
          result.Type = TokenType.True;
        }
        else
        {
          writer.Append("false");
          result.Type = TokenType.False;
        }
      }
      else if (value is DateTime)
      {
        var date = (DateTime)value;
        if (version.SupportsV4())
        {
          var time = date.TimeOfDay;
          if (time.TotalMilliseconds > 0)
          {
            writer.Append(new DateTimeOffset(date).ToUniversalTime().ToString("s"));
            writer.Append("Z");
          }
          else
          {
            writer.Append(date.ToString("yyyy-MM-dd"));
          }
        }
        else
        {
          writer.Append("datetime'");
          writer.Append(date.ToString("s"));
          writer.Append("'");
        }
        result.Type = TokenType.Date;
      }
      else if (value is DateTimeOffset)
      {
        var offset = (DateTimeOffset)value;
        if (version.SupportsV4())
        {
          writer.Append(offset.ToUniversalTime().ToString("s"));
          writer.Append("Z");
        }
        else
        {
          writer.Append("datetimeoffset'");
          writer.Append(offset.ToUniversalTime().ToString("s"));
          writer.Append("Z'");
        }
        result.Type = TokenType.Date;
      }
      else if (value is decimal)
      {
        writer.Append(value);
        if (version.SupportsV2OrV3() && !version.SupportsV4())
        {
          writer.Append("m");
        }
        result.Type = TokenType.Decimal;
      }
      else if (value is double)
      {
        if (double.IsPositiveInfinity((double)value))
        {
          writer.Append("INF");
          result.Type = TokenType.PosInfinity;
        }
        else if (double.IsNegativeInfinity((double)value))
        {
          writer.Append("-INF");
          result.Type = TokenType.NegInfinity;
        }
        else
        {
          writer.Append(value);
          if (version.SupportsV2OrV3() && !version.SupportsV4())
          {
            writer.Append("d");
          }
          result.Type = double.IsNaN((double)value) ? TokenType.NaN : TokenType.Double;
        }
      }
      else if (value is float)
      {
        if (float.IsPositiveInfinity((float)value))
        {
          writer.Append("INF");
          result.Type = TokenType.PosInfinity;
        }
        else if (float.IsNegativeInfinity((float)value))
        {
          writer.Append("-INF");
          result.Type = TokenType.NegInfinity;
        }
        else
        {
          writer.Append(value);
          if (version.SupportsV2OrV3() && !version.SupportsV4())
          {
            writer.Append("f");
          }
          result.Type = float.IsNaN((float)value) ? TokenType.NaN : TokenType.Single;
        }
      }
      else if (value is Guid)
      {
        if (version.SupportsV4())
        {
          writer.Append(value.ToString());
        }
        else
        {
          writer.Append("guid'");
          writer.Append(value.ToString());
          writer.Append("'");
        }
        result.Type = TokenType.Guid;
      }
      else if (value is int || value is uint
        || value is short || value is ushort
        || value is byte || value is sbyte)
      {
        writer.Append(value);
        result.Type = TokenType.Integer;
      }
      else if (value is long || value is ulong)
      {
        writer.Append(value);
        if (version.SupportsV2OrV3() && !version.SupportsV4())
        {
          writer.Append("L");
        }
        result.Type = TokenType.Long;
      }
      else if (value is TimeSpan)
      {
        var time = (TimeSpan)value;
        var dur = time.Duration();
        writer.Append("duration'");
        if (time.TotalMilliseconds < 0)
          writer.Append('-');
        writer.Append("P");
        if (dur.Days > 0)
        {
          writer.Append(dur.Days);
          writer.Append("D");
        }
        if (dur.Hours > 0 || dur.Minutes > 0 || dur.Seconds > 0 || dur.Milliseconds > 0)
        {
          writer.Append("T");
          writer.Append(dur.Hours);
          writer.Append("H");
          if (dur.Minutes > 0 || dur.Seconds > 0 || dur.Milliseconds > 0)
          {
            writer.Append(dur.Minutes);
            writer.Append("M");
            if (dur.Seconds > 0)
            {
              writer.Append(dur.Seconds);
              writer.Append("S");
              if (dur.Milliseconds > 0)
              {
                writer.Append(".");
                writer.Append(dur.Minutes.ToString("d3"));
              }
            }
          }
        }
        writer.Append("'");
        result.Type = TokenType.Duration;
      }
      else
      {
        writer.Append("'");
        writer.Append(value.ToString().Replace("'", "''"));
        writer.Append("'");
        result.Type = TokenType.String;
      }

      result.Text = writer.ToString();
      return result;
    }
  }
}
