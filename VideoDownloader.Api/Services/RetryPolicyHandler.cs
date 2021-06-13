﻿using Polly;
using System;
using System.Net.Http;

namespace VideoDownloader.Api.Services
{
    public static class RetryPolicyHandler
    {
        public static AsyncPolicy DownloadRetryPolicy()
        {
            return Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(new[] { 
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(20),
                        TimeSpan.FromSeconds(30)
                });
        }
    }
}