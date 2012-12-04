using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapGridSample
{
    /// <summary>
    /// GridIndexWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GridIndexWindow : Window
    {
        const double WIDTH = 720d;
        const double HEIGHT = 360d;
        const double BOUNDING_BOX_X_MIN = 113.75;   // 东经
        const double BOUNDING_BOX_Y_MIN = 22.42;    // 北纬
        const double BOUNDING_BOX_X_MAX = 114.65;   // 东经
        const double BOUNDING_BOX_Y_MAX = 22.87;    // 北纬
        const int LEVEL1_GRID = 4;
        const int LEVEL2_GRID = 4;
        const int LEVEL3_GRID = 4;
        const int LEVEL4_GRID = 4;

        private readonly Brush _level1Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xDC, 0x79, 0x1F));
        private readonly Brush _level2Brush = new SolidColorBrush(Color.FromArgb(0xff, 0x72, 0x87, 0xC8));
        private readonly Brush _level3Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xF8, 0x31, 0x0E));
        private readonly Brush _level4Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xB1, 0xDF, 0xA9));

        private int _level = 1;
        private int _operation = 0;
        private Point _originPoint;
        private Positioning _offsetPositioning = new Positioning(); // 原始屏幕起始经纬度坐标

        public GridIndexWindow()
        {
            InitializeComponent();
            CanvasGrid.Width = WIDTH;
            CanvasGrid.Height = HEIGHT;
            GridCanvas.Width = WIDTH;
            GridCanvas.Height = HEIGHT;
            ShapCanvas.Width = WIDTH;
            ShapCanvas.Height = HEIGHT;
            CanvasGrid.Clip = new RectangleGeometry(new Rect(0, 0, WIDTH, HEIGHT));

            DrawGrid();
        }

        #region 绘制网格

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point point = e.GetPosition(GridCanvas);
            Vector vector = point - new Point();
            if (e.Delta > 0 && _level < 4)
            {
                _offsetPositioning -= TranformVector(vector);
                _level++;
                _offsetPositioning += TranformVector(vector);
                DrawGrid();
                GridLevelTextBlock.Text = _level.ToString();
            }
            else if (e.Delta < 0 && _level > 1)
            {
                _offsetPositioning -= TranformVector(vector);
                _level--;
                _offsetPositioning += TranformVector(vector);
                DrawGrid();
                GridLevelTextBlock.Text = _level.ToString();
            }
        }

        private void DrawGrid()
        {
            this.GridCanvas.Children.Clear();
            switch (_level)
            {
                case 1:
                    DrawLevelGrid(1, 1, LEVEL1_GRID, _level1Brush);
                    break;
                case 2:
                    DrawLevelGrid(1, LEVEL1_GRID, LEVEL2_GRID, _level2Brush);
                    DrawLevelGrid(LEVEL1_GRID, 1, LEVEL1_GRID, _level1Brush);
                    break;
                case 3:
                    DrawLevelGrid(1, LEVEL1_GRID * LEVEL2_GRID, LEVEL3_GRID, _level3Brush);
                    DrawLevelGrid(LEVEL2_GRID, LEVEL1_GRID, LEVEL2_GRID, _level2Brush);
                    DrawLevelGrid(LEVEL1_GRID * LEVEL2_GRID, 1, LEVEL1_GRID, _level1Brush);
                    break;
                case 4:
                    DrawLevelGrid(1, LEVEL1_GRID * LEVEL2_GRID * LEVEL3_GRID, LEVEL4_GRID, _level4Brush);
                    DrawLevelGrid(LEVEL3_GRID, LEVEL1_GRID * LEVEL2_GRID, LEVEL3_GRID, _level3Brush);
                    DrawLevelGrid(LEVEL2_GRID * LEVEL3_GRID, LEVEL1_GRID, LEVEL2_GRID, _level2Brush);
                    DrawLevelGrid(LEVEL1_GRID * LEVEL2_GRID * LEVEL3_GRID, 1, LEVEL1_GRID, _level1Brush);
                    break;
            };
        }

        private void DrawLevelGrid(int flag1, int flag2, int levelGrid, Brush brush)
        {
            double lv1CellWidth = CanvasGrid.Width * flag1 / levelGrid;
            double lv1CellHeight = CanvasGrid.Height * flag1 / levelGrid;
            double offsetX = _offsetPositioning.Longitude * WIDTH * flag1 * flag2 / (BOUNDING_BOX_X_MAX - BOUNDING_BOX_X_MIN);
            double offsetY = _offsetPositioning.Latitude * HEIGHT * flag1 * flag2 / (BOUNDING_BOX_Y_MAX - BOUNDING_BOX_Y_MIN);

            double beginX = offsetX % lv1CellWidth;
            double beginY = offsetY % lv1CellHeight;

            double x1 = 0d;
            double x2 = WIDTH;
            double y1 = 0d;
            double y2 = HEIGHT;

            List<TextBlock> textBlocks = new List<TextBlock>();
            for (int i = 0; i <= levelGrid; i++)
            {
                var y = i * lv1CellHeight + beginY;
                this.GridCanvas.Children.Add(new Line()
                {
                    X1 = x1,
                    X2 = x2,
                    Y1 = y,
                    Y2 = y,
                    Stroke = brush,
                    StrokeThickness = flag1,
                });

                var textBlock = new TextBlock();
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.FontSize = 15d + flag1;
                textBlock.Height = textBlock.FontSize * 1.1;
                textBlock.Width = textBlock.FontSize * 1.2;
                textBlock.Background = brush;
                textBlock.Foreground = Brushes.White;
                textBlock.Text = (Math.Floor(-offsetY / lv1CellHeight + i) % levelGrid).ToString();
                Canvas.SetLeft(textBlock, x1);
                Canvas.SetTop(textBlock, y - textBlock.Height / 2d);
                textBlocks.Add(textBlock);
            }

            for (int i = 0; i <= levelGrid; i++)
            {
                var x = i * lv1CellWidth + beginX ;
                this.GridCanvas.Children.Add(new Line()
                {
                    X1 = x,
                    X2 = x,
                    Y1 = y1,
                    Y2 = y2,
                    Stroke = brush,
                    StrokeThickness = flag1,
                });

                var textBlock = new TextBlock();
                textBlock.FontSize = 15d + flag1;
                textBlock.Height = textBlock.FontSize * 1.1;
                textBlock.Background = brush;
                textBlock.Foreground = Brushes.White;
                textBlock.Text = (Math.Floor(- offsetX / lv1CellWidth + i) % levelGrid).ToString();
                textBlock.TextAlignment = TextAlignment.Center;
                Canvas.SetLeft(textBlock, x);
                Canvas.SetTop(textBlock, y1);
                textBlocks.Add(textBlock);
            }

            foreach (TextBlock textBlock in textBlocks)
            {
                this.GridCanvas.Children.Add(textBlock);
            }
            textBlocks.Clear();
        }

        #endregion

        #region 画图

        private void MoveRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _operation = 0;
            CanvasGrid.Cursor = Cursors.Hand;
        }

        private void LineRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _operation = 1;
            CanvasGrid.Cursor = Cursors.Pen;
        }

        private void PolygonRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _operation = 2;
            CanvasGrid.Cursor = Cursors.Cross;
        }

        private Positioning TranformVector(Vector vector)
        {
            Positioning positioning = new Positioning();
            switch (_level)
            {
                case 1:
                    positioning.Longitude = (BOUNDING_BOX_X_MAX - BOUNDING_BOX_X_MIN) * vector.X / WIDTH;
                    positioning.Latitude = (BOUNDING_BOX_Y_MAX - BOUNDING_BOX_Y_MIN) * vector.Y / HEIGHT;
                    break;
                case 2:
                    positioning.Longitude = (BOUNDING_BOX_X_MAX - BOUNDING_BOX_X_MIN) * vector.X / (WIDTH * LEVEL1_GRID);
                    positioning.Latitude = (BOUNDING_BOX_Y_MAX - BOUNDING_BOX_Y_MIN) * vector.Y / (HEIGHT * LEVEL1_GRID);
                    break;
                case 3:
                    positioning.Longitude = (BOUNDING_BOX_X_MAX - BOUNDING_BOX_X_MIN) * vector.X / (WIDTH * LEVEL1_GRID * LEVEL2_GRID);
                    positioning.Latitude = (BOUNDING_BOX_Y_MAX - BOUNDING_BOX_Y_MIN) * vector.Y / (HEIGHT * LEVEL1_GRID * LEVEL2_GRID);
                    break;
                case 4:
                    positioning.Longitude = (BOUNDING_BOX_X_MAX - BOUNDING_BOX_X_MIN) * vector.X / (WIDTH * LEVEL1_GRID * LEVEL2_GRID * LEVEL3_GRID);
                    positioning.Latitude = (BOUNDING_BOX_Y_MAX - BOUNDING_BOX_Y_MIN) * vector.Y / (HEIGHT * LEVEL1_GRID * LEVEL2_GRID * LEVEL3_GRID);
                    break;
            };
            return positioning;
        }

        private void CanvasGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanvasGrid.CaptureMouse();

            _originPoint = e.GetPosition(CanvasGrid);

            CanvasGrid.MouseMove -= CanvasGrid_PreviewMouseMove;
            CanvasGrid.MouseMove += CanvasGrid_PreviewMouseMove;
            CanvasGrid.PreviewMouseLeftButtonUp -= CanvasGrid_PreviewMouseLeftButtonUp;
            CanvasGrid.PreviewMouseLeftButtonUp += CanvasGrid_PreviewMouseLeftButtonUp;
        }

        private void CanvasGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(CanvasGrid);
            double offsetX = point.X - _originPoint.X;
            double offsetY = point.Y - _originPoint.Y;
            Canvas.SetLeft(GridCanvas, offsetX);
            Canvas.SetTop(GridCanvas, offsetY);
        }

        private void CanvasGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanvasGrid.ReleaseMouseCapture();
            CanvasGrid.MouseMove -= CanvasGrid_PreviewMouseMove;
            CanvasGrid.PreviewMouseLeftButtonUp -= CanvasGrid_PreviewMouseLeftButtonUp;

            var vector = e.GetPosition(CanvasGrid) - _originPoint;

            _offsetPositioning += TranformVector(vector);

            DrawGrid();

            GridCanvas.ClearValue(Canvas.LeftProperty);
            GridCanvas.ClearValue(Canvas.TopProperty);
        }

        #endregion

        #region Struct

        /// <summary>
        /// 地理经度和纬坐标
        /// </summary>
        struct Positioning
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
        }

        /// <summary>
        /// 单元格
        /// </summary>
        struct Cell
        {
            /// <summary>
            /// 行
            /// </summary>
            public int Row { get; set; }

            /// <summary>
            /// 列
            /// </summary>
            public int Column { get; set; }
        }

        #endregion
    }
}
