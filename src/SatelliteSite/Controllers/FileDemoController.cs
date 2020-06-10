using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Plag;
using System.IO.Compression;
using SatelliteSite.Data.Match;
using SatelliteSite.Data;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.Models;

namespace SatelliteSite.Controllers
{
    public class FileDemoController : Controller2
    {
        private DemoContext Context { get; }

        private DataUtil Util { get; }
        public FileDemoController(DemoContext context, DataUtil util)
        {
            Context = context;
            Util = util;
        }
        public IActionResult Index()
        {
            ViewData["FileList"] = Directory.GetFiles(Directory.GetCurrentDirectory() + "/wwwroot/file/");
            return View();
        }
        public IActionResult List()
        {
            return View();
        }
        public IActionResult Compare()
        {
            var dir = Directory.GetFiles(Directory.GetCurrentDirectory() + "/wwwroot/file/");
            //TO DO 语言选择 文件选择
            var lang = (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Csharp.Language));
            var zip1 = System.IO.Compression.ZipFile.OpenRead(dir[0]);
            var zip2 = System.IO.Compression.ZipFile.OpenRead(dir[2]);

            var submission1 = new Plag.Submission(lang, new SubmissionZipArchive(zip1, lang.Suffixes));
            var submission2 = new Plag.Submission(lang, new SubmissionZipArchive(zip2, lang.Suffixes));

            var tp1 = new Data.Submit.Submission()
            {
                Id = Guid.NewGuid().ToString(),
                Files = zip1.Entries.Select(i => new Data.Submit.File
                {
                    FileName = i.Name,
                    FilePath = i.FullName,
                    Content = new StreamReader(i.Open()).ReadToEnd()
                }).ToList()
            };
            var tp2 = new Data.Submit.Submission()
            {
                Id = Guid.NewGuid().ToString(),
                Files = zip2.Entries.Select(i => new Data.Submit.File
                {
                    FileName = i.Name,
                    FilePath = i.FullName,
                    Content = new StreamReader(i.Open()).ReadToEnd()
                }).ToList()
            };
            if (submission1.IL.Size > submission2.IL.Size) (tp1, tp2) = (tp2, tp1);
            var compareResult = GSTiling.Compare(submission1, submission2, lang.MinimalTokenMatch);
            ViewBag.Content1 = tp1.Files.First().Content;
            ViewBag.Content2 = tp2.Files.First().Content;
            Report res = Report.Create(compareResult);
            ViewBag.Result = res;
            return View();
        }

        public static List<(T,T)> C<T>(List<T> ori)
        {
            var ssd = new List<T>(ori);
            var res = new List<(T,T)>();
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
        public async Task<IActionResult> CompareX()
        {
            var myid = "6b9e72ee-293f-4852-9806-897c56097aef";
            var sid = new List<string>()
            {
                "6b9e72ee-293f-4852-9806-897c56097aef",
                "72fe55e4-55da-46b8-9cdf-65c1bceb57af"
            };

            var test = C(sid);

            var subs = await Context.Submissions
                .AsNoTracking()
                .Where(s => sid.Contains(s.Id))
                .ToListAsync();

            var lang = (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Csharp.Language));
            var submissions = subs.Select(i => new Submission(
                lang,
                new SubmissionFileProxy(i),
                i.Id,
                i.Tokens.Select(j => lang.CreateToken(j.Type, j.Line, j.Column, j.Length, j.FileId))
                )).ToList();
            
            var Results = test.Select(i => Report.Create(GSTiling.Compare(
                submissions.Where(j => j.Id == i.Item1).First(),
                submissions.Where(j => j.Id == i.Item2).First(),
                lang.MinimalTokenMatch)));
            ViewBag.myid = myid;

            var res = Results.Where(i => i.SubmissionA == myid || i.SubmissionB == myid).ToList();

            ViewBag.Report = res;


            var ret = new List<(CodeModel, CodeModel)>();
            foreach(var k in res)
            {
                var subA = subs.Where(o => o.Id == k.SubmissionA).First();
                var FilesA = from f in k.MatchPairs
                             orderby f.ContentStartA
                             group f by f.FileA;
                CodeModel retA = new CodeModel()
                {
                    Sid = k.SubmissionA,
                    Files = FilesA.Select(f =>
                    {
                        var ff = subA.Files.Where(i => i.FileId == f.Key).First();
                       // var str = ff.Content.Select(i => new CodeChar() { Content = i, Marks = new List<int>() }).ToList();
                        foreach (var i in f)
                        {
                            for (int p = i.ContentStartA; p < i.ContentEndA; p++)
                            {
                                //str[p].Marks.Add(i.Mid);
                            }
                        }
                        return new CodeFile()
                        {
                            FilePath = ff.FilePath,
                            //Code = str
                        };
                    }).ToList()
                };
                var subB = subs.Where(o => o.Id == k.SubmissionB).First();
                var FilesB = from f in k.MatchPairs
                             orderby f.ContentStartB
                             group f by f.FileB;
                CodeModel retB = new CodeModel()
                {
                    Sid = k.SubmissionB,
                    Files = FilesB.Select(f =>
                    {
                        var ff = subB.Files.Where(i => i.FileId == f.Key).First();
                       // var str = ff.Content.Select(i => new CodeChar() { Content = i, Marks = new List<int>() }).ToList();
                        foreach (var i in f)
                        {
                            for (int p = i.ContentStartB; p < i.ContentEndB; p++)
                            {
                                //str[p].Marks.Add(i.Mid);
                            }
                        }
                        return new CodeFile()
                        {
                            FilePath = ff.FilePath,
                           // Code = str
                        };
                    }).ToList()
                };
                ret.Add((retA, retB));

            }
            return View(ret);
        }
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile FileUpload)
        {
            if (FileUpload == null) return BadRequest();

            var filePath = Path.GetTempFileName();

            var wpath = Directory.GetCurrentDirectory() + "/wwwroot/file/" + FileUpload.FileName;

            using (var stream = System.IO.File.Create(wpath))
            {
                await FileUpload.CopyToAsync(stream);
            }

            StatusMessage = "File Upload Succeed.";

            ViewData["FileList"] = Directory.GetFiles(Directory.GetCurrentDirectory() + "/wwwroot/file/");
            return View(nameof(Index));
        }
    }
}