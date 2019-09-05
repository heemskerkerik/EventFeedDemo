using EventFeed.Consumer.Infrastructure;
using Microsoft.Extensions.Logging;

namespace EventFeed.Consumer.Events
{
    internal class ClickedEventHandler: IEventHandler<ClickedEvent>
    {
        private readonly ILogger<ClickedEventHandler> _logger;

        public void HandleEvent(ClickedEvent @event)
        {
            _logger.LogInformation("Noticed a click!");
        }

        public ClickedEventHandler(ILogger<ClickedEventHandler> logger)
        {
            _logger = logger;
        }
    }
}
