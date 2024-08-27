﻿namespace WeatherAppAPI.RateLimits
{
    public class ConsumptionData
    {
        public ConsumptionData(
            DateTime lastResponse,
            int numberOfRequests)
        {
            LastResponse = lastResponse;
            NumberOfRequests = numberOfRequests;
        }

        public DateTime LastResponse { get; private set; }
        public int NumberOfRequests { get; private set; }

        public bool HasConsumedAllRequests(TimeSpan window, int maxRequests)
            => DateTime.UtcNow < LastResponse.Add(window) && NumberOfRequests == maxRequests;

        public void IncreaseRequests(int maxRequests)
        {
            LastResponse = DateTime.UtcNow;

            if (NumberOfRequests == maxRequests)
                NumberOfRequests = 1;

            else
                NumberOfRequests++;
        }
    }
}
