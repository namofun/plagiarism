using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq;
using System;
using Plag;
using System.Threading.Tasks;

namespace SatelliteSite.Data
{
    public class DataUtil
    {
        private DemoContext Context { get; }

        public DataUtil(DemoContext context)
        {
            Context = context;
        }
        public async Task<Submit.Submission> StoreSubmission(ZipArchive zip)
        {
            var sub =  new Submit.Submission() {
                Id = Guid.NewGuid().ToString(),
                Files = zip.Entries.Select(i => new Submit.File
                {
                    FileName = i.Name,
                    FilePath = i.FullName,
                    Content = new StreamReader(i.Open()).ReadToEnd()
                }).ToList()
            };

            Context.Add(sub);
            await Context.SaveChangesAsync();
            return sub;
        }
        public async Task<int> StoreTokens(Submit.Submission sub,Plag.Submission submission)
        {
            sub.Tokens = new List<Submit.Token>();
            for (var i = 0; i < submission.IL.Size; i++)
            {
                sub.Tokens.Add(submission.IL[i]);
            }
            sub.Language = submission.Language.Name;

            Context.Update(sub);
            return await Context.SaveChangesAsync();
        }
    }
}
