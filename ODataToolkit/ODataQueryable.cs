using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ODataToolkit
{
  /// <summary>
  /// Queryable data source after OData options have been applied
  /// </summary>
  public class ODataQueryable<T> : IQueryable<T>
  {
    private IQueryable _child;
    private ODataUri _uri;

    public IQueryable Child { get { return _child; } }
    public Type ElementType { get { return _child.ElementType; } }
    public Expression Expression { get { return _child.Expression; } }
    public IQueryProvider Provider { get { return _child.Provider; } }

    internal ODataQueryable(IQueryable child, ODataUri uri)
    {
      _child = child;
      _uri = uri;
    }

    public ODataQueryable<TOut> Cast<TOut>()
    {
      return new ODataQueryable<TOut>(_child, _uri);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return ((IEnumerable<T>)_child).GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
      return _child.GetEnumerator();
    }

    public IEnumerable<Dictionary<string, object>> ToDictionaries()
    {
      return ToDictionaries(this);
    }

    public IEnumerable<Dictionary<string, object>> ToDictionaries(IEnumerable enumerable, Func<object, Dictionary<string, object>> converter = null)
    {
      foreach (var obj in enumerable)
      {
        var record = obj as IProjectedRecord;
        if (record == null)
        {
          if (converter == null)
            throw new NotSupportedException("Can only convert records generated from a select statement");
          else
            yield return converter(obj);
        }
        else
        {
          var fields = _uri.QueryOption["$select"].Children
            .Select(c => c.Text)
            .ToArray();
          var result = new Dictionary<string, object>();
          var i = 0;
          foreach (var value in record)
          {
            result.Add(fields[i++], value);
          }
          yield return result;
        }
      }
    }

    public override string ToString()
    {
      return _child.ToString();
    }
  }
}
