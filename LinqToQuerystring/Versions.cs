using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToQuerystring
{
  [Flags]
  public enum Versions
  {
    All = -1,
    v2 = 1,
    v3 = 2,
    v4 = 4,
  }
}
