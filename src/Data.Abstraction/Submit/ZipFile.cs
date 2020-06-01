using System;
using System.Collections.Generic;
using System.Text;

namespace SatelliteSite.Data.Submit
{
    public class ZipFile
    {
        public string FilePath { set; get; }
        public string FileName { set; get; }
        public ICollection<File> Files { get; set; }
    }
}
