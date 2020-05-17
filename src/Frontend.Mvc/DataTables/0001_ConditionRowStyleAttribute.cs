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
    public sealed class DtRowStyleAttribute : Attribute, IDataTablePipeline
    {
        public string? CssClass { get; set; }

        public string? CssStyle { get; set; }

        public string? CssClassElse { get; set; }

        public string? CssStyleElse { get; set; }

        public string[] Conditions { get; }

        public int Order => -999;

        public DtRowStyleAttribute(params string[] conditions)
        {
            Conditions = conditions;
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

            var ifTrue = Enumerable.Empty<Expression>();
            var ifFalse = Enumerable.Empty<Expression>();
            if (CssClass != null)
                ifTrue = ifTrue.Append(TR.WriteCssClass2(TR.GetFormatted(CssClass, modelExpression)));
            if (CssStyle != null)
                ifTrue = ifTrue.Append(TR.WriteAttr2("style", TR.GetFormatted(CssStyle, modelExpression)));
            if (CssClassElse != null)
                ifFalse = ifTrue.Append(TR.WriteCssClass2(TR.GetFormatted(CssClassElse, modelExpression)));
            if (CssStyleElse != null)
                ifFalse = ifTrue.Append(TR.WriteAttr2("style", TR.GetFormatted(CssStyleElse, modelExpression)));

            return expressions.Prepend(Expression.IfThenElse(
                condition, Expression.Block(ifTrue), Expression.Block(ifFalse)));
        }
    }
}
