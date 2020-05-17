using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    public class DataTableAjaxResult<T> : JsonResult
    {
        public DataTableAjaxResult(IEnumerable<T> data, int draw, int count) : base(new { draw, recordsTotal = count, recordsFiltered = count, data })
        {
        }

        public DataTableAjaxResult(IEnumerable<T> data, int draw, int count, object serializerSettings) : base(new { draw, recordsTotal = count, recordsFiltered = count, data }, serializerSettings)
        {
        }
    }
}
