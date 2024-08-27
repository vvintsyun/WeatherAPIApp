using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace WeatherAppAPI.RateLimits
{
    public static class DistributedCacheExtension
    {
        public async static Task<ConsumptionData?> GetCustomerConsumptionDataFromContextAsync(
            this IDistributedCache cache,
            HttpContext context,
            CancellationToken cancellation = default)
        {
            if (!context.Request.RouteValues.TryGetValue("id", out var id) || id is null)
            {
                return null;
            }
            var result = await cache.GetStringAsync(id.ToString(), cancellation);
            if (result is null)
                return null;

            return JsonSerializer.Deserialize<ConsumptionData>(result);
        }

        public async static Task SetCacheValueAsync(
            this IDistributedCache cache,
            string key,
            ConsumptionData? customerRequests,
            CancellationToken cancellation = default)
        {
            customerRequests ??= new ConsumptionData(DateTime.UtcNow, 1);

            await cache.SetStringAsync(key, JsonSerializer.Serialize(customerRequests), cancellation);
        }
    }
}
