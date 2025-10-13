using Microsoft.EntityFrameworkCore;
using WeatherAppAPI.Data;

namespace WeatherAppAPI.RateLimits
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RedisUserRateLimiter _rateLimiter;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly WeatherDbContext _dbContext;

        public RateLimitMiddleware(RequestDelegate next, RedisUserRateLimiter rateLimiter, ILogger<RateLimitMiddleware> logger, WeatherDbContext dbContext)
        {
            _next = next;
            _rateLimiter = rateLimiter;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.HasRateLimitAttribute(out var decorator))
            {
                await _next(context);
                return;
            }

            if (!context.Request.RouteValues.TryGetValue("id", out var id)
                || id is null)
            {
                await _next(context);
                return;
            }
            if (!int.TryParse(id.ToString(), out var userId))
            {
                await _next(context);
                return;
            }

            var userRate = await _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.AllowedRate)
                .FirstOrDefaultAsync(context.RequestAborted);

            var rateCheck = await _rateLimiter.TryAcquireAsync(userId, userRate);
            if (!rateCheck.Item1)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Append("XRateLimit-Limit", userRate.ToString());
                context.Response.Headers.Append("RateLimit-Reset", rateCheck.Item2.ToString("o"));
                await context.Response.WriteAsync("Too Many Requests. Please try again later.");
                return;
            }

            await _next(context);
        }
    }
}
