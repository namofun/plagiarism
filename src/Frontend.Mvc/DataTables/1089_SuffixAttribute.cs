using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtSuffixAttribute : Attribute, IDataTablePipeline
    {
        public string Suffix { get; }

        public int Order => 89;

        public DtSuffixAttribute(string suffix)
        {
            Suffix = suffix;
        }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            return expressions.Append(TR.WriteText(TR.GetFormatted(Suffix, modelExpression)));
        }
    }
}
