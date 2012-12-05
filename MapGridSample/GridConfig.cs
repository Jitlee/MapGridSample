﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGridSample
{
    public static class GridConfig
    {
        // 东经
        /// <summary>
        /// 东经
        /// </summary>
        public const double BOUNDING_BOX_X_MIN = 113.75;
        
        // 北纬
        /// <summary>
        /// // 北纬
        /// </summary>
        public const double BOUNDING_BOX_Y_MIN = 22.42;

        // 东经
        /// <summary>
        /// 东经
        /// </summary>
        public const double BOUNDING_BOX_X_MAX = 114.65;
        
        // 北纬
        /// <summary>
        /// 北纬
        /// </summary>
        public const double BOUNDING_BOX_Y_MAX = 22.87; 

        public const int LEVEL1_GRID = 4;
        public const int LEVEL2_GRID = 4;
        public const int LEVEL3_GRID = 4;
        public const int LEVEL4_GRID = 4;

        #region 属性

        public readonly static double LongitudeSpan = BOUNDING_BOX_X_MAX - BOUNDING_BOX_X_MIN;
        public readonly static double LatitudeSpan = BOUNDING_BOX_Y_MAX - BOUNDING_BOX_Y_MIN;
        public readonly static double Level1LongitudeCellSpan = LongitudeSpan / LEVEL1_GRID;
        public readonly static double Level1LatitudeCellSpan = LatitudeSpan / LEVEL1_GRID;
        public readonly static double Level2LongitudeCellSpan = LongitudeSpan / (LEVEL1_GRID * LEVEL2_GRID);
        public readonly static double Level2LatitudeCellSpan = LatitudeSpan / (LEVEL1_GRID * LEVEL2_GRID);
        public readonly static double Level3LongitudeCellSpan = LongitudeSpan / (LEVEL1_GRID * LEVEL2_GRID * LEVEL3_GRID);
        public readonly static double Level3LatitudeCellSpan = LatitudeSpan / (LEVEL1_GRID * LEVEL2_GRID * LEVEL3_GRID);
        public readonly static double Level4LongitudeCellSpan = LongitudeSpan / (LEVEL1_GRID * LEVEL2_GRID * LEVEL3_GRID * LEVEL4_GRID);
        public readonly static double Level4LatitudeCellSpan = LatitudeSpan / (LEVEL1_GRID * LEVEL2_GRID * LEVEL3_GRID * LEVEL4_GRID);
        
        #endregion
    }
}