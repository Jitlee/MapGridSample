using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGridSample
{
    public struct GridCell
    {
        private int _row;
        private int _column;
        public int Row { get { return _row; } set { _row = value; } }
        public int Column { get { return _column; } set { _column = value; } }
        public GridCell(int row, int column)
        {
            _row = row;
            _column = column;
        }
    }
}
