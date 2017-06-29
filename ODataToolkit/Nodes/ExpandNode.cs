using ODataToolkit.Nodes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace ODataToolkit.Nodes
{
  public class ExpandNode : QueryModifier
  {
    private static Dictionary<Type, Delegate> _delegates = new Dictionary<System.Type, Delegate>();

    public ExpandNode(Token payload) : base(payload) { }

    public override Expression BuildLinqExpression(ExpressionOptions options)
    {
      throw new NotSupportedException(
          "Expand is just a placeholder and should be handled differently in Extensions.cs");
    }

    public override IQueryable ModifyQuery(ExpressionOptions options)
    {
      MethodInfo method;
      var action = default(Delegate);
      if (!_delegates.TryGetValue(options.Query.GetType(), out action)
        && options.Query.GetType().TryGetMethod("Include", null, out method))
      {
        var instance = Expression.Parameter(options.Query.GetType(), "i");
        var path = Expression.Parameter(typeof(string), "p");
        action = Expression.Lambda(Expression.Call(instance, method, path), instance, path).Compile();
        _delegates[options.Query.GetType()] = action;
      }

      var queryresult = options.Query;
      if (action != null)
      {
        foreach (var child in Children)
        {
          queryresult = (IQueryable)action.DynamicInvoke(queryresult, child.Text);
        }
      }

      return queryresult;
    }
  }
}
