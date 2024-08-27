using Microsoft.AspNetCore.RateLimiting;

namespace WeatherAppAPI.RateLimits
{
    public static class HttpContextExtension
    {
        public static bool HasRateLimitAttribute(this HttpContext context, out RateLimitAttribute? rateLimitAttribute)
        {
            rateLimitAttribute = context.GetEndpoint()?.Metadata.GetMetadata<RateLimitAttribute>();
            return rateLimitAttribute is not null;
        }
    }
}
