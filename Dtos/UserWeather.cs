using WeatherAppAPI.Models;

namespace WeatherAppAPI.Dtos
{
    public record UserWeather(User User, decimal Min, decimal Max, decimal FeelsLike, decimal Temp)
    {
    }
}
