using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGridSample
{
    /// <summary>
    /// 地理经度和纬坐标
    /// </summary>
    public struct GridPoint
    {
        private double _x;
        private double _y;

        /// <summary>
        /// 经度(东经)
        /// </summary>
        public double X { get { return _x; } set { _x = value; } }

        /// <summary>
        /// 纬度（北纬）
        /// </summary>
        public double Y { get { return _y; } set { _y = value; } }

        public GridPoint(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public static GridPoint operator -(GridPoint gridPoint1, GridPoint gridPoint2)
        {
            GridPoint result = new GridPoint();
            result.X = gridPoint1.X - gridPoint2.X;
            result.Y = gridPoint1.Y - gridPoint2.Y;
            return result;
        }

        public static GridPoint operator +(GridPoint gridPoint1, GridPoint gridPoint2)
        {
            GridPoint result = new GridPoint();
            result.X = gridPoint1.X + gridPoint2.X;
            result.Y = gridPoint1.Y + gridPoint2.Y;
            return result;
        }

        public int TranformGridIndex(int levelGrid, double levelXCellSpan, double levelYCellSpan)
        {
            int row = (int)(Math.Floor(Y / levelYCellSpan) % levelGrid);
            int column = (int)(Math.Floor(X / levelXCellSpan) % levelGrid);
            return (row * levelGrid + column + 1);
        }
    }
}
