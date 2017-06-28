using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ODataToolkit
{
  internal static class Utils
  {
    public static bool StringIsNullOrWhitespace(string value)
    {
      if (string.IsNullOrEmpty(value))
        return true;
      for (var i = 0; i < value.Length; i++)
      {
        if (!char.IsWhiteSpace(value[i]))
          return false;
      }
      return true;
    }

    public static bool SupportsV4(this ODataVersion version)
    {
      return (version & ODataVersion.v4) == ODataVersion.v4;
    }

    public static bool SupportsV3(this ODataVersion version)
    {
      return (version & ODataVersion.v3) == ODataVersion.v3;
    }

    public static bool SupportsV2(this ODataVersion version)
    {
      return (version & ODataVersion.v2) == ODataVersion.v2;
    }

    public static bool SupportsV2OrV3(this ODataVersion version)
    {
      return (version & ODataVersion.v3) == ODataVersion.v3
        || (version & ODataVersion.v2) == ODataVersion.v2;
    }

    public static bool OnlySupportsV2OrV3(this ODataVersion version)
    {
      return version != 0 && (version & (ODataVersion.v3 | ODataVersion.v2)) == version;
    }

    public static void RemoveByFilter<T>(this IList<T> list, Func<T, bool> predicate)
    {
      var i = 0;
      while (i < list.Count)
      {
        if (predicate(list[i]))
          list.RemoveAt(i);
        else
          i++;
      }
    }
  }
}
