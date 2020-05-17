using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    public interface IDataTablePipeline
    {
        IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> expressions,
            Expression modelExpression);

        int Order { get; }
    }
}
