using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToQuerystring
{
  public class ODataUri : Uri
  {
    public ODataUri(string uri) : base(uri, UriKind.RelativeOrAbsolute) { }

    private ODataQuery _query = new ODataQuery();

    public ODataQuery QueryOption { get { return _query; } }
  }
}
