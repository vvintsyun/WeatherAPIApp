using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using WeatherAppAPI.Data;

namespace WeatherAppAPI.RateLimits
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;

        public RateLimitMiddleware(RequestDelegate next,
            IDistributedCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, WeatherDbContext dbContext)
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

            var userRate = await dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.AllowedRate)
                .FirstOrDefaultAsync(context.RequestAborted);

            var consumptionData = await _cache.GetCustomerConsumptionDataFromContextAsync(context);
            if (consumptionData is not null)
            {
                if (consumptionData.HasConsumedAllRequests(TimeSpan.FromHours(1), userRate))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    return;
                }

                consumptionData.IncreaseRequests(userRate);
            }

            await _cache.SetCacheValueAsync(id.ToString(), consumptionData);

            await _next(context);
        }
    }
}
