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
                GetPolygonLevel1GridIndex(result, region, gridPoints, segments);
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

        private static void GetPolygonLevel1GridIndex(List<GridIndex> result, GridSegment region, IList<GridPoint> gridPoints, IEnumerable<GridSegment> segments)
        {
            var beginX = Math.Floor(region.X1 / GridConfig.Level1XCellSpan) * GridConfig.Level1XCellSpan;
            var beginY = Math.Floor(region.Y1 / GridConfig.Level1YCellSpan) * GridConfig.Level1YCellSpan;
            var endX = Math.Floor(region.X2 / GridConfig.Level1XCellSpan) * GridConfig.Level1XCellSpan;
            var endY = Math.Floor(region.Y2 / GridConfig.Level1YCellSpan) * GridConfig.Level1YCellSpan;
            var row = (int)Math.Round((endY - beginY) / GridConfig.Level1YCellSpan) + 1;
            var column = (int)Math.Round((endX - beginX) / GridConfig.Level1XCellSpan) + 1;

            if (beginX == endX && beginY == endY)
            {
                // 只占用顶级一个格子
                GetPolygonLevel2GridIndex(result, region, gridPoints, segments);
            }
            else
            {
                for (int i = 0; i < column; i++)
                {
                    var x = beginX + i * GridConfig.Level1XCellSpan;
                    for (int j = 0; j < row; j++)
                    {
                        var y = beginY + j * GridConfig.Level1YCellSpan;
                        var gridRect = new GridRect(x, y, GridConfig.Level1XCellSpan, GridConfig.Level1YCellSpan);
                        if (gridPoints.Any(p => RelationPointAndRect(p, gridRect) == GridRelationShip.Within))
                        {
                            // 多边形顶点所在的格子
                            // 不能压到边界，否则会判断过界，所以边界需要退一点
                            // Intersect
                            GetPolygonLevel2GridIndex(result, new GridSegment(gridRect.Point1.X, gridRect.Point1.Y, gridRect.Point4.X - 0.5d * GridConfig.Level4XCellSpan, gridRect.Point4.Y - 0.5d * GridConfig.Level4YCellSpan), gridPoints, segments);
                        }
                        else
                        {
                            var relationship = RelationRectAndPanel(gridRect, segments);
                            if (relationship == GridRelationShip.Within)
                            {
                                // 多边形背部完全包含的顶级格子，无需再分
                                //Within
                                result.Add(new GridIndex()
                                {
                                    Index1 = new GridPoint(x + 0.5d * GridConfig.Level1XCellSpan,
                                            y + 0.5d * GridConfig.Level1YCellSpan)
                                        .TranformGridIndex(GridConfig.LEVEL1_GRID,
                                            GridConfig.Level1XCellSpan,
                                            GridConfig.Level1YCellSpan)
                                });
                            }
                            else if (relationship == GridRelationShip.Intersect)
                            {
                                // 多边形与格子相交, 继续再分
                                // 不能压到边界，否则会判断过界，所以边界需要退一点
                                // Intersect
                                GetPolygonLevel2GridIndex(result, new GridSegment(gridRect.Point1.X, gridRect.Point1.Y, gridRect.Point4.X - 0.5d * GridConfig.Level4XCellSpan, gridRect.Point4.Y - 0.5d * GridConfig.Level4YCellSpan), gridPoints, segments);
                            }
                        }
                    }
                }
            }
        }

        private static void GetPolygonLevel2GridIndex(List<GridIndex> result, GridSegment region, IList<GridPoint> gridPoints, IEnumerable<GridSegment> segments)
        {
            var beginX = Math.Floor(region.X1 / GridConfig.Level2XCellSpan) * GridConfig.Level2XCellSpan;
            var beginY = Math.Floor(region.Y1 / GridConfig.Level2YCellSpan) * GridConfig.Level2YCellSpan;
            var endX = Math.Floor(region.X2 / GridConfig.Level2XCellSpan) * GridConfig.Level2XCellSpan;
            var endY = Math.Floor(region.Y2 / GridConfig.Level2YCellSpan) * GridConfig.Level2YCellSpan;
            var row = (int)Math.Round((endY - beginY) / GridConfig.Level2YCellSpan) + 1;
            var column = (int)Math.Round((endX - beginX) / GridConfig.Level2XCellSpan) + 1;

            if (beginX == endX && beginY == endY)
            {
                // 只占用二级一个格子
                GetPolygonLevel3GridIndex(result, region, gridPoints, segments);
            }
            else
            {

                for (int i = 0; i < column; i++)
                {
                    var x = beginX + i * GridConfig.Level2XCellSpan;
                    for (int j = 0; j < row; j++)
                    {
                        var y = beginY + j * GridConfig.Level2YCellSpan;
                        var gridRect = new GridRect(x, y, GridConfig.Level2XCellSpan, GridConfig.Level2YCellSpan);
                        if (gridPoints.Any(p => RelationPointAndRect(p, gridRect) == GridRelationShip.Within))
                        {
                            // 多边形顶点所在的格子
                            // 不能压到边界，否则会判断过界，所以边界需要退一点
                            // Intersect
                            GetPolygonLevel3GridIndex(result, new GridSegment(gridRect.Point1.X, gridRect.Point1.Y, gridRect.Point4.X - 0.5d * GridConfig.Level4XCellSpan, gridRect.Point4.Y - 0.5d * GridConfig.Level4YCellSpan), gridPoints, segments);
                        }
                        else
                        {
                            var relationship = RelationRectAndPanel(gridRect, segments);
                            if (relationship == GridRelationShip.Within)
                            {
                                // 多边形背部完全包含的二级格子，无需再分
                                // Within
                                var point = new GridPoint(x + 0.5d * GridConfig.Level4XCellSpan,
                                            y + 0.5d * GridConfig.Level4YCellSpan);
                                result.Add(new GridIndex()
                                {
                                    Index1 = 
                                        point.TranformGridIndex(GridConfig.LEVEL1_GRID,
                                            GridConfig.Level1XCellSpan,
                                            GridConfig.Level1YCellSpan),
                                    Index2 =
                                        point.TranformGridIndex(GridConfig.LEVEL2_GRID,
                                            GridConfig.Level2XCellSpan,
                                            GridConfig.Level2YCellSpan),
                                });
                            }
                            else if (relationship == GridRelationShip.Intersect)
                            {
                                // 多边形与格子相交, 继续再分
                                // 不能压到边界，否则会判断过界，所以边界需要退一点
                                // Intersect
                                GetPolygonLevel3GridIndex(result, new GridSegment(gridRect.Point1.X, gridRect.Point1.Y, gridRect.Point4.X - 0.5d * GridConfig.Level4XCellSpan, gridRect.Point4.Y - 0.5d * GridConfig.Level4YCellSpan), gridPoints, segments);
                            }
                        }
                    }
                }
            }
        }

        private static void GetPolygonLevel3GridIndex(List<GridIndex> result, GridSegment region, IList<GridPoint> gridPoints, IEnumerable<GridSegment> segments)
        {
            var beginX = Math.Floor(region.X1 / GridConfig.Level3XCellSpan) * GridConfig.Level3XCellSpan;
            var beginY = Math.Floor(region.Y1 / GridConfig.Level3YCellSpan) * GridConfig.Level3YCellSpan;
            var endX = Math.Floor(region.X2 / GridConfig.Level3XCellSpan) * GridConfig.Level3XCellSpan;
            var endY = Math.Floor(region.Y2 / GridConfig.Level3YCellSpan) * GridConfig.Level3YCellSpan;
            var row = (int)Math.Round((endY - beginY) / GridConfig.Level3YCellSpan) + 1;
            var column = (int)Math.Round((endX - beginX) / GridConfig.Level3XCellSpan) + 1;

            if (beginX == endX && beginY == endY)
            {
                // 只占用三级一个格子
                GetPolygonLevel4GridIndex(result, region, gridPoints, segments);
            }
            else
            {
                for (int i = 0; i < column; i++)
                {
                    var x = beginX + i * GridConfig.Level3XCellSpan;
                    for (int j = 0; j < row; j++)
                    {
                        var y = beginY + j * GridConfig.Level3YCellSpan;
                        var gridRect = new GridRect(x, y, GridConfig.Level3XCellSpan, GridConfig.Level3YCellSpan);
                        if (gridPoints.Any(p => RelationPointAndRect(p, gridRect) == GridRelationShip.Within))
                        {
                            // 多边形顶点所在的格子
                            // 不能压到边界，否则会判断过界，所以边界需要退一点
                            // Intersect
                            GetPolygonLevel4GridIndex(result, new GridSegment(gridRect.Point1.X, gridRect.Point1.Y, gridRect.Point4.X - 0.5d * GridConfig.Level4XCellSpan, gridRect.Point4.Y - 0.5d * GridConfig.Level4YCellSpan), gridPoints, segments);
                        }
                        else
                        {
                            var relationship = RelationRectAndPanel(gridRect, segments);
                            if (relationship == GridRelationShip.Within)
                            {
                                // 多边形背部完全包含的三级格子，无需再分
                                // Within
                                var point = new GridPoint(x + 0.5d * GridConfig.Level4XCellSpan,
                                            y + 0.5d * GridConfig.Level4YCellSpan);
                                result.Add(new GridIndex()
                                {
                                    Index1 =
                                        point.TranformGridIndex(GridConfig.LEVEL1_GRID,
                                            GridConfig.Level1XCellSpan,
                                            GridConfig.Level1YCellSpan),
                                    Index2 =
                                        point.TranformGridIndex(GridConfig.LEVEL2_GRID,
                                            GridConfig.Level2XCellSpan,
                                            GridConfig.Level2YCellSpan),
                                    Index3 =
                                        point.TranformGridIndex(GridConfig.LEVEL3_GRID,
                                            GridConfig.Level3XCellSpan,
                                            GridConfig.Level3YCellSpan),
                                });
                            }
                            else if (relationship == GridRelationShip.Intersect)
                            {
                                // 多边形与格子相交, 继续再分
                                // 不能压到边界，否则会判断过界，所以边界需要退一点
                                // Intersect
                                GetPolygonLevel4GridIndex(result, new GridSegment(gridRect.Point1.X, gridRect.Point1.Y, gridRect.Point4.X - 0.5d * GridConfig.Level4XCellSpan, gridRect.Point4.Y - 0.5d * GridConfig.Level4YCellSpan), gridPoints, segments);
                            }
                        }
                    }
                }
            }
        }

        private static void GetPolygonLevel4GridIndex(List<GridIndex> result, GridSegment region, IList<GridPoint> gridPoints, IEnumerable<GridSegment> segments)
        {
            var beginX = Math.Floor(region.X1 / GridConfig.Level4XCellSpan) * GridConfig.Level4XCellSpan;
            var beginY = Math.Floor(region.Y1 / GridConfig.Level4YCellSpan) * GridConfig.Level4YCellSpan;
            var endX = Math.Floor(region.X2 / GridConfig.Level4XCellSpan) * GridConfig.Level4XCellSpan;
            var endY = Math.Floor(region.Y2 / GridConfig.Level4YCellSpan) * GridConfig.Level4YCellSpan;
            var row = (int)Math.Round((endY - beginY) / GridConfig.Level4YCellSpan) + 1;
            var column = (int)Math.Round((endX - beginX) / GridConfig.Level4XCellSpan) + 1;

            if (beginX == endX && beginY == endY)
            {
                // 只占用四级一个格子，无需再分
                var point = new GridPoint(beginX, beginY);
                result.Add(GetPointGridIndex(point));
            }
            else
            {
                for (int i = 0; i < column; i++)
                {
                    var x = beginX + i * GridConfig.Level4XCellSpan;
                    for (int j = 0; j < row; j++)
                    {
                        var y = beginY + j * GridConfig.Level4YCellSpan;
                        var gridRect = new GridRect(x, y, GridConfig.Level4XCellSpan, GridConfig.Level4YCellSpan);
                        
                        var point = new GridPoint(x + 0.5d * GridConfig.Level4XCellSpan, y + 0.5d * GridConfig.Level4YCellSpan);
                        if (gridPoints.Any(p => RelationPointAndRect(p, gridRect) == GridRelationShip.Within))
                        {
                            result.Add(GetPointGridIndex(point));
                        }
                        else
                        {
                            var relationship = RelationRectAndPanel(gridRect, segments);
                            if (relationship != GridRelationShip.None)
                            {
                                if (relationship == GridRelationShip.Within)
                                {
                                }
                                result.Add(GetPointGridIndex(point));
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
            return GridRelationShip.None;
        }

        private static GridRelationShip RelationRectAndPanel(GridRect rect, IEnumerable<GridSegment> segments)
        {
            if (RelationSegmentAndPanel(new GridSegment(rect.Point1, rect.Point2), segments) == GridRelationShip.Intersect
                || RelationSegmentAndPanel(new GridSegment(rect.Point2, rect.Point3), segments) == GridRelationShip.Intersect
                || RelationSegmentAndPanel(new GridSegment(rect.Point3, rect.Point4), segments) == GridRelationShip.Intersect
                  || RelationSegmentAndPanel(new GridSegment(rect.Point4, rect.Point1), segments) == GridRelationShip.Intersect)
            {
                return GridRelationShip.Intersect;
            }

            var relationship1 = RelationPointAndPanel(rect.Point1, segments);
            var relationship2 = RelationPointAndPanel(rect.Point2, segments);
            var relationship3 = RelationPointAndPanel(rect.Point3, segments);
            var relationship4 = RelationPointAndPanel(rect.Point4, segments);
            if (relationship1 == GridRelationShip.None
                && relationship2 == GridRelationShip.None
                && relationship3 == GridRelationShip.None
                && relationship4 == GridRelationShip.None)
            {
                return GridRelationShip.None;
            }
            else if (relationship1 == GridRelationShip.None
                || relationship2 == GridRelationShip.None
                || relationship3 == GridRelationShip.None
                || relationship4 == GridRelationShip.None)
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
                    return flag ? GridRelationShip.None : GridRelationShip.Within;
                }
                flag = !flag;
            }
            return GridRelationShip.None;
        }

        private static GridRelationShip RelationSegmentAndPanel(GridSegment segment, IEnumerable<GridSegment> segments)
        {
            if(segments.Any(s => RelationSegmentAndSegment(segment, s) == GridRelationShip.Intersect))
            {
                return GridRelationShip.Intersect;
            }
            return GridRelationShip.None;
        }

        private static GridRelationShip RelationSegmentAndSegment(GridSegment segment1, GridSegment segment2)
        {
            var vector1 = GetVector(segment1.Point1, segment2.Point1);
            var vector2 = GetVector(segment1.Point2, segment2.Point1);
            var vector3 = GetVector(segment2.Point2, segment2.Point1);
            var vector4 = GetVector(segment2.Point1, segment1.Point1);
            var vector5 = GetVector(segment2.Point2, segment1.Point1);
            var vector6 = GetVector(segment1.Point2, segment1.Point1);
            if(CrossMul(vector1, vector3) * CrossMul(vector2, vector3) <=0
                && CrossMul(vector4, vector6) * CrossMul(vector5, vector6) <=0)
            {
                return GridRelationShip.Intersect;
            }
            return GridRelationShip.None;
        }

        private static GridPoint GetVector(GridPoint point1, GridPoint point2)
        {
            return new GridPoint(point1.X - point2.X, point1.Y - point2.Y);
        }

        private static double CrossMul(GridPoint point1, GridPoint point2)
        {
            return point1.X * point2.Y - point1.Y * point2.X;
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
