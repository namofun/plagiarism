using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtForceWidthAttribute : Attribute, IDataTablePipeline
    {
        public int Order => -49;

        public double Width { get; }

        public DtForceWidthAttribute(double maxWidth)
        {
            Width = maxWidth;
        }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            return expressions
                .Append(TR.WriteHtml("</span>"))
                .Prepend(TR.WriteHtml($"<span class=\"forceWidth\" style=\"max-width:{Width}em\">"));
        }
    }
}
