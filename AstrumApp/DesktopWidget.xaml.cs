using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AstrumApp
{
    public partial class DesktopWidget : Window
    {
        public DesktopWidget()
        {
            InitializeComponent();
            Loaded += DesktopLoaded;

            Loaded += DesktopWidget_Loaded;
        }

        private void DesktopWidget_Loaded(object sender, RoutedEventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            var hwnd = helper.Handle;

            // Снимаем все стили рамки, чтобы окно выглядело как настоящий виджет
            helper.EnsureHandle(); // на всякий случай

            // Убираем из Alt+Tab и панели задач
            this.ShowInTaskbar = false;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;        // если нужно прозрачное окно
            this.Background = Brushes.Transparent; // или нужный тебе фон

            DesktopHelper.AttachToDesktop(hwnd);

            // Позиционирование — обязательно **после** SetParent
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                var area = SystemParameters.WorkArea;

                this.Left = area.Right - this.Width - 10;
                this.Top = 10;

                // Принудительно показываем и перерисовываем
                this.Show();
                this.Activate();

                // Иногда помогает
                //SetWindowPos(hwnd, IntPtr.Zero, (int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height,
                             //SWP_SHOWWINDOW | SWP_NOACTIVATE);
            }));
        }

        private async void DesktopLoaded(object? sender, RoutedEventArgs e)
        {
            var weatherService = new WeatherService();
            Dictionary<string, object> weather = await weatherService.GetWeather("Almaty", "");
            TextFilling(weather);
        }

        public void TextFilling(Dictionary<string, object> weather)
        {
            if (weather == null) { return; }

            if (weather.ContainsKey("city")) WeatherCity.Text += weather["city"].ToString();
            if (weather.ContainsKey("desc")) WeatherDesc.Text += weather["desc"].ToString();
            if (weather.ContainsKey("temp")) WeatherTemp.Text += weather["temp"].ToString();
            if (weather.ContainsKey("feels")) WeatherFeels.Text += weather["feels"].ToString();
            if (weather.ContainsKey("humidity")) WeatherHumidity.Text += weather["humidity"].ToString();
            if (weather.ContainsKey("wind")) WeatherWind.Text += weather["wind"].ToString();
        }
    }
}

