using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Text.Encodings.Web;

namespace Microsoft.AspNetCore.Mvc.DataTables.Internal
{
    public class TableCell : IHtmlContent
    {
        private readonly IHtmlContentBuilder _builder;
        private readonly TagBuilder _tag;

        public void AppendText(string unencoded)
        {
            _builder.Append(unencoded);
        }

        public void AppendHtml(string encoded)
        {
            _builder.AppendHtml(encoded);
        }

        public void AddCssClass(string @class)
        {
            _tag.AddCssClass(@class);
        }

        public void AddAttribute(string key, string value)
        {
            _tag.MergeAttribute(key, value, true);
        }

        public void AppendFormat(string format, object[] args)
        {
            _builder.AppendFormat(format, args);
        }

        void IHtmlContent.WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _tag.WriteTo(writer, encoder);
        }

        public TableCell()
        {
            _tag = new TagBuilder("td");
            _builder = _tag.InnerHtml;
        }
    }
}
