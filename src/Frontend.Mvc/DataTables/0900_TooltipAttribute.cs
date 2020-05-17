using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtTooltipAttribute : Attribute, IDataTablePipeline
    {
        public int Order => -100;

        public string Format { get; }

        public DtTooltipAttribute(string format)
        {
            Format = format;
        }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            return expressions
                .Append(TR.WriteAttr("title", TR.GetFormatted(Format, modelExpression)));
        }
    }
}
