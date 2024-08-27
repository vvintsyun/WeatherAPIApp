using WeatherAppAPI.Dtos;

namespace WeatherAppAPI.Services
{
    public interface IWeatherService
    {
        public Task<UserWeather> GetWeather(long Id, CancellationToken ct);
    }
}
