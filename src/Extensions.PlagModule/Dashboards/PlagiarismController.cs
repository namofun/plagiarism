using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using SatelliteSite.PlagModule.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.PlagModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize("HasDashboard")]
    [Route("[area]/[controller]")]
    public class PlagiarismController : ViewControllerBase
    {
        public IPlagiarismDetectService Store { get; }

        public ILogger<PlagiarismController> Logger { get; }

        public PlagiarismController(IPlagiarismDetectService store, ILogger<PlagiarismController> logger)
        {
            Store = store;
            Logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            if (page < 1) return NotFound();
            const int PageCount = 30;
            int? userId = !User.IsInRole("Administrator") && int.TryParse(User.GetUserId(), out var uuid) ? uuid : default(int?);
            var lsts = await Store.ListSetsAsync(uid: userId, skip: (page - 1) * PageCount, limit: PageCount);
            ViewBag.Page = page;
            
            return View(lsts.Select(s => new SetListModel
            {
                Id = s.Id.ToString(),
                CreateTime = s.CreateTime,
                Name = s.Name,
                TotalReports = s.ReportCount,
                PendingReports = s.ReportPending,
                FinishedSubmissions = s.SubmissionFailed + s.SubmissionSucceeded,
                TotalSubmissions = s.SubmissionCount,
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


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SetCreateModel model)
        {
            int? userId = int.TryParse(User.GetUserId(), out var uuid) ? uuid : default(int?);
            if (!ModelState.IsValid) return View(model);
            var set = await Store.CreateSetAsync(new SetCreation { Name = model.Name, UserId = userId });
            return RedirectToAction(nameof(Detail), new { sid = set.Id });
        }


        [HttpGet("{sid}")]
        public async Task<IActionResult> Detail(string sid, string language = null, int? exclusive_category = null, int? inclusive_category = null, double? min_percent = null)
        {
            var set = await Store.FindSetAsync(sid);
            if (set == null) return NotFound();

            var ss = await Store.ListSubmissionsAsync(sid, language, exclusive_category, inclusive_category, min_percent);
            ViewBag.ViewModel = ss.Select(ReportListModel.Conv);
            return View(set);
        }


        [HttpGet("{sid}/[action]")]
        public async Task<IActionResult> Upload(string sid)
        {
            var report = await Store.FindSetAsync(sid);
            if (report == null) return NotFound();
            ViewBag.Languages = await Store.ListLanguageAsync();
            return View(new SetUploadModel());
        }


        [HttpPost("{sid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(string sid, SetUploadModel model)
        {
            var set = await Store.FindSetAsync(sid);
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
                        SetId = sid,
                        InclusiveCategory = model.Inclusive,
                        ExclusiveCategory = model.Exclusive,
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
            return RedirectToAction(nameof(Detail), new { sid });
        }


        [HttpGet("{sid}/submissions/{id}")]
        public async Task<IActionResult> Submission(string sid, int id)
        {
            var ss = await Store.FindSubmissionAsync(sid, id, false);
            if (ss == null) return NotFound();

            var rep = await Store.GetComparisonsBySubmissionAsync(sid, id);
            ViewBag.Reports = rep.Select(c => new SubmissionListModel(sid, c));

            if (ss.TokenProduced == false)
            {
                var er = await Store.GetCompilationAsync(sid, id);
                ViewBag.Error = er.Error;
            }
            else
            {
                ViewBag.Error = null;
            }

            return View(ss);
        }


        [HttpGet("{sid}/submissions/{id}/[action]")]
        public async Task<IActionResult> SourceCode(string sid, int id)
        {
            var ss = await Store.FindSubmissionAsync(sid, id);
            if (ss == null) return NotFound();
            return View(ss);
        }


        [HttpGet("{sid}/reports/{rid}")]
        public async Task<IActionResult> Compare(string sid, string rid)
        {
            var report = await Store.FindReportAsync(rid);
            if (report == null || sid != report.SetId) return NotFound();

            var subA = await Store.FindSubmissionAsync(sid, report.SubmissionA);
            var subB = await Store.FindSubmissionAsync(sid, report.SubmissionB);

            var retA = CodeModel.CreateView(report, c => c.FileA, c => c.ContentStartA, c => c.ContentEndA, subA);
            var retB = CodeModel.CreateView(report, c => c.FileB, c => c.ContentStartB, c => c.ContentEndB, subB);
            
            return View(new ReportModel(report, retA, retB));
        }
    }
}
