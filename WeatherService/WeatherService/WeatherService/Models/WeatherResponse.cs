using System.Text.Json.Serialization;

namespace WeatherService.Models
{
    public class WeatherResponse
    {
        [JsonPropertyName("main")]
        public Main Main { get; set; }
    }

    public class Main
    {
        [JsonPropertyName("temp")]
        public double Temperature { get; set; }
    }
}
