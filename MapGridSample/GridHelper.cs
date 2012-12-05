using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MapGridSample
{
    public static class GridHelper
    {
        #region 获取点的空间索引

        public static GridIndex GetPointGridIndex(Positioning positioning)
        {
            GridIndex result = new GridIndex();
            result.Index1 = positioning.TranformGridIndex(GridConfig.LEVEL1_GRID, GridConfig.Level1LongitudeCellSpan, GridConfig.Level1LatitudeCellSpan);
            result.Index2 = positioning.TranformGridIndex(GridConfig.LEVEL2_GRID, GridConfig.Level2LongitudeCellSpan, GridConfig.Level2LatitudeCellSpan);
            result.Index3 = positioning.TranformGridIndex(GridConfig.LEVEL3_GRID, GridConfig.Level3LongitudeCellSpan, GridConfig.Level3LatitudeCellSpan);
            result.Index4 = positioning.TranformGridIndex(GridConfig.LEVEL4_GRID, GridConfig.Level4LongitudeCellSpan, GridConfig.Level4LatitudeCellSpan);
            return result;
        }

        #endregion

        #region 获取线的空间索引

        //private static IEnumerable<Point> GetPolygonRegion(PointCollection points)
        //{
        //    var maxX = Math.Floor(points.Max(p => p.X) / GRID_SIZE);
        //    var maxY = Math.Floor(points.Max(p => p.Y) / GRID_SIZE);
        //    var minX = Math.Floor(points.Min(p => p.X) / GRID_SIZE);
        //    var minY = Math.Floor(points.Min(p => p.Y) / GRID_SIZE);

        //    for (double x = minX; x <= maxX; x++)
        //    {
        //        for (double y = minY; y <= maxY; y++)
        //        {
        //            yield return new Point(x, y);
        //        }
        //    }
        //}

        //private static IEnumerable<Point> GetPolyLineRegion(PointCollection points)
        //{
        //    var result = new List<Point>();
        //    var prev = points.First();
        //    for (int i = 1; i < points.Count; i++)
        //    {
        //        result.AddRange(GetLineRegion(prev, points[i]));
        //        prev = points[i];
        //    }
        //    return result.Distinct();
        //}

        //private static IEnumerable<Point> GetLineRegion(Point startPoint, Point endPoint)
        //{
        //    //  一次函数公式: kx + b = y
        //    var k = (startPoint.Y - endPoint.Y) / (startPoint.X - endPoint.X);
        //    var b = startPoint.Y - k * startPoint.X;

        //    if (!double.IsInfinity(k))
        //    {
        //        var maxX = Math.Floor(Math.Max(startPoint.X, endPoint.X) / GRID_SIZE);
        //        var minX = Math.Ceiling(Math.Min(startPoint.X, endPoint.X) / GRID_SIZE);
        //        for (double x = minX; x <= maxX; x++)
        //        {
        //            var y = Math.Floor((k * x * GRID_SIZE + b) / GRID_SIZE);
        //            yield return new Point(x, y);
        //            yield return new Point(x - 1, y);
        //        }
        //    }

        //    if (k != 0)
        //    {
        //        var maxY = Math.Floor((Math.Max(startPoint.Y, endPoint.Y) / GRID_SIZE));
        //        var minY = Math.Ceiling(Math.Min(startPoint.Y, endPoint.Y) / GRID_SIZE);
        //        var isInfinity = double.IsInfinity(k);
        //        var fixedX = Math.Floor(endPoint.X / GRID_SIZE);

        //        for (double y = minY; y <= maxY; y++)
        //        {
        //            var x = isInfinity ? fixedX : Math.Floor((y * GRID_SIZE - b) / (k * GRID_SIZE));
        //            yield return new Point(x, y);
        //            yield return new Point(x, y - 1);
        //        }
        //    }

        //    yield return new Point(Math.Floor(startPoint.X / GRID_SIZE), Math.Floor(startPoint.Y / GRID_SIZE));
        //    yield return new Point(Math.Floor(endPoint.X / GRID_SIZE), Math.Floor(endPoint.Y / GRID_SIZE));
        //}

        #endregion
    }
}
