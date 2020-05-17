using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtBoolSelectAttribute : Attribute, IDataTablePipeline
    {
        public int Order => 92;

        public string IfTrue { get; }
        
        public string IfFalse { get; }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            if (prop.PropertyType != typeof(bool)
                && prop.PropertyType != typeof(bool?))
                throw new TypeAccessException("Not bool or Nullable<bool>.");
            var ifTrue = TR.WriteText(TR.GetFormatted(IfTrue, modelExpression));
            var ifFalse = TR.WriteText(TR.GetFormatted(IfFalse, modelExpression));
            var cond = Expression.Equal(Expression.Property(modelExpression, prop), Expression.Constant(true, prop.PropertyType));
            var display = Expression.IfThenElse(cond, ifTrue, ifFalse);
            return Enumerable.Repeat(display, 1);
        }

        public DtBoolSelectAttribute(string ifTrue, string ifFalse)
        {
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }
    }
}
