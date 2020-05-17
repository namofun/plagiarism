using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtPrefixAttribute : Attribute, IDataTablePipeline
    {
        public string Prefix { get; }

        public int Order => 90;

        public DtPrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            return expressions.Prepend(TR.WriteText(TR.GetFormatted(Prefix, modelExpression)));
        }
    }
}
