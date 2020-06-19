using Microsoft.AspNetCore.Mvc.DataTables;
using System;

namespace SatelliteSite.Models
{
    [DtWrapUrl("/check/{Id}")]
    public class CheckListModel
    {
        [DtDisplay(0, "Id", Sortable = true)]
        public string Id { get; set; }

        [DtDisplay(1, "CreateTime", Sortable = true)]
        public DateTimeOffset CreateTime { get; set; }

    }
}
