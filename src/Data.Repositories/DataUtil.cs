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
        public async Task<Submit.Submission> StoreSubmission(string uid,ZipArchive zip,ILanguage lang)
        {
            var t = 0;
            var sub =  new Submit.Submission() {
                Uid = uid,
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
    }
}
