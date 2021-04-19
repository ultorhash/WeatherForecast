using Newtonsoft.Json;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using WeatherForecast;

namespace Tests
{
    [TestFixture]
    public class TestsWeatherForecastApplication
    {
        private static string[] _correctCityNames = { "Warsaw", "Washington", "Kuala Lumpur", "Paris" };
        private static string[] _wrongCityNames = { "abcde", "FFrankfurt", "Viennna", "Sim city" };

        [Test]
        [TestCaseSource(nameof(_correctCityNames))]
        public async Task Get_Weather_Forecast_For_Specific_City_Name(string city)
        {
            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?APPID=50d53005c0fd5f556bb4ef15224c4209&lang=en&units=metric&q={city}");
            var weather = JsonConvert.DeserializeObject<Weather>(response);

            Assert.AreEqual(city, weather.Name);
        }

        [Test]
        [TestCaseSource(nameof(_wrongCityNames))]
        public void Throw_HttpRequestException_When_City_Is_Incorrect(string city)
        {
            HttpClient client = new HttpClient();

            Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?APPID=50d53005c0fd5f556bb4ef15224c4209&lang=en&units=metric&q={city}"), "Bad");
        }
    }
}