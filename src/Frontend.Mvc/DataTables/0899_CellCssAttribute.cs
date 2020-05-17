using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtCellCssAttribute : Attribute, IDataTablePipeline
    {
        public int Order => -101;

        public string? Class { get; set; }

        public string? Style { get; set; }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> e,
            Expression modelExpression)
        {
            if (Class != null) e = e.Append(TR.WriteCssClass(Class));
            if (Style != null) e = e.Append(TR.WriteAttr("style", Expression.Constant(Style)));
            return e;
        }
    }
}
