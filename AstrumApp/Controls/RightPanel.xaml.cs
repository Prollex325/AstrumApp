
using AstrumApp.Services;
using System.Windows;
using System.Windows.Controls;

namespace AstrumApp.Controls
{
    public partial class RightPanel : UserControl
    {
        public RightPanel()
        {
            InitializeComponent();
            Loaded += RightPanelLoaded;
        }

        private async void RightPanelLoaded(object? sender, RoutedEventArgs e)
        {
            var weatherService = new WeatherService();
            Dictionary<string, object> weather = await weatherService.GetWeather("Almaty", "");
            if (weather == null)
            {
            }
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
