using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstrumApp.Controls.Seasonal
{
    public partial class GarlandControl : UserControl
    {
        private static readonly RadialGradientBrush GlassBrush = CreateGlassBrush();
        private static readonly RadialGradientBrush GlowBrush = CreateGlowBrush();
        private static readonly LinearGradientBrush SocketBrush = CreateSocketBrush();
        public GarlandControl()
        {
            InitializeComponent();
        }


        private static RadialGradientBrush CreateGlowBrush()
        {
            RadialGradientBrush brush = new RadialGradientBrush();

            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb(204, 255, 214, 147), 0));

            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb(102, 255, 190, 101), 0.3));

            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb(30, 255, 168, 73), 0.65));

            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb(0, 255, 168, 73), 1));

            brush.Freeze();

            return brush;
        }

        private static RadialGradientBrush CreateGlassBrush()
        {
            RadialGradientBrush brush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.4, 0.35)
            };

            brush.GradientStops.Add(new GradientStop(
                Colors.White, 0));

            brush.GradientStops.Add(new GradientStop(
                Color.FromRgb(255, 249, 232), 0.5));

            brush.GradientStops.Add(new GradientStop(
                Color.FromRgb(255, 228, 175), 0.8));

            brush.GradientStops.Add(new GradientStop(
                Color.FromRgb(255, 188, 96), 1));

            brush.Freeze();

            return brush;
        }

        private static LinearGradientBrush CreateSocketBrush()
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            brush.GradientStops.Add(new GradientStop(
                Color.FromRgb(41, 53, 47), 0));

            brush.GradientStops.Add(new GradientStop(
                Color.FromRgb(104, 121, 112), 0.5));

            brush.GradientStops.Add(new GradientStop(
                Color.FromRgb(41, 53, 47), 1));

            brush.Freeze();

            return brush;
        }

        private void GarlandCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (GarlandCanvas.ActualWidth <= 0)
                return;

            if (GarlandCanvas.Children.Count > 1)
                return;

            double width = GarlandCanvas.ActualWidth;
            int segments = 4, bulbsPerSegment = 8;

            double segmentWidth = width / segments;

            PathFigure figure = new PathFigure
            {
                StartPoint = new Point(0, 1)
            };

            for (int i = 0; i < segments; i++)
            {
                double startX = i * segmentWidth;
                double endX = startX + segmentWidth;
                double middleX = startX + segmentWidth / 2;

                figure.Segments.Add(new QuadraticBezierSegment
                {
                    Point1 = new Point(middleX, 65),
                    Point2 = new Point(endX, 1),
                    IsStroked = true
                });

                Point p0 = new Point(startX, 1);
                Point p1 = new Point(middleX, 65);
                Point p2 = new Point(endX, 1);

                for (int j = 0; j < bulbsPerSegment; j++)
                {
                    double t = (j + 0.5) / bulbsPerSegment;

                    Point result = GetBezierPoint(p0, p1, p2, t);

                    CreateBulb(result, j);
                }
            }

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            GarlandWire.Data = geometry;
        }

        private void CreateBulb(Point point, int num)
        {
            double scale = 0.75;

            Canvas bulb = new Canvas
            {
                Width = 24,
                Height = 28,
                IsHitTestVisible = false,
                ClipToBounds = false,
                RenderTransform = new ScaleTransform(scale, scale)
            };

            // Свечение
            Ellipse glow = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = GlowBrush
            };

            // Цоколь
            Border socket = new Border
            {
                Width = 10,
                Height = 8,
                CornerRadius = new CornerRadius(2),
                Background = SocketBrush
            };

            // Колба
            Path glass = new Path
            {
                Data = Geometry.Parse(
                    "M 8,8 " +
                    "L 16,8 " +
                    "L 16,10 " +
                    "C 24,14 21,26 12,26 " +
                    "C 3,26 0,14 8,10 Z"),
                Fill = GlassBrush
            };

            // Позиционирование
            Canvas.SetLeft(glow, -13);
            Canvas.SetTop(glow, -7);

            Canvas.SetLeft(socket, 7);
            Canvas.SetTop(socket, 0);

            // Сборка
            bulb.Children.Add(glow);
            bulb.Children.Add(socket);
            bulb.Children.Add(glass);

            // Размещение на проводе
            Canvas.SetLeft(bulb, point.X - bulb.Width * scale / 2);
            Canvas.SetTop(bulb, point.Y);

            GarlandCanvas.Children.Add(bulb);

            bool isEven = num % 2 == 0;

            AnimateOpacity(glass, isEven, 0.65);
            AnimateOpacity(glow, isEven, 0.05);
        }

        private void AnimateOpacity(UIElement element, bool isEven, double minOpacity)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = isEven ? 1 : minOpacity,
                To = isEven ? minOpacity : 1,

                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,

                EasingFunction = new SineEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private Point GetBezierPoint(Point p0, Point p1, Point p2, double t)
        {
            double u = 1 - t;

            double x = u * u * p0.X + 2 * u * t * p1.X + t * t * p2.X;

            double y = u * u * p0.Y + 2 * u * t * p1.Y + t * t * p2.Y;

            return new Point(x, y);
        }
    }
}
