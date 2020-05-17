using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DtIgnoreAttribute : Attribute, IDataTablePipeline
    {
        public int Order => -1000;

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression)
        {
            return Enumerable.Empty<Expression>();
        }
    }
}
