using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Microsoft.Office.Interop;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace WeatherForecast
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime _weatherTimeChecked;
        public MainWindow()
        {
            InitializeComponent();
        }

        #region ButtonsAction
        private async void BtnCheckClick(object sender, RoutedEventArgs e)
        {
            _weatherTimeChecked = DateTime.Now;
            string cityName = GetCityName();

            try { await GetCityWeatherInfo(cityName); }
            catch (HttpRequestException) { await ActionInfo(MessageType.IncorrectCityName); }
            finally { ClearInputCity(); }
        }

        private void BtnExitClick(object sender, RoutedEventArgs e) => Close();

        private void BtnCleanUpClick(object sender, RoutedEventArgs e)
        {
            spWeatherInfo.Visibility = Visibility.Hidden;
            OnOffSaveButton(false);
        }

        private async void BtnSaveWeatherDataToFile(object sender, RoutedEventArgs e)
        {
            OnOffSaveButton(false);
            string cityInfo = tblCityInfo.Text;

            await SaveDataToTextFile(
                cityInfo,
                _weatherTimeChecked.ToShortDateString(),
                _weatherTimeChecked.ToShortTimeString(),
                lblTemp.Content.ToString(),
                lblWind.Content.ToString(),
                lblClouds.Content.ToString(),
                lblPressure.Content.ToString()
                );
        }

        private void CheckBoxDetailsClick(object sender, RoutedEventArgs e)
        {
            spWeatherDetails.Visibility = (bool)cbxShowDetails.IsChecked ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion

        #region TasksForFunctionsWithAwait
        private async Task GetCityWeatherInfo(string cityName)
        {
            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?APPID=50d53005c0fd5f556bb4ef15224c4209&lang=en&units=metric&q={cityName}");
            var weather = JsonConvert.DeserializeObject<Weather>(response);

            DataContext = weather;
            spWeatherInfo.Visibility = Visibility.Visible;

            OnOffSaveButton(true);
        }

        private async Task ActionInfo(MessageType type)
        {
            switch (type)
            {
                case MessageType.IncorrectCityName:
                    await FeedbackInfo("Maroon", "Hotpink", "Incorrect city name. Try again.");
                    break;

                case MessageType.Saved:
                    await FeedbackInfo("Green", "Limegreen", "Saved successfully!");
                    break;

                case MessageType.CreatedAndSaved:
                    await FeedbackInfo("Green", "Limegreen", "File created successfully!");
                    break;

                default:
                    break;
            }
        }

        private async Task FeedbackInfo(string background, string foreground, string text)
        {
            var converter = new BrushConverter();
            var brushFG = (Brush)converter.ConvertFromString(foreground);
            var brushBG = (Brush)converter.ConvertFromString(background);

            lblActionStatus.Foreground = brushFG;
            lblActionStatus.Background = brushBG;
            lblActionStatus.Content = text;
            lblActionStatus.Visibility = Visibility.Visible;

            await Task.Delay(2500);

            lblActionStatus.Visibility = Visibility.Hidden;
            lblActionStatus.Content = string.Empty;
            lblActionStatus.Foreground = Brushes.Black;
            lblActionStatus.Background = Brushes.Transparent;
        }

        private async Task SaveDataToTextFile(params string[] data)
        {
            string filePath = "../../../weather_data.txt";

            if (File.Exists(filePath))
            {
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    string dataToWrite = string.Empty;
                    string currentData = string.Empty;

                    for (int i = 0; i < data.Length; i++)
                    {
                        currentData = i == data.Length - 1 ?
                            data[i] :
                            data[i] + " | ";

                        dataToWrite += currentData;
                    }

                    sw.WriteLine(dataToWrite);
                }
                await ActionInfo(MessageType.Saved);
            }

            else
            {
                using (FileStream fs = File.Create(filePath))
                {
                    var description = new UTF8Encoding(true)
                        .GetBytes("City | Date | Time | Temperature (°C) | Wind (m/s) | Clouds (%) | Pressure (hPa)\n\n");
                    fs.Write(description, 0, description.Length);

                    for (int i = 0; i < data.Length; i++)
                    {
                        byte[] dataToWrite = i == data.Length - 1 ?
                            new UTF8Encoding(true).GetBytes(data[i] + "\n") :
                            new UTF8Encoding(true).GetBytes(data[i] + " | ");

                        fs.Write(dataToWrite, 0, dataToWrite.Length);
                    }
                }
                await ActionInfo(MessageType.CreatedAndSaved);
            }
        }
        #endregion

        #region AuxiliaryFunctions

        private string GetCityName() => tbxCityName.Text;
        private string ClearInputCity() => tbxCityName.Text = string.Empty;
        private void OnOffSaveButton(bool state)
        {
            if (state)
            {
                btnSaveWeather.IsEnabled = true;
                btnSaveWeather.Opacity = 1;
            }
            else
            {
                btnSaveWeather.IsEnabled = false;
                btnSaveWeather.Opacity = 0.8;
            }
        }
        #endregion
    }
}

