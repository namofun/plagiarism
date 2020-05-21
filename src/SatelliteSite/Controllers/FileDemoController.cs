using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Plag;
using System.IO.Compression;

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
        public IActionResult Compair()
        {
            var dir = Directory.GetFiles(Directory.GetCurrentDirectory() + "/wwwroot/file/");

            string[] submit = new string[2];int i = 0;
            foreach(var f in dir)
            {
                using (ZipArchive archive = ZipFile.OpenRead(f))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                        {
                            using (StreamReader writer = new StreamReader(entry.Open()))
                            {
                                submit[i++] = writer.ReadToEnd();
                            }
                        }
                    }
                }
            }
            
            var lang = (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Csharp.Language));
            var sub1 = new Submission(lang, new SubmissionString("A.cs", submit[0]));
            var sub2 = new Submission(lang, new SubmissionString("B.cs", submit[1]));
            var compareResult = GSTiling.Compare(sub1, sub2, lang.MinimalTokenMatch);

            return View(compareResult.Percent);
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