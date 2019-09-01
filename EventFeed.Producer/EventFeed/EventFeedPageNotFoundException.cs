using System;

namespace EventFeed.Producer.EventFeed
{
    public class EventFeedPageNotFoundException: Exception
    {
        public EventFeedPageNotFoundException()
            : base("Cannot find request event feed page.")
        {
        }
    }
}
