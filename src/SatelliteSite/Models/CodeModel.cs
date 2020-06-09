using Plag;
using System;
using System.Collections.Generic;
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
        public List<CodeChar> Code { set; get; }
    }
    public class CodeChar
    {
        public char Content { set; get; }
        public List<int> Marks { set; get; }
    }
}
