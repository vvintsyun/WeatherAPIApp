using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WeatherAppAPI.Data;
using WeatherAppAPI.Dtos;
using WeatherAppAPI.Models;

namespace WeatherAppAPI.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly WeatherDbContext _dbContext;
        private readonly ILogger<WeatherService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _weatherApiKey;

        public WeatherService(IHttpClientFactory httpClientFactory, ILogger<WeatherService> logger, WeatherDbContext dbContext, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _dbContext = dbContext;
            _weatherApiKey = config["Weather:ApiKey"];
        }

        public async Task<UserWeather> GetWeather(long id, CancellationToken ct)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (user is null)
            {
                _logger.LogError("GetWeather: User with id {UserId} not found", id);
                throw new InvalidOperationException("User does not exist");
            }

            var httpClient = _httpClientFactory.CreateClient("Weather");
            var url = $"weather?q={user.Location}&appid={_weatherApiKey}&units={user.Units.GetUnitValue()}";

            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWeather: API call failed for userId {UserId}", user.Id);
                throw new HttpRequestException("Failed to reach weather service", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "GetWeather: API returned error for userId {UserId}. Status: {StatusCode}, Content: {ErrorContent}",
                    user.Id, response.StatusCode, errorContent);

                throw new HttpRequestException("Weather service returned an error, please try again later");
            }

            WeatherAPIResponseDto? result;
            try
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                result = JsonSerializer.Deserialize<WeatherAPIResponseDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "GetWeather: Deserialization failed for userId {UserId}", user.Id);
                throw new InvalidDataException("Weather service returned invalid data", ex);
            }

            if (result?.Main is null)
            {
                _logger.LogError("GetWeather: Empty or malformed response for userId {UserId}", user.Id);
                throw new InvalidDataException("Weather service returned incomplete data");
            }

            return new UserWeather(user, result.Main.TempMin, result.Main.TempMax, result.Main.FeelsLike, result.Main.Temp);
        }
    }
}
