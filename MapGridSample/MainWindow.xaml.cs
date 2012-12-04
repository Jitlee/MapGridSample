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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapGridSample
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        const double GRID_SIZE = 20;
        public MainWindow()
        {
            InitializeComponent();
            
            for (int i = 0; i <= 18; i++)
            {
                var y = i * GRID_SIZE;
                this.GridCanvas.Children.Add(new Line() {
                    X1 = 0,
                    X2 = 720,
                    Y1 = y,
                    Y2 = y,
                    Stroke = Brushes.Silver,
                    StrokeThickness = 1,
                    Opacity = 0.5
                });
            }

            for (int i = 0; i <= 36; i++)
            {
                var x = i * GRID_SIZE;
                this.GridCanvas.Children.Add(new Line()
                {
                    X1 = x,
                    X2 = x,
                    Y1 = 0,
                    Y2 = 360,
                    Stroke = Brushes.Silver,
                    StrokeThickness = 1,
                    Opacity = 0.5
                });
            }
        }

        private bool _isDrawing;
        private void CellCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing)
            {
                CellCanvas.CaptureMouse();
                CellCanvas.MouseMove += CellCanvas_PreviewMouseMove;
                CellCanvas.PreviewMouseRightButtonDown += CellCanvas_PreviewMouseRightButtonDown;
                TestPolyline.Points.Clear();
                TestPolygon.Points.Clear();
                CellCanvas.Children.Clear();
                _isDrawing = true;
                if (LineRadioButton.IsChecked == true)
                {
                    TestPolyline.Points.Add(e.GetPosition(CellCanvas));
                }
                else
                {
                    TestPolygon.Points.Add(e.GetPosition(CellCanvas));
                }
            }
            if (LineRadioButton.IsChecked == true)
            {
                TestPolyline.Points.Add(e.GetPosition(CellCanvas));
            }
            else
            {
                TestPolygon.Points.Add(e.GetPosition(CellCanvas));
            }
        }

        private void CellCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            if (LineRadioButton.IsChecked == true)
            {

                TestPolyline.Points.RemoveAt(TestPolyline.Points.Count - 1);

                TestPolyline.Points.Add(e.GetPosition(CellCanvas));
            }
            else
            {

                TestPolygon.Points.RemoveAt(TestPolygon.Points.Count - 1);

                TestPolygon.Points.Add(e.GetPosition(CellCanvas));
            }
        }

        private void CellCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            CellCanvas.MouseMove -= CellCanvas_PreviewMouseMove;
            CellCanvas.PreviewMouseRightButtonDown -= CellCanvas_PreviewMouseRightButtonDown;
            CellCanvas.ReleaseMouseCapture();

            var cells = LineRadioButton.IsChecked == true ? GetPolyLineRegion(TestPolyline.Points) : GetPolygonRegion(TestPolygon.Points);

            foreach (var cell in cells)
            {
                var grid = new Rectangle();
                grid.Fill = Brushes.Green;
                Canvas.SetLeft(grid, cell.X * GRID_SIZE);
                Canvas.SetTop(grid, cell.Y * GRID_SIZE);
                grid.Width = grid.Height = GRID_SIZE;
                CellCanvas.Children.Add(grid);
            }
        }

        private IEnumerable<Point> GetPolygonRegion(PointCollection points)
        {
            var maxX = Math.Floor(points.Max(p => p.X) / GRID_SIZE);
            var maxY = Math.Floor(points.Max(p => p.Y) / GRID_SIZE);
            var minX = Math.Floor(points.Min(p => p.X) / GRID_SIZE);
            var minY = Math.Floor(points.Min(p => p.Y) / GRID_SIZE);

            for (double x = minX; x <= maxX; x++)
            {
                for (double y = minY; y <= maxY; y++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        private IEnumerable<Point> GetPolyLineRegion(PointCollection points)
        {
            var result = new List<Point>();
            var prev = points.First();
            for(int i = 1; i< points.Count; i++)
            {
                result.AddRange(GetLineRegion(prev, points[i]));
                prev = points[i];
            }
            return result.Distinct();
        }

        private IEnumerable<Point> GetLineRegion(Point startPoint, Point endPoint)
        {
            //  一次函数公式: kx + b = y
            var k = (startPoint.Y - endPoint.Y) / (startPoint.X - endPoint.X);
            var b = startPoint.Y - k * startPoint.X;

            if (!double.IsInfinity(k))
            {
                var maxX = Math.Floor(Math.Max(startPoint.X, endPoint.X) / GRID_SIZE);
                var minX = Math.Ceiling(Math.Min(startPoint.X, endPoint.X) / GRID_SIZE);
                for (double x = minX; x <= maxX; x++)
                {
                    var y = Math.Floor((k * x * GRID_SIZE + b) / GRID_SIZE);
                    yield return new Point(x, y);
                    yield return new Point(x - 1, y);
                }
            }

            if (k != 0)
            {
                var maxY = Math.Floor((Math.Max(startPoint.Y, endPoint.Y) / GRID_SIZE));
                var minY = Math.Ceiling(Math.Min(startPoint.Y, endPoint.Y) / GRID_SIZE);
                var isInfinity = double.IsInfinity(k);
                var fixedX = Math.Floor(endPoint.X / GRID_SIZE);

                for (double y = minY; y <= maxY; y++)
                {
                    var x = isInfinity ? fixedX : Math.Floor((y * GRID_SIZE - b) / (k * GRID_SIZE));
                    yield return new Point(x, y);
                    yield return new Point(x, y - 1);
                }
            }

            yield return new Point(Math.Floor(startPoint.X / GRID_SIZE), Math.Floor(startPoint.Y / GRID_SIZE));
            yield return new Point(Math.Floor(endPoint.X / GRID_SIZE), Math.Floor(endPoint.Y / GRID_SIZE));
        }
    }
}
