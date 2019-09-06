using System.Threading.Tasks;
using EventFeed.Consumer.Hubs;
using EventFeed.Consumer.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventFeed.Consumer.Events
{
    internal class ClickedEventHandler: IEventHandler<ClickedEvent>
    {
        private readonly ICachedClickStorage _storage;
        private readonly IHubContext<ClicksHub> _hubContext;
        private readonly ILogger<ClickedEventHandler> _logger;

        public void HandleEvent(ClickedEvent @event)
        {
            int oldClickCount = _storage.GetClickCount();
            int newClickCount = oldClickCount + 1;
            _logger.LogInformation("Noticed a click! Click count is now {ClickCount}", newClickCount);
            
            _storage.StoreClickCount(newClickCount);
            Task.Run(() => _hubContext.Clients.All.SendAsync("Click", newClickCount)).GetAwaiter().GetResult();

            _logger.LogDebug("Stored and broadcast click count {ClickCount} to SignalR clients", newClickCount);
        }

        public ClickedEventHandler(
            ICachedClickStorage storage,
            IHubContext<ClicksHub> hubContext,
            ILogger<ClickedEventHandler> logger
        )
        {
            _storage = storage;
            _hubContext = hubContext;
            _logger = logger;
        }
    }
}
