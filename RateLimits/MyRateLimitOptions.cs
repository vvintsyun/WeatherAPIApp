namespace WeatherAppAPI.RateLimits
{
    public class MyRateLimitOptions
    {
        public const string MyRateLimit = "MyRateLimit";
        public int PermitLimit { get; set; } = 100;
        public int Window { get; set; } = 20;
        public int QueueLimit { get; set; } = 0;
        public int SegmentsPerWindow { get; set; } = 8;
    }
}
