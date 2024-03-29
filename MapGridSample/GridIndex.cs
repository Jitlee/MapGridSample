﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGridSample
{
    public struct GridIndex
    {
        public int Index1 { get; set; }

        public int? Index2 { get; set; }

        public int? Index3 { get; set; }

        public int? Index4 { get; set; }

        public override string ToString()
        {
            return string.Format("({0},{1},{2},{3})", Index1, Index2.HasValue ? Index2.Value.ToString() : "NULL", Index3.HasValue ? Index3.Value.ToString() : "NULL", Index4.HasValue ? Index4.Value.ToString() : "NULL");
        }
    }
}
