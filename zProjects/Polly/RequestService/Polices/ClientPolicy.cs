using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RequestService.Polices
{
    public class ClientPolicy
    {
        public AsyncRetryPolicy<HttpResponseMessage> ImmediateHttpRetry { get; }
        public AsyncRetryPolicy<HttpResponseMessage> LinearHttpRetry { get; }
        public AsyncRetryPolicy<HttpResponseMessage> ExponentialHttpRetry { get; }
        public AsyncTimeoutPolicy TimeoutPolicy { get; }

        public ClientPolicy()
        {
            ImmediateHttpRetry = 
                Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                      .RetryAsync(10);

            LinearHttpRetry =
                Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                      .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(3));

            ExponentialHttpRetry =
                Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                      .WaitAndRetryAsync(5, currNthRetry => TimeSpan.FromSeconds(Math.Pow(2, currNthRetry)));

            TimeoutPolicy = 
                Policy.TimeoutAsync(TimeSpan.FromMilliseconds(1000), TimeoutStrategy.Optimistic, 
                    (context, timeout, task , exception) =>
                    {
                        Console.WriteLine("request is timeout!");
                        return Task.CompletedTask;
                    }
            );
        }
    }
}
