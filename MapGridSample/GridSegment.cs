using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGridSample
{
    public struct GridSegment
    {
        private GridPoint _point1;
        private GridPoint _point2;

        public GridPoint Point1 { get { return _point1; } set { _point1 = value; } }
        public GridPoint Point2 { get { return _point2; } set { _point2 = value; } }
        public double X1 { get { return _point1.X; } set { _point1.X = value; } }
        public double X2 { get { return _point2.X; } set { _point2.X = value; } }
        public double Y1 { get { return _point1.Y; } set { _point1.Y = value; } }
        public double Y2 { get { return _point2.Y; } set { _point2.Y = value; } }

        public GridSegment(GridPoint gridPoint1, GridPoint gridPoint2)
        {
            _point1 = gridPoint1;
            _point2 = gridPoint2;
        }

        public GridSegment(double x1, double y1, double x2, double y2)
            : this()
        {
            _point1.X = x1;
            _point1.Y = y1;
            _point2.X = x2;
            _point2.Y = y2;
        }
    }
}
