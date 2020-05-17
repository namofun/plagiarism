using Microsoft.AspNetCore.Mvc.DataTables.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtEmptyIfAttribute : Attribute, IDataTablePipeline
    {
        public int Order => -500;

        public string[] Conditions { get; }

        public DtEmptyIfAttribute(params string[] conds)
        {
            Conditions = conds;
        }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            Expression condition = Expression.Constant(true);
            foreach (var item in Conditions)
            {
                var cur = modelExpression.Navigate(item);
                if (cur.Type != typeof(bool) && cur.Type != typeof(bool?))
                    throw new InvalidOperationException();
                if (cur.Type == typeof(bool?))
                    cur = Expression.Coalesce(cur, Expression.Constant(false));
                condition = Expression.AndAlso(condition, cur);
            }

            var final = Expression.IfThen(condition, Expression.Block(expressions));
            return Enumerable.Repeat(final, 1);
        }
    }
}
