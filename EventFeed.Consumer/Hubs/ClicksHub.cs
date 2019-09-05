using System.Threading.Tasks;
using EventFeed.Consumer.Events;
using Microsoft.AspNetCore.SignalR;

namespace EventFeed.Consumer.Hubs
{
    public class ClicksHub: Hub
    {
        public Task Refresh()
        {
            return Clients.Caller.SendAsync("Click", _storage.GetClickCount());
        }

        public ClicksHub(ICachedClickStorage storage)
        {
            _storage = storage;
        }

        private readonly ICachedClickStorage _storage;
    }
}
