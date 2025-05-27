using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;

namespace Bookstore.API.Attributes
{
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _maxRequests;
        private readonly int _timeWindowInSeconds;
        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public RateLimitAttribute(int maxRequests = 5, int timeWindowInSeconds = 300) // 5 requests in 5 minutes
        {
            _maxRequests = maxRequests;
            _timeWindowInSeconds = timeWindowInSeconds;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var key = $"admin_rate_limit_{clientIp}_{context.ActionDescriptor.DisplayName}";

            if (_cache.TryGetValue(key, out int requestCount))
            {
                if (requestCount >= _maxRequests)
                {
                    context.Result = new ObjectResult(new
                    {
                        Status = "RateLimitExceeded",
                        ErrorCode = "ADMIN-RATE-LIMIT-001",
                        Message = $"Too many admin login attempts. Try again after {_timeWindowInSeconds / 60} minutes.",
                        RetryAfter = _timeWindowInSeconds
                    })
                    {
                        StatusCode = (int)HttpStatusCode.TooManyRequests
                    };
                    return;
                }

                _cache.Set(key, requestCount + 1, TimeSpan.FromSeconds(_timeWindowInSeconds));
            }
            else
            {
                _cache.Set(key, 1, TimeSpan.FromSeconds(_timeWindowInSeconds));
            }

            base.OnActionExecuting(context);
        }
    }
}