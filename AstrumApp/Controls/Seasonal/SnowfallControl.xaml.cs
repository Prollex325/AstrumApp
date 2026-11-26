using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace AstrumApp.Controls.Seasonal
{
    public partial class SnowfallControl : UserControl
    {
        private readonly Random _random = new Random();

        private static readonly RadialGradientBrush GlowBrush =
            CreateGlowBrush();

        public int SnowflakeCount { get; set; } = 230;

        public SnowfallControl()
        {
            InitializeComponent();
        }

        private static RadialGradientBrush CreateGlowBrush()
        {
            RadialGradientBrush brush = new RadialGradientBrush();

            brush.GradientStops.Add(new GradientStop(
                Colors.White, 0));

            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb(230, 220, 240, 255), 0.25));

            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb(60, 130, 190, 255), 0.55));

            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb(0, 130, 190, 255), 1));

            brush.Freeze();

            return brush;
        }

        private void CreateSnowflake()
        {
            double depth = _random.NextDouble();

            double size = 1 + depth * 4;
            double opacity = 0.15 + depth * 0.55;
            double duration = 15 - depth * 10;

            double x = -120 + _random.NextDouble()
                * (SnowCanvas.ActualWidth + 120);

            Ellipse snowflake = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = Brushes.White,
                Opacity = opacity
            };

            if (_random.NextDouble() < 0.1)
            {
                snowflake.Fill = GlowBrush;
                snowflake.Width *= 3;
                snowflake.Height *= 3;
            }

            Canvas.SetLeft(snowflake, x);
            Canvas.SetTop(snowflake, 0);

            TranslateTransform windTransform = new TranslateTransform();
            TranslateTransform swayTransform = new TranslateTransform();
            TranslateTransform fallTransform = new TranslateTransform();

            TransformGroup group = new TransformGroup();

            group.Children.Add(windTransform);
            group.Children.Add(swayTransform);
            group.Children.Add(fallTransform);

            snowflake.RenderTransform = group;

            SnowCanvas.Children.Add(snowflake);

            double startOffset = -_random.NextDouble() * duration;

            DoubleAnimation fallAnimation = new DoubleAnimation
            {
                From = -snowflake.Height,
                To = SnowCanvas.ActualHeight,
                Duration = TimeSpan.FromSeconds(duration),
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(startOffset)
            };

            double windDrift = _random.Next(50, 150);

            DoubleAnimation windAnimation = new DoubleAnimation
            {
                From = 0,
                To = windDrift,
                Duration = TimeSpan.FromSeconds(duration),
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(startOffset)
            };

            double swayDrift = 3 + _random.NextDouble() * 8;
            double swayDuration = 2.5 + _random.NextDouble() * 3.5;

            DoubleAnimation swayAnimation = new DoubleAnimation
            {
                From = -swayDrift,
                To = swayDrift,
                Duration = TimeSpan.FromSeconds(swayDuration),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,

                BeginTime = TimeSpan.FromSeconds(
                    -_random.NextDouble() * swayDuration * 2),

                EasingFunction = new SineEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            fallTransform.BeginAnimation(
                TranslateTransform.YProperty, fallAnimation);

            windTransform.BeginAnimation(
                TranslateTransform.XProperty, windAnimation);

            swayTransform.BeginAnimation(
                TranslateTransform.XProperty, swayAnimation);
        }

        public void Start()
        {
            if (SnowCanvas.Children.Count > 0)
                return;

            for (int i = 0; i < SnowflakeCount; i++)
            {
                CreateSnowflake();
            }
        }

        public void Stop()
        {
            foreach (UIElement element in SnowCanvas.Children)
            {
                if (element.RenderTransform is TransformGroup group)
                {
                    foreach (Transform child in group.Children)
                    {
                        if (child is TranslateTransform transform)
                        {
                            transform.BeginAnimation(
                                TranslateTransform.XProperty, null);

                            transform.BeginAnimation(
                                TranslateTransform.YProperty, null);
                        }
                    }
                }
            }

            SnowCanvas.Children.Clear();
        }

        private void SnowfallControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsVisible)
                Start();
        }

        private void SnowfallControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void SnowfallControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (IsVisible)
                Start();
            else
                Stop();
        }
    }
}