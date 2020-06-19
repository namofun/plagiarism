using Plag;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.Models
{
    public class CodeModel
    {
        public string Sid { set; get; }
        public List<CodeFile> Files { set; get; }
    }
    public class CodeFile
    {
        public string FilePath { set; get; }
        public string Content { set; get; }
        public List<CodeChar> Code { set; get; }
    }
    public class CodeChar
    {
        public int Begin { set; get; }
        public int End { set; get; }
        public List<int> Marks { set; get; }
    }
    public class Boundary:IComparable<Boundary>
    {
        //对应的matchpairID
        public int MatchingId { set; get; }
        //对应的编号
        public int index { set; get; }

        public int CompareTo(Boundary other)
        {
            if(this.index != other.index)
            return this.index - other.index;
            return this.MatchingId - other.MatchingId;
        }

        public Boundary(int MatchingId,int index)
        {
            this.index = index;
            this.MatchingId = MatchingId;
        }
    }
}
