using Prometheus;
using System.Text.Json;
using WeatherService.Models;

namespace WeatherService.Services
{
    public class WeatherInfoService : IHostedService, IDisposable
    {
        private readonly ILogger<WeatherInfoService> _logger;
        private Timer _timer;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ApiKey = "f010786f29eed26883a3413e42090fe1";

        private LocationResponse? _locationResponse = null;

        private readonly Gauge _weatherGauge = Metrics.CreateGauge("tallinn_weather_celsius", "Tallinn Weather Temperature in Celsius");
        private readonly Counter _exceptionCounter = Metrics.CreateCounter("weather_fetch_exception_counter", "Weather Service Request exception counter");
        private readonly Counter _weatherMeasurementCounter = Metrics.CreateCounter("weather_fetch_counter", "Weather Service request counter");

        public WeatherInfoService(ILogger<WeatherInfoService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(RequestWeatherInfo, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private async Task FetchCoords (string location = "Tallinn")
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                try
                {
                    var response = await httpClient.GetAsync($"http://api.openweathermap.org/geo/1.0/direct?q={location}&limit=5&appid={ApiKey}");

                    var contentStream = await response.Content.ReadAsStreamAsync();
                    var content = await new StreamReader(contentStream).ReadToEndAsync();
                    var locationResponse = JsonSerializer.Deserialize<List<LocationResponse>>(content).FirstOrDefault();
                    if (locationResponse == null)
                    {
                        _logger.LogError("Unable to parse location!");
                        return;
                    }

                    _locationResponse = locationResponse;
                } catch (Exception e)
                {
                    _exceptionCounter.Inc();
                    _logger.LogError(e, $"Exception requesting location info");
                }
            }
        }

        private async void RequestWeatherInfo(object? state)
        {
            if (_locationResponse == null)
                await FetchCoords();

            if (_locationResponse == null)
                return;

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                try
                {
                    var response = await httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?lat={_locationResponse.Latitude}&lon={_locationResponse.Longitude}&appid={ApiKey}&units=metric");
                    var contentStream = await response.Content.ReadAsStreamAsync();
                    var content = await new StreamReader(contentStream).ReadToEndAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Received error response to weather request. Error code: {response.StatusCode}, Message: {content}");
                        return;
                    }
                    var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(content);

                    _logger.LogInformation($"Weather temperature: {weatherResponse?.Main.Temperature}");
                    _weatherMeasurementCounter.Inc();
                    _weatherGauge.Set(weatherResponse.Main.Temperature);
                } catch (Exception e)
                {
                    _exceptionCounter.Inc();
                    _logger.LogError(e, $"Exception requesting weather info");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
