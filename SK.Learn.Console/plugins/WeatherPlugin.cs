using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using SK.Learn.ConsoleChat.config;
using System.ComponentModel;

namespace SK.Learn.ConsoleChat.plugins
{
    public sealed class WeatherPlugin
    {
        private readonly HttpClient _httpClient;
        private readonly KernelSettings _settings;

        public WeatherPlugin(HttpClient httpClient, KernelSettings settings)
        {
            this._httpClient = httpClient;
            this._settings = settings;
        }

        [KernelFunction("GetWeather")]
        [Description("Get weather of today")]
        public async Task<string> GetWeatherAsync([Description("The 6 digital city code")] string cityCode)
        {
            var url = $"https://restapi.amap.com/v3/weather/weatherInfo?city={cityCode}&key={this._settings.WeatherApiKey}&extensions=base";

            var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(content);

            var info = weatherResponse.Lives.FirstOrDefault();
            return $"" +
                $"City: {info.City}\n" +
                $"Weather: {info.Weather}\n" +
                $"Temperature: {info.Temperature}\n" +
                $"Winddirection: {info.Winddirection}\n" +
                $"Windpower: {info.Windpower}\n" +
                $"Humidity: {info.Humidity}\n" +
                $"ReportTime: {info.ReportTime}\n";
        }
    }

    public class WeatherResponse
    {
        public string Status { get; set; }
        public string Count { get; set; }
        public string Info { get; set; }
        public string Infocode { get; set; }

        public List<WeatherInfo> Lives { get; set; }
    }

    public class WeatherInfo
    {
        public string City { get; set; }
        public string Adcode { get; set; }
        public string Province { get; set; }
        public string ReportTime { get; set; }
        public string Weather { get; set; }
        public string Temperature { get; set; }
        public string Winddirection { get; set; }
        public string Windpower { get; set; }
        public string Humidity { get; set; }
    }
}
