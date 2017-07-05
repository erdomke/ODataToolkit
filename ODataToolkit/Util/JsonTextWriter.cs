using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ODataToolkit
{
  public class JsonTextWriter : IDisposable
  {
    protected TextWriter _writer;
    protected bool _needsComma = false;
    protected bool _useUTCDateTime = true;
    private bool _useEscapedUnicode = false;
    private bool _disposeWriter = true;

    public bool UseEscapedUnicode
    {
      get { return _useEscapedUnicode; }
      set { _useEscapedUnicode = value; }
    }

    public bool UseUtcDateTime
    {
      get { return _useUTCDateTime; }
      set { _useUTCDateTime = value; }
    }

    public JsonTextWriter(Stream stream, bool disposeWriter)
    {
      _writer = new StreamWriter(stream);
      _disposeWriter = disposeWriter;
    }
    public JsonTextWriter(TextWriter writer, bool disposeWriter)
    {
      _writer = writer;
      _disposeWriter = disposeWriter;
    }

    public JsonTextWriter NeedsComma()
    {
      _needsComma = true;
      return this;
    }

    public JsonTextWriter WriteStartObject()
    {
      if (_needsComma) _writer.Write(",");
      _writer.Write("{");
      _needsComma = false;
      return this;
    }
    public JsonTextWriter WriteEndObject()
    {
      _writer.Write("}");
      _needsComma = true;
      return this;
    }
    public JsonTextWriter WriteStartArray()
    {
      if (_needsComma) _writer.Write(",");
      _writer.Write("[");
      _needsComma = false;
      return this;
    }
    public JsonTextWriter WriteEndArray()
    {
      _writer.Write("]");
      _needsComma = true;
      return this;
    }
    public void Flush()
    {
      _writer.Flush();
    }
    //public IJsonWriter ListSeparator()
    //{
    //  if (_needsComma) _writer.Write(",");
    //  _needsComma = false;
    //  return this;
    //}
    public virtual JsonTextWriter WritePropertyName(string name)
    {
      if (_needsComma) _writer.Write(",");
      WriteStringCore(name);
      _writer.Write(":");
      _needsComma = false;
      return this;
    }
    public JsonTextWriter WriteNullProperty(string name)
    {
      WritePropertyName(name);
      WriteNull();
      _needsComma = true;
      return this;
    }
    public JsonTextWriter WriteProperty(string name, object value)
    {
      WritePropertyName(name);
      WriteValue(value);
      _needsComma = true;
      return this;
    }
    public JsonTextWriter WriteRaw(string value)
    {
      _writer.Write(value);
      return this;
    }
    public virtual JsonTextWriter WriteValue(object obj)
    {
      if (_needsComma) _writer.Write(",");
      _needsComma = true;

      if (obj == null || obj is DBNull)
      {
        WriteNull();
      }
      else if (obj is Guid)
      {
        WriteStringCore(obj.ToString(), true);
      }
      else if (obj is bool)
      {
        _writer.Write(((bool)obj) ? "true" : "false"); // conform to standard
      }
      else if (
          obj is int || obj is long || obj is double ||
          obj is decimal || obj is float ||
          obj is byte || obj is short ||
          obj is sbyte || obj is ushort ||
          obj is uint || obj is ulong
      )
      {
        _writer.Write(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
      }
      else if (obj is DateTime)
      {
        WriteDateTime((DateTime)obj);
      }
      else if (obj is Enum)
      {
        _writer.Write(((int)obj).ToString(NumberFormatInfo.InvariantInfo));
      }
      else
      {
        WriteStringCore(obj.ToString(), false);
      }

      return this;
    }

    protected virtual void WriteDateTime(DateTime dt)
    {
      // datetime format standard : yyyy-MM-dd HH:mm:ss
      if (_useUTCDateTime) dt = TimeZoneInfo.ConvertTimeToUtc(dt);

      _writer.Write("\"");
      _writer.Write(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));
      _writer.Write("-");
      _writer.Write(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));
      _writer.Write("-");
      _writer.Write(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));
      _writer.Write(" ");
      _writer.Write(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));
      _writer.Write(":");
      _writer.Write(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));
      _writer.Write(":");
      _writer.Write(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));

      if (_useUTCDateTime) _writer.Write("Z");

      _writer.Write("\"");
    }
    protected void WriteNull()
    {
      _writer.Write("null");
    }
    private void WriteStringCore(string value)
    {
      WriteStringCore(value, false);
    }
    protected virtual void WriteStringCore(string value, bool fast)
    {
      if (value == null)
      {
        WriteNull();
      }
      else
      {
        _writer.Write("\"");
        if (fast)
        {
          _writer.Write(value);
        }
        else
        {
          WriteStringValueCore(value, '"');
        }
        _writer.Write("\"");
      }
    }
    protected void WriteStringValueCore(string str, char quote)
    {
      var chars = str.ToCharArray();

      int runIndex = -1;

      for (var index = 0; index < chars.Length; ++index)
      {
        var c = chars[index];

        if (_useEscapedUnicode)
        {
          if (c >= ' ' && c < 128 && c != quote && c != '\\')
          {
            if (runIndex == -1)
              runIndex = index;

            continue;
          }
        }
        else
        {
          if (c != '\t' && c != '\n' && c != '\r' && c != quote && c != '\\' && c != '\f' && c != '\b')// && c != ':' && c!=',')
          {
            if (runIndex == -1)
              runIndex = index;

            continue;
          }
        }

        if (runIndex != -1)
        {
          _writer.Write(chars, runIndex, index - runIndex);
          runIndex = -1;
        }

        switch (c)
        {
          case '\t': _writer.Write("\\t"); break;
          case '\r': _writer.Write("\\r"); break;
          case '\n': _writer.Write("\\n"); break;
          case '\f': _writer.Write("\\f"); break;
          case '\b': _writer.Write("\\b"); break;
          default:
            if (c == quote || c == '\\')
            {
              _writer.Write('\\');
              _writer.Write(c);
            }
            else
            {
              if (_useEscapedUnicode)
              {
                _writer.Write("\\u");
                _writer.Write(((int)c).ToString("X4", NumberFormatInfo.InvariantInfo));
              }
              else
                _writer.Write(c);
            }
            break;
        }
      }

      if (runIndex != -1)
        _writer.Write(chars, runIndex, chars.Length - runIndex);
    }
    public override string ToString()
    {
      return _writer.ToString();
    }

    public void Dispose()
    {
      if (_disposeWriter)
        _writer.Dispose();
    }
  }
}
