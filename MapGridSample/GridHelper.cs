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

        public static GridIndex GetPointGridIndex(GridPoint gridPoint)
        {
            GridIndex result = new GridIndex();
            result.Index1 = gridPoint.TranformGridIndex(GridConfig.LEVEL1_GRID, GridConfig.Level1XCellSpan, GridConfig.Level1YCellSpan);
            result.Index2 = gridPoint.TranformGridIndex(GridConfig.LEVEL2_GRID, GridConfig.Level2XCellSpan, GridConfig.Level2YCellSpan);
            result.Index3 = gridPoint.TranformGridIndex(GridConfig.LEVEL3_GRID, GridConfig.Level3XCellSpan, GridConfig.Level3YCellSpan);
            result.Index4 = gridPoint.TranformGridIndex(GridConfig.LEVEL4_GRID, GridConfig.Level4XCellSpan, GridConfig.Level4YCellSpan);
            return result;
        }

        #endregion

        #region 获取线的空间索引

        public static IEnumerable<GridIndex> GetPolylineGrindIndex(IList<GridPoint> gridPoints)
        {
            var result = new List<GridIndex>();
            var prev = gridPoints.First();
            for (int i = 1; i < gridPoints.Count; i++)
            {
                result.AddRange(GetLineGridIndex(prev, gridPoints[i]));
                prev = gridPoints[i];
            }
            return result.Distinct();
        }

        private static IEnumerable<GridIndex> GetLineGridIndex(GridPoint startPositioning, GridPoint endPositioning)
        {
            //  一次函数公式: kx + b = y
            var k = (startPositioning.Y - endPositioning.Y) / (startPositioning.X - endPositioning.X);
            var b = startPositioning.Y - k * startPositioning.X;

            if (!double.IsInfinity(k))
            {
                var maxColumn = (int)Math.Floor(Math.Max(startPositioning.X, endPositioning.X) / GridConfig.Level4XCellSpan);
                var minColumn = (int)Math.Ceiling(Math.Min(startPositioning.X, endPositioning.X) / GridConfig.Level4XCellSpan);
                for (int column = minColumn; column <= maxColumn; column++)
                {
                    // 多加 1/2 的值是为了 避免 刚好在边界点因为小数点误差而计算错误
                    var x = (column + 0.00001d) * GridConfig.Level4XCellSpan;
                    var y = k * x + b;
                    yield return GetPointGridIndex(new GridPoint(x, y));
                    yield return GetPointGridIndex(new GridPoint(x - GridConfig.Level4XCellSpan, y));
                }
            }

            if (k != 0)
            {
                var maxRow = (int)Math.Floor((Math.Max(startPositioning.Y, endPositioning.Y) / GridConfig.Level4YCellSpan));
                var minRow = (int)Math.Ceiling(Math.Min(startPositioning.Y, endPositioning.Y) / GridConfig.Level4YCellSpan);
                var isInfinity = double.IsInfinity(k);

                for (int row = minRow; row <= maxRow; row++)
                {
                    // 多加 1/2 的值是为了 避免 刚好在边界点因为小数点误差而计算错误
                    var y = (row + 0.00001d) * GridConfig.Level4YCellSpan;
                    var x = isInfinity ? endPositioning.X : ((y - b) / k);
                    yield return GetPointGridIndex(new GridPoint(x, y));
                    yield return GetPointGridIndex(new GridPoint(x, y - GridConfig.Level4YCellSpan));
                }
            }
            yield return GetPointGridIndex(startPositioning);
            yield return GetPointGridIndex(endPositioning);
        }

        #endregion

        #region 获取面的空间索引

        public static IEnumerable<GridIndex> GetPolygonGridIndex(IList<GridPoint> gridPoints)
        {
            var result = new List<GridIndex>();
            if (gridPoints.Count == 1)
            {
                result.Add(GetPointGridIndex(gridPoints[0]));
            }
            else if (gridPoints.Count > 2)
            {
                var segments = GetPolygonSegments(gridPoints);
                var region = GetPolygonRegion(gridPoints);
                result.AddRange(GetPolygonLevel1GridIndex(region, gridPoints, segments));
            }
            return result.Distinct();
        }

        private static IEnumerable<GridSegment> GetPolygonSegments(IList<GridPoint> gridPoints)
        {
            if (gridPoints.Count > 2)
            {
                var last = gridPoints.Last();
                foreach (GridPoint gridPoint in gridPoints)
                {
                    yield return new GridSegment(last, gridPoint);
                    last = gridPoint;
                }
            }
        }

        private static GridSegment GetPolygonRegion(IList<GridPoint> gridPoints)
        {
            var xMin = gridPoints.Min(p => p.X);
            var yMin = gridPoints.Min(p => p.Y);
            var xMax = gridPoints.Max(p => p.X);
            var yMax = gridPoints.Max(p => p.Y);
            return new GridSegment(xMin, yMin, xMax, yMax);
        }

        private static List<GridIndex> GetPolygonLevel1GridIndex(GridSegment region, IList<GridPoint> gridPoints, IEnumerable<GridSegment> segments)
        {
            var result = new List<GridIndex>();
            var beginX = Math.Floor(region.X1 / GridConfig.Level1XCellSpan) * GridConfig.Level1XCellSpan;
            var beginY = Math.Floor(region.Y1 / GridConfig.Level1YCellSpan) * GridConfig.Level1YCellSpan;
            var endX = Math.Floor(region.X2 / GridConfig.Level1XCellSpan) * GridConfig.Level1XCellSpan;
            var endY = Math.Floor(region.Y2 / GridConfig.Level1YCellSpan) * GridConfig.Level1YCellSpan;

            if (beginX == endX && beginY == endY)
            {
                //result.AddRange(GetPolygonLevel2GridIndex(region, gridPoints, segments));
            }
            else
            {
                for (double x = beginX; x <= endX; x += GridConfig.Level1XCellSpan)
                {
                    for (double y = beginY; y <= endY; y += GridConfig.Level1YCellSpan)
                    {
                        var gridRect = new GridRect(x, y, GridConfig.Level1XCellSpan, GridConfig.Level1YCellSpan);
                        if (gridPoints.Any(p => RelationPointAndRect(p, gridRect) == GridRelationShip.Within))
                        {
                            // Intersect
                            result.AddRange(GetPolygonLevel2GridIndex(new GridSegment(gridRect.Point1, gridRect.Point4), gridPoints, segments));
                        }
                        else
                        {
                            var relationship = RelationRectAndPanel(gridRect, segments);
                            if (relationship == GridRelationShip.Within)
                            {
                                // Within
                                //result.Add(new GridIndex()
                                //{
                                //    Index1 = new GridPoint(x + 0.1d * GridConfig.Level1XCellSpan,
                                //            y + 0.1d * GridConfig.Level1YCellSpan)
                                //        .TranformGridIndex(GridConfig.LEVEL1_GRID,
                                //            GridConfig.Level1XCellSpan,
                                //            GridConfig.Level1YCellSpan)
                                //});
                            }
                            else if (relationship == GridRelationShip.Intersect)
                            {
                                // Intersect
                                result.AddRange(GetPolygonLevel2GridIndex(new GridSegment(gridRect.Point1, gridRect.Point4), gridPoints, segments));
                            }
                        }
                    }
                }
            }
            A:
            return result;
        }

        private static IEnumerable<GridIndex> GetPolygonLevel2GridIndex(GridSegment region, IList<GridPoint> gridPoints, IEnumerable<GridSegment> segments)
        {
            var beginX = Math.Floor(region.X1 / GridConfig.Level2XCellSpan) * GridConfig.Level2XCellSpan;
            var beginY = Math.Floor(region.Y1 / GridConfig.Level2YCellSpan) * GridConfig.Level2YCellSpan;
            var endX = Math.Floor(region.X2 / GridConfig.Level2XCellSpan) * GridConfig.Level2XCellSpan;
            var endY = Math.Floor(region.Y2 / GridConfig.Level2YCellSpan) * GridConfig.Level2YCellSpan;

            if (beginX == endX && beginY == endY)
            {

            }
            else
            {
                for (double x = beginX; x < endX; x += GridConfig.Level2XCellSpan)
                {
                    for (double y = beginY; y < endY; y += GridConfig.Level2YCellSpan)
                    {
                        var gridRect = new GridRect(x, y, GridConfig.Level2XCellSpan, GridConfig.Level2YCellSpan);
                        if (gridPoints.Any(p => RelationPointAndRect(p, gridRect) == GridRelationShip.Within))
                        {
                            // Intersect
                        }
                        else
                        {
                            var relationship = RelationRectAndPanel(gridRect, segments);
                            if (relationship == GridRelationShip.Within)
                            {
                                // Within
                                var point = new GridPoint(x + 0.1d * GridConfig.Level4XCellSpan,
                                            y + 0.1d * GridConfig.Level4YCellSpan);
                                yield return new GridIndex()
                                {
                                    Index1 = 
                                        point.TranformGridIndex(GridConfig.LEVEL1_GRID,
                                            GridConfig.Level1XCellSpan,
                                            GridConfig.Level1YCellSpan),
                                    Index2 =
                                        point.TranformGridIndex(GridConfig.LEVEL2_GRID,
                                            GridConfig.Level2XCellSpan,
                                            GridConfig.Level2YCellSpan),
                                };
                            }
                            else if (relationship == GridRelationShip.Intersect)
                            {
                                // Intersect
                            }
                        }
                    }
                }
            }
        }

        private static GridRelationShip RelationPointAndRect(GridPoint point, GridRect rect)
        {
            if (point.X > rect.Point1.X
                && point.X < rect.Point4.X
                && point.Y > rect.Point1.Y
                && point.Y < rect.Point4.Y)
            {
                return GridRelationShip.Within;
            }
            // TODO: GridRelationShip.Intersect
            return GridRelationShip.Outside;
        }

        private static GridRelationShip RelationRectAndPanel(GridRect rect, IEnumerable<GridSegment> segments)
        {
            var relationship1 = RelationPointAndPanel(rect.Point1, segments);
            var relationship2 = RelationPointAndPanel(rect.Point2, segments);
            var relationship3 = RelationPointAndPanel(rect.Point3, segments);
            var relationship4 = RelationPointAndPanel(rect.Point4, segments);
            if (relationship1 == GridRelationShip.Outside
                && relationship2 == GridRelationShip.Outside
                && relationship3 == GridRelationShip.Outside
                && relationship4 == GridRelationShip.Outside)
            {
                return GridRelationShip.Outside;
            }
            else if (relationship1 == GridRelationShip.Outside
                || relationship2 == GridRelationShip.Outside
                || relationship3 == GridRelationShip.Outside
                || relationship4 == GridRelationShip.Outside)
            {
                return GridRelationShip.Intersect;
            }
            return GridRelationShip.Within;
        }

        private static GridRelationShip RelationPointAndPanel(GridPoint point, IEnumerable<GridSegment> segments)
        {
            var xArray = GetScaleX(point, segments.Where(s => PointContain(point.Y, s.Y1, s.Y2))).OrderBy(x => x);
            var flag = true;
            foreach (var x in xArray)
            {
                if(point.X == x)
                {
                    return GridRelationShip.Intersect;
                }
                else if (point.X < x)
                {
                    return flag ? GridRelationShip.Outside : GridRelationShip.Within;
                }
                flag = !flag;
            }
            return GridRelationShip.Outside;
        }

        private static IEnumerable<double> GetScaleX(GridPoint point, IEnumerable<GridSegment> segments)
        {
            foreach (var segment in segments)
            {
                if (segment.Y1 == segment.Y2)
                {

                    if (PointContain(point.X, segment.X1, segment.X2))
                    {
                        yield return point.X;
                        yield return point.X;
                        break;
                    }
                    yield return segment.X1;
                    yield return segment.X2;
                }
                else
                {
                    yield return GetX(point.Y, segment);
                }
            }
        }

        private static double GetX(double y, GridSegment segment)
        {
            if (segment.X1 == segment.X2)
            {
                return segment.X1;
            }
            return (y - segment.Y1) * (segment.X1 - segment.X2) / (segment.Y1 - segment.Y2) + segment.X1;
        }

        private static bool PointContain(double x1, double x2, double x3)
        {
            var min = Math.Min(x2, x3);
            var max = Math.Max(x2, x3);
            return x1 >= min && x1 <= max;
        }

        #endregion
    }
}
