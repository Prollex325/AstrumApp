using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;


namespace AstrumApp.Services
{
    public class WeatherService
    {
        public async Task<Dictionary<string, object>?> GetWeather(string city, string apiKey)
        {
            string response = await GetRawWeather(city, apiKey);

            if (response == null)
            {
                // Нет интернета / сервер недоступен
                return null;
            }

            var parsed = ParseWeather(response);

            return parsed;
        }
        private static async Task<string?> GetRawWeather(string city, string apiKey)
        {
            using HttpClient client = new();

            try
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";

                return await client.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Ошибка сети: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        private static Dictionary<string, object> ParseWeather(string weatherJson)
        {
            var weather = new Dictionary<string, object>();

            using JsonDocument doc = JsonDocument.Parse(weatherJson);

            var root = doc.RootElement;

            string city = root.GetProperty("name").GetString()!;

            string main = root.GetProperty("weather")[0]
                              .GetProperty("main")
                              .GetString()!;

            double temp = root.GetProperty("main").GetProperty("temp").GetDouble();
            double feels = root.GetProperty("main").GetProperty("feels_like").GetDouble();
            int humidity = root.GetProperty("main").GetProperty("humidity").GetInt32();

            string description = root.GetProperty("weather")[0]
                                        .GetProperty("description")
                                        .GetString()!;
            description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(description);

            double wind = root.GetProperty("wind").GetProperty("speed").GetDouble();

            weather["city"] = city;
            weather["temp"] = temp + "°C";
            weather["feels"] = feels + "°C";
            weather["humidity"] = humidity + "%";
            weather["desc"] = $"{GetIcon(main)}{description}";
            weather["wind"] = wind + "км/ч";

            return weather;
        }

        private static string GetIcon(string main)
        {
            return main switch
            {
                "Clear" => "☀️",
                "Clouds" => "☁️",
                "Rain" => "🌧️",
                "Drizzle" => "🌦️",
                "Thunderstorm" => "⛈️",
                "Snow" => "❄️",
                "Mist" => "🌫️",
                _ => "🌍"
            };
        }
    }
}
