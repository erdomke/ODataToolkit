using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ODataToolkit
{
  internal interface IProjectedRecord : IEnumerable { }

  /// <summary>
  /// List that stores the results of a SQL select (i.e. projection) operation
  /// </summary>
  /// <remarks>
  /// The shape of this object is based on the fact that LINQ to Entities does not support
  /// constructors with parameters or projection onto list initialisers with more than one value
  /// </remarks>
  internal class ProjectedRecord<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49>
    : IProjectedRecord
  {
    private List<object> _data = new List<object>();

    public T0 Value0 { set { Add(value); } }
    public T1 Value1 { set { Add(value); } }
    public T2 Value2 { set { Add(value); } }
    public T3 Value3 { set { Add(value); } }
    public T4 Value4 { set { Add(value); } }
    public T5 Value5 { set { Add(value); } }
    public T6 Value6 { set { Add(value); } }
    public T7 Value7 { set { Add(value); } }
    public T8 Value8 { set { Add(value); } }
    public T9 Value9 { set { Add(value); } }
    public T10 Value10 { set { Add(value); } }
    public T11 Value11 { set { Add(value); } }
    public T12 Value12 { set { Add(value); } }
    public T13 Value13 { set { Add(value); } }
    public T14 Value14 { set { Add(value); } }
    public T15 Value15 { set { Add(value); } }
    public T16 Value16 { set { Add(value); } }
    public T17 Value17 { set { Add(value); } }
    public T18 Value18 { set { Add(value); } }
    public T19 Value19 { set { Add(value); } }
    public T20 Value20 { set { Add(value); } }
    public T21 Value21 { set { Add(value); } }
    public T22 Value22 { set { Add(value); } }
    public T23 Value23 { set { Add(value); } }
    public T24 Value24 { set { Add(value); } }
    public T25 Value25 { set { Add(value); } }
    public T26 Value26 { set { Add(value); } }
    public T27 Value27 { set { Add(value); } }
    public T28 Value28 { set { Add(value); } }
    public T29 Value29 { set { Add(value); } }
    public T30 Value30 { set { Add(value); } }
    public T31 Value31 { set { Add(value); } }
    public T32 Value32 { set { Add(value); } }
    public T33 Value33 { set { Add(value); } }
    public T34 Value34 { set { Add(value); } }
    public T35 Value35 { set { Add(value); } }
    public T36 Value36 { set { Add(value); } }
    public T37 Value37 { set { Add(value); } }
    public T38 Value38 { set { Add(value); } }
    public T39 Value39 { set { Add(value); } }
    public T40 Value40 { set { Add(value); } }
    public T41 Value41 { set { Add(value); } }
    public T42 Value42 { set { Add(value); } }
    public T43 Value43 { set { Add(value); } }
    public T44 Value44 { set { Add(value); } }
    public T45 Value45 { set { Add(value); } }
    public T46 Value46 { set { Add(value); } }
    public T47 Value47 { set { Add(value); } }
    public T48 Value48 { set { Add(value); } }
    public T49 Value49 { set { Add(value); } }

    private void Add(object value)
    {
      _data.Add(value);
    }

    public IEnumerator GetEnumerator()
    {
      return _data.GetEnumerator();
    }
  }
}
