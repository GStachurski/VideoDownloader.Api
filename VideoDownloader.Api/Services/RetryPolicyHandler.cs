using Polly;
using Serilog;
using System;
using System.Net.Http;

namespace VideoDownloader.Api.Services
{
    public static class RetryPolicyHandler
    {
        public static AsyncPolicy RetryPolicy()
        {
            return Policy
                .Handle<HttpRequestException>()
                .Or<OperationCanceledException>()
                .WaitAndRetryAsync(new[] { 
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15),
                        TimeSpan.FromSeconds(25)
                }, (exception, timeSpan, context) =>
                {
                    Log.Error(exception, $"retry seconds {timeSpan.TotalSeconds}", context);
                });
        }
    }
}
