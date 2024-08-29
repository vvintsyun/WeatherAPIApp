using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using WeatherAppAPI.Data;

namespace WeatherAppAPI.RateLimits
{
    public class RateLimiterPolicy : IRateLimiterPolicy<string>
    {
        private Func<OnRejectedContext, CancellationToken, ValueTask>? _onRejected;
        private readonly MyRateLimitOptions _options;
        private readonly IServiceProvider _serviceProvider;
        public RateLimiterPolicy(ILogger<RateLimiterPolicy> logger,
            IOptions<MyRateLimitOptions> options,
            IServiceProvider serviceProvider)
        {
            _onRejected = (ctx, token) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                logger.LogWarning($"Request rejected by {nameof(RateLimiterPolicy)}");
                return ValueTask.CompletedTask;
            };
            _options = options.Value;
            _serviceProvider = serviceProvider;
        }

        public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => _onRejected;

        public RateLimitPartition<string> GetPartition(HttpContext httpContext)
        {
            if (!httpContext.Request.RouteValues.TryGetValue("id", out var id)
                || id is null)
            {
                return GetLimiter();
            }
            if (!int.TryParse(id.ToString(), out var userId))
            {
                return GetLimiter();
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();

                var userRate = dbContext.Users
                    .Where(x => x.Id == userId)
                    .Select(x => x.AllowedRate)
                    .FirstOrDefault();

                _options.PermitLimit = userRate;
            }
            return GetLimiter();
        }

        private RateLimitPartition<string> GetLimiter()
        {
            return RateLimitPartition.GetSlidingWindowLimiter(string.Empty,
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit,
                    Window = TimeSpan.FromSeconds(_options.Window),
                    SegmentsPerWindow = _options.SegmentsPerWindow
                });
        }
    }
}
