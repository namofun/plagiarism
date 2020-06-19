using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq;
using Plag;
using System.Threading.Tasks;
using SatelliteSite.Data.Match;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.Data.Check;

namespace SatelliteSite.Data
{
    public class DataUtil
    {
        private DemoContext Context { get; }

        public DataUtil(DemoContext context)
        {
            Context = context;
        }
        public async Task<Submit.Submission> StoreSubmission(string stuName,DateTimeOffset time, ZipArchive zip,ILanguage lang)
        {
            var t = 0;
            var sub =  new Submit.Submission() {
                StuName = stuName,
                UploadTime = time,
                Id = Guid.NewGuid().ToString(),
                Files = zip.Entries.Where(
                    i => {
                        foreach (var suffix in lang.Suffixes)
                        {
                            if (i.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                        return false;
                    }
                ).Select(i => new Submit.File
                    {
                        FileId = ++t,
                        FileName = i.Name,
                        FilePath = i.FullName,
                        Content = new StreamReader(i.Open()).ReadToEnd()
                    }
                ).ToList()
            };
            sub.Language = lang.Name;

            Context.Add(sub);
            await Context.SaveChangesAsync();
            return sub;
        }

        public void CreateTeamReport(TeamReport teamReport)
        {
            var sub_A_id = teamReport.SubmissionA;
            var res = Context.Reports.AsNoTracking().Where(i => i.SubmissionA == sub_A_id || i.SubmissionB == sub_A_id)
                .Select(i => new MatchReport()
                {
                    ReportId = i.Id,
                    SubmissionB = i.SubmissionA == sub_A_id ? i.SubmissionB : i.SubmissionA,
                    Percent = i.Percent
                }).ToList();
            teamReport.matchReports = res;
            teamReport.MaxPercent = res.Max(i => i.Percent);
            return;
        }

        public async Task<int> StoreTokens(Submit.Submission sub,Plag.Submission submission)
        {
            sub.Tokens = new List<Submit.Token>();
            for (var i = 0; i < submission.IL.Size; i++)
            {
                sub.Tokens.Add(submission.IL[i]);
            }
            
            Context.Update(sub);
            return await Context.SaveChangesAsync();
        }

        public async Task<Report> GetReportAsync(string sub1,string sub2)
        {
            Report res = null;
            var r = await Context.Reports
                .AsNoTracking()
                .Where(s => (s.SubmissionA == sub1 && s.SubmissionB == sub2) || (s.SubmissionA == sub2 && s.SubmissionB == sub1))
                .ToListAsync();
            if(r.Count!=0)
            {
                res = r.First();
                return res;
            }
            else
            {
                return await DoReport(sub1, sub2);
            }
            
        }
        
        public async Task<Report> DoReport(string sub1,string sub2)
        {
            var submissions = await Context.Submissions
                .AsNoTracking()
                .Where(s => s.Id == sub1 || s.Id == sub2)
                .ToListAsync();

            if (submissions.Count < 2 )
            {
                return null;
            }
            else if(submissions[0].Language != submissions[1].Language)
            {
                return null;
            }
            else
            {
                var lang = submissions.First().Language switch
                {
                    "C#8" => (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Csharp.Language)),
                    "C++14" => (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Cpp.Language)),
                    "Java9" => (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Java.Language)),
                    "Python3" => (ILanguage)Activator.CreateInstance(typeof(Plag.Frontend.Python.Language)),
                    _ => throw new NotImplementedException(),
                };
                var subs = submissions.Select(i => new Plag.Submission(
                lang,
                new SubmissionFileProxy(i),
                i.Id,
                i.Tokens.Select(j => lang.CreateToken(j.Type, j.Line, j.Column, j.Length, j.FileId))
                )).ToList();
                var result = Data.Match.Report.Create(GSTiling.Compare(subs[0],subs[1],lang.MinimalTokenMatch));
                
                Context.Add(result);
                await Context.SaveChangesAsync();
                return result;
            }
        }
    }
}
