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
  public class ODataQueryable<T> : ODataEnumerableBase<T>, IQueryable<T>
  {
    private IQueryable _child;
    
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
      return new ODataQueryable<TOut>(_child, _uri) { LookupByKey = LookupByKey };
    }
    
    /// <summary>
    /// Provides a method for converting objects to dictionaries
    /// </summary>
    public ODataQueryable<T> WithDictionaryFactory(Func<object, Dictionary<string, object>> factory)
    {
      _dictFactory = factory;
      return this;
    }

    /// <summary>
    /// Create a new enumerable using results that you already queried (e.g. during 
    /// an async operation)
    /// </summary>
    public ODataEnumerable<T> WithResults(IEnumerable<T> results)
    {
      return new ODataEnumerable<T>(results, _uri);
    }

    /// <summary>
    /// Indicates how many of the initial segments in the URL are part of the root (or base) path
    /// </summary>
    /// <param name="count">Number of segments</param>
    /// <remarks>
    /// Consider the URL http://host/services/_api/Products(0).  This has 2 root segments such that
    /// the root path is http://host/services/_api/.
    /// </remarks>
    public ODataQueryable<T> WithRootSegmentCount(int count)
    {
      _uri.WithRootSegmentCount(count);
      return this;
    }

    public override IEnumerator<T> GetEnumerator()
    {
      return ((IEnumerable<T>)_child).GetEnumerator();
    }

    protected override IEnumerator GetEnumeratorBase()
    {
      return _child.GetEnumerator();
    }

    public override string ToString()
    {
      return _child.ToString();
    }
  }
}
