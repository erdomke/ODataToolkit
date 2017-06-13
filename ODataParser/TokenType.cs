using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODataParser
{
  public enum TokenType
  {
    Identifier,
    Scheme,
    Authority,
    PathSeparator,
    Port,
    OpenParen,
    CloseParen,
    Assign,
    Comma,
    Alias,
    Null,
    True, 
    False,
    Guid,
    Date,
    TimeOfDay,
    NaN,
    NegInfinity,
    PosInfinity,
    Double,
    Integer,
    String,
    Question,
    QueryName,
    Amperstand,
    Star,
    Period,
    Whitespace,
    Operator
  }
}
