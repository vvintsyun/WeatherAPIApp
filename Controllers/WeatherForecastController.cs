using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherAppAPI.Services;

namespace WeatherAppAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherService _service;

        public WeatherForecastController(IWeatherService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("rateLimiterPolicy")]
        public async Task<ActionResult<string>> Get(long id, CancellationToken ct)
        {
            var result = await _service.GetWeather(id, ct);
            return Ok(result);
        }
    }
}
