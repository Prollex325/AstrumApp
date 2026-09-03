using AstrumApp.Apps.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class LeftPanel : UserControl
    {
        DispatcherTimer hideTimer = new DispatcherTimer();
        public ObservableCollection<NavButton> Items { get; set; }
        bool opened = false;

        public LeftPanel()
        {
            InitializeComponent();

            hideTimer.Interval = TimeSpan.FromMilliseconds(700);
            hideTimer.Tick += HideTimer_Tick;

            Items = new ObservableCollection<NavButton>
            {
                new NavButton
                {
                    Title = "Параметры",
                    IconPath = "/Assets/icons/Navigator/setting-lines.png"
                },

                new NavButton
                {
                    Title = "Приложения",
                    IconPath = "/Assets/icons/Navigator/apps.png"
                },

                new NavButton
                {
                    Title = "Игры",
                    IconPath = "/Assets/icons/Navigator/games.png"
                }
            };

            DataContext = this;
        }

        private void Trigger_MouseEnter(object sender, MouseEventArgs e)
        {
            double to = -340;
            LeftPanel_Moving(to);
        }

        private void Trigger_MouseLeave(object sender, MouseEventArgs e)
        {
            hideTimer.Start();
        }

        private void LeftPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            opened = true;
            double to = -40;
            LeftPanel_Moving(to);
        }

        private void LeftPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            opened = false;
            double to = -340;
            LeftPanel_Moving(to);
            hideTimer.Start();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 0.7;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 1;
            MainWindow _mainWindow = Application.Current.MainWindow as MainWindow;
            _mainWindow?.opensets();
        }

        private void LeftPanel_Moving(double to)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            LeftPanelContent_Transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void HideTimer_Tick(object? sender, EventArgs e)
        {
            hideTimer.Stop();

            if (!opened)
            {
                LeftPanel_Moving(-500);
            }
        }
    }
    public class NavButton 
    {
        public string Title { get; set; } = "Упс.. не указано";
        public string IconPath { get; set; } = "/Assets/icons/default-icon.png";
    }
}
