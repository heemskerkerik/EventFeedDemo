using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EventFeed.Consumer.EventFeed.Atom;
using Microsoft.Extensions.Logging;
using Polly;

namespace EventFeed.Consumer.EventFeed
{
    public class RealTimeNotificationDiscoverer
    {
        public Task<Uri> DiscoverNotificationUriAsync(CancellationToken token)
        {
            return _policy.ExecuteAsync(_ => DiscoverAsync(), token);

            async Task<Uri> DiscoverAsync()
            {
                var page = await ReadAtomPageAsync(_uri);
                return page.RealTimeNotificationUri;
            }
        }
        
        private async Task<AtomPage> ReadAtomPageAsync(Uri uri)
        {
            try
            {
                var httpClient = _httpClientFactory();
                var reader = new AtomReader();

                using (var stream = await httpClient.GetStreamAsync(uri))
                {
                    return await reader.ReadAtomPage(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Exception reading Atom page at {Uri}", uri);
                throw;
            }
        }

        public RealTimeNotificationDiscoverer(
            Uri uri,
            Func<HttpClient> httpClientFactory,
            ILogger<RealTimeNotificationDiscoverer> logger
        )
        {
            _uri = uri;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _policy = CreatePolicy();
        }

        private static IAsyncPolicy<Uri> CreatePolicy()
        {
            var retryPolicy = Policy<Uri>.Handle<HttpRequestException>()
                                         .WaitAndRetryForeverAsync(_ => _retryInterval);
            var fallbackPolicy = Policy<Uri>.Handle<Exception>()
                                            .FallbackAsync(fallbackValue: null);

            return Policy.WrapAsync(fallbackPolicy, retryPolicy);
        }

        private readonly Uri _uri;
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly ILogger<RealTimeNotificationDiscoverer> _logger;
        private readonly IAsyncPolicy<Uri> _policy;
        
        private static readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(5);
    }
}
