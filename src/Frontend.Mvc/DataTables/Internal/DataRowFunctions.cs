using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.DataTables.Internal
{
    public static class DataRowFunctions
    {
        private static readonly MethodInfo DoAppendText
            = typeof(TableCell).GetMethod(nameof(TableCell.AppendText))
            ?? throw new NotSupportedException();

        private static readonly MethodInfo DoAppendHtml
            = typeof(TableCell).GetMethod(nameof(TableCell.AppendHtml))
            ?? throw new NotSupportedException();

        private static readonly MethodInfo DoAppendFormat
            = typeof(TableCell).GetMethod(nameof(TableCell.AppendFormat))
            ?? throw new NotSupportedException();

        private static readonly MethodInfo DoAddCssClass
            = typeof(TableCell).GetMethod(nameof(TableCell.AddCssClass))
            ?? throw new NotSupportedException();

        private static readonly MethodInfo DoAddCssClass2
            = typeof(TagBuilder).GetMethod(nameof(TagBuilder.AddCssClass))
            ?? throw new NotSupportedException();

        private static readonly MethodInfo DoAddAttr2
            = typeof(TagBuilder).GetMethod(nameof(TagBuilder.MergeAttribute), new[] { typeof(string), typeof(string) })
            ?? throw new NotSupportedException();

        private static readonly MethodInfo DoAddAttr
            = typeof(TableCell).GetMethod(nameof(TableCell.AddAttribute))
            ?? throw new NotSupportedException();

        private static readonly MethodInfo StringFormat
            = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object[]) })
            ?? throw new NotSupportedException();

        private static readonly Expression FactoryTableCell
            = Expression.New(typeof(TableCell).GetConstructors().Single());

        private static readonly Expression FactoryTableRow
            = Expression.New(typeof(TagBuilder).GetConstructors().Single(), Expression.Constant("tr"));

        public static readonly ParameterExpression ExpTableCell
            = Expression.Variable(typeof(TableCell), "cell");

        public static readonly ParameterExpression ExpTableRow
            = Expression.Variable(typeof(TagBuilder), "row");

        private static readonly Expression FactoryFiller
            = Expression.Call(
                Expression.Property(ExpTableRow, nameof(TagBuilder.InnerHtml)),
                nameof(IHtmlContentBuilder.AppendHtml),
                Array.Empty<Type>(),
                ExpTableCell);

        private const BindingFlags FindFlag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        public static TagBuilder WithBody(
            this TagBuilder tag,
            IHtmlContent content)
        {
            tag.InnerHtml.AppendHtml(content);
            return tag;
        }

        public static Expression Navigate(
            this Expression expression,
            string navigation)
        {
            var cur = expression;
            foreach (var nav in navigation.Split('.'))
            {
                var prop = cur.Type.GetProperty(nav, FindFlag);
                cur = Expression.Property(cur, prop);
            }

            return cur;
        }

        public static TagBuilder CreateHeader(
            this DtDisplayAttribute attribute)
        {
            var th = new TagBuilder("th");
            th.MergeAttribute("scope", "col");
            if (attribute.Searchable)
                th.AddCssClass("searchable");
            if (attribute.Sortable)
                th.AddCssClass("sortable");
            th.InnerHtml.Append(attribute.Title);
            return th;
        }

        public static Expression GetFormatted(
            string pattern,
            Expression modelExpression)
        {
            if (!pattern.Contains("{"))
                return Expression.Constant(pattern);

            int id = 0;
            var objs = new List<Expression>();

            string Evaluator(Match match)
            {
                var value = match.Value;
                int idx = value.IndexOf(':');
                if (idx == -1) idx = value.Length - 1;
                objs.Add(modelExpression.Navigate(value[1..idx]));
                return $"{{{id++}" + value[idx..];
            }

            var mcts = Regex.Replace(pattern, "\\{[\\w\\W]+?\\}", Evaluator);
            var fmts = objs.Select(e => e.Type.IsClass ? e : Expression.Convert(e, typeof(object)));
            var par = Expression.NewArrayInit(typeof(object), fmts);
            return Expression.Call(StringFormat, Expression.Constant(mcts), par);
        }

        public static Expression WriteAttr(string key, Expression content)
            => Expression.Call(ExpTableCell, DoAddAttr, Expression.Constant(key), content);

        public static Expression WriteText(string str)
            => WriteText(Expression.Constant(str));

        public static Expression WriteText(Expression content)
            => Expression.Call(ExpTableCell, DoAppendText, ToString(content));

        public static Expression WriteHtml(string str)
            => WriteHtml(Expression.Constant(str));

        public static Expression WriteHtml(Expression content)
            => Expression.Call(ExpTableCell, DoAppendHtml, ToString(content));

        public static Expression ToString(Expression content)
            => content.Type == typeof(string) ? content
            : Expression.Call(content, "ToString", Array.Empty<Type>());

        public static Expression WriteCssClass(string @class)
            => WriteCssClass(Expression.Constant(@class));

        public static Expression WriteCssClass(Expression @class)
            => Expression.Call(ExpTableCell, DoAddCssClass, @class);

        public static Expression WriteCssClass2(Expression @class)
            => Expression.Call(ExpTableRow, DoAddCssClass2, @class);

        public static Expression WriteAttr2(string key, Expression content)
            => Expression.Call(ExpTableRow, DoAddAttr2, Expression.Constant(key), content);

        public static Expression WriteCssClassIf(Expression condition, string @class)
            => Expression.IfThen(condition, Expression.Block(WriteCssClass(@class)));

        public static IEnumerable<Expression> CreateCell(
            this DtDisplayAttribute dtd,
            IDataTablePipeline[] ppl,
            PropertyInfo prop,
            Expression model)
        {   
            var lst = Enumerable.Empty<Expression>();
            for (int i = 0; i < ppl.Length; i++)
            {
                if (i != 0 && ppl[i-1].Order == ppl[i].Order)
                    continue;
                lst = ppl[i].Process(prop, lst, model);
            }

            lst = lst.Prepend(Expression.Assign(ExpTableCell, FactoryTableCell));
            lst = lst.Append(FactoryFiller);
            lst = lst.Append(Expression.Assign(ExpTableCell, Expression.Constant(null, typeof(TableCell))));
            return lst;
        }

        public static Task<DataTableViewModel> Factory(Type type)
        {
            var modelModel = Expression.Parameter(typeof(object), "_model");
            var modelFinal = Expression.Convert(modelModel, type);

            var bodyLst = new List<Expression>();
            bodyLst.Add(Expression.Assign(ExpTableRow, FactoryTableRow));

            var headLst = new TagBuilder("tr");
            var attrs = type.GetCustomAttributes().OfType<IDataTablePipeline>().ToList();

            var query =
                from prop in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                let ppl = prop.GetCustomAttributes()
                    .OfType<IDataTablePipeline>().Concat(attrs)
                    .OrderByDescending(a => a.Order).ToArray()
                where ppl.Length > 0 && !ppl.OfType<DtIgnoreAttribute>().Any()
                    && ppl.OfType<DtDisplayAttribute>().Any()
                let dtd = ppl[0] as DtDisplayAttribute
                orderby dtd.Priority
                select new { prop, ppl, dtd };

            var queryResult = query.ToList();
            bool searchable = false, sortable = false;
            var sorts = new List<string>();
            int i = 0;
            foreach (var prop in queryResult)
            {
                searchable |= prop.dtd.Searchable;
                sortable |= prop.dtd.Sortable;
                bodyLst.AddRange(prop.dtd.CreateCell(prop.ppl, prop.prop, modelFinal));
                headLst.InnerHtml.AppendHtml(prop.dtd.CreateHeader());
                if (prop.dtd.DefaultAscending != null)
                    sorts.Add($"[{i},'{prop.dtd.DefaultAscending}']");
            }

            bodyLst.Add(ExpTableRow);
            var bodyExp = Expression.Lambda<Func<object, TagBuilder>>(
                Expression.Block(typeof(TagBuilder),
                new[] { ExpTableCell, ExpTableRow },
                bodyLst), modelModel);

            return Task.FromResult(new DataTableViewModel
            {
                Scripts = "",
                THead = headLst,
                Searchable = searchable,
                Sortable = sortable,
                Sort = string.Join(',', sorts),
                TRow = bodyExp.Compile()
            });
        }
    }
}
