using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Plag.Backend;
using SatelliteSite.PlagModule.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IStoreService = Plag.Backend.Services.IPlagiarismDetectService;

namespace SatelliteSite.PlagModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize("HasDashboard")]
    [Route("[area]/[controller]")]
    public class PlagiarismController : ViewControllerBase
    {
        public IStoreService Store { get; }

        public ILogger<PlagiarismController> Logger { get; }

        public PlagiarismController(IStoreService store, ILogger<PlagiarismController> logger)
        {
            Store = store;
            Logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            if (page < 1) return NotFound();
            const int PageCount = 30;
            var lsts = await Store.ListSetsAsync((page - 1) * PageCount, PageCount);
            ViewBag.Page = page;
            
            return View(lsts.Select(s => new SetListModel
            {
                Id = s.Id.ToString(),
                CreateTime = s.CreateTime,
                Name = s.Name,
                TotalReports = s.ReportCount,
                PendingReports = s.ReportPending,
            }));
        }


        [HttpGet("[action]")]
        public IActionResult Refresh()
        {
            return AskPost(
                title: "Plagiarism service",
                message: "If the service isn't running, use this function to notify.",
                area: "Dashboard", controller: "Plagiarism", action: "Refresh", routeValues: new { },
                type: BootstrapColor.danger);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refresh(bool _ = false)
        {
            await Store.RescueAsync();
            return RedirectToAction(nameof(List));
        }


        [HttpGet("[action]")]
        public IActionResult Create()
        {
            return View(new SetCreateModel());
        }


        [HttpGet("set/{pid}")]
        public async Task<IActionResult> Detail(string pid)
        {
            var report = await Store.FindSetAsync(pid);
            if (report == null) return NotFound();

            report.Submissions = await Store.ListSubmissionsAsync(pid);
            ViewBag.ViewModel = report.Submissions.Select(ReportListModel.Conv);
            return View(report);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SetCreateModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var set = await Store.CreateSetAsync(model.Name);
            return RedirectToAction(nameof(Detail), new { pid = set.Id });
        }


        [HttpGet("set/{pid}/[action]")]
        public async Task<IActionResult> Upload(string pid)
        {
            var report = await Store.FindSetAsync(pid);
            if (report == null) return NotFound();
            ViewBag.Languages = await Store.ListLanguageAsync();
            return View(new SetUploadModel());
        }


        [HttpPost("set/{pid}/[action]")]
        [ValidateAjaxWindow]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(string pid, SetUploadModel model)
        {
            var set = await Store.FindSetAsync(pid);
            if (set == null) ModelState.AddModelError("checkset", "Plagiarism set not found.");
            var lang = await Store.FindLanguageAsync(model.Language);
            if (lang == null) ModelState.AddModelError("lang", "Language not found.");
            if (!ModelState.IsValid) return View(model);

            var err = new StringBuilder();

            foreach (var item in model.Files)
            {
                try
                {
                    using var stream = item.OpenReadStream();
                    using var zip = new ZipArchive(stream);

                    var sub = new SubmissionCreation
                    {
                        Name = Path.GetFileNameWithoutExtension(item.FileName),
                        Language = model.Language,
                        SetId = pid,
                    };

                    var files = new List<SubmissionCreation.SubmissionFileCreation>();
                    foreach (var i in zip.Entries)
                    {
                        if (!lang.Suffixes.Any(j => i.Name.EndsWith(j, StringComparison.OrdinalIgnoreCase))) continue;
                        using var sr = new StreamReader(i.Open());
                        files.Add(new SubmissionCreation.SubmissionFileCreation
                        {
                            FileName = i.Name,
                            FilePath = i.FullName,
                            Content = sr.ReadToEnd()
                        });
                    }

                    if (files.Count == 0)
                    {
                        err.AppendLine($"Error importing `{item.FileName}`, file empty.");
                        continue;
                    }

                    sub.Files = files;
                    await Store.SubmitAsync(sub);
                }
                catch (Exception ex)
                {
                    err.AppendLine($"Error importing `{item.FileName}`, unexpected exception.");
                    Logger.LogError(ex, $"Error importing `{item.FileName}`.");
                }
            }

            err.Append("Import finished.");
            StatusMessage = err.ToString();
            return RedirectToAction(nameof(Detail), new { pid });
        }


        [HttpGet("submit/{sid}")]
        public async Task<IActionResult> Submission(string sid)
        {
            var ss = await Store.FindSubmissionAsync(sid, false);
            if (ss == null) return NotFound();

            var rep = await Store.GetComparisonsBySubmissionAsync(sid);
            ViewBag.Reports = rep.Select(c => new SubmissionListModel(c));

            if (ss.TokenProduced == false)
            {
                var er = await Store.GetCompilationAsync(sid);
                ViewBag.Error = er.Error;
            }
            else
            {
                ViewBag.Error = null;
            }

            return View(ss);
        }


        [HttpGet("submit/{sid}/[action]")]
        public async Task<IActionResult> SourceCode(string sid)
        {
            var ss = await Store.FindSubmissionAsync(sid);
            if (ss == null) return NotFound();
            return View(ss);
        }


        [HttpGet("report/{rid}")]
        public async Task<IActionResult> Compare(string rid)
        {
            var report = await Store.FindReportAsync(rid);
            if (report == null) return NotFound();

            var subA = await Store.FindSubmissionAsync(report.SubmissionA);
            var subB = await Store.FindSubmissionAsync(report.SubmissionB);

            var retA = CodeModel.CreateView(report, c => c.FileA, c => c.ContentStartA, c => c.ContentEndA, subA);
            var retB = CodeModel.CreateView(report, c => c.FileB, c => c.ContentStartB, c => c.ContentEndB, subB);
            
            return View(new ReportModel(report, retA, retB));
        }
    }
}
