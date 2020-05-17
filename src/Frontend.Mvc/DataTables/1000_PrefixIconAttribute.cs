using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtPrefixIconAttribute : Attribute, IDataTablePipeline
    {
        public string Class { get; }

        public int Order => 0;

        public DtPrefixIconAttribute(string @class)
        {
            Class = @class;
        }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression>? expressions,
            Expression modelExpression)
        {
            return expressions
                .Prepend(TR.WriteHtml("\"></i> "))
                .Prepend(TR.WriteText(TR.GetFormatted(Class, modelExpression)))
                .Prepend(TR.WriteHtml("<i class=\""));
        }
    }
}
