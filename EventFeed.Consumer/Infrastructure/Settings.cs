using System;

namespace EventFeed.Consumer.Infrastructure
{
    public class Settings
    {
        public Uri ProducerEventFeedUri { get; set; } = new Uri("http://localhost:5000/events/latest");
        public bool EnableRealTimeNotifications { get; set; } = true;
        public int PollingIntervalSeconds { get; set; } = 5;
    }
}
