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

namespace SatelliteSite.Controllers
{
    public class FileDemoController : Controller2
    {
        
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