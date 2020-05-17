using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtIconAttribute : DtDisplayAttribute
    {
        public string Class { get; }

        public DtIconAttribute(int priority, string @class) :
            base(priority, "", null)
        {
            Class = @class;
        }

        public override IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression>? expressions,
            Expression modelExpression)
        {
            return new[]
            {
                TR.WriteHtml("<i class=\""),
                TR.WriteText(Class),
                TR.WriteHtml("\"></i>"),
            };
        }
    }
}
