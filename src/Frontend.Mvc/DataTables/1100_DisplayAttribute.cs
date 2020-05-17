using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DtDisplayAttribute : Attribute, IDataTablePipeline
    {
        public string? Content { get; }

        public int Priority { get; }

        public string Title { get; }

        public bool Searchable { get; set; }

        public bool Sortable { get; set; }

        public string? DefaultAscending { get; set; }

        public int Order => 100;

        public DtDisplayAttribute(int priority, string title, string? content = null)
        {
            Priority = priority;
            Title = title;
            Content = content;
        }

        public virtual IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression>? expressions,
            Expression modelExpression)
        {
            var ret = Content == null
                ? TR.WriteText(Expression.Property(modelExpression, prop))
                : TR.WriteText(TR.GetFormatted(Content, modelExpression));
            return Enumerable.Repeat(ret, 1);
        }
    }
}
