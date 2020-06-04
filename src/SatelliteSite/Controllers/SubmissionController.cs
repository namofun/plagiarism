using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plag;
using SatelliteSite.Data;
using SatelliteSite.Data.Submit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;
using SatelliteSite.Models;

namespace SatelliteSite.Controllers
{
    [Route("[controller]/[action]")]
    public class SubmissionController : Controller2
    {
        private DemoContext Context { get; }

        private DataUtil Util { get; }
        public SubmissionController(DemoContext context,DataUtil util)
        {
            Context = context;
            Util = util;
        }

        [HttpGet("/[controller]")]
        public async Task<IActionResult> List()
        {
            var lst = await Context.Submissions
                .AsNoTracking()
                .Select(s => new SubmissionListModel
                {
                    Id = s.Id,
                    Language = s.Language
                })
                .ToListAsync();

            return View(lst);
        }

        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var submissions = await Context.Submissions.ToListAsync();
            
            if (submissions.Count > 0)
            {
                var item = submissions[new Random().Next(0, submissions.Count)];
                Context.Submissions.Remove(item);
                await Context.SaveChangesAsync();
                StatusMessage = "Item remove.";
            }
            else
            {
                StatusMessage = "Error Empty.";
            }

            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile FileUpload)
        {
            if (FileUpload == null) return BadRequest();
            //TO DO 语言选择
            var lang = (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Csharp.Language));
            var zip = new ZipArchive(FileUpload.OpenReadStream(), ZipArchiveMode.Read);

            var task = Util.StoreSubmission(zip);

            var submission = new Plag.Submission(lang, new SubmissionZipArchive(zip, lang.Suffixes));

            var sub = await task;

            await Util.StoreTokens(sub,submission);

            StatusMessage = "File Upload Succeed.";
            return RedirectToAction(nameof(List));
        }

        [HttpGet("/[controller]/{sid}")]
        public async Task<IActionResult> Detail(string sid)
        {
            var ss = await Context.Submissions
                .AsNoTracking()
                .Where(s => s.Id == sid)
                .Select(s => new { s.Files, s.Language })
                .SingleOrDefaultAsync();

            if (ss == null) return NotFound();
            ViewBag.Language = ss.Language;
            ViewBag.Id = sid;
            return View(ss.Files);
        }
    }
}
