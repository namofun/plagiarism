using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.PlagModule.Models
{
    public class CodeModel
    {
        public string Sid { get; set; }

        public List<CodeFile> Files { get; set; }

        public static CodeModel CreateView(
            PlagiarismReport report,
            Func<MatchPair, int> fileId,
            Func<MatchPair, int> contentStart,
            Func<MatchPair, int> contentEnd,
            PlagiarismSubmission sub)
        {
            if (report.Matches == null || report.Matches.Length == 0)
            {
                return new CodeModel
                {
                    Sid = $"{sub.Name} (s{sub.Id})",
                    Files = sub.Files
                        .Select(i => new CodeFile
                        {
                            FilePath = i.FilePath,
                            Content = i.Content,
                            Code = new List<CodeChar>()
                        })
                        .ToList()
                };
            }

            var rep = PdsRegistry.Deserialize(report.Matches);
            var files = rep.OrderBy(contentStart).ToLookup(fileId);

            CodeFile CreateFromGroup(IGrouping<int, MatchPair> f)
            {
                var ff = sub.Files.First(i => i.FileId == f.Key);
                var bound = new SortedSet<Boundary>();

                foreach (var mp in f)
                {
                    bound.Add(new Boundary(mp.MatchingId, contentStart(mp)));
                    bound.Add(new Boundary(mp.MatchingId, contentEnd(mp)));
                }

                var to = new HashSet<Boundary>();
                var cur = new List<CodeChar>();
                int begin = 0;

                foreach (var bd in bound)
                {
                    bool remove = false;

                    var cc = new CodeChar
                    {
                        Begin = begin,
                        End = bd.Index,
                        Marks = new List<int>(to.Select(a => a.MatchingId))
                    };

                    if (cc.Begin != cc.End - 1) cur.Add(cc);

                    begin = bd.Index;
                    var tmp = to.Where(a => a.MatchingId == bd.MatchingId);

                    if (tmp.Count() != 0)
                    {
                        to.Remove(tmp.First());
                        remove = true;
                    }
                    
                    if (!remove) to.Add(bd);
                }

                return new CodeFile()
                {
                    FilePath = ff.FilePath,
                    Content = ff.Content,
                    Code = cur
                };
            }

            return new CodeModel
            {
                Sid = $"{sub.Name} (s{sub.Id})",
                Files = files.Select(CreateFromGroup).ToList()
            };
        }

        public class CodeFile
        {
            public string FilePath { get; set; }
            public string Content { get; set; }
            public List<CodeChar> Code { get; set; }
        }

        public class CodeChar
        {
            public int Begin { get; set; }
            public int End { get; set; }
            public List<int> Marks { get; set; }
        }

        public class Boundary : IComparable<Boundary>
        {
            public int MatchingId { get; set; }
            public int Index { get; set; }

            public int CompareTo(Boundary other)
            {
                if (Index != other.Index)
                    return Index.CompareTo(other.Index);
                return MatchingId.CompareTo(other.MatchingId);
            }

            public Boundary(int matchingId, int index)
            {
                Index = index;
                MatchingId = matchingId;
            }
        }
    }
}
