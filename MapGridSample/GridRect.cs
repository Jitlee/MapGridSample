using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGridSample
{
    public struct GridRect
    {
        private GridPoint _point1;
        private GridPoint _point2;
        private GridPoint _point3;
        private GridPoint _point4;
        public GridPoint Point1 { get { return _point1; } set { _point1 = value; } }
        public GridPoint Point2 { get { return _point2; } set { _point2 = value; } }
        public GridPoint Point3 { get { return _point3; } set { _point3 = value; } }
        public GridPoint Point4 { get { return _point4; } set { _point4 = value; } }

        public GridRect(GridPoint location, double width, double height)
        {
            _point1 = location;
            _point2 = new GridPoint(location.X + width, location.Y);
            _point3 = new GridPoint(location.X, location.Y + height);
            _point4 = new GridPoint(location.X + width, location.Y + height);
        }

        public GridRect(double x, double y, double width, double height)
        {
            _point1 = new GridPoint(x, y);
            _point2 = new GridPoint(x + width, y);
            _point3 = new GridPoint(x, y + height);
            _point4 = new GridPoint(x + width, y + height);
        }
    }
}