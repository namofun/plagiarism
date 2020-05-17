using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Microsoft.AspNetCore.Mvc.DataTables.Internal
{
    public class DataTableViewModel
    {
        private TagBuilder? tHead;
        private string? scripts;
        private Func<object, TagBuilder>? tRow;

        public TagBuilder THead
        {
            get => tHead ?? throw new InvalidOperationException();
            set => tHead = value;
        }

        public Func<object, TagBuilder> TRow
        {
            get => tRow ?? throw new InvalidOperationException();
            set => tRow = value;
        }

        public string Scripts
        {
            get => scripts ?? throw new InvalidOperationException();
            set => scripts = value;
        }

        public bool Searchable { get; set; }

        public bool Sortable { get; set; }

        public string? Sort { get; set; }
    }
}
