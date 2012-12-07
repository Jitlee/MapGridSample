using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGridSample
{
    public struct GridSegment
    {
        private double _x1;
        private double _x2;
        private double _y1;
        private double _y2;

        public double X1 { get { return _x1; } set { _x1 = value; } }
        public double X2 { get { return _x2; } set { _x2 = value; } }
        public double Y1 { get { return _y1; } set { _y1 = value; } }
        public double Y2 { get { return _y2; } set { _y2 = value; } }

        public GridSegment(GridPoint gridPoint1, GridPoint gridPoint2)
        {
            _x1 = gridPoint1.X;
            _y1 = gridPoint1.Y;
            _x2 = gridPoint2.X;
            _y2 = gridPoint2.Y;
        }

        public GridSegment(double x1, double y1, double x2, double y2)
        {
            _x1 = x1;
            _y1 = y1;
            _x2 = x2;
            _y2 = y2;
        }
    }
}
