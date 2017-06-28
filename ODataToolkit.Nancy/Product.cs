using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataToolkit
{
  public class Product
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTimeOffset ReleaseDate { get; set; }
    public DateTimeOffset? DiscontinuedDate { get; set; }
    public short Rating { get; set; }
    public double Price { get; set; }
  }
}
