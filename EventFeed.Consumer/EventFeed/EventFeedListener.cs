using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace EventFeed.Consumer.EventFeed
{
    internal class EventFeedListener: BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _eventFeed.PollForChangesAsync();

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        public EventFeedListener(EventFeed eventFeed)
        {
            _eventFeed = eventFeed;
        }

        private readonly EventFeed _eventFeed;
    }
}
