using AstrumApp.Apps.Settings.MenuControls;
using AstrumApp.Services;
using AstrumApp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.ComponentModel;
using System.Diagnostics;

namespace AstrumApp.Controls
{
    public enum AvatarEditMode
    {
        ProfileEdit,
        Registration
    }

    public partial class CropAndSaveImage : UserControl
    {
        private bool _dragging;

        private bool _userTouchedZoom = false;

        private Point _startMouse;

        private double minX, maxX, minY, maxY = 0;

        private double MinScale;
        private const double MaxScale = 3.0;

        private bool isSaved = false;
        private bool _hasChanges = false;

        public event Action<BitmapSource>? AvatarSaved;
        public event Action? AvatarDeleted;
        public event Action? EditCancelled;
        public event Action? EditBack;

        public CropAndSaveImage()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;


            DragAndDropControl.FilesDropped += (s, files) =>
            {
                LoadImage(files[0]);
            };

            Loaded += (s, e) =>
            {
                UpdateMode();
            };

            this.DataContext = App.Session.Profile;
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(CropAndSaveImage),
            new PropertyMetadata("Аватар")
        );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public AvatarEditMode Mode
        {
            get => (AvatarEditMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(
                nameof(Mode),
                typeof(AvatarEditMode),
                typeof(AvatarEdit),
                new PropertyMetadata(AvatarEditMode.ProfileEdit));

        private void UpdateMode()
        {
            bool isRegistrationMode = Mode == AvatarEditMode.Registration;

            BackButton.ToolTip = "Назад";
            NextButton.ToolTip = "Далее";

            if (isRegistrationMode)
            {
                DeleteButton.Visibility = Visibility.Collapsed;
                BackButton.Visibility = Visibility.Visible;
                NextButton.Visibility = Visibility.Visible;
            }
            else
            {
                DeleteButton.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Visible;
                NextButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadImage(string path)
        {
            BitmapImage bitmap = new BitmapImage(new Uri(path));

            AvatarImage.Source = bitmap;

            MinScale = UpdateImageBounds(0);

            ZoomSlider.Maximum = MaxScale; ZoomSlider.Minimum = MinScale; ZoomSlider.Value = MinScale;

            _userTouchedZoom = true;
        }

       
        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            var cropped = GetCroppedAvatar(250);

            BitmapSource image = cropped;

            AvatarSaved?.Invoke(image);
        }

        private void DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            AvatarDeleted?.Invoke();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            EditBack?.Invoke();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            EditCancelled?.Invoke();
        }

        private void Avatar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragging = true;

            _startMouse = e.GetPosition(AvatarEditor);

            Mouse.Capture(AvatarEditor);
        }

        private void Avatar_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
                return;

            Point current = e.GetPosition(AvatarEditor);
            Vector delta = current - _startMouse;

            AvatarTranslate.X += delta.X;
            AvatarTranslate.Y += delta.Y;

            ClampPosition();

            _startMouse = current;
        }

