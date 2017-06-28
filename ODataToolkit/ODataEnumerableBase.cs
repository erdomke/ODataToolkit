using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ODataToolkit
{
  public abstract class ODataEnumerableBase<T> : IEnumerable<T>
  {
    protected ODataUri _uri;
    protected Func<object, Dictionary<string, object>> _dictFactory;

    public bool LookupByKey { get; set; }
    public ODataUri Uri { get { return _uri; } }

    public IEnumerable<Dictionary<string, object>> ToDictionaries()
    {
      return ToDictionaries(this);
    }

    public IEnumerable<Dictionary<string, object>> ToDictionaries(IEnumerable enumerable, Func<object, Dictionary<string, object>> factory = null)
    {
      foreach (var obj in enumerable)
      {
        var record = obj as IProjectedRecord;
        if (record == null)
        {
          var dictFactory = factory ?? _dictFactory ?? ObjectMembers;
          yield return dictFactory(obj);
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

    public ODataResponse CreateResponse(IEdmModel model, int? totalCount = null)
    {
      return CreateResponse(_uri.FindEntitySet(model), totalCount);
    }

    public ODataResponse CreateResponse(IEdmNavigationSource path, int? totalCount = null)
    {
      if (LookupByKey)
        return new ODataItemResponse(_uri, path
          , ToDictionaries().Cast<IEnumerable<KeyValuePair<string, object>>>().Single());

      return new ODataListResponse(_uri, path
        , ToDictionaries().Cast<IEnumerable<KeyValuePair<string, object>>>()
        , totalCount);
    }

    private Dictionary<string, object> ObjectMembers(object value)
    {
      var accessor = FastMember.TypeAccessor.Create(value.GetType());
      return accessor.GetMembers()
        .Where(p => p.CanRead)
        .ToDictionary(p => p.Name, p => accessor[value, p.Name]);
    }

    public abstract IEnumerator<T> GetEnumerator();
    protected abstract IEnumerator GetEnumeratorBase();
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumeratorBase();
    }
  }
}
