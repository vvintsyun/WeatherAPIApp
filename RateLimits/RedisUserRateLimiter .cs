using System.Threading.RateLimiting;
using StackExchange.Redis;

namespace WeatherAppAPI.RateLimits
{
    public class RedisUserRateLimiter : RateLimiter
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly string _keyPrefix;
        private readonly TimeSpan _window;

        public RedisUserRateLimiter(IConnectionMultiplexer redis, string keyPrefix, TimeSpan window)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
            _keyPrefix = keyPrefix;
            _window = window;
        }

        public override TimeSpan? IdleDuration => throw new NotImplementedException();

        public override RateLimiterStatistics? GetStatistics()
        {
            throw new NotImplementedException();
        }

        public async Task<(bool, DateTimeOffset)> TryAcquireAsync(int userId, int userRate)
        {
            var redisKey = $"rate_limit:user:{userId}";
            var currentCount = await _db.StringIncrementAsync(redisKey);

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var windowStart = now - _window.TotalMilliseconds;

            await _db.SortedSetRemoveRangeByScoreAsync(redisKey, double.NegativeInfinity, windowStart);

            var requestCount = await _db.SortedSetLengthAsync(redisKey);            

            if (requestCount >= userRate)
            {
                var lastRequest = await _db.SortedSetRangeByRankWithScoresAsync(redisKey, -1, -1);

                var lastTimestamp = lastRequest[0].Score;
                return (false, DateTimeOffset.FromUnixTimeMilliseconds((long)lastTimestamp));
            }

            // Add the new request
            await _db.SortedSetAddAsync(redisKey, now.ToString(), now);
            await _db.KeyExpireAsync(redisKey, _window); // Ensure expiration

            return (true, new DateTime());
        }

        protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override RateLimitLease AttemptAcquireCore(int tokenCount) =>
            throw new NotImplementedException();

        protected override ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;   
    }
}
