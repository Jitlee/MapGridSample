﻿using System;
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

        private readonly Brush _containsBackground = new SolidColorBrush(Color.FromArgb(0x85, 0x00, 0x80, 0x00));
        private readonly Brush _level1Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xDC, 0x79, 0x1F));
        private readonly Brush _level2Brush = new SolidColorBrush(Color.FromArgb(0xff, 0x72, 0x87, 0xC8));
        private readonly Brush _level3Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xF8, 0x31, 0x0E));
        private readonly Brush _level4Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xB1, 0xDF, 0xA9));
        private readonly ObservableCollection<GridIndex> _gridIndexes = new ObservableCollection<GridIndex>();
        private readonly List<Positioning> _polylinePositionings = new List<Positioning>();
        private readonly List<Positioning> _polygonPositionings = new List<Positioning>();
        public ObservableCollection<GridIndex> GridIndexes { get { return _gridIndexes; } }

        private int _level = 1;
        private int _operation = 0;
        private Point _originPoint;
        private Positioning _offsetPositioning = new Positioning(); // 原始屏幕起始经纬度坐标
        private Positioning _ellipsePositioning = new Positioning();

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
            if (_operation == 0)
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
                    DrawLabel(1, 1, GridConfig.LEVEL1_GRID, GridConfig.Level1LongitudeCellSpan, GridConfig.Level1LatitudeCellSpan);
                    DrawShape(1, 1, GridConfig.LEVEL1_GRID, 1d);
                    DrawLevelGrid(1, 1, GridConfig.LEVEL1_GRID, _level1Brush, 1d);
                    break;
                case 2:
                    DrawLabel(1, GridConfig.LEVEL1_GRID, GridConfig.LEVEL2_GRID, GridConfig.Level2LongitudeCellSpan, GridConfig.Level2LatitudeCellSpan);
                    DrawShape(1, GridConfig.LEVEL1_GRID, GridConfig.LEVEL2_GRID, 2d);
                    DrawLevelGrid(1, GridConfig.LEVEL1_GRID, GridConfig.LEVEL2_GRID, _level2Brush, 1d);
                    DrawLevelGrid(GridConfig.LEVEL1_GRID, 1, GridConfig.LEVEL1_GRID, _level1Brush, 4d);
                    break;
                case 3:
                    DrawLabel(1, GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID, GridConfig.LEVEL3_GRID, GridConfig.Level3LongitudeCellSpan, GridConfig.Level3LatitudeCellSpan);
                    DrawShape(1, GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID, GridConfig.LEVEL3_GRID, 3d);
                    DrawLevelGrid(1, GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID, GridConfig.LEVEL3_GRID, _level3Brush, 1d);
                    DrawLevelGrid(GridConfig.LEVEL2_GRID, GridConfig.LEVEL1_GRID, GridConfig.LEVEL2_GRID, _level2Brush, 4d);
                    DrawLevelGrid(GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID, 1, GridConfig.LEVEL1_GRID, _level1Brush, 8d);
                    break;
                case 4:
                    DrawLabel(1, GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID, GridConfig.LEVEL4_GRID, GridConfig.Level4LongitudeCellSpan, GridConfig.Level4LatitudeCellSpan);
                    DrawShape(1, GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID, GridConfig.LEVEL4_GRID, 4d);
                    DrawLevelGrid(1, GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID, GridConfig.LEVEL4_GRID, _level4Brush, 1d);
                    DrawLevelGrid(GridConfig.LEVEL3_GRID, GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID, GridConfig.LEVEL3_GRID, _level3Brush, 4d);
                    DrawLevelGrid(GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID, GridConfig.LEVEL1_GRID, GridConfig.LEVEL2_GRID, _level2Brush, 8d);
                    DrawLevelGrid(GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID, 1, GridConfig.LEVEL1_GRID, _level1Brush, 16d);
                    break;
            };
        }

        private void DrawLevelGrid(int flag1, int flag2, int levelGrid, Brush brush, double thickness)
        {
            double cellWidth = WIDTH * flag1 / levelGrid;
            double cellHeight = HEIGHT * flag1 / levelGrid;
            double offsetX = _offsetPositioning.Longitude * WIDTH * flag1 * flag2 / GridConfig.LongitudeSpan;
            double offsetY = _offsetPositioning.Latitude * HEIGHT * flag1 * flag2 / GridConfig.LatitudeSpan;
            double beginX = offsetX % cellWidth;
            double beginY = offsetY % cellHeight;
            double x1 = -WIDTH;
            double x2 = 2d * WIDTH;
            double y1 = -HEIGHT;
            double y2 = 2d * HEIGHT;

            for (int i = -1; i <= levelGrid + 1; i++)
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

            for (int i = -1; i <= levelGrid + 1; i++)
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

        private void DrawLabel(int flag1, int flag2, int levelGrid, double longitudeCellWidth, double latitudeCellHeight)
        {
            double cellWidth = WIDTH * flag1 / levelGrid;
            double cellHeight = HEIGHT * flag1 / levelGrid;
            double offsetX = _offsetPositioning.Longitude * WIDTH * flag1 * flag2 / GridConfig.LongitudeSpan;
            double offsetY = _offsetPositioning.Latitude * HEIGHT * flag1 * flag2 / GridConfig.LatitudeSpan;
            double beginX = offsetX % cellWidth;
            double beginY = offsetY % cellHeight;

            Positioning positioning = new Positioning();
            for(int row = -1; row <= levelGrid + 1;row++)
            {
                double y = row * cellHeight + beginY;
                for (int col = -1; col < levelGrid + 1; col++)
                {
                    double x = col * cellWidth + beginX;
                    bool isContains = false;
                    // 多加 1/2 的值是为了 避免 刚好在边界点因为小数点误差而计算错误
                    positioning = TranformPositioning(x - offsetX + cellWidth / 2d, y - offsetY + cellHeight / 2d); 

                    if (positioning.Latitude > 0d
                        && positioning.Longitude > 0d
                        && positioning.Latitude < GridConfig.LatitudeSpan
                        && positioning.Longitude < GridConfig.LongitudeSpan)
                    {
                        StackPanel stackPanel = new StackPanel();
                        stackPanel.Orientation = Orientation.Horizontal;
                        stackPanel.VerticalAlignment = VerticalAlignment.Center;
                        stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                        stackPanel.Children.Add(new TextBlock() { Text = "(" });

                        int lv1Index = positioning.TranformGridIndex(GridConfig.LEVEL1_GRID, GridConfig.Level1LongitudeCellSpan, GridConfig.Level1LatitudeCellSpan);

                        stackPanel.Children.Add(new TextBlock() { Text = lv1Index.ToString(), Foreground = _level1Brush });

                        if (_level > 1)
                        {
                            int lv2Index = positioning.TranformGridIndex(GridConfig.LEVEL2_GRID, GridConfig.Level2LongitudeCellSpan, GridConfig.Level2LatitudeCellSpan);
                            stackPanel.Children.Add(new TextBlock() { Text = ", " });
                            stackPanel.Children.Add(new TextBlock() { Text = lv2Index.ToString(), Foreground = _level2Brush });
                            if (_level > 2)
                            {
                                int lv3Index = positioning.TranformGridIndex(GridConfig.LEVEL3_GRID, GridConfig.Level3LongitudeCellSpan, GridConfig.Level3LatitudeCellSpan);

                                stackPanel.Children.Add(new TextBlock() { Text = ", " });
                                stackPanel.Children.Add(new TextBlock() { Text = lv3Index.ToString(), Foreground = _level3Brush });
                                if (_level > 3)
                                {
                                    int lv4Index = positioning.TranformGridIndex(GridConfig.LEVEL4_GRID, GridConfig.Level4LongitudeCellSpan, GridConfig.Level4LatitudeCellSpan);

                                    stackPanel.Children.Add(new TextBlock() { Text = ", " });
                                    stackPanel.Children.Add(new TextBlock() { Text = lv4Index.ToString(), Foreground = _level4Brush });

                                    if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index && g.Index3 == lv3Index && g.Index4 == lv4Index))
                                    {
                                        isContains = true;
                                    }
                                }
                                else if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index && g.Index3 == lv3Index))
                                {
                                    isContains = true;
                                }
                            }
                            else if (_gridIndexes.Any(g => g.Index1 == lv1Index && g.Index2 == lv2Index))
                            {
                                isContains = true;
                            }
                        }
                        else if(_gridIndexes.Any(g=> g.Index1 == lv1Index))
                        {
                            isContains = true;
                        }
                        stackPanel.Children.Add(new TextBlock() { Text = ")" });

                        Border border = new Border();
                        border.Child = stackPanel;
                        border.Height = cellHeight;
                        border.Width = cellWidth;
                        border.SetValue(Canvas.LeftProperty, x);
                        border.SetValue(Canvas.TopProperty, y);
                        if (isContains)
                        {
                            border.Background = _containsBackground;
                        }
                        else
                        {
                            border.Background = Brushes.White;
                        }
                        GridCanvas.Children.Add(border);
                    }
                }
            }
        }

        private void DrawShape(int flag1, int flag2, int levelGrid, double thickness)
        {
            double offsetX = _offsetPositioning.Longitude * WIDTH * flag1 * flag2 / GridConfig.LongitudeSpan;
            double offsetY = _offsetPositioning.Latitude * HEIGHT * flag1 * flag2 / GridConfig.LatitudeSpan;

            Point ellipsLocation = TranformPositioning(_ellipsePositioning);
            TestEllipse.Width = TestEllipse.Height = _level * 4d;
            Canvas.SetLeft(TestEllipse, ellipsLocation.X - _level * 2d + offsetX);
            Canvas.SetTop(TestEllipse, ellipsLocation.Y - _level * 2d + offsetY);

            TestPolyline.Points.Clear();
            TestPolyline.StrokeThickness = thickness;
            foreach (Positioning positioning in _polylinePositionings)
            {
                Point point = TranformPositioning(positioning);
                point.X += offsetX;
                point.Y += offsetY;
                TestPolyline.Points.Add(point);
            }

        }

        private void DrawHighlightRectangle(int flag1, int flag2, int levelGrid, Brush brush)
        {
            double cellWidth = WIDTH * flag1 / levelGrid;
            double cellHeight = HEIGHT * flag1 / levelGrid;
            double offsetX = _offsetPositioning.Longitude * WIDTH * flag1 * flag2 / GridConfig.LongitudeSpan;
            double offsetY = _offsetPositioning.Latitude * HEIGHT * flag1 * flag2 / GridConfig.LatitudeSpan;
            double beginX = offsetX % cellWidth;
            double beginY = offsetY % cellHeight;


        }

        private Positioning TranformPositioning(Vector vector)
        {
            return TranformPositioning(vector.X, vector.Y);
        }

        private Positioning TranformPositioning(Point point)
        {
            return TranformPositioning(point.X, point.Y);
        }

        private Positioning TranformPositioning(double x, double y)
        {
            Positioning positioning = new Positioning();
            switch (_level)
            {
                case 1:
                    positioning.Longitude = GridConfig.LongitudeSpan * x / WIDTH;
                    positioning.Latitude = GridConfig.LatitudeSpan * y / HEIGHT;
                    break;
                case 2:
                    positioning.Longitude = GridConfig.LongitudeSpan * x / (WIDTH * GridConfig.LEVEL1_GRID);
                    positioning.Latitude = GridConfig.LatitudeSpan * y / (HEIGHT * GridConfig.LEVEL1_GRID);
                    break;
                case 3:
                    positioning.Longitude = GridConfig.LongitudeSpan * x / (WIDTH * GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID);
                    positioning.Latitude = GridConfig.LatitudeSpan * y / (HEIGHT * GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID);
                    break;
                case 4:
                    positioning.Longitude = GridConfig.LongitudeSpan * x / (WIDTH * GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID);
                    positioning.Latitude = GridConfig.LatitudeSpan * y / (HEIGHT * GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID);
                    break;
            };
            return positioning;
        }

        private Point TranformPositioning(Positioning positioning)
        {
            Point point = new Point();
            switch (_level)
            {
                case 1:
                    point.X = positioning.Longitude * WIDTH /  GridConfig.LongitudeSpan;
                    point.Y = positioning.Latitude * HEIGHT / GridConfig.LatitudeSpan;
                    break;
                case 2:
                    point.X = positioning.Longitude * WIDTH * GridConfig.LEVEL1_GRID / GridConfig.LongitudeSpan;
                    point.Y = positioning.Latitude * HEIGHT * GridConfig.LEVEL1_GRID / GridConfig.LatitudeSpan;
                    break;
                case 3:
                    point.X = positioning.Longitude * WIDTH * GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID / GridConfig.LongitudeSpan;
                    point.Y = positioning.Latitude * HEIGHT * GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID / GridConfig.LatitudeSpan;
                    break;
                case 4:
                    point.X = positioning.Longitude * WIDTH * GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID / GridConfig.LongitudeSpan;
                    point.Y = positioning.Latitude * HEIGHT * GridConfig.LEVEL1_GRID * GridConfig.LEVEL2_GRID * GridConfig.LEVEL3_GRID / GridConfig.LatitudeSpan;
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
            }
        }

        private void ResetShape()
        {
            _gridIndexes.Clear();
            _polylinePositionings.Clear();
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

            var gridIndexes = GridHelper.GetPolyLineRegion(_polylinePositionings).OrderBy(p => p.Index1).ThenBy(p => p.Index2).ThenBy(p => p.Index3).ThenBy(p => p.Index4);
            
            foreach (var gridIndex in gridIndexes)
            {
                _gridIndexes.Add(gridIndex);
            }

            DrawGrid();
        }

        #endregion

        #endregion
    }
}
