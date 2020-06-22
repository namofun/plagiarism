using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SatelliteSite.Data;
using SatelliteSite.Models;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.Controllers
{
    [Route("[controller]")]
    public class PlagiarismController : Controller2
    {
        public PlagiarismContext Context { get; }

        public ILogger<PlagiarismController> Logger { get; }

        public PlagiarismController(PlagiarismContext context, ILogger<PlagiarismController> logger)
        {
            Context = context;
            Logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            var lst = await Context.CheckSets
                .Select(s => new SetListModel
                {
                    Id = s.Id.ToString(),
                    CreateTime = s.CreateTime,
                    Name = s.Name,
                    TotalReports = s.ReportCount,
                    PendingReports = s.ReportPending,
                })
                .ToListAsync();

            return View(lst);
        }


        [HttpGet("[action]")]
        public IActionResult Create()
        {
            return View(new SetCreateModel());
        }


        [HttpGet("set/{pid}")]
        public async Task<IActionResult> Detail(int pid)
        {
            var report = await Context.CheckSets
                .Where(s => s.Id == pid)
                .SingleOrDefaultAsync();
            if (report == null) return null;

            report.Submissions = await Context.Submissions
                .Where(s => s.SetId == pid)
                .ToListAsync();

            ViewBag.ViewModel = report.Submissions.Select(ReportListModel.Conv);

            return View(report);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SetCreateModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var e = Context.CheckSets.Add(new CheckSet
            {
                CreateTime = DateTimeOffset.Now,
                Name = model.Name,
            });

            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(Detail), new { pid = e.Entity.Id });
        }


        [HttpGet("set/{pid}/[action]")]
        public async Task<IActionResult> Upload(int pid)
        {
            var report = await Context.CheckSets
                .Where(s => s.Id == pid)
                .SingleOrDefaultAsync();
            if (report == null) return null;
            return View(new SetUploadModel());
        }


        [HttpPost("set/{pid}/[action]")]
        [ValidateInAjax]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int pid, SetUploadModel model)
        {
            if (!PdsRegistry.SupportedLanguages.ContainsKey(model.Language))
                ModelState.AddModelError("lang", "Language not found.");
            if (!ModelState.IsValid)
                return View(model);

            var cs = await Context.CheckSets.SingleOrDefaultAsync(s => s.Id == pid);
            var time = DateTimeOffset.Now;
            var lang = PdsRegistry.SupportedLanguages[model.Language];
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
                    var s = Context.Add(sub);
                    await Context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    err.AppendLine($"Error importing `{item.FileName}`, unexpected exception.");
                    Logger.LogError(ex, $"Error importing `{item.FileName}`.");
                }
            }

            err.Append("Import finished.");
            StatusMessage = err.ToString();
            SubmissionTokenizeService.Notify();
            return RedirectToAction(nameof(Detail), new { pid });
        }


        [HttpGet("[action]")]
        public IActionResult Refresh()
        {
            return AskPost(
                title: "Plagiarism service",
                message: "If the service isn't running, use this function to notify.",
                area: null, ctrl: "Plagiarism", act: "Refresh", new { }, MessageType.Danger);
        }


        [HttpPost("[action]")]
        public IActionResult Refresh(bool post = true)
        {
            ReportGenerationService.Notify();
            SubmissionTokenizeService.Notify();
            return RedirectToAction(nameof(List));
        }


        [HttpGet("submit/{sid}")]
        public async Task<IActionResult> Submission(int sid)
        {
            var ss = await Context.Submissions
                .Where(s => s.Id == sid)
                .SingleOrDefaultAsync();
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
            return View(ss);
        }


        [HttpGet("submit/{sid}/[action]")]
        public async Task<IActionResult> SourceCode(int sid)
        {
            var ss = await Context.Submissions
                .Where(s => s.Id == sid)
                .Include(s => s.Files)
                .SingleOrDefaultAsync();
            if (ss == null) return NotFound();
            return View(ss);
        }


        [HttpGet("report/{rid}")]
        public async Task<IActionResult> Compare(Guid rid)
        {
            var report = await Context.Reports
                .Where(r => r.Id == rid)
                .SingleOrDefaultAsync();
            if (report == null) return NotFound();
            ViewBag.Report = report;

            var sids = new[] { report.SubmissionA, report.SubmissionB };
            var submissions = await Context.Submissions
                .Where(s => sids.Contains(s.Id))
                .Include(s => s.Files)
                .ToListAsync();

            if (submissions.Count != 2)
                throw new InvalidOperationException();
            var (subA, subB) = (submissions[0], submissions[1]);
            if (subA.Id != report.SubmissionA)
                (subA, subB) = (subB, subA);

            var retA = CodeModel.CreateView(report, c => c.FileA, c => c.ContentStartA, c => c.ContentEndA, subA);
            var retB = CodeModel.CreateView(report, c => c.FileB, c => c.ContentStartB, c => c.ContentEndB, subB);
            return View((retA, retB));
        }
    }
}
