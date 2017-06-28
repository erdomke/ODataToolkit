using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ODataToolkit
{
  public class ODataUriBuilder
  {
    private StringBuilder _writer;
    private ODataVersion _version;

    public Uri Uri { get { return new Uri(_writer.ToString()); } }

    public ODataUriBuilder(ODataVersion version = ODataVersion.All)
    {
      _writer = new StringBuilder();
      _version = version;
    }

    public ODataUriBuilder Argument(params object[] values)
    {
      var first = true;
      foreach (var value in values)
      {
        if (!first || (_writer.Length > 0 && _writer[_writer.Length - 1] != '('))
          _writer.Append(',');
        first = false;
        Value(value);
      }
      return this;
    }

    public ODataUriBuilder Raw(char value)
    {
      _writer.Append(value);
      return this;
    }

    public ODataUriBuilder Raw(string value)
    {
      _writer.Append(value);
      return this;
    }

    public ODataUriBuilder Segment(params string[] values)
    {
      var first = true;
      foreach (var value in values)
      {
        if (!first || (_writer.Length > 0 && _writer[_writer.Length - 1] != '/'))
          _writer.Append('/');
        first = false;
        _writer.Append(value);
      }
      return this;
    }

    public ODataUriBuilder Value(object value)
    {
      _writer.Append(Token.FromPrimative(value, _version).Text);
      return this;
    }

    public override string ToString()
    {
      return _writer.ToString();
    }
  }
}
