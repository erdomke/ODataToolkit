namespace LinqToQuerystring.TreeNodes
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using LinqToQuerystring.Exceptions;
  using LinqToQuerystring.TreeNodes.Base;

  public class FunctionNode : TreeNode
  {
    public FunctionNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      if (Children.Count < 2)
        throw new InvalidOperationException();

      Expression firstExpression, secondExpression, thirdExpression;
      switch (Children[0].Text)
      {
        case "ceiling":
          firstExpression = Children[1].BuildLinqExpression(options);
          if (!typeof(double).IsAssignableFrom(firstExpression.Type) && !typeof(decimal).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "floor");

          return Expression.Call(typeof(Math), "Ceiling", null, new[] { firstExpression });
        case "concat":
          firstExpression = Children[1].BuildLinqExpression(options);
          secondExpression = Children[2].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          if (!typeof(string).IsAssignableFrom(secondExpression.Type))
            secondExpression = Expression.Convert(secondExpression, typeof(string));

          return Expression.Call(typeof(string), "Concat", null, new[] { firstExpression, secondExpression });
        case "date":
          firstExpression = Children[1].BuildLinqExpression(options);
          
          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "date");

          return Expression.Property(firstExpression, "Date");
        case "day":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (typeof(TimeSpan).IsAssignableFrom(firstExpression.Type))
            return Expression.Property(firstExpression, "Days");

          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "day");

          return Expression.Property(firstExpression, "Day");
        case "endswith":
          firstExpression = Children[1].BuildLinqExpression(options);
          secondExpression = Children[2].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          if (!typeof(string).IsAssignableFrom(secondExpression.Type))
            secondExpression = Expression.Convert(secondExpression, typeof(string));

          return Expression.Call(firstExpression, "EndsWith", null, new[] { secondExpression });
        case "floor":
          firstExpression = Children[1].BuildLinqExpression(options);
          if (!typeof(double).IsAssignableFrom(firstExpression.Type) && !typeof(decimal).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "floor");

          return Expression.Call(typeof(Math), "Floor", null, new[] { firstExpression });
        case "fractionalseconds":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "fractionalseconds");

          return Expression.Divide(Expression.Property(firstExpression, "Millisecond"), Expression.Constant(1000.0));
        case "hour":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (typeof(TimeSpan).IsAssignableFrom(firstExpression.Type))
            return Expression.Property(firstExpression, "Hours");

          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "hour");

          return Expression.Property(firstExpression, "Hour");
        case "indexof":
          firstExpression = Children[1].BuildLinqExpression(options);
          secondExpression = Children[2].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          if (!typeof(string).IsAssignableFrom(secondExpression.Type))
            secondExpression = Expression.Convert(secondExpression, typeof(string));

          return Expression.Call(firstExpression, "IndexOf", null, new[] { secondExpression });
        case "length":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          return Expression.Property(firstExpression, "Length");
        case "maxdatetime":
          return Expression.Call(typeof(DateTimeOffset), "MaxValue", null, null);
        case "mindatetime":
          return Expression.Call(typeof(DateTimeOffset), "MinValue", null, null);
        case "minute":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (typeof(TimeSpan).IsAssignableFrom(firstExpression.Type))
            return Expression.Property(firstExpression, "Minutes");

          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "minute");

          return Expression.Property(firstExpression, "Minute");
        case "month":
          firstExpression = Children[1].BuildLinqExpression(options);
          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "month");

          return Expression.Property(firstExpression, "Month");
        case "now":
          return Expression.Call(typeof(DateTimeOffset), "Now", null, null);
        case "replace":
          firstExpression = Children[1].BuildLinqExpression(options);
          secondExpression = Children[2].BuildLinqExpression(options);
          thirdExpression = Children[3].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          if (!typeof(string).IsAssignableFrom(secondExpression.Type))
            secondExpression = Expression.Convert(secondExpression, typeof(string));

          if (!typeof(string).IsAssignableFrom(secondExpression.Type))
            thirdExpression = Expression.Convert(secondExpression, typeof(string));

          return Expression.Call(firstExpression, "Replace", null, new[] { secondExpression, thirdExpression });
        case "round":
          firstExpression = Children[1].BuildLinqExpression(options);
          if (!typeof(double).IsAssignableFrom(firstExpression.Type) && !typeof(decimal).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "round");

          return Expression.Call(typeof(Math), "Round", null, new[] { firstExpression });
        case "second":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (typeof(TimeSpan).IsAssignableFrom(firstExpression.Type))
            return Expression.Property(firstExpression, "Seconds");

          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "second");

          return Expression.Property(firstExpression, "Second");
        case "startswith":
          firstExpression = Children[1].BuildLinqExpression(options);
          secondExpression = Children[2].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          if (!typeof(string).IsAssignableFrom(secondExpression.Type))
            secondExpression = Expression.Convert(secondExpression, typeof(string));

          return Expression.Call(firstExpression, "StartsWith", null, new[] { secondExpression });
        case "substring":
          firstExpression = Children[1].BuildLinqExpression(options);
          secondExpression = Children[2].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          if (!typeof(int).IsAssignableFrom(secondExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "substring");

          if (Children.Count > 3)
          {
            thirdExpression = Children[3].BuildLinqExpression(options);

            if (!typeof(int).IsAssignableFrom(thirdExpression.Type))
              throw new FunctionNotSupportedException(firstExpression.Type, "substring");

            return Expression.Call(firstExpression, "Substring", null, new[] { secondExpression, thirdExpression });
          }
          return Expression.Call(firstExpression, "Substring", null, new[] { secondExpression });
        case "substringof":
          firstExpression = Children[1].BuildLinqExpression(options);
          secondExpression = Children[2].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          if (!typeof(string).IsAssignableFrom(secondExpression.Type))
            secondExpression = Expression.Convert(secondExpression, typeof(string));

          return Expression.Call(secondExpression, "Contains", null, new[] { firstExpression });
        case "time":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "time");

          return Expression.Property(firstExpression, "TimeOfDay");
        case "tolower":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          return Expression.Call(firstExpression, "ToLower", null, null);
        case "totaloffsetminutes":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (!typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "totaloffsetminutes");

          return Expression.Property(Expression.Property(firstExpression, "Offset"), "TotalMinutes");
        case "totalseconds":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (!typeof(TimeSpan).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "totalseconds");

          return Expression.Property(firstExpression, "TotalSeconds");
        case "toupper":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          return Expression.Call(firstExpression, "ToUpper", null, null);
        case "trim":
          firstExpression = Children[1].BuildLinqExpression(options);

          if (!typeof(string).IsAssignableFrom(firstExpression.Type))
            firstExpression = Expression.Convert(firstExpression, typeof(string));

          return Expression.Call(firstExpression, "Trim", null, null);
        case "year":
          firstExpression = Children[1].BuildLinqExpression(options);
          if (!typeof(DateTime).IsAssignableFrom(firstExpression.Type) && !typeof(DateTimeOffset).IsAssignableFrom(firstExpression.Type))
            throw new FunctionNotSupportedException(firstExpression.Type, "year");

          return Expression.Property(firstExpression, "Year");
      }

      if (Children.Count > 1)
      {
        firstExpression = Children[1].BuildLinqExpression(options);
        throw new FunctionNotSupportedException(firstExpression.Type, Children[0].Text);
      }

      throw new NotSupportedException();
    }
  }
}
