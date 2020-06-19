using SatelliteSite.Data.Match;
using SatelliteSite.Data.Submit;
using SatelliteSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.Utils
{
    public class ModelUtil
    {
        public static (CodeModel,CodeModel) CreateCodeModel(Report report,Submission subA,Submission subB)
        {
            
            var FilesA = from f in report.MatchPairs
                         orderby f.ContentStartA
                         group f by f.FileA;
            CodeModel retA = new CodeModel()
            {
                Sid = report.SubmissionA,
                Files = FilesA.Select(f =>
                {
                    var ff = subA.Files.Where(i => i.FileId == f.Key).First();

                    var matchpair = from m in report.MatchPairs
                                    where m.FileA == f.Key
                                    select m;
                    SortedSet<Boundary> bound = new SortedSet<Boundary>();
                    foreach (var mp in matchpair)
                    {
                        Boundary b1 = new Boundary(mp.MatchingId, mp.ContentStartA);
                        Boundary b2 = new Boundary(mp.MatchingId, mp.ContentEndA);
                        bound.Add(b1);
                        bound.Add(b2);
                    }
                    HashSet<Boundary> to = new HashSet<Boundary>();
                    List<CodeChar> cur = new List<CodeChar>();
                    int begin = 0;
                    foreach (var bd in bound)
                    {
                        bool remove = false;
                        CodeChar cc = new CodeChar();
                        cc.Begin = begin;
                        cc.End = bd.index;
                        cc.Marks = new List<int>();
                        foreach (var a in to)
                        {
                            cc.Marks.Add(a.MatchingId);
                        }
                        if (cc.Begin != cc.End - 1)
                        {
                            cur.Add(cc);
                            Console.WriteLine(cc.Begin);
                            Console.WriteLine(cc.End);
                        }
                        begin = bd.index;
                        var tmp = (from a in to
                                   where a.MatchingId == bd.MatchingId
                                   select a);
                        if (tmp.Count() != 0)
                        {
                            to.Remove(tmp.First());
                            remove = true;
                        }
                        if (!remove) to.Add(bd);

                    }
                    //cur.RemoveAt(0);
                    return new CodeFile()
                    {
                        FilePath = ff.FilePath,
                        Content = ff.Content,
                        Code = cur
                    };
                }).ToList()
            };
            var FilesB = from f in report.MatchPairs
                         orderby f.ContentStartB
                         group f by f.FileB;
            CodeModel retB = new CodeModel()
            {
                Sid = report.SubmissionB,
                Files = FilesB.Select(f =>
                {
                    var ff = subB.Files.Where(i => i.FileId == f.Key).First();
                    var matchpair = from m in report.MatchPairs
                                    where m.FileB == f.Key
                                    select m;
                    SortedSet<Boundary> bound = new SortedSet<Boundary>();
                    foreach (var mp in matchpair)
                    {
                        Boundary b1 = new Boundary(mp.MatchingId, mp.ContentStartB);
                        Boundary b2 = new Boundary(mp.MatchingId, mp.ContentEndB);
                        bound.Add(b1);
                        bound.Add(b2);
                    }
                    HashSet<Boundary> to = new HashSet<Boundary>();
                    List<CodeChar> cur = new List<CodeChar>();
                    int begin = 0;
                    foreach (var bd in bound)
                    {
                        bool remove = false;
                        CodeChar cc = new CodeChar();
                        cc.Begin = begin;
                        cc.End = bd.index;
                        cc.Marks = new List<int>();
                        foreach (var a in to)
                        {
                            cc.Marks.Add(a.MatchingId);
                        }
                        if (cc.Begin != cc.End - 1)
                        {
                            cur.Add(cc);
                            Console.WriteLine(cc.Begin);
                            Console.WriteLine(cc.End);
                        }
                        begin = bd.index;
                        var tmp = (from a in to
                                   where a.MatchingId == bd.MatchingId
                                   select a);
                        if (tmp.Count() != 0)
                        {
                            to.Remove(tmp.First());
                            remove = true;
                        }

                        if (!remove) to.Add(bd);

                    }
                    //cur.RemoveAt(0);
                    return new CodeFile()
                    {
                        FilePath = ff.FilePath,
                        Content = ff.Content,
                        Code = cur
                    };
                }).ToList()
            };
            return (retA, retB);
        }
    }
}
