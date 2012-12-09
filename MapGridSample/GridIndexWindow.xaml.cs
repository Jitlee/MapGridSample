using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private readonly SolidColorBrush _containsBackground = new SolidColorBrush(Color.FromArgb(0x85, 0x00, 0x80, 0x00));
        private readonly SolidColorBrush _level1Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xDC, 0x79, 0x1F));
        private readonly SolidColorBrush _level2Brush = new SolidColorBrush(Color.FromArgb(0xff, 0x72, 0x87, 0xC8));
        private readonly SolidColorBrush _level3Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xF8, 0x31, 0x0E));
        private readonly SolidColorBrush _level4Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xB1, 0xDF, 0xA9));
        private readonly ObservableCollection<GridIndex> _gridIndexes = new ObservableCollection<GridIndex>();
        private readonly List<GridPoint> _polylinePositionings = new List<GridPoint>();
        private readonly List<GridPoint> _polygonPositionings = new List<GridPoint>();
        public ObservableCollection<GridIndex> GridIndexes { get { return _gridIndexes; } }

        private bool _isDrawing = false;
        private int _level = 1;
        private int _operation = 0;
        private Point _originPoint;
        private GridPoint _offsetPositioning = new GridPoint(); // 原始屏幕起始经纬度坐标
        private GridPoint _ellipsePositioning = new GridPoint();

        public GridIndexWindow()
        {
            InitializeComponent();

            RootCanvas.Width = WIDTH;
            RootCanvas.Height = HEIGHT;
            GridCanvas.Width = WIDTH;
            GridCanvas.Height = HEIGHT;
            ShapCanvas.Width = WIDTH;
            ShapCanvas.Height = HEIGHT;
            GridIndexesListBox.Height = HEIGHT + 120;
            RootCanvas.SetValue(MarginProperty, new Thickness(30d));
            CanvasGrid.Clip = new RectangleGeometry(new Rect(0, 0, WIDTH + 60d, HEIGHT + 60d));
            DrawGrid();
            GridIndexesListBox.DataContext = this;
        }

        #region 绘制网格

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_isDrawing)
            {
                Point point = e.GetPosition(GridCanvas);
                if (e.Delta > 0 && _level < 4)
                {
                    _offsetPositioning -= TranformPositioning(point);
                    _level++;
                    _offsetPositioning += TranformPositioning(point);
                    DrawGrid();
                    GridLevelTextBlock.Text = _level.ToString();
                }
                else if (e.Delta < 0 && _level > 1)
                {
                    _offsetPositioning -= TranformPositioning(point);
                    _level--;
                    _offsetPositioning += TranformPositioning(point);
                    DrawGrid();
                    GridLevelTextBlock.Text = _level.ToString();
                }
            }
        }

        private void DrawGrid()
        {
            this.GridCanvas.Children.Clear();
            switch (_level)
            {
                case 1:
                    DrawLabel(1, 1, 1, 1, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID);
                    DrawShape(1, 1, 1, 1, 1d);
                    DrawLevelGrid(1, 1, 1, 1, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, _level1Brush, 1d);
                    break;
                case 2:
                    DrawLabel(1, 1, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL2_Y_GRID);
                    DrawShape(1, 1, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, 2d);
                    DrawLevelGrid(1,1 , GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL2_Y_GRID, _level2Brush, 1d);
                    DrawLevelGrid(GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, 1, 1, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, _level1Brush, 4d);
                    break;
                case 3:
                    DrawLabel(1, 1, GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID, GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL3_Y_GRID);
                    DrawShape(1, 1, GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID, 3d);
                    DrawLevelGrid(1, 1, GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID, GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL3_Y_GRID, _level3Brush, 1d);
                    DrawLevelGrid(GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL2_Y_GRID, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL2_Y_GRID, _level2Brush, 4d);
                    DrawLevelGrid(GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID, 1, 1, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, _level1Brush, 8d);
                    break;
                case 4:
                    DrawLabel(1, 1, GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID * GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID * GridConfig.LEVEL3_Y_GRID, GridConfig.LEVEL4_X_GRID, GridConfig.LEVEL4_Y_GRID);
                    DrawShape(1, 1, GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID * GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID * GridConfig.LEVEL3_Y_GRID, 4d);
                    DrawLevelGrid(1, 1, GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID * GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID * GridConfig.LEVEL3_Y_GRID, GridConfig.LEVEL4_X_GRID, GridConfig.LEVEL4_Y_GRID, _level4Brush, 1d);
                    DrawLevelGrid(GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL3_Y_GRID, GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID, GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL3_Y_GRID, _level3Brush, 4d);
                    DrawLevelGrid(GridConfig.LEVEL2_X_GRID * GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL2_Y_GRID * GridConfig.LEVEL3_Y_GRID, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL2_Y_GRID, _level2Brush, 8d);
                    DrawLevelGrid(GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID * GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID * GridConfig.LEVEL3_Y_GRID, 1, 1, GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, _level1Brush, 16d);
                    break;
            };
        }

        private void DrawLevelGrid(int xFlag1, int yFlag1, int xFlag2, int yFlag2, int levelXGrid, int levelYGrid, Brush brush, double thickness)
        {
            double cellWidth = WIDTH * xFlag1 / levelXGrid;
            double cellHeight = HEIGHT * yFlag1 / levelYGrid;
            double offsetX = _offsetPositioning.X * WIDTH * xFlag1 * xFlag2 / GridConfig.XSpan;
            double offsetY = _offsetPositioning.Y * HEIGHT * yFlag1 * yFlag2 / GridConfig.YSpan;
            double beginX = offsetX % cellWidth;
            double beginY = offsetY % cellHeight;
            double x1 = -WIDTH;
            double x2 = 2d * WIDTH;
            double y1 = -HEIGHT;
            double y2 = 2d * HEIGHT;

            for (int i = -1; i <= levelYGrid + 1; i++)
            {
                double y = i * cellHeight + beginY;
                this.GridCanvas.Children.Add(new Line()
                {
                    X1 = x1,
                    X2 = x2,
                    Y1 = y,
                    Y2 = y,
                    Stroke = brush,
                    StrokeThickness = thickness,
                });
            }

            for (int i = -1; i <= levelXGrid + 1; i++)
            {
                double x = i * cellWidth + beginX;
                this.GridCanvas.Children.Add(new Line()
                {
                    X1 = x,
                    X2 = x,
                    Y1 = y1,
                    Y2 = y2,
                    Stroke = brush,
                    StrokeThickness = thickness,
                });
            }
        }

        private void DrawLabel(int xFlag1, int yFlag1, int xFlag2, int yFlag2, int levelXGrid, int levelYGrid)
        {
            double cellWidth = WIDTH * xFlag1 / levelXGrid;
            double cellHeight = HEIGHT * yFlag1 / levelYGrid;
            double offsetX = _offsetPositioning.X * WIDTH * xFlag1 * xFlag2 / GridConfig.XSpan;
            double offsetY = _offsetPositioning.Y * HEIGHT * yFlag1 * yFlag2 / GridConfig.YSpan;
            double beginX = offsetX % cellWidth;
            double beginY = offsetY % cellHeight;

            GridPoint gridPoint = new GridPoint();
            for (int row = -1; row <= levelYGrid + 1; row++)
            {
                double y = row * cellHeight + beginY;
                for (int col = -1; col < levelXGrid + 1; col++)
                {
                    var brush = Brushes.White;
                    double x = col * cellWidth + beginX;

                    // 多加 1/2 的值是为了 避免 刚好在边界点因为小数点误差而计算错误
                    gridPoint = TranformPositioning(x - offsetX + cellWidth / 2d, y - offsetY + cellHeight / 2d); 

                    if (gridPoint.Y > 0d
                        && gridPoint.X > 0d
                        && gridPoint.Y < GridConfig.YSpan
                        && gridPoint.X < GridConfig.XSpan)
                    {
                        StackPanel stackPanel = new StackPanel();
                        stackPanel.Orientation = Orientation.Horizontal;
                        stackPanel.VerticalAlignment = VerticalAlignment.Center;
                        stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                        stackPanel.Children.Add(new TextBlock() { Text = "(" });

                        int lv1Index = gridPoint.TranformGridIndex(GridConfig.LEVEL1_X_GRID, GridConfig.LEVEL1_Y_GRID, GridConfig.Level1XCellSpan, GridConfig.Level1YCellSpan);

                        stackPanel.Children.Add(new TextBlock() { Text = lv1Index.ToString(), Foreground = _level1Brush });

                        if (_level > 1)
                        {
                            int lv2Index = gridPoint.TranformGridIndex(GridConfig.LEVEL2_X_GRID, GridConfig.LEVEL2_Y_GRID, GridConfig.Level2XCellSpan, GridConfig.Level2YCellSpan);
                            stackPanel.Children.Add(new TextBlock() { Text = ", " });
                            stackPanel.Children.Add(new TextBlock() { Text = lv2Index.ToString(), Foreground = _level2Brush });
                            if (_level > 2)
                            {
                                int lv3Index = gridPoint.TranformGridIndex(GridConfig.LEVEL3_X_GRID, GridConfig.LEVEL3_Y_GRID, GridConfig.Level3XCellSpan, GridConfig.Level3YCellSpan);

                                stackPanel.Children.Add(new TextBlock() { Text = ", " });
                                stackPanel.Children.Add(new TextBlock() { Text = lv3Index.ToString(), Foreground = _level3Brush });
                                if (_level > 3)
                                {
                                    int lv4Index = gridPoint.TranformGridIndex(GridConfig.LEVEL4_X_GRID, GridConfig.LEVEL4_Y_GRID, GridConfig.Level4XCellSpan, GridConfig.Level4YCellSpan);

                                    stackPanel.Children.Add(new TextBlock() { Text = ", " });
                                    stackPanel.Children.Add(new TextBlock() { Text = lv4Index.ToString(), Foreground = _level4Brush });

                                    if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index && g.Index3 == lv3Index && g.Index4 == lv4Index))
                                    {
                                        brush = _level4Brush;
                                    }
                                    else if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index && g.Index3 == lv3Index && !g.Index4.HasValue))
                                    {
                                        brush = _level3Brush;
                                    }
                                    else if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index && !g.Index3.HasValue))
                                    {
                                        brush = _level2Brush;
                                    }
                                    else if (_gridIndexes.Any(g => g.Index1 == lv1Index && !g.Index2.HasValue))
                                    {
                                        brush = _level1Brush;
                                    }
                                }
                                else if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index && g.Index3 == lv3Index))
                                {
                                    brush = _level3Brush;
                                }
                                else if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index && !g.Index3.HasValue))
                                {
                                    brush = _level2Brush;
                                }
                                else if (_gridIndexes.Any(g => g.Index1 == lv1Index && !g.Index2.HasValue))
                                {
                                    brush = _level1Brush;
                                }
                            }
                            else if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index))
                            {
                                brush = _level2Brush;
                            }
                            else if(_gridIndexes.Any(g => g.Index1 == lv1Index && !g.Index2.HasValue))
                            {
                                brush = _level1Brush;
                            }
                        }
                        else if(_gridIndexes.Any(g=> g.Index1 == lv1Index))
                        {
                            brush = _level1Brush;
                        }
                        stackPanel.Children.Add(new TextBlock() { Text = ")" });

                        Grid grid = new Grid();
                        grid.Children.Add(new Rectangle() { Opacity = brush != Brushes.White ? 0.3d : 1d, Fill = brush });
                        grid.Children.Add(stackPanel);
                        grid.Height = cellHeight;
                        grid.Width = cellWidth;
                        grid.SetValue(Canvas.LeftProperty, x);
                        grid.SetValue(Canvas.TopProperty, y);
                        GridCanvas.Children.Add(grid);
                    }
                }
            }
        }

        private void DrawShape(int xFlag1, int yFlag1, int xFlag2, int yFlag2, double thickness)
        {
            double offsetX = _offsetPositioning.X * WIDTH * xFlag1 * xFlag2 / GridConfig.XSpan;
            double offsetY = _offsetPositioning.Y * HEIGHT * yFlag1 * yFlag2 / GridConfig.YSpan;

            Point ellipsLocation = TranformPositioning(_ellipsePositioning);
            TestEllipse.Width = TestEllipse.Height = _level * 4d;
            Canvas.SetLeft(TestEllipse, ellipsLocation.X - _level * 2d + offsetX);
            Canvas.SetTop(TestEllipse, ellipsLocation.Y - _level * 2d + offsetY);

            TestPolyline.Points.Clear();
            TestPolyline.StrokeThickness = thickness;
            foreach (GridPoint gridPoint in _polylinePositionings)
            {
                Point point = TranformPositioning(gridPoint);
                point.X += offsetX;
                point.Y += offsetY;
                TestPolyline.Points.Add(point);
            }

            TestPolygon.Points.Clear();
            TestPolygon.StrokeThickness = thickness;
            foreach (GridPoint gridPoint in _polygonPositionings)
            {
                Point point = TranformPositioning(gridPoint);
                point.X += offsetX;
                point.Y += offsetY;
                TestPolygon.Points.Add(point);
            }
        }

        private GridPoint TranformPositioning(Vector vector)
        {
            return TranformPositioning(vector.X, vector.Y);
        }

        private GridPoint TranformPositioning(Point point)
        {
            return TranformPositioning(point.X, point.Y);
        }

        private GridPoint TranformPositioning(double x, double y)
        {
            GridPoint gridPoint = new GridPoint();
            switch (_level)
            {
                case 1:
                    gridPoint.X = GridConfig.XSpan * x / WIDTH;
                    gridPoint.Y = GridConfig.YSpan * y / HEIGHT;
                    break;
                case 2:
                    gridPoint.X = GridConfig.XSpan * x / (WIDTH * GridConfig.LEVEL1_X_GRID);
                    gridPoint.Y = GridConfig.YSpan * y / (HEIGHT * GridConfig.LEVEL1_Y_GRID);
                    break;
                case 3:
                    gridPoint.X = GridConfig.XSpan * x / (WIDTH * GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID);
                    gridPoint.Y = GridConfig.YSpan * y / (HEIGHT * GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID);
                    break;
                case 4:
                    gridPoint.X = GridConfig.XSpan * x / (WIDTH * GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID * GridConfig.LEVEL3_X_GRID);
                    gridPoint.Y = GridConfig.YSpan * y / (HEIGHT * GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID * GridConfig.LEVEL3_Y_GRID);
                    break;
            };
            return gridPoint;
        }

        private Point TranformPositioning(GridPoint gridPoint)
        {
            Point point = new Point();
            switch (_level)
            {
                case 1:
                    point.X = gridPoint.X * WIDTH /  GridConfig.XSpan;
                    point.Y = gridPoint.Y * HEIGHT / GridConfig.YSpan;
                    break;
                case 2:
                    point.X = gridPoint.X * WIDTH * GridConfig.LEVEL1_X_GRID / GridConfig.XSpan;
                    point.Y = gridPoint.Y * HEIGHT * GridConfig.LEVEL1_Y_GRID / GridConfig.YSpan;
                    break;
                case 3:
                    point.X = gridPoint.X * WIDTH * GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID / GridConfig.XSpan;
                    point.Y = gridPoint.Y * HEIGHT * GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID / GridConfig.YSpan;
                    break;
                case 4:
                    point.X = gridPoint.X * WIDTH * GridConfig.LEVEL1_X_GRID * GridConfig.LEVEL2_X_GRID * GridConfig.LEVEL3_X_GRID / GridConfig.XSpan;
                    point.Y = gridPoint.Y * HEIGHT * GridConfig.LEVEL1_Y_GRID * GridConfig.LEVEL2_Y_GRID * GridConfig.LEVEL3_Y_GRID / GridConfig.YSpan;
                    break;
            };
            return point;
        }

        #endregion

        #region 画图

        private void MoveRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _operation = 0;
            CanvasGrid.Cursor = Cursors.Hand;
        }

        private void PointRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _operation = 1;
            CanvasGrid.Cursor = Cursors.Cross;
        }

        private void LineRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _operation = 2;
            CanvasGrid.Cursor = Cursors.Cross;
        }

        private void PolygonRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _operation = 3;
            CanvasGrid.Cursor = Cursors.Pen;
        }

        private void CanvasGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanvasGrid.CaptureMouse();

            _originPoint = e.GetPosition(RootCanvas);

            switch (_operation)
            {
                case 0:
                    CanvasGrid.MouseMove -= MoveCanvasGrid_PreviewMouseMove;
                    CanvasGrid.MouseMove += MoveCanvasGrid_PreviewMouseMove;
                    CanvasGrid.PreviewMouseLeftButtonUp -= MoveCanvasGrid_PreviewMouseLeftButtonUp;
                    CanvasGrid.PreviewMouseLeftButtonUp += MoveCanvasGrid_PreviewMouseLeftButtonUp;
                    break;
                case 1:
                    ResetShape();
                    DrawPoint();
                    break;
                case 2:
                    _isDrawing = true;
                    CanvasGrid.MouseMove -= DrawLineCanvasGrid_PreviewMouseMove;
                    CanvasGrid.MouseMove += DrawLineCanvasGrid_PreviewMouseMove;
                    CanvasGrid.PreviewMouseLeftButtonDown -= CanvasGrid_PreviewMouseLeftButtonDown;
                    CanvasGrid.PreviewMouseLeftButtonDown -= DrawLineCanvasGrid_PreviewMouseLeftButtonDown;
                    CanvasGrid.PreviewMouseLeftButtonDown += DrawLineCanvasGrid_PreviewMouseLeftButtonDown;
                    CanvasGrid.PreviewMouseRightButtonDown -= DrawLineCanvasGrid_PreviewMouseRightButtonDown;
                    CanvasGrid.PreviewMouseRightButtonDown += DrawLineCanvasGrid_PreviewMouseRightButtonDown;
                    ResetShape();
                    TestPolyline.Points.Add(_originPoint);
                    TestPolyline.Points.Add(_originPoint);
                    break;
                case 3:
                    _isDrawing = true;
                    CanvasGrid.MouseMove -= DrawPlaneCanvasGrid_PreviewMouseMove;
                    CanvasGrid.MouseMove += DrawPlaneCanvasGrid_PreviewMouseMove;
                    CanvasGrid.PreviewMouseLeftButtonDown -= CanvasGrid_PreviewMouseLeftButtonDown;
                    CanvasGrid.PreviewMouseLeftButtonDown -= DrawPlaneCanvasGrid_PreviewMouseLeftButtonDown;
                    CanvasGrid.PreviewMouseLeftButtonDown += DrawPlaneCanvasGrid_PreviewMouseLeftButtonDown;
                    CanvasGrid.PreviewMouseRightButtonDown -= DrawPlaneCanvasGrid_PreviewMouseRightButtonDown;
                    CanvasGrid.PreviewMouseRightButtonDown += DrawPlaneCanvasGrid_PreviewMouseRightButtonDown;
                    ResetShape();
                    TestPolygon.Points.Add(_originPoint);
                    TestPolygon.Points.Add(_originPoint);
                    break;
            }
        }

        private void ResetShape()
        {
            _gridIndexes.Clear();
            _polylinePositionings.Clear();
            _polygonPositionings.Clear();
            TestEllipse.Opacity = 0d;
            TestPolyline.Points.Clear();
            TestPolygon.Points.Clear();
        }

        #region 漫游

        private void MoveCanvasGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(RootCanvas);
            double offsetX = point.X - _originPoint.X;
            double offsetY = point.Y - _originPoint.Y;
            Canvas.SetLeft(GridCanvas, offsetX);
            Canvas.SetTop(GridCanvas, offsetY);
            Canvas.SetLeft(ShapCanvas, offsetX);
            Canvas.SetTop(ShapCanvas, offsetY);
        }

        private void MoveCanvasGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanvasGrid.ReleaseMouseCapture();
            CanvasGrid.MouseMove -= MoveCanvasGrid_PreviewMouseMove;
            CanvasGrid.PreviewMouseLeftButtonUp -= MoveCanvasGrid_PreviewMouseLeftButtonUp;

            var vector = e.GetPosition(RootCanvas) - _originPoint;

            _offsetPositioning += TranformPositioning(vector);

            DrawGrid();

            GridCanvas.ClearValue(Canvas.LeftProperty);
            GridCanvas.ClearValue(Canvas.TopProperty);
            ShapCanvas.ClearValue(Canvas.LeftProperty);
            ShapCanvas.ClearValue(Canvas.TopProperty);
        }

        #endregion

        #region 画点

        private void DrawPoint()
        {
            CanvasGrid.ReleaseMouseCapture();
            _ellipsePositioning = TranformPositioning(_originPoint) - _offsetPositioning;
            TestEllipse.Opacity = 1d;
            TestEllipse.Width = TestEllipse.Height = _level * 4d;
            Canvas.SetLeft(TestEllipse, _originPoint.X - _level * 2d);
            Canvas.SetTop(TestEllipse, _originPoint.Y - _level * 2d);
            MoveRadioButton.IsChecked = true;
            _gridIndexes.Add(GridHelper.GetPointGridIndex(_ellipsePositioning));

            DrawGrid();
        }

        #endregion

        #region 画线

        private void DrawLineCanvasGrid_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(RootCanvas);
            TestPolyline.Points.Add(point);
        }

        private void DrawLineCanvasGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(RootCanvas);
            TestPolyline.Points.RemoveAt(TestPolyline.Points.Count - 1);
            TestPolyline.Points.Add(point);
        }

        private void DrawLineCanvasGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanvasGrid.ReleaseMouseCapture();
            CanvasGrid.MouseMove -= DrawLineCanvasGrid_PreviewMouseMove;
            CanvasGrid.PreviewMouseRightButtonDown -= DrawLineCanvasGrid_PreviewMouseRightButtonDown;
            CanvasGrid.PreviewMouseLeftButtonDown -= DrawLineCanvasGrid_PreviewMouseLeftButtonDown;
            CanvasGrid.PreviewMouseLeftButtonDown += CanvasGrid_PreviewMouseLeftButtonDown;
            MoveRadioButton.IsChecked = true;

            TestPolyline.Points.RemoveAt(TestPolyline.Points.Count - 1);
            foreach (Point point in TestPolyline.Points)
            {
                _polylinePositionings.Add(TranformPositioning(point) - _offsetPositioning);
            }

            var gridIndexes = GridHelper.GetPolylineGrindIndex(_polylinePositionings).OrderBy(p => p.Index1).ThenBy(p => p.Index2).ThenBy(p => p.Index3).ThenBy(p => p.Index4);

            foreach (var gridIndex in gridIndexes)
            {
                _gridIndexes.Add(gridIndex);
            }

            DrawGrid();

            _isDrawing = false;
        }

        #endregion
        
        #region 画面

        private void DrawPlaneCanvasGrid_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(RootCanvas);
            TestPolygon.Points.Add(point);
        }

        private void DrawPlaneCanvasGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(RootCanvas);
            TestPolygon.Points.RemoveAt(TestPolygon.Points.Count - 1);
            TestPolygon.Points.Add(point);
        }

        private void DrawPlaneCanvasGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanvasGrid.ReleaseMouseCapture();
            CanvasGrid.MouseMove -= DrawPlaneCanvasGrid_PreviewMouseMove;
            CanvasGrid.PreviewMouseRightButtonDown -= DrawPlaneCanvasGrid_PreviewMouseRightButtonDown;
            CanvasGrid.PreviewMouseLeftButtonDown -= DrawPlaneCanvasGrid_PreviewMouseLeftButtonDown;
            CanvasGrid.PreviewMouseLeftButtonDown += CanvasGrid_PreviewMouseLeftButtonDown;
            MoveRadioButton.IsChecked = true;
            foreach (Point point in TestPolygon.Points)
            {
                _polygonPositionings.Add(TranformPositioning(point) - _offsetPositioning);
            }

            var gridIndexes = GridHelper.GetPolygonGridIndex(_polygonPositionings).OrderBy(p => p.Index1).ThenBy(p => p.Index2).ThenBy(p => p.Index3).ThenBy(p => p.Index4);

            foreach (var gridIndex in gridIndexes)
            {
                _gridIndexes.Add(gridIndex);
            }

            DrawGrid();

            _isDrawing = false;
        }

        #endregion

        #endregion
    }
}
