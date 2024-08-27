using System.Text.Json.Serialization;

namespace WeatherAppAPI.Dtos
{
    public class WeatherAPIResponseDto
    {
        [JsonPropertyName("main")]
        public MainObj Main { get; set; }
    }

    public class MainObj
    {
        [JsonPropertyName("temp")]
        public decimal Temp { get; set; }
        [JsonPropertyName("feels_like")]
        public decimal FeelsLike { get; set; }
        [JsonPropertyName("temp_min")]
        public decimal TempMin { get; set; }
        [JsonPropertyName("temp_max")]
        public decimal TempMax { get; set; }
    }
}
