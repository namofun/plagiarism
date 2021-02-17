using Microsoft.AspNetCore.Mvc;
using SatelliteSite.PlagModule.Models;

namespace SatelliteSite.PlagModule.Components.PlagiarismReport
{
    public class PlagiarismReportViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ReportModel model)
        {
            return View("Default", model);
        }
    }
}
