using EventFeed.Consumer.Infrastructure;
using Microsoft.Extensions.Logging;

namespace EventFeed.Consumer.Events
{
    internal class ClickedEventHandler: IEventHandler<ClickedEvent>
    {
        private readonly ICachedClickStorage _storage;
        private readonly ILogger<ClickedEventHandler> _logger;

        public void HandleEvent(ClickedEvent @event)
        {
            int oldClickCount = _storage.GetClickCount();
            _storage.StoreClickCount(oldClickCount + 1);

            _logger.LogInformation("Noticed a click! Click count is now {ClickCount}", oldClickCount + 1);
        }

        public ClickedEventHandler(
            ICachedClickStorage storage,
            ILogger<ClickedEventHandler> logger
        )
        {
            _storage = storage;
            _logger = logger;
        }
    }
}
