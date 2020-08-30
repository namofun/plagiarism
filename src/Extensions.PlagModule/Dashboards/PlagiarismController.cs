using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plag.Backend.Entities;
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
            var lsts = await Store.ListSetsAsync(page);
            
            return View(lsts.As(s => new SetListModel
            {
                Id = s.Id.ToString(),
                CreateTime = s.CreateTime,
                Name = s.Name,
                TotalReports = s.ReportCount,
                PendingReports = s.ReportPending,
            }));
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
            if (report == null) return null;

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

            var time = DateTimeOffset.Now;
            var err = new StringBuilder();

            foreach (var item in model.Files)
            {
                try
                {
                    using var stream = item.OpenReadStream();
                    using var zip = new ZipArchive(stream);

                    var fid = 0;

                    var sub = new Submission
                    {
                        Name = Path.GetFileNameWithoutExtension(item.FileName),
                        UploadTime = time,
                        Language = model.Language,
                        SetId = pid,
                    };

                    var files = new List<SubmissionFile>();
                    foreach (var i in zip.Entries)
                    {
                        if (!lang.Suffixes.Any(j => i.Name.EndsWith(j, StringComparison.OrdinalIgnoreCase))) continue;
                        using var sr = new StreamReader(i.Open());
                        files.Add(new SubmissionFile
                        {
                            FileId = ++fid,
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

            var reportA =
                from r in Context.Reports
                where r.SubmissionB == sid
                join s in Context.Submissions on r.SubmissionA equals s.Id
                select new SubmissionListModel
                {
                    BiggestMatch = r.BiggestMatch,
                    SubmissionAnother = s.Name,
                    SubmissionIdAnother = s.Id,
                    Id = r.Id,
                    Pending = r.Pending,
                    TokensMatched = r.TokensMatched,
                    Percent = r.Percent,
                    PercentIt = r.PercentA,
                    PercentSelf = r.PercentB
                };

            var reportB =
                from r in Context.Reports
                where r.SubmissionA == sid
                join s in Context.Submissions on r.SubmissionB equals s.Id
                select new SubmissionListModel
                {
                    BiggestMatch = r.BiggestMatch,
                    SubmissionAnother = s.Name,
                    SubmissionIdAnother = s.Id,
                    Id = r.Id,
                    Pending = r.Pending,
                    TokensMatched = r.TokensMatched,
                    Percent = r.Percent,
                    PercentIt = r.PercentB,
                    PercentSelf = r.PercentA
                };

            var rep = await reportA.Concat(reportB).ToListAsync();
            rep.ForEach(a => a.EnsurePending());
            ViewBag.Reports = rep;

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
            ViewBag.Report = report;

            var subA = await Store.FindSubmissionAsync(report.SubmissionA);
            var subB = await Store.FindSubmissionAsync(report.SubmissionB);

            var retA = CodeModel.CreateView(report, c => c.FileA, c => c.ContentStartA, c => c.ContentEndA, subA);
            var retB = CodeModel.CreateView(report, c => c.FileB, c => c.ContentStartB, c => c.ContentEndB, subB);
            
            return View((retA, retB));
        }
    }
}
