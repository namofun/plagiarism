using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.DataTables.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Render a table using <c>dataTables</c>.
    /// </summary>
    [HtmlTargetElement("datatable", TagStructure = TagStructure.WithoutEndTag)]
    public class DataTablesTagHelper : TagHelper
    {
        private static readonly IMemoryCache FactoryCache =
            new MemoryCache(new MemoryCacheOptions { Clock = new Extensions.Internal.SystemClock() });

        [HtmlAttributeName("data")]
        public IEnumerable? Data { get; set; }

        [HtmlAttributeName("element-type")]
        public Type? DataType { get; set; }

        [HtmlAttributeName("asp-url")]
        public string? UpdateUrl { get; set; }

        [HtmlAttributeName("paging")]
        public int? Paging { get; set; }

        [HtmlAttributeName("auto-width")]
        public bool AutoWidth { get; set; } = true;

        [HtmlAttributeName("thead-class")]
        public string? TableHeaderClass { get; set; }

        private void PrintScript(
            string uniqueId,
            DataTables.Internal.DataTableViewModel model,
            TagHelperContent content)
        {
            content.AppendHtml("<script>$().ready(function(){$('#");
            content.Append(uniqueId);
            content.AppendHtmlLine("').DataTable({");
            content.AppendHtml("\"searching\":" + (model.Searchable ? "true" : "false") + ",");

            if (model.Sortable)
                content.AppendHtml("\"ordering\":true,\"order\":[" + model.Sort + "],");
            else
                content.AppendHtml("\"ordering\":false,");

            if (Paging.HasValue)
                content.AppendHtml("\"paging\":true,\"pageLength\":" + Paging.Value + ",\"lengthChange\":false,");
            else
                content.AppendHtml("\"paging\": false,");

            content.AppendHtml("\"info\":false,\"autoWidth\":" + (AutoWidth ? "true" : "false") + ",");
            content.AppendHtml("'language': { 'searchPlaceholder': 'filter table', 'search': '_INPUT_', 'oPaginate': {'sPrevious': '&laquo;', 'sNext': '&raquo;'} },");
            content.AppendHtml("'aoColumnDefs': [{ aTargets: ['sortable'], bSortable: true }, { aTargets: ['searchable'], bSearchable: true }, { aTargets: ['_all'], bSortable: false, bSearchable: false }],");

            if (UpdateUrl != null)
            {
                content.AppendHtml("\"serverSide\":true,\"ajax\":{\"url\":\"" + UpdateUrl + "\"},");
                content.AppendHtml(model.Scripts);
            }

            content.AppendHtmlLine("});});</script>");
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);
            output.Attributes.SetAttribute("id", context.UniqueId);

            if (Data == null && DataType == null)
                throw new InvalidOperationException("No data specified.");
            if (Data != null)
                DataType = Data.GetType().GetInterface("IEnumerable`1")?.GetGenericArguments()[0]
                    ?? throw new NotSupportedException();

            var viewModel = await FactoryCache.GetOrCreateAsync(DataType,
                entry => DataRowFunctions.Factory((Type)entry.Key));

            output.TagName = "table";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.AddClass("data-table table table-sm table-striped");
            output.PreElement.AppendHtml("<div class=\"table-wrapper\">");
            output.PostElement.AppendHtml("</div>");
            PrintScript(context.UniqueId, viewModel, output.PostElement);
            if (AutoWidth) output.Attributes.Add("style", "width:auto");

            var thead = new TagBuilder("thead").WithBody(viewModel.THead);
            if (TableHeaderClass != null) thead.AddCssClass(TableHeaderClass);
            output.Content.AppendHtml(thead);

            if (Data != null)
            {
                var tbody = new TagBuilder("tbody");

                foreach (var item in Data)
                {
                    if (item == null)
                    {
                        tbody.InnerHtml.SetHtmlContent("<tr><td>NULL error</td></tr>");
                        break;
                    }
                    else
                    {
                        var tr = viewModel.TRow(item);
                        tr.MergeAttribute("role", "row");
                        tbody.InnerHtml.AppendHtml(tr);
                    }
                }

                output.Content.AppendHtml(tbody);
            }
        }
    }
}
