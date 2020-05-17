using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TR = Microsoft.AspNetCore.Mvc.DataTables.Internal.DataRowFunctions;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class DtWrapUrlAttribute : Attribute, IDataTablePipeline
    {
        public string? Url { get; }

        public string? Ajax { get; }

        public int Order => -50;

        public DtWrapUrlAttribute(string? url)
        {
            Url = url;
        }

        public DtWrapUrlAttribute(string? url, string ajax)
        {
            Url = url;
            Ajax = ajax;
        }

        public IEnumerable<Expression> Process(
            PropertyInfo prop,
            IEnumerable<Expression> e,
            Expression modelExpression)
        {
            if (Url == null) return e;
            var content =  Url.Contains("{")
                ? TR.GetFormatted(Url, modelExpression)
                : Expression.Constant(Url);

            e = e.Append(TR.WriteHtml("</a>"));
            e = e.Prepend(TR.WriteHtml(Ajax != null
                ? $"\" data-toggle=\"ajaxWindow\" data-target=\"{Ajax}\">"
                : "\">"));
            e = e.Prepend(TR.WriteText(content));
            e = e.Prepend(TR.WriteHtml("<a href=\""));
            return e;
        }
    }
}
