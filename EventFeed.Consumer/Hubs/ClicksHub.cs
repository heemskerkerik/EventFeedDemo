using System.Threading.Tasks;
using EventFeed.Consumer.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventFeed.Consumer.Hubs
{
    public class ClicksHub: Hub
    {
        public async Task Refresh()
        {
            int clickCount = _storage.GetClickCount();
            await Clients.Caller.SendAsync("Click", clickCount);

            _logger.LogDebug("Refreshed client {ClientId} with click count {ClickCount}", Context.ConnectionId, clickCount);
        }

        public ClicksHub(ICachedClickStorage storage, ILogger<ClicksHub> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        private readonly ICachedClickStorage _storage;
        private readonly ILogger<ClicksHub> _logger;
    }
}
