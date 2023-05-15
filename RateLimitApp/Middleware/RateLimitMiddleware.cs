using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json;
using RateLimitApp.Models;
using System.Collections.Concurrent;
using System.Net;

namespace RateLimitApp.Middleware
{
    public class RateLimitMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly int _maxRequests;
        private readonly int _ratePeriodSeconds;
        private readonly ConcurrentDictionary<string, ClientStatistics> _cache;

        public RateLimitMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _maxRequests = Convert.ToInt32(configuration["RateLimitMaxRequests"]);
            _ratePeriodSeconds = Convert.ToInt32(configuration["RateLimitPeriodSeconds"]);
            _cache = new ConcurrentDictionary<string, ClientStatistics>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var decorator = endpoint?.Metadata.GetMetadata<LimitRequest>();
            if (decorator is null)
            {
                await _next(context);
                return;
            }

            var key = GenerateClientKey(context);

            _cache.TryGetValue(key, out var clientStatistics);

            if (clientStatistics == null)
            {
                _cache[key] = new ClientStatistics { NumberOfRequestsCompletedSuccessfully = 1, LastSuccessfulResponseTime = DateTime.Now };
            }
            else
            {
                var unblockTime = clientStatistics.LastSuccessfulResponseTime.AddSeconds(_ratePeriodSeconds);
                if (DateTime.UtcNow < unblockTime &&
                   clientStatistics.NumberOfRequestsCompletedSuccessfully >= _maxRequests)
                {
                    await HandleError(context, clientStatistics, unblockTime);
                    return;
                }
                else
                {
                    UpdateStatistics(key, clientStatistics, unblockTime);
                }
            }

            await _next(context);
        }

        private void UpdateStatistics(string key, ClientStatistics clientStatistics, DateTime unblockTime)
        {

            if (unblockTime < DateTime.Now)
                clientStatistics.NumberOfRequestsCompletedSuccessfully = 1;
            else
                clientStatistics.NumberOfRequestsCompletedSuccessfully++;
            clientStatistics.LastSuccessfulResponseTime = DateTime.Now;
            _cache[key] = clientStatistics;
        }

        private Task HandleError(HttpContext context, ClientStatistics clientStatistics, DateTime unblockTime)
        {

            var response = new BaseResponse
            {
                ErrorMessage = $"You have exceeded your calls. Remining time until restriction removed is {unblockTime}",
                HasErrors = true
            };

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(response);

        }
        private string GenerateClientKey(HttpContext context) => $"{context.Request.Headers["token"]}_{context.Connection.RemoteIpAddress}";

    }
}
