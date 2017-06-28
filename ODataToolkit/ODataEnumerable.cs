using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ODataToolkit
{
  /// <summary>
  /// Enumerable of OData items supporting conversion to Dictionary objects and OData responses
  /// </summary>
  public class ODataEnumerable<T> : ODataEnumerableBase<T>
  {
    private IEnumerable<T> _enum;

    public ODataEnumerable(IEnumerable<T> enumerable, ODataUri uri)
    {
      _uri = uri;
      _enum = enumerable;
    }

    /// <summary>
    /// Provides a method for converting objects to dictionaries
    /// </summary>
    public ODataEnumerable<T> WithDictionaryFactory(Func<object, Dictionary<string, object>> factory)
    {
      _dictFactory = factory;
      return this;
    }

    public override IEnumerator<T> GetEnumerator()
    {
      return _enum.GetEnumerator();
    }
    protected override IEnumerator GetEnumeratorBase()
    {
      return GetEnumerator();
    }
  }
}
