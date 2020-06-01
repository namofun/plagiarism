using System;
using System.Collections.Generic;
using System.Text;

namespace SatelliteSite.Data.Submit
{
    public class Token
    {
        public Token(int type, int line, int column = -1, int length = -1)
        {
            Type = type;
            Line = line > 0 ? line : 1;
            Column = column;
            Length = length;
        }
        public int Type { get;  set; }
        public int Line { get;  set; }
        public int Column { get;  set; }
        public int Length { get;  set; }

    }
}
