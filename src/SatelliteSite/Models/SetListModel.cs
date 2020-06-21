using Microsoft.AspNetCore.Mvc.DataTables;
using System;

namespace SatelliteSite.Models
{
    [DtWrapUrl("/plagiarism/set/{Id}")]
    public class SetListModel
    {
        [DtDisplay(0, "ID", Sortable = true)]
        public string Id { get; set; }

        [DtDisplay(1, "name", Sortable = true)]
        public string Name { get; set; }

        [DtDisplay(1, "time", Sortable = true)]
        public DateTimeOffset CreateTime { get; set; }
    }
}
