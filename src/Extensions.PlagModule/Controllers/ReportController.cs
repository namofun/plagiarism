using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Services;
using SatelliteSite.PlagModule.Models;
using System.Threading.Tasks;

namespace SatelliteSite.PlagModule.Controllers
{
    [Area("Plagiarism")]
    public class ReportController : ViewControllerBase
    {
        public IPlagiarismDetectService Store { get; }

        public ReportController(IPlagiarismDetectService store) => Store = store;


        [HttpGet("/plagiarism-reports/{rid}")]
        public async Task<IActionResult> Compare(string rid)
        {
            var report = await Store.FindReportAsync(rid);
            if (report == null) return NotFound();

            var subA = await Store.FindSubmissionAsync(report.SetId, report.SubmissionA);
            var subB = await Store.FindSubmissionAsync(report.SetId, report.SubmissionB);

            var retA = CodeModel.CreateView(report, c => c.FileA, c => c.ContentStartA, c => c.ContentEndA, subA);
            var retB = CodeModel.CreateView(report, c => c.FileB, c => c.ContentStartB, c => c.ContentEndB, subB);

            return View(new ReportModel(report, retA, retB));
        }
    }
}
