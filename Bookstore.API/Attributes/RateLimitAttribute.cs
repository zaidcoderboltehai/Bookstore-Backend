using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using Bookstore.API.Services;

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

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var key = $"admin_rate_limit_{clientIp}_{context.ActionDescriptor.DisplayName}";

            // Try to use Redis if available, otherwise fall back to in-memory cache
            try
            {
                var redisService = context.HttpContext.RequestServices.GetService<IRedisService>();

                if (redisService != null)
                {
                    // Use Redis for distributed rate limiting
                    var currentCountStr = await redisService.GetStringAsync(key);
                    var currentCount = int.TryParse(currentCountStr, out var count) ? count : 0;

                    if (currentCount >= _maxRequests)
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

                    await redisService.SetStringAsync(key, (currentCount + 1).ToString(), TimeSpan.FromSeconds(_timeWindowInSeconds));
                }
                else
                {
                    // Fall back to in-memory cache if Redis service is not available
                    UseInMemoryCache(context, key);
                }
            }
            catch
            {
                // If Redis fails for any reason, fall back to in-memory cache
                UseInMemoryCache(context, key);
            }

            base.OnActionExecuting(context);
        }

        private void UseInMemoryCache(ActionExecutingContext context, string key)
        {
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
        }
    }
}