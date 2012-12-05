using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGridSample
{
    /// <summary>
    /// 地理经度和纬坐标
    /// </summary>
    public struct Positioning
    {
        /// <summary>
        /// 经度(东经)
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// 纬度（北纬）
        /// </summary>
        public double Latitude { get; set; }

        public static Positioning operator -(Positioning positioning1, Positioning positioning2)
        {
            Positioning result = new Positioning();
            result.Longitude = positioning1.Longitude - positioning2.Longitude;
            result.Latitude = positioning1.Latitude - positioning2.Latitude;
            return result;
        }

        public static Positioning operator +(Positioning positioning1, Positioning positioning2)
        {
            Positioning result = new Positioning();
            result.Longitude = positioning1.Longitude + positioning2.Longitude;
            result.Latitude = positioning1.Latitude + positioning2.Latitude;
            return result;
        }

        public int TranformGridIndex(double levelGrid, double levelLongitudeCellSpan, double levelLatitudeCellSpan)
        {
            double row = Math.Floor(Latitude / levelLatitudeCellSpan) % levelGrid;
            double column = Math.Floor(Longitude / levelLongitudeCellSpan) % levelGrid;
            return (int)(row * levelGrid + column + 1);
        }
    }
}
