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

        public static IEnumerable<GridIndex> GetPolylineRegion(IList<Positioning> positionings)
        {
            var result = new List<GridIndex>();
            var prev = positionings.First();
            for (int i = 1; i < positionings.Count; i++)
            {
                result.AddRange(GetLineRegion(prev, positionings[i]));
                prev = positionings[i];
            }
            return result.Distinct();
        }

        private static IEnumerable<GridIndex> GetLineRegion(Positioning startPositioning, Positioning endPositioning)
        {
            //  一次函数公式: kx + b = y
            var k = (startPositioning.Latitude - endPositioning.Latitude) / (startPositioning.Longitude - endPositioning.Longitude);
            var b = startPositioning.Latitude - k * startPositioning.Longitude;

            if (!double.IsInfinity(k))
            {
                var maxColumn = (int)Math.Floor(Math.Max(startPositioning.Longitude, endPositioning.Longitude) / GridConfig.Level4LongitudeCellSpan);
                var minColumn = (int)Math.Ceiling(Math.Min(startPositioning.Longitude, endPositioning.Longitude) / GridConfig.Level4LongitudeCellSpan);
                for (int column = minColumn; column <= maxColumn; column++)
                {
                    // 多加 1/2 的值是为了 避免 刚好在边界点因为小数点误差而计算错误
                    var longitude = (column + 0.00001d) * GridConfig.Level4LongitudeCellSpan;
                    var latitude = k * longitude + b;
                    yield return GetPointGridIndex(new Positioning() { Latitude = latitude, Longitude = longitude });
                    yield return GetPointGridIndex(new Positioning() { Latitude = latitude, Longitude = longitude - GridConfig.Level4LongitudeCellSpan });
                }
            }

            if (k != 0)
            {
                var maxRow = (int)Math.Floor((Math.Max(startPositioning.Latitude, endPositioning.Latitude) / GridConfig.Level4LatitudeCellSpan));
                var minRow = (int)Math.Ceiling(Math.Min(startPositioning.Latitude, endPositioning.Latitude) / GridConfig.Level4LatitudeCellSpan);
                var isInfinity = double.IsInfinity(k);

                for (int row = minRow; row <= maxRow; row++)
                {
                    // 多加 1/2 的值是为了 避免 刚好在边界点因为小数点误差而计算错误
                    var latitude = (row + 0.00001d) * GridConfig.Level4LatitudeCellSpan;
                    var longitude = isInfinity ? endPositioning.Longitude : ((latitude - b) / k);
                    yield return GetPointGridIndex(new Positioning() { Latitude = latitude, Longitude = longitude });
                    yield return GetPointGridIndex(new Positioning() { Latitude = latitude - GridConfig.Level4LatitudeCellSpan, Longitude = longitude });
                }
            }
            yield return GetPointGridIndex(startPositioning);
            yield return GetPointGridIndex(endPositioning);
        }

        #endregion

        #region 获取面的空间索引

        public static IEnumerable<GridIndex> GetPolygonRegion(IList<Positioning> positionings)
        {
            var result = new List<GridIndex>();
            if (positionings.Count > 2)
            {
                var last = positionings.Last();
                foreach (Positioning positioning in positionings)
                {
                    result.AddRange(GetLineRegion(last, positioning));
                    last = positioning;
                }
            }
            result.AddRange(GetPolygonContainedRegion(result.ToArray()));
            return result.Distinct();
        }

        private static IEnumerable<GridIndex> GetPolygonContainedRegion(IEnumerable<GridIndex> gridIndexes)
        {
            var cells = gridIndexes.Select(g => new
            {
                Row = ((g.Index1 - 1) / GridConfig.LEVEL1_GRID) * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID * GridConfig.LEVEL4_GRID
                    + ((g.Index2 - 1) / GridConfig.LEVEL2_GRID) * GridConfig.LEVEL3_GRID * GridConfig.LEVEL4_GRID
                    + ((g.Index3 - 1) / GridConfig.LEVEL3_GRID) * GridConfig.LEVEL4_GRID
                    + (g.Index4.Value - 1) / GridConfig.LEVEL4_GRID,
                Column = ((g.Index1 - 1) % GridConfig.LEVEL1_GRID) * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID * GridConfig.LEVEL4_GRID
                    + ((g.Index2 - 1) % GridConfig.LEVEL2_GRID) * GridConfig.LEVEL3_GRID * GridConfig.LEVEL4_GRID
                    + ((g.Index3 - 1) % GridConfig.LEVEL3_GRID) * GridConfig.LEVEL4_GRID
                    + (g.Index4.Value - 1) % GridConfig.LEVEL4_GRID
            });

            var rows = cells.Select(c => c.Row.Value).Distinct().OrderBy(c => c);

            foreach (var row in rows)
            {
                var columns = cells.Where(c => c.Row == row).Select(c => c.Column.Value).Distinct().OrderBy(c => c).ToArray();
                var length = columns.Length;
                if (length > 1)
                {
                    if (length % 2 == 0)
                    {
                        var currentColumn = columns[0];
                        var pass = true;
                        for (int i = 1; i < columns.Length; i++)
                        {
                            if (pass)
                            {
                                if (columns[i] - currentColumn > 1)
                                {
                                    for (var column = currentColumn + 1; column < columns[i]; column++)
                                    {
                                        if (row > cells.Where(c=> c.Column == column).Max(c => c.Row))
                                        {
                                                
                                        }

                                        yield return new GridIndex()
                                        {
                                            Index1 = row / (GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID * GridConfig.LEVEL4_GRID) % GridConfig.LEVEL1_GRID * GridConfig.LEVEL1_GRID + column / (GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID * GridConfig.LEVEL4_GRID) % GridConfig.LEVEL1_GRID + 1,
                                            Index2 = row / (GridConfig.LEVEL3_GRID * GridConfig.LEVEL4_GRID) % GridConfig.LEVEL2_GRID * GridConfig.LEVEL2_GRID + column / (GridConfig.LEVEL3_GRID * GridConfig.LEVEL4_GRID) % GridConfig.LEVEL2_GRID + 1,
                                            Index3 = row / GridConfig.LEVEL4_GRID % GridConfig.LEVEL3_GRID * GridConfig.LEVEL3_GRID + column / GridConfig.LEVEL4_GRID % GridConfig.LEVEL3_GRID + 1,
                                            Index4 = row % GridConfig.LEVEL4_GRID * GridConfig.LEVEL4_GRID + column % GridConfig.LEVEL4_GRID + 1,
                                        };
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            pass = !pass;
                            currentColumn = columns[1];
                        }
                    }
                }
            }
        }
        #endregion
    }
}