        private void Avatar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
            Mouse.Capture(null);
        }

        private void Avatar_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (AvatarImage.Source == null)
                return;

            double oldScale = AvatarScale.ScaleX;

            double factor = e.Delta > 0 ? 1.1 : 0.9;
            double newScale = Math.Clamp(oldScale * factor, MinScale, MaxScale);

            if (Math.Abs(newScale - oldScale) < 0.0001)
                return;

            Point mouse = e.GetPosition(AvatarEditor);

            AvatarTranslate.X = mouse.X - (mouse.X - AvatarTranslate.X) * (newScale / oldScale);
            AvatarTranslate.Y = mouse.Y - (mouse.Y - AvatarTranslate.Y) * (newScale / oldScale);

            AvatarScale.ScaleX = newScale;
            AvatarScale.ScaleY = newScale;

            ZoomSlider.Value = newScale;

            UpdateImageBounds(newScale);
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = (Border)sender;

            switch (border.Tag?.ToString())
            {
                case "Minus":
                    ZoomSlider.Value -= 0.1;
                    break;

                case "Plus":
                    ZoomSlider.Value += 0.1;
                    break;
            }
        }

        private void Zoom_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ZoomSlider.Value = Math.Min(ZoomSlider.Value * 1.1, ZoomSlider.Maximum);
            else
                ZoomSlider.Value = Math.Max(ZoomSlider.Value * 0.9, ZoomSlider.Minimum);
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double newScale = ZoomSlider.Value;

            AvatarTranslate.X = AvatarEditor.ActualWidth / 2 - (AvatarEditor.ActualWidth / 2 - AvatarTranslate.X) * (newScale / AvatarScale.ScaleX);
            AvatarTranslate.Y = AvatarEditor.ActualHeight / 2 - (AvatarEditor.ActualHeight / 2 - AvatarTranslate.Y) * (newScale / AvatarScale.ScaleX);

            AvatarScale.ScaleX = newScale;
            AvatarScale.ScaleY = newScale;

            UpdateImageBounds(newScale);
        }

        private double UpdateImageBounds(double newScale)
        {
            if (AvatarImage.Source == null) return 0;

            var bmp = AvatarImage.Source as BitmapImage;
            if (bmp == null) return 0;

            double viewportWidth = AvatarEditor.ActualWidth;
            double viewportHeight = AvatarEditor.ActualHeight;

            double scale = newScale != 0 ? newScale : Math.Max(
                viewportWidth / bmp.Width,
                viewportHeight / bmp.Height);

            double scaledWidth = bmp.Width * scale;
            double scaledHeight = bmp.Height * scale;

            if (!_userTouchedZoom)
            {
                AvatarScale.ScaleX = scale;
                AvatarScale.ScaleY = scale;

                AvatarTranslate.X = (viewportWidth - scaledWidth) / 2;
                AvatarTranslate.Y = (viewportHeight - scaledHeight) / 2;
            }

            minX = viewportWidth - scaledWidth;

            minY = viewportHeight - scaledHeight;

            if (scaledWidth < viewportWidth)
            {
                minX = maxX = (viewportWidth - scaledWidth) / 2;
            }
            if (scaledHeight < viewportHeight)
            {
                minY = maxY = (viewportHeight - scaledHeight) / 2;
            }

            _hasChanges = true;

            ClampPosition();

            return scale;
        }

        private void ClampPosition()
        {
            AvatarTranslate.X = Math.Clamp(AvatarTranslate.X, minX, maxX);
            AvatarTranslate.Y = Math.Clamp(AvatarTranslate.Y, minY, maxY);
        }

        private BitmapSource? GetCroppedAvatar(int targetSize = 256)
        {
            if (AvatarImage.Source == null) return null;

            var render = new RenderTargetBitmap(
                targetSize, targetSize, 96, 96, PixelFormats.Pbgra32);

            var visual = new DrawingVisual();

            using (var dc = visual.RenderOpen())
            {
                // круглая маска
                dc.PushClip(new EllipseGeometry(
                    new Point(targetSize / 2, targetSize / 2),
                    targetSize / 2,
                    targetSize / 2));

                double scaleFactor = targetSize / AvatarEditor.ActualWidth;

                var transform = new TransformGroup();

                // ВАЖНО: берём ТОЛЬКО UI transform
                transform.Children.Add(new ScaleTransform(
                    AvatarScale.ScaleX * scaleFactor,
                    AvatarScale.ScaleY * scaleFactor));

                transform.Children.Add(new TranslateTransform(
                    AvatarTranslate.X * scaleFactor,
                    AvatarTranslate.Y * scaleFactor));

                dc.PushTransform(transform);

                dc.DrawImage(
                    AvatarImage.Source,
                    new Rect(0, 0,
                        AvatarImage.Source.Width,
                        AvatarImage.Source.Height));

                dc.Pop();
                dc.Pop();
            }

            render.Render(visual);
            return render;
        }

        public void Unload(object sender, RoutedEventArgs e)
        {
            AvatarImage.Source = null;
            ZoomSlider.Value = 1;
            _hasChanges = false;
            isSaved = false;
        }

        public bool CanClose()
        {
            return isSaved || !_hasChanges;
        }
    }
}
