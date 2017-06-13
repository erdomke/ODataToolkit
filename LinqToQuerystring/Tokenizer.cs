using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LinqToQuerystring
{
  public class Tokenizer : IEnumerator<Token>
  {
    private Versions _versionSupport = Versions.All;

    private string _value;
    private int _idx;
    private State _state;
    private Token _current;

    public Token Current { get { return _current; } }
    object IEnumerator.Current { get { return _current; } }

    private enum State
    {
      PathStart,
      PathSeparator,
      ParamOpen,
      ParamName,
      ParamEq,
      ParamValue,
      ParamEnd,
      QueryName,
      QueryEq,
      QueryValue,
    }

    internal Tokenizer(string value)
    {
      _value = value;
      Reset();
    }

    public bool MoveNext()
    {
      if (_idx >= _value.Length)
        return false;

      for (var i = _idx; i < _value.Length; i++)
      {
        switch (_state)
        {
          case State.PathStart:
            switch (_value[i])
            {
              case ':':
                if (i == 0)
                  throw new ParseException(_value, i);

                if (_idx == 0 && (i + 2) < _value.Length && _value[i + 1] == '/' && _value[i + 2] == '/')
                {
                  _current = new Token(TokenType.Scheme, _value.Substring(_idx, i - _idx));
                  _state = State.PathSeparator;
                  _idx = i;
                  return true;
                }
                else if (_current != null && _current.Text == "://")
                {
                  _current = new Token(TokenType.Authority, _value.Substring(_idx, i - _idx));
                  _state = State.PathSeparator;
                  _idx = i;
                  return true;
                }
                else
                {
                  throw new ParseException(_value, i);
                }
              case '/':
              case '?':
                if (i > _idx)
                {
                  var type = TokenType.Identifier;
                  if (_current != null && _current.Text == "://")
                    type = TokenType.Authority;
                  else if (_current != null && _current.Text == ":")
                    type = TokenType.Port;

                  _current = new Token(type, _value.Substring(_idx, i - _idx));
                  _state = State.PathSeparator;
                  _idx = i;
                  return true;
                }
                else if (_value[i] == '?')
                {
                  _current = new Token(TokenType.Question, "?");
                  _state = State.QueryName;
                  _idx = i + 1;
                  return true;
                }
                else if (_value[i] == '/')
                {
                  _current = new Token(TokenType.PathSeparator, "/");
                  _state = State.PathStart;
                  _idx = i + 1;
                  return true;
                }
                break;
              case '=':
                if (i > _idx)
                {
                  _current = new Token(TokenType.QueryName, _value.Substring(_idx, i - _idx));
                  _state = State.QueryEq;
                  _idx = i;
                  return true;
                }
                break;
            }
            if (TryUnencode(i) == '(')
            {
              _current = new Token(TokenType.Identifier, _value.Substring(_idx, i - _idx));
              _state = State.ParamOpen;
              _idx = i;
              return true;
            }
            break;
          case State.PathSeparator:
            if (_value[i] == '?')
            {
              _current = new Token(TokenType.Question, "?");
              _state = State.QueryName;
              _idx = i + 1;
              return true;
            }
            else if (_value[i] != '/' && _value[i] != ':')
            {
              _current = new Token(TokenType.PathSeparator, _value.Substring(_idx, i - _idx));
              _state = State.PathStart;
              _idx = i;
              return true;
            }
            break;
          case State.ParamOpen:
            if (i == _idx && TryConsumeChar(ref i, '('))
            {
              _current = new Token(TokenType.OpenParen, _value.Substring(_idx, 1));
              _state = State.ParamName;
              _idx = i;
              return true;
            }
            throw new ParseException(_value, i);
          case State.ParamName:
            if (TryConsumeChar(ref i, ')'))
            {
              _current = new Token(TokenType.CloseParen, ")");
              _state = State.PathSeparator;
              _idx = i;
              return true;
            }
            _current = TryConsumeIdentifier()
              ?? TryConsumeLiteral();
            if (_current == null)
              throw new ParseException(_value, _idx);
            _state = _current.Type == TokenType.Identifier ? State.ParamEq : State.ParamEnd;
            return true;
          case State.ParamEq:
            if (i == _idx && _value[i] == '=')
            {
              _current = new Token(TokenType.Equals, _value.Substring(_idx, 1));
              _state = State.ParamValue;
              _idx = i + 1;
              return true;
            }
            throw new ParseException(_value, i);
          case State.ParamValue:
            _current = TryConsumeAlias()
              ?? TryConsumeLiteral();
            if (_current == null)
              throw new ParseException(_value, _idx);
            _state = State.ParamEnd;
            return true;
          case State.ParamEnd:
            if (TryConsumeChar(ref i, ','))
            {
              _current = new Token(TokenType.Comma, ",");
              _state = State.ParamName;
              _idx = i;
              return true;
            }
            else if (TryConsumeChar(ref i, ')'))
            {
              _current = new Token(TokenType.CloseParen, ")");
              _state = State.PathSeparator;
              _idx = i;
              return true;
            }
            throw new ParseException(_value, _idx);
          case State.QueryName:
            if (_value[i] == '@')
            {
              _current = TryConsumeAlias();
              if (_current == null)
                throw new ParseException(_value, _idx);
              _state = State.QueryEq;
              return true;
            }
            else if(_value[i] == '$')
            {
              _idx++;
              _current = TryConsumeIdentifier();
              _current.Text = "$" + _current.Text;
              _current.Type = TokenType.QueryName;
              _state = State.QueryEq;
              return true;
            }
            else if (_value[i] == '=')
            {
              _current = new Token(TokenType.QueryName, _value.Substring(_idx, i - _idx));
              _state = State.QueryEq;
              _idx = i;
              return true;
            }
            break;
          case State.QueryEq:
            if (i == _idx && _value[i] == '=')
            {
              _current = new Token(TokenType.Equals, _value.Substring(_idx, 1));
              _state = State.QueryValue;
              _idx = i + 1;
              return true;
            }
            throw new ParseException(_value, i);
          case State.QueryValue:
            if (i == _idx)
            {
              if (_value[i] == '&')
              {
                _current = new Token(TokenType.Amperstand, _value.Substring(_idx, 1));
                _state = State.QueryName;
                _idx = i + 1;
                return true;
              }

              switch (TryUnencode(_idx))
              {
                case '*':
                  _current = new Token(TokenType.Star, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '.':
                  _current = new Token(TokenType.Period, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '/':
                  _current = new Token(TokenType.Navigation, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case ',':
                  _current = new Token(TokenType.Comma, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '(':
                  _current = new Token(TokenType.OpenParen, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case ')':
                  _current = new Token(TokenType.CloseParen, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case ':':
                  _current = new Token(TokenType.Colon, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '$':
                  _idx++;
                  _current = TryConsumeIdentifier();
                  if (_current == null)
                    throw new ParseException(_value, _idx);
                  _current.Text = "$" + _current.Text;
                  return true;
              }
            }

            _current = TryConsumeWhitespace()
              ?? TryConsumeKeyword()
              ?? TryConsumeLiteral() 
              ?? TryConsumeAlias()
              ?? TryConsumeIdentifier();
            if (_current == null)
              throw new ParseException(_value, _idx);
            return true;
        }
      }

      if (_idx < _value.Length)
      {
        switch (_state)
        {
          case State.PathStart:
            _current = new Token(TokenType.Identifier, _value.Substring(_idx));
            _idx = _value.Length;
            return true;
        }
      }

      throw new ParseException(_value, _idx);
    }

    public Token TryConsumeWhitespace()
    {
      var i = _idx;
      while (_value[i] == '+' || TryUnencode(i) == ' ')
      {
        if (_value[i] == ' ' || _value[i] == '+')
          i++;
        else
          i += 3;
      }

      if (i == _idx)
        return null;

      var result = new Token(TokenType.Whitespace, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    public bool TryConsumeChar(ref int index, char match)
    {
      if (index >= _value.Length)
        return false;

      int ascii;
      if (_value[index] != '%' || (index + 2) >= _value.Length
        || !int.TryParse(_value.Substring(index + 1, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ascii))
      {
        if (_value[index] == match)
        {
          index++;
          return true;
        }
        return false;
      }


      if ((char)ascii == match)
      {
        index += 3;
        return true;
      }
      return false;
    }

    public char TryUnencode(int index)
    {
      int ascii;
      if (_value[index] != '%' || (index + 2) >= _value.Length
        || !int.TryParse(_value.Substring(index + 1, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ascii))
        return _value[index];
      return (char)ascii;
    }

    public Token TryConsumeAlias()
    {
      if (_value[_idx] != '@')
        return null;

      _idx++;
      var result = TryConsumeIdentifier();
      if (result == null)
      {
        _idx--;
        return null;
      }
      result.Text = "@" + result.Text;
      result.Type = TokenType.Alias;
      return result;
    }

    public Token TryConsumeIdentifier()
    {
      if (!char.IsLetter(_value[_idx]) && _value[_idx] != '_')
        return null;

      var i = _idx + 1;
      while (i < _value.Length && (char.IsLetter(_value[i]) || char.IsDigit(_value[i]) || _value[i] == '_'))
        i++;

      var result = new Token(TokenType.Identifier, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    public Token TryConsumeKeyword()
    {
      var i = _idx;
      while (i < _value.Length && char.IsLetter(_value[i]))
        i++;

      if (i == _idx)
        return null;

      switch (_value.Substring(_idx, i - _idx))
      {
        case "and":
        case "or":
        case "eq":
        case "ne":
        case "lt":
        case "le":
        case "gt":
        case "ge":
        case "has":
        case "add":
        case "sub":
        case "mul":
        case "div":
        case "mod":
        case "not":
          var result = new Token(TokenType.Operator, _value.Substring(_idx, i - _idx));
          _idx = i;
          return result;
      }

      return null;
    }

    public Token TryConsumeLiteral()
    {
      Token result;

      switch (TryUnencode(_idx))
      {
        case 'n':
          if ((_idx + 4) <= _value.Length && _value.Substring(_idx, 4) == "null")
          {
            _idx += 4;
            return new Token(TokenType.Null, "null");
          }
          return null;
        case 'N':
          if ((_idx + 3) <= _value.Length && _value.Substring(_idx, 3) == "NaN")
          {
            _idx += 3;
            return new Token(TokenType.NaN, "NaN");
          }
          else if ((_idx + 4) <= _value.Length 
            && SupportV2OrV3()
            && (_value.Substring(_idx, 4) == "NaNd"
              || _value.Substring(_idx, 4) == "NaND"
              || _value.Substring(_idx, 4) == "NaNf"
              || _value.Substring(_idx, 4) == "NaNF"))
          {
            _idx += 4;
            return new Token(TokenType.NaN, _value.Substring(_idx, 4));
          }
          return null;
        case 't':
          if ((_idx + 4) <= _value.Length && _value.Substring(_idx, 4) == "true")
          {
            _idx += 4;
            return new Token(TokenType.True, "true");
          }
          return TryConsumeDuration();
        case 'a':
        case 'c':
        case 'e':
          return TryConsumeGuid();
        case 'b':
          return TryConsumeBinary()
            ?? TryConsumeGuid();
        case 'd':
          return TryConsumeDuration()
            ?? TryConsumeDateTime()
            ?? TryConsumeGuid();
        case 'f':
          if ((_idx + 5) <= _value.Length && _value.Substring(_idx, 5) == "false")
          {
            _idx += 5;
            return new Token(TokenType.False, "false");
          }
          else
          {
            return TryConsumeGuid();
          }
        case 'g':
          return TryConsumeGuid();
        case 'I':
          if ((_idx + 3) <= _value.Length && _value.Substring(_idx, 3) == "INF")
          {
            _idx += 3;
            return new Token(TokenType.PosInfinity, "INF");
          }
          else if ((_idx + 4) <= _value.Length
            && SupportV2OrV3()
            && (_value.Substring(_idx, 4) == "INFd"
              || _value.Substring(_idx, 4) == "INFD"
              || _value.Substring(_idx, 4) == "INFf"
              || _value.Substring(_idx, 4) == "INFF"))
          {
            _idx += 4;
            return new Token(TokenType.PosInfinity, _value.Substring(_idx, 4));
          }
          return null;
        case 'X':
          return TryConsumeBinary();
        case '-':
        case '+':
          if ((_idx + 4) <= _value.Length && _value.Substring(_idx, 4) == "-INF")
          {
            _idx += 4;
            return new Token(TokenType.NegInfinity, "-INF");
          }
          else if ((_idx + 5) <= _value.Length
            && SupportV2OrV3()
            && (_value.Substring(_idx, 5) == "-INFd"
              || _value.Substring(_idx, 5) == "-INFD"
              || _value.Substring(_idx, 5) == "-INFf"
              || _value.Substring(_idx, 5) == "-INFF"))
          {
            _idx += 5;
            return new Token(TokenType.NegInfinity, _value.Substring(_idx, 5));
          }
          return TryConsumeNumber();
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
          return TryConsumeGuid()
            ?? TryConsumeDateTime()
            ?? TryConsumeTimeOfDay()
            ?? TryConsumeNumber();
        case '\'':
          var i = _idx + 1;
          while (i < _value.Length)
          {
            if (TryConsumeChar(ref i, '\''))
            {
              if (!TryConsumeChar(ref i, '\''))
              {
                break;
              }
            }
            else
            {
              i++;
            }
          }
          result = new Token(TokenType.String, _value.Substring(_idx, i - _idx));
          _idx = i;
          return result;
      }

      return null;
    }

    private Token TryConsumeGuid()
    {
      if ((_idx + 36) > _value.Length)
        return null;

      if (SupportV4()
          && IsHexPhrase(_idx, _idx + 8)
          && _value[_idx + 8] == '-'
          && IsHexPhrase(_idx + 9, _idx + 13)
          && _value[_idx + 13] == '-'
          && IsHexPhrase(_idx + 14, _idx + 18)
          && _value[_idx + 18] == '-'
          && IsHexPhrase(_idx + 19, _idx + 23)
          && _value[_idx + 23] == '-'
          && IsHexPhrase(_idx + 24, _idx + 36))
      {
        _idx += 36;
        return new Token(TokenType.Guid, _value.Substring(_idx - 36, 36));
      }

      if (SupportV2OrV3()
        && (_idx + 42) <= _value.Length
        && _value.Substring(_idx, 5) == "guid'"
        && IsHexPhrase(_idx + 5, _idx + 13)
        && _value[_idx + 13] == '-'
        && IsHexPhrase(_idx + 14, _idx + 18)
        && _value[_idx + 18] == '-'
        && IsHexPhrase(_idx + 19, _idx + 23)
        && _value[_idx + 23] == '-'
        && IsHexPhrase(_idx + 24, _idx + 28)
        && _value[_idx + 28] == '-'
        && IsHexPhrase(_idx + 29, _idx + 41)
        && _value[_idx + 41] == '\'')
      {
        _idx += 42;
        return new Token(TokenType.Guid, _value.Substring(_idx - 42, 42));
      }

      return null;
    }

    private Token TryConsumeBinary()
    {
      var i = _idx;
      var start = 0;
      var type = TokenType.Base64;

      if ((_idx + 7) <= _value.Length
        && _value.Substring(_idx, 7) == "binary'")
      {
        start = 7;
        if (!SupportV4())
          type = TokenType.Binary;
      }
      else if ((_idx + 2) <= _value.Length
        && _value.Substring(_idx, 2) == "X'"
        && SupportV2OrV3())
      {
        start = 2;
        type = TokenType.Binary;
      }
      else
      {
        return null;
      }
      i += start;

      while (i < _value.Length && _value[i] != '\'')
      {
        if (type == TokenType.Binary)
        {
          switch (_value[i])
          {
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case 'a':
            case 'b':
            case 'c':
            case 'd':
            case 'e':
            case 'f':
            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
              break;
            default:
              return null;
          }
        }
        else
        {
          if (!char.IsLetterOrDigit(_value[i])
            && _value[i] != '-'
            && _value[i] != '_'
            && _value[i] != '=')
            return null;
        }
        i++;
      }

      if (i >= _value.Length)
        return null;
      if (type == TokenType.Binary
        && (i - _idx - start) % 2 != 0)
        return null;

      i++;
      var result = new Token(type, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    private Token TryConsumeDateTime()
    {
      // Just a date
      var length = 10;
      var start = _idx;
      if ((_idx + length) > _value.Length)
        return null;

      var startsWithPrefix = false;
      if (_value.Substring(_idx, 9) == "datetime'")
      {
        startsWithPrefix = true;
        start += 9;
      }
      else if ((_idx + 15) <= _value.Length 
        && _value.Substring(_idx, 15) == "datetimeoffset'")
      {
        startsWithPrefix = true;
        start += 15;
      }

      // Prefix required for v2 & v3
      if (!SupportV4() && !startsWithPrefix)
        return null;
      // Prefix should not be there for v4
      if (!SupportV2OrV3() && startsWithPrefix)
        return null;
      // Make sure there are enough characters
      if (startsWithPrefix && (_idx + 10 + length) > _value.Length)
        return null;
      
      
      int year, month, day;
      if (_value[start + 4] != '-' 
        || _value[start + 7] != '-'
        || !int.TryParse(_value.Substring(start, 4), out year)
        || !int.TryParse(_value.Substring(start + 5, 2), out month)
        || !int.TryParse(_value.Substring(start + 8, 2), out day)
        || month < 1 || month > 12 || day < 1 || day > 31)
        return null;
      
      // Date and Time
      if ((start + 16) <= _value.Length
        && _value[start + 10] == 'T' && _value[start + 13] == ':')
      {
        length += 6;
        int hour, minute, second;
        if (!int.TryParse(_value.Substring(start + 11, 2), out hour)
          || !int.TryParse(_value.Substring(start + 14, 2), out minute)
          || hour < 0 || hour > 23 || minute < 0 || minute > 59)
          return null;
        
        // Fractional seconds
        if ((start + length + 3) <= _value.Length
          && _value[start + length] == ':'
          && int.TryParse(_value.Substring(start + length + 1, 2), out second)
          && second >= 0 && second <= 59)
        {
          length += 3;

          if ((start + length + 2) > _value.Length
            && _value[start + length] == '.'
            && char.IsDigit(_value[start + length + 1]))
          {
            length += 2;
            while ((start + length) < _value.Length && char.IsDigit(_value[start + length]))
              length++;
          }
        }

        // Timezone
        int tzHour, tzMinute;
        if ((start + length) < _value.Length
          && _value[start + length] == 'Z')
        {
          length++;
        }
        else if ((start + length + 6) <= _value.Length
          && (_value[start + length] == '+' || _value[start + length] == '-')
          && int.TryParse(_value.Substring(start + length + 1, 2), out tzHour)
          && _value[start + length + 3] == ':'
          && int.TryParse(_value.Substring(start + length + 4, 2), out tzMinute)
          && tzHour >= 0 && tzHour <= 23
          && tzMinute >= 0 && tzMinute <= 59)
        {
          length += 6;
        }
      }

      if (startsWithPrefix
        && _value[start + length] != '\'')
        return null;
      if (startsWithPrefix)
        length += (start - _idx + 1);

      _idx += length;
      return new Token(TokenType.Date, _value.Substring(_idx - length, length));
    }

    private Token TryConsumeDuration()
    {
      var i = _idx;
      if (SupportV4()
        && (_idx + 9) <= _value.Length
        && _value.Substring(_idx, 9) == "duration'")
      {
        i += 9;
      }
      else if (SupportV2OrV3()
        && (_idx + 5) <= _value.Length
        && _value.Substring(_idx, 5) == "time'")
      {
        i += 5;
      }
      else
      {
        return null;
      }

      if (i < _value.Length
        && (_value[i] == '-') || (_value[i] == '+'))
        i++;
      if (i >= _value.Length || _value[i] != 'P')
        return null;
      i++;

      if (i < _value.Length && char.IsDigit(_value[i]))
      {
        while (i < _value.Length && char.IsDigit(_value[i]))
          i++;
        if (i >= _value.Length || _value[i] != 'D')
          return null;
        i++;
      }
      
      if (i < _value.Length && _value[i] == 'T')
      {
        i++;
        if (i >= _value.Length || !char.IsDigit(_value[i]))
          return null;

        while (i < _value.Length && char.IsDigit(_value[i]))
        {
          while (i < _value.Length && char.IsDigit(_value[i]))
            i++;

          if (i < _value.Length && _value[i] == '.')
          {
            i++;
            while (i < _value.Length && char.IsDigit(_value[i]))
              i++;
            if (i >= _value.Length || _value[i] != 'S')
              return null;
          }
          else if (i >= _value.Length || (_value[i] != 'H' && _value[i] != 'M'))
          {
            return null;
          }
          i++;
        }
      }

      if (i >= _value.Length || _value[i] != '\'')
        return null;
      i++;

      var result = new Token(TokenType.Duration, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    private Token TryConsumeTimeOfDay()
    {
      var length = 5;
      if ((_idx + 5) > _value.Length)
        return null;

      if (_value[_idx + 2] != ':')
        return null;

      int hour, minute, second;
      if (!int.TryParse(_value.Substring(_idx, 2), out hour)
        || !int.TryParse(_value.Substring(_idx + 3, 2), out minute))
        return null;

      if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
        return null;

      if ((_idx + length + 3) <= _value.Length
        && _value[_idx + length] == ':'
        && int.TryParse(_value.Substring(_idx + length + 1, 2), out second)
        && second >= 0 && second <= 59)
      {
        length += 3;

        if ((_idx + length + 2) <= _value.Length
          && _value[_idx + length] == '.'
          && char.IsDigit(_value[_idx + length + 1]))
        {
          length += 2;
          while ((_idx + length) < _value.Length && char.IsDigit(_value[_idx + length]))
            length++;
        }
      }

      _idx += length;
      return new Token(TokenType.TimeOfDay, _value.Substring(_idx - length, length));
    }

    private Token TryConsumeNumber()
    {
      var type = TokenType.Integer;

      var i = _idx + 1;
      while (i < _value.Length && char.IsDigit(_value[i]))
        i++;
      if ((i - _idx) == 1
        && (_value[i - 1] == '-' || _value[i - 1] == '+'))
        return null;

      if (i < _value.Length && _value[i] == '.')
      {
        type = TokenType.Double;
        i++;
        while (i < _value.Length && char.IsDigit(_value[i]))
          i++;
      }
      else if (SupportV2OrV3()
        && i < _value.Length
        && _value[i] == 'L')
      {
        i++;
        type = TokenType.Long;
      }

      if ((i + 1) < _value.Length && _value[i] == 'e'
        && (_value[i + 1] == '-' || _value[i + 1] == '+' || char.IsDigit(_value[i + 1]))
        && type != TokenType.Long)
      {
        if (char.IsDigit(_value[i + 1]) || ((i + 2) < _value.Length && char.IsDigit(_value[i + 2])))
        {
          type = TokenType.Double;
          i += 2;
          while (i < _value.Length && char.IsDigit(_value[i]))
            i++;
        }
      }
      else if (SupportV2OrV3()
        && i < _value.Length 
        && (_value[i] == 'M' || _value[i] == 'm')
        && type != TokenType.Long)
      {
        i++;
        type = TokenType.Decimal;
      }

      if (SupportV2OrV3()
        && i < _value.Length
        && (_value[i] == 'D' || _value[i] == 'd'))
      {
        i++;
        type = TokenType.Double;
      }
      else if (SupportV2OrV3()
        && i < _value.Length
        && (_value[i] == 'F' || _value[i] == 'f'))
      {
        i++;
        type = TokenType.Single;
      }

      var result = new Token(type, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    private bool IsHexPhrase(int start, int end)
    {
      for (var i = start; i < end; i++)
      {
        switch (_value[i])
        {
          case '0':
          case '1':
          case '2':
          case '3':
          case '4':
          case '5':
          case '6':
          case '7':
          case '8':
          case '9':
          case 'a':
          case 'b':
          case 'c':
          case 'd':
          case 'e':
          case 'f':
          case 'A':
          case 'B':
          case 'C':
          case 'D':
          case 'E':
          case 'F':
            break;
          default:
            return false;
        }
      }
      return true;
    }

    public void Reset()
    {
      _idx = 0;
      _state = State.PathStart;
    }

    private bool SupportV4()
    {
      return (_versionSupport & Versions.v4) == Versions.v4;
    }
    private bool SupportV3()
    {
      return (_versionSupport & Versions.v3) == Versions.v3;
    }
    private bool SupportV2()
    {
      return (_versionSupport & Versions.v2) == Versions.v2;
    }
    private bool SupportV2OrV3()
    {
      return (_versionSupport & Versions.v3) == Versions.v3
        || (_versionSupport & Versions.v2) == Versions.v2;
    }

    public void Dispose()
    {
      // Do nothing
    }
  }
}
