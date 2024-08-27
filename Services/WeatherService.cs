using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WeatherAppAPI.Data;
using WeatherAppAPI.Dtos;
using WeatherAppAPI.Helpers;

namespace WeatherAppAPI.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly WeatherDbContext _dbContext;
        private readonly ILogger<WeatherService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public WeatherService(WeatherDbContext dbContext, ILogger<WeatherService> logger, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<UserWeather> GetWeather(long id, CancellationToken ct)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (user is null) 
            {
                _logger.LogError("GetWeather: User with id {UserId} doesn't exist", id);
                throw new Exception("User doesn't exist");
            }

            using (var httpClient = _httpClientFactory.CreateClient("Weather"))
            {
                var url = $"http://api.openweathermap.org/data/2.5/weather?q={user.Location}&appid=0edeaab478ab9dd3dc60043dabf6cb6c&units={user.Units.GetDisplayName}";
                var response = await httpClient.GetAsync(url, ct);

                if (response.IsSuccessStatusCode)
                {
                    WeatherAPIResponseDto? result;
                    try
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonSerializer.Deserialize<WeatherAPIResponseDto>(json);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("GetWeather: Get request api.openweathermap.org with userId {UserId} deserialization failed with {Message}", user.Id, ex.Message);
                        throw new Exception("External server returned an incorrect data");
                    }
                    if (result is null)
                    {
                        _logger.LogError("GetWeather: Get request api.openweathermap.org with userId {UserId} returned null response", user.Id);
                        throw new Exception("External server returned an incorrect data");
                    }

                    return new UserWeather(user, result.Main.TempMin, result.Main.TempMax, result.Main.FeelsLike, result.Main.Temp);
                }
                else
                {
                    _logger.LogError("GetWeather: Get request api.openweathermap.org with userId {UserId} call returns {Content} {StatusCode}", user.Id, response.Content, response.StatusCode);
                    throw new Exception("External server returns an error, please try again later");
                }
            }                     
        }
    }
}
