using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ODataToolkit
{
  internal static class QueryableExtensions
  {
    /// <summary>
    /// Just a dummy method to use with the expand node
    /// </summary>
    public static IQueryable Include(this IQueryable source, string path)
    {
      return source;
    }
    /// <summary>
    /// Just a dummy method to use with the expand node
    /// </summary>
    public static IQueryable<T> Include<T>(this IQueryable<T> source, string path) where T : class
    {
      return source;
    }
  }
}
