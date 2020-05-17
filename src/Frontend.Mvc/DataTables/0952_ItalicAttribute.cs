using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtItalicAttribute : Attribute, IDataTablePipeline
    {
        public int Order => -48;

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            return expressions
                .Append(TR.WriteHtml("</i>"))
                .Prepend(TR.WriteHtml("<i>"));
        }
    }
}
