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

namespace SatelliteSite.Controllers
{
    [Route("[controller]/[action]")]
    public class SubmissionController : Controller2
    {
        private DemoContext Context { get; }

        public SubmissionController(DemoContext context)
        {
            Context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var submissions = await Context.Submissions.ToListAsync();
            return View("List", submissions.PrettyJson());
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var submissions = await Context.Submissions.ToListAsync();
            return View("List", submissions.PrettyJson());
        }

        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var families = await Context.Submissions.ToListAsync();
            
            if (families.Count > 0)
            {
                var item = families[new Random().Next(0, families.Count)];
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

            var filePath = Path.GetTempFileName();

            var wpath = Directory.GetCurrentDirectory() + "/wwwroot/file/" + FileUpload.FileName;

            using (var stream = System.IO.File.Create(wpath))
            {
                await FileUpload.CopyToAsync(stream);
            }


            var lang = (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Csharp.Language));
            var zip = new ZipArchive(FileUpload.OpenReadStream(), ZipArchiveMode.Read);
            var submission = new Plag.Submission(lang, new SubmissionZipArchive(zip,lang.Suffixes));

            var tp = new List<Data.Submit.File>();
            foreach(var i in zip.Entries)
            {
                string con;
                using (StreamReader writer = new StreamReader(i.Open()))
                {
                     con = writer.ReadToEnd();
                }
                tp.Add(new Data.Submit.File
                {
                    FileName = i.Name,
                    FilePath = i.FullName,
                    Content = con
                });
            }

            var tok = new List<Data.Submit.Token>();
            for(var i = 0;i<submission.IL.Size;i++)
            {
                tok.Add(new Data.Submit.Token(submission.IL[i].Type, submission.IL[i].Line, submission.IL[i].Column, submission.IL[i].Length));
            }

            var sub = new Data.Submit.Submission
            {
                Id = Guid.NewGuid().ToString(),
                Files = tp,
                Language = submission.Language.Name,
                Tokens = tok
            };
            var item = Context.Submissions.Add(sub);
            await Context.SaveChangesAsync();
            StatusMessage = "File Upload Succeed.";
            return RedirectToAction(nameof(List));
        }
    }
}
