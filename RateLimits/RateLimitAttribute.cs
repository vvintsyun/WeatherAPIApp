namespace WeatherAppAPI.RateLimits
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RateLimitAttribute : Attribute
    {
    }
}
