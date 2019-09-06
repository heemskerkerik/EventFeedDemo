using EventFeed.Producer.EventFeed;
using Microsoft.Extensions.Logging;

namespace EventFeed.Producer.Clicks
{
    public class ClickService: IClickService
    {
        public void RegisterClick()
        {
            _logger.LogDebug("Registering a click");
            
            _storage.IncrementClickCount();
            _eventStorage.StoreEvent(new ClickedEvent());
        }

        public ClickService(IClickStorage storage, IWriteEventStorage eventStorage, ILogger<ClickService> logger)
        {
            _storage = storage;
            _eventStorage = eventStorage;
            _logger = logger;
        }

        private readonly IClickStorage _storage;
        private readonly IWriteEventStorage _eventStorage;
        private readonly ILogger<ClickService> _logger;
    }
}
