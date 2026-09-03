using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using static System.Net.WebRequestMethods;

namespace AstrumApp.Controls
{
    public partial class DragAndDrop : UserControl
    {
        private SolidColorBrush _borderBrush;
        private SolidColorBrush _backgroundBrush;

        private Color _baseBorderColor;
        private Color _baseBackgroundColor;

        private Color _hoverBorderColor;
        private Color _hoverBackgroundColor;

        private Color _activeBorderColor;
        private Color _activeBackgroundColor;

        private string[] _allowedExtensions;

        public event EventHandler<string[]>? FilesDropped;

        public DragAndDrop()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                _borderBrush = new SolidColorBrush(((SolidColorBrush)DropZoneBorderBrush).Color);
                _backgroundBrush = new SolidColorBrush(((SolidColorBrush)DropZoneBackground).Color);

                DropZoneBorder.BorderBrush = _borderBrush;
                DropZoneBorder.Background = _backgroundBrush;

                _baseBorderColor = _borderBrush.Color;
                _baseBackgroundColor = _backgroundBrush.Color;

                _hoverBorderColor = Lighten(_baseBorderColor, 0.1);
                _hoverBackgroundColor = Lighten(_baseBackgroundColor, 0.1);

                _activeBorderColor = Lighten(_baseBorderColor, 0.2);
                _activeBackgroundColor = Lighten(_baseBackgroundColor, 0.2);

                _allowedExtensions = AllowedExtensions.Split(";");
            };
        }

        public static readonly DependencyProperty DropZoneBorderBrushProperty =
            DependencyProperty.Register(
                nameof(DropZoneBorderBrush),
                typeof(Brush),
                typeof(DragAndDrop),
                new PropertyMetadata(
                    new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55))));
        public Brush DropZoneBorderBrush
        {
            get => (Brush)GetValue(DropZoneBorderBrushProperty);
            set => SetValue(DropZoneBorderBrushProperty, value);
        }

        public static readonly DependencyProperty DropZoneBackgroundProperty =
            DependencyProperty.Register(
                nameof(DropZoneBackground),
                typeof(Brush),
                typeof(DragAndDrop),
                new PropertyMetadata(Brushes.White));
        public Brush DropZoneBackground
        {
            get => (Brush)GetValue(DropZoneBackgroundProperty);
            set => SetValue(DropZoneBackgroundProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(DragAndDrop),
            new PropertyMetadata("Перетащите файл"));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty MaxFilesCountProperty =
        DependencyProperty.Register(
            nameof(MaxFilesCount),
            typeof(int),
            typeof(DragAndDrop),
            new PropertyMetadata(1));

        public int MaxFilesCount
        {
            get => (int)GetValue(MaxFilesCountProperty);
            set => SetValue(MaxFilesCountProperty, value);
        }

        public static readonly DependencyProperty AllowedExtensionsProperty =
        DependencyProperty.Register(
            nameof(AllowedExtensions),
            typeof(string),
            typeof(DragAndDrop),
            new PropertyMetadata(".png;.jpg;.jpeg;.bmp;.ico"));


        public string AllowedExtensions
        {
            get => (string)GetValue(AllowedExtensionsProperty);
            set => SetValue(AllowedExtensionsProperty, value);
        }

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bool isValid = IsValidDrop(e);
                e.Effects = isValid ? DragDropEffects.Copy : DragDropEffects.None;
                e.Handled = true;

                if (isValid)
                {
                    _borderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
                    {
                        To = _activeBorderColor,
                        Duration = TimeSpan.FromMilliseconds(200)
                    });

                    _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
                    {
                        To = _activeBackgroundColor,
                        Duration = TimeSpan.FromMilliseconds(200)
                    });
                }
            } else e.Effects = DragDropEffects.None;
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            _borderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation 
            {
                To = _baseBorderColor,
                Duration = TimeSpan.FromMilliseconds(200)
            });

            _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
            {
                To = _baseBackgroundColor,
                Duration = TimeSpan.FromMilliseconds(200)
            });
        }

        private void DropZone_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = IsValidDrop(e) ? DragDropEffects.Copy : DragDropEffects.None;

            e.Handled = true;
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            DropZone_DragLeave(sender, e);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length <= MaxFilesCount) if (IsValidDrop(e)) FilesDropped?.Invoke(this, files);
                else MessageBox.Show($"Вы можете загрузить не более {MaxFilesCount} файлов.", MessageBoxButton.OK, 
                                    MessageBoxType.Error);
            }
        }

        private void DropZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _borderBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);

            _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);

            _backgroundBrush.Color = _activeBackgroundColor;
            _borderBrush.Color = _activeBorderColor;
        }

        private void DropZone_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _backgroundBrush.Color = _hoverBackgroundColor;
            _borderBrush.Color = _hoverBorderColor;

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "Выберите изображение";
            dialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.webp";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string path = dialog.FileName;
                FilesDropped?.Invoke(this, [path]);
            }
        }

        private void DropZone_MouseEnter(object sender, MouseEventArgs e)
        {
            _borderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
            {
                To = _hoverBorderColor,
                Duration = TimeSpan.FromMilliseconds(200)
            });

            _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
            {
                To = _hoverBackgroundColor,
                Duration = TimeSpan.FromMilliseconds(200)
            });
        }

        private void DropZone_MouseLeave(object sender, MouseEventArgs e)
        {
            _borderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
            {
                To = _baseBorderColor,
                Duration = TimeSpan.FromMilliseconds(200)
            });

            _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
            {
                To = _baseBackgroundColor,
                Duration = TimeSpan.FromMilliseconds(200)
            });
        }

        private bool IsValidDrop(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return false;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            return files.All(file =>
                _allowedExtensions.Contains(
                    Path.GetExtension(file).ToLowerInvariant()));
        }
        private static Color Lighten(Color c, double factor)
        {
            byte L(byte v) => (byte)Math.Min(255, v + 255 * factor);

            return Color.FromRgb(L(c.R), L(c.G), L(c.B));
        }
    }
}
