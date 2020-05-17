using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtCoalesceAttribute : Attribute, IDataTablePipeline
    {
        public string Coalesce { get; }

        public int Order => 88;

        public DtCoalesceAttribute(string coalesce)
        {
            Coalesce = coalesce;
        }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            Expression isNotNull;

            if (prop.PropertyType.IsValueType)
            {
                if (prop.PropertyType.IsGenericType
                    && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    isNotNull = Expression.Property(
                        Expression.Property(modelExpression, prop),
                        nameof(Nullable<int>.HasValue));
                else
                    return expressions;
            }
            else
            {
                isNotNull = Expression.Equal(
                    Expression.Property(modelExpression, prop),
                    Expression.Constant(null));
            }

            var nulled = TR.WriteText(TR.GetFormatted(Coalesce, modelExpression));
            var notNulled = Expression.IfThenElse(isNotNull,
                Expression.Block(expressions),
                Expression.Block(nulled));
            return Enumerable.Repeat(notNulled, 1);
        }
    }
}
