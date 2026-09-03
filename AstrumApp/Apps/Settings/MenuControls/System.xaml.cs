using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstrumApp.Apps.Settings
{
    public partial class SystemInfo : UserControl
    {
        public ObservableCollection<InfoItem> Items { get; set; }
        public SystemInfo()
        {
            InitializeComponent();

            TimeZoneInfo localZone = TimeZoneInfo.Local;

            Items = new ObservableCollection<InfoItem>
            {
                new InfoItem
                {
                    Title = "Название",
                    Description = "AstrumApp",
                    Details = "Эксперементальное многофункциональное приложение с современным дизайном."
                },

                new InfoItem
                {
                    Title = "Версия приложения",
                    Description = "0.1.0",
                    Details = "Закрытая ранняя версия приложения."
                },

                new InfoItem
                {
                    Title = "Сборка",
                    Description = "Dev.",
                    Details = "Сборка для разработки. Может содержать ошибки и недоработки."
                },

                new InfoItem
                {
                    Title = "ОС",
                    Description = Environment.OSVersion.ToString(),
                    Details = $"Платформа: {Environment.OSVersion.Platform}\nВерсия: {Environment.OSVersion.Version}\nВерсия ОС: {Environment.OSVersion.VersionString}",
                    Ratio = 1.9
                },

                new InfoItem
                {
                    Title = "Язык",
                    Description = System.Globalization.CultureInfo.CurrentCulture.DisplayName,
                    Details = $"Текущий язык системы: {System.Globalization.CultureInfo.CurrentCulture.DisplayName}\n" +
                              $"Текущая культура системы: {System.Globalization.CultureInfo.CurrentCulture.Name}",
                    Ratio = 1.7
                }
            };

            DataContext = this;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border infoBlock = (Border)sender;

            InfoItem clickedItem = (InfoItem)infoBlock.DataContext;
            bool isOpened = clickedItem.IsOpened;
            if (!isOpened)
            {
                foreach (InfoItem sibling in Items)
                {
                    if (sibling.IsOpened)
                        sibling.Toggle();
                }
            }
            clickedItem.Toggle();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = (Border)sender;
            InfoItem item = (InfoItem)border.DataContext;
            item.UiBorder = border;
        }

        private void Path_Loaded(object sender, RoutedEventArgs e)
        {
            Path arrow = (Path)sender;
            var item = (InfoItem)arrow.DataContext;
            item.Arrow = arrow;
        }

        private void Details_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock details = (TextBlock)sender;
            var item = (InfoItem)details.DataContext;
            item.UIDetails = details;
        }
    }

    public class InfoItem
    {
        public string Title { get; set; } = "Заголовок";
        public string Description { get; set; } = "Описание";
        public string Details { get; set; } = "Детали...";
        public bool IsOpened { get; set; }
        public int Height { get; set; } = 70;
        public double Ratio { get; set; } = 1.4;
        public FrameworkElement? UiBorder { get; set; }
        public Path? Arrow { get; set; }
        public TextBlock? UIDetails { get; set; }
        public void Toggle()
        {
            IsOpened = !IsOpened;
            Height = IsOpened ? (int)(Height * Ratio) : (int)(Height / Ratio);
            Animate_Moving();
            Animate_Path();
            Animate_Details();
        }
        private void Animate_Moving()
        {
            if (UiBorder == null)
                return;

            DoubleAnimation anim = new DoubleAnimation
            {
                To = Height,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            UiBorder.BeginAnimation(Border.HeightProperty, anim);
        } 
        private void Animate_Path()
        {
            if (Arrow == null)
                return;
            var anim = new DoubleAnimation
            {
                To = IsOpened ? 180 : 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new SineEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            var rotate = Arrow.RenderTransform as RotateTransform;
            rotate?.BeginAnimation(RotateTransform.AngleProperty, anim);
        }

        private void Animate_Details()
        {
            if (UIDetails == null)
                return;
            int from = IsOpened ? 0 : 1;
            int to = IsOpened ? 1 : 0;

            DoubleAnimation opacityAnim = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(150)
            };

            UIDetails.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }
    }
}
