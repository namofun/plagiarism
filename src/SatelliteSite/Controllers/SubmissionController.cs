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
using SatelliteSite.Utils;

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
                    Uid = s.Uid,
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
        public async Task<IActionResult> Delete(string sid)
        {
            var submissions = await Context.Submissions.ToListAsync();

            var item = submissions.Where(i => i.Id == sid).ToList();
            if (item.Count > 0) 
            { 
                Context.Submissions.Remove(item.First());
                await Context.SaveChangesAsync();
                StatusMessage = "Item remove.";
            }
            else
            {
                StatusMessage = "Error Not Exist.";
            }
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Upload(string uid,string lang, IFormFile FileUpload)
        {
            if (FileUpload == null) return BadRequest();
            //TO DO 语言选择
            var language = lang switch
            {
                "C#8" => (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Csharp.Language)),
                "C++14" => (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Cpp.Language)),
            };
            var zip = new ZipArchive(FileUpload.OpenReadStream(), ZipArchiveMode.Read);

            var task = Util.StoreSubmission(uid,zip,language);

            var submission = new Plag.Submission(language, new SubmissionZipArchive(zip, language.Suffixes));

            var sub = await task;

            await Util.StoreTokens(sub,submission);

            return RedirectToAction(sub.Id);
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


        public static List<(T, T)> C<T>(List<T> ori)
        {
            var ssd = new List<T>(ori);
            var res = new List<(T, T)>();
            foreach (var i in ori)
            {
                ssd.Remove(i);
                foreach (var j in ssd)
                {
                    res.Add((i, j));
                }
            }
            return res;
        }
        [HttpPost]
        public async Task<IActionResult> Compare(string sub1, string sub2)
        {
            var myid = sub1;

            //var test = C(sid);

            var Result = await Util.GetReportAsync(sub1, sub2);

            ViewBag.myid = myid;
            ViewBag.Report = Result;

            var subA = Context.Submissions.AsNoTracking().Where(o => o.Id == Result.SubmissionA).First();
            var subB = Context.Submissions.AsNoTracking().Where(o => o.Id == Result.SubmissionB).First();

            var res = ModelUtil.CreateCodeModel(Result, subA, subB);
            return View(res);
        }
    }
}
