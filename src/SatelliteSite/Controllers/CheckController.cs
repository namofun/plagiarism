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
using SatelliteSite.Data.Check;

namespace SatelliteSite.Controllers
{
    [Route("[controller]/[action]")]
    public class CheckController : Controller2
    {
        private DemoContext Context { get; }

        private DataUtil Util { get; }
        public CheckController(DemoContext context,DataUtil util)
        {
            Context = context;
            Util = util;
        }

        [HttpGet("/[controller]")]
        public async Task<IActionResult> List()
        {
            var lst = await Context.CheckSets
                .AsNoTracking()
                .Select(s => new CheckListModel
                {
                    Id = s.Id,
                    CreateTime = s.CreateTime
                })
                .ToListAsync();

            return View(lst);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var teams = Context.Submissions.AsNoTracking()
                .Select(i => new TeamReport() { 
                    SubmissionA = i.Id,
                    AuthorName = i.StuName,
                }).ToList();
            //TO DO BackgroundTask
            //Parallel.ForEach(teams, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            //    i => Util.CreateTeamReport(i)
            //);
            foreach(var i in teams)
            {
                Util.CreateTeamReport(i);
            }
            //--------------
            var checkset = new CheckSet()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTimeOffset.Now,
                Teams = teams,
            };
            Context.CheckSets.Add(checkset);
            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var checkSet = await Context.CheckSets.ToListAsync();
            
            if (checkSet.Count > 0)
            {
                var item = checkSet[new Random().Next(0, checkSet.Count)];
                Context.CheckSets.Remove(item);
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
            var checkSet = await Context.CheckSets.ToListAsync();

            var item = checkSet.Where(i => i.Id == id).ToList();
            if (item.Count > 0) 
            { 
                Context.CheckSets.Remove(item.First());
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
            var checkSet = Context.CheckSets.AsNoTracking().Where(o => o.Id == id).First();
            ViewBag.Id = checkSet.Id;
            ViewBag.CreateTime = checkSet.CreateTime;

            return View(checkSet.Teams);
        }


    }
}
