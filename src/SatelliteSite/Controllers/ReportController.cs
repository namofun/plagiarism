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
    public class ReportController : Controller2
    {
        private DemoContext Context { get; }

        private DataUtil Util { get; }
        public ReportController(DemoContext context,DataUtil util)
        {
            Context = context;
            Util = util;
        }

        [HttpGet("/[controller]")]
        public async Task<IActionResult> List()
        {
            var lst = await Context.Reports
                .AsNoTracking()
                .Select(s => new ReportListModel
                {
                    Id = s.Id,
                    SubmissionA = s.SubmissionA,
                    SubmissionB = s.SubmissionB,
                    Percent = s.Percent
                })
                .ToListAsync();

            return View(lst);
        }

        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var reports = await Context.Reports.ToListAsync();
            
            if (reports.Count > 0)
            {
                var item = reports[new Random().Next(0, reports.Count)];
                Context.Reports.Remove(item);
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
        public async Task<IActionResult> Delete(string id)
        {
            var reports = await Context.Reports.ToListAsync();

            var item = reports.Where(i => i.Id == id).ToList();
            if (item.Count > 0) 
            { 
                Context.Reports.Remove(item.First());
                await Context.SaveChangesAsync();
                StatusMessage = "Item remove.";
            }
            else
            {
                StatusMessage = "Error Not Exist.";
            }
            return RedirectToAction(nameof(List));
        }

        [HttpGet("/[controller]/{id}")]
        public IActionResult Detail(string id)
        {
            var Result = Context.Reports.AsNoTracking().Where(o => o.Id == id).First();
            ViewBag.Report = Result;

            var subA = Context.Submissions.AsNoTracking().Where(o => o.Id == Result.SubmissionA).First();
            var subB = Context.Submissions.AsNoTracking().Where(o => o.Id == Result.SubmissionB).First();

            var res = ModelUtil.CreateCodeModel(Result, subA, subB);
            return View(res);
        }


    }
}
