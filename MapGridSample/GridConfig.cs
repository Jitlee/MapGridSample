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

        public const int LEVEL1_X_GRID = 5;
        public const int LEVEL1_Y_GRID = 4;

        public const int LEVEL2_X_GRID = 5;
        public const int LEVEL2_Y_GRID = 4;

        public const int LEVEL3_X_GRID = 5;
        public const int LEVEL3_Y_GRID = 4;

        public const int LEVEL4_X_GRID = 5;
        public const int LEVEL4_Y_GRID = 4;

        #region 属性

        public readonly static double XSpan = BOUNDING_BOX_X_MAX - BOUNDING_BOX_X_MIN;
        public readonly static double YSpan = BOUNDING_BOX_Y_MAX - BOUNDING_BOX_Y_MIN;
        public readonly static double Level1XCellSpan = XSpan / LEVEL1_X_GRID;
        public readonly static double Level1YCellSpan = YSpan / LEVEL1_Y_GRID;
        public readonly static double Level2XCellSpan = XSpan / (LEVEL1_X_GRID * LEVEL2_X_GRID);
        public readonly static double Level2YCellSpan = YSpan / (LEVEL1_Y_GRID * LEVEL2_Y_GRID);
        public readonly static double Level3XCellSpan = XSpan / (LEVEL1_X_GRID * LEVEL2_X_GRID * LEVEL3_X_GRID);
        public readonly static double Level3YCellSpan = YSpan / (LEVEL1_Y_GRID * LEVEL2_Y_GRID * LEVEL3_Y_GRID);
        public readonly static double Level4XCellSpan = XSpan / (LEVEL1_X_GRID * LEVEL2_X_GRID * LEVEL3_X_GRID * LEVEL4_X_GRID);
        public readonly static double Level4YCellSpan = YSpan / (LEVEL1_Y_GRID * LEVEL2_Y_GRID * LEVEL3_Y_GRID * LEVEL4_Y_GRID);
        #endregion
    }
}
