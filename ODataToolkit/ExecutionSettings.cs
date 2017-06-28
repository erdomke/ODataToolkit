using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ODataToolkit
{
  /// <summary>
  /// Settings for controlling how an OData URL is executed against an <see cref="IQueryable"/>
  /// </summary>
  public class ExecutionSettings
  {
    private Type _inputType;
    private Expression<Func<object, string, object>> _dynamicAccessor;
    private IEdmModel _model;
    private IEdmNavigationSource _path;
    private int? _maxPageSize;

    /// <summary>
    /// Spectifies the CLR type to be queried against
    /// </summary>
    public ExecutionSettings WithType(Type inputType)
    {
      _inputType = inputType;
      return this;
    }

    /// <summary>
    /// Sets the expression used to access fields on "dynamic" objects
    /// </summary>
    public ExecutionSettings WithDynamicAccessor(Expression<Func<object, string, object>> dynamicAccessor)
    {
      _dynamicAccessor = dynamicAccessor;
      return this;
    }

    /// <summary>
    /// Sets the expression used to access fields on "dynamic" objects
    /// of type <see cref="IDictionary{String, Object}"/>
    /// </summary>
    public ExecutionSettings WithDictionaryAccessor()
    {
      _dynamicAccessor = (obj, key) => ((IDictionary<string, object>)obj)[key];
      return this;
    }


    /// <summary>
    /// Specifies the EDM model to query for metadata (e.g. the names of the
    /// primary key properties for the input type)
    /// </summary>
    public ExecutionSettings WithEdmModel(IEdmModel model)
    {
      _model = model;
      return this;
    }

    /// <summary>
    /// Specifies the EDM model to query for metadata (e.g. the names of the
    /// primary key properties for the input type)
    /// </summary>
    public ExecutionSettings WithEdmModel(IEdmNavigationSource path)
    {
      _path = path;
      return this;
    }

    /// <summary>
    /// Specifies the maximum page size to use
    /// </summary>
    public ExecutionSettings WithMaxPageSize(int maxPageSize)
    {
      _maxPageSize = maxPageSize;
      return this;
    }

    internal Type GetInputType(IQueryable query)
    {
      return _inputType ?? query.GetType().GetGenericArguments()[0];
    }

    internal Expression<Func<object, string, object>> GetDynamicAccessor()
    {
      return _dynamicAccessor;
    }

    internal IEdmNavigationSource GetEdmSource(ODataUri uri)
    {
      return _path ?? uri.FindEntitySet(_model);
    }

    internal int? GetMaxPageSize()
    {
      return _maxPageSize;
    }

    private static ExecutionSettings _empty = new ExecutionSettings();
    internal static ExecutionSettings Empty { get { return _empty; } }
  }
}
