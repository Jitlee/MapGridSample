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

        public static IEnumerable<GridIndex> GetPolyLineRegion(IList<Positioning> positionings)
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
                    var longitude = column * GridConfig.Level4LongitudeCellSpan;
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
                    var latitude = row * GridConfig.Level4LatitudeCellSpan;
                    var longitude = isInfinity ? endPositioning.Longitude : ((latitude - b) / k);
                    yield return GetPointGridIndex(new Positioning() { Latitude = latitude, Longitude = longitude });
                    yield return GetPointGridIndex(new Positioning() { Latitude = latitude - GridConfig.Level4LatitudeCellSpan, Longitude = longitude });
                }
            }
            yield return GetPointGridIndex(startPositioning);
            yield return GetPointGridIndex(endPositioning);
        }

        #endregion
    }
}
