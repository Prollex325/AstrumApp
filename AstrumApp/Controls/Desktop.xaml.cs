using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Windows.Threading;

namespace AstrumApp.Controls
{
    public partial class Desktop : UserControl
    {

        DispatcherTimer clock = new DispatcherTimer();

        private readonly DispatcherTimer _meteorTimer = new();

        public Desktop()
        {
            InitializeComponent();

            clock.Tick += Clock_Tick;
            clock.Interval = TimeSpan.FromSeconds(1);
            clock.Start();
        }

        private void Clock_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            Clock.Text = now.ToString("HH:mm");
            Date.Text = now.ToString("dddd, dd MMMM");
        }

        private void MeteorCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            _meteorTimer.Tick -= MeteorTimer_Tick;
            _meteorTimer.Tick += MeteorTimer_Tick;

            _meteorTimer.Interval = TimeSpan.FromSeconds(3);
            _meteorTimer.Start();
        }

        private void MeteorTimer_Tick(object? sender, EventArgs e)
        {
            CreateMeteor();

            _meteorTimer.Interval = TimeSpan.FromSeconds(Random.Shared.Next(15, 41));
        }

        private void MeteorCanvas_Unloaded(object sender, RoutedEventArgs e)
        {
            _meteorTimer.Stop();
            _meteorTimer.Tick -= MeteorTimer_Tick;

            MeteorCanvas.Children.Clear();
        }

        private void CreateMeteor()
        {
            if (!MeteorCanvas.IsVisible ||
                MeteorCanvas.ActualWidth <= 0 ||
                MeteorCanvas.Children.Count > 0)
                return;

            Canvas meteor = new Canvas
            {
                Width = 100,
                Height = 10,
                ClipToBounds = false,
                IsHitTestVisible = false
            };

            // Градиент хвоста
            LinearGradientBrush tailBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };

            tailBrush.GradientStops.Add(
                new GradientStop(Color.FromArgb(0, 180, 220, 255), 0));

            tailBrush.GradientStops.Add(
                new GradientStop(Color.FromArgb(150, 180, 220, 255), 0.7));

            tailBrush.GradientStops.Add(
                new GradientStop(Colors.White, 1));

            tailBrush.Freeze();

            // Хвост звезды
            Path tail = new()
            {
                Data = Geometry.Parse("M 0,5 L 96,3 L 96,7 Z"),
                Fill = tailBrush
            };

            // Яркая сердцевина
            Ellipse head = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.White
            };

            Canvas.SetLeft(head, 93.5);
            Canvas.SetTop(head, 2.5);

            meteor.Children.Add(tail);
            meteor.Children.Add(head);

            // Начальное положение
            double x = MeteorCanvas.ActualWidth * (0.25 + Random.Shared.NextDouble() * 0.15);

            double y = Random.Shared.Next(65, 90);

            Canvas.SetLeft(meteor, x - 96);
            Canvas.SetTop(meteor, y - 5);

            // Направление полёта
            double angleDegrees = 12;

            RotateTransform rotation = new RotateTransform(angleDegrees, 96, 5);

            TranslateTransform movement = new TranslateTransform();

            TransformGroup group = new TransformGroup();

            group.Children.Add(rotation);
            group.Children.Add(movement);

            meteor.RenderTransform = group;

            MeteorCanvas.Children.Add(meteor);

            // Анимация
            TimeSpan duration = TimeSpan.FromMilliseconds(Random.Shared.Next(300, 500));

            double distance = Random.Shared.Next(250, 350);
            double angle = angleDegrees * Math.PI / 180;

            DoubleAnimation flyX = new DoubleAnimation(
                0, distance * Math.Cos(angle), duration);

            DoubleAnimation flyY = new DoubleAnimation(
                0, distance * Math.Sin(angle), duration);

            DoubleAnimation fade = new DoubleAnimation(1, 0, duration)
            {
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseIn
                }
            };

            fade.Completed += (s, e) =>
            {
                MeteorCanvas.Children.Remove(meteor);
            };

            movement.BeginAnimation(
                TranslateTransform.XProperty, flyX);

            movement.BeginAnimation(
                TranslateTransform.YProperty, flyY);

            meteor.BeginAnimation(OpacityProperty, fade);
        }
    }
}
