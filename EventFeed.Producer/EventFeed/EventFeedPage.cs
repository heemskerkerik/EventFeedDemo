using System.Collections.Generic;

namespace EventFeed.Producer.EventFeed
{
    public class EventFeedPage
    {
        public string? Id { get; }
        public string? PreviousPageId { get; }
        public string? NextPageId { get; }
        public IReadOnlyCollection<Event> Events { get; }

        public EventFeedPage(
            string? id,
            string? previousPageId,
            string? nextPageId, 
            IReadOnlyCollection<Event> events
        )
        {
            Id = id;
            PreviousPageId = previousPageId;
            NextPageId = nextPageId;
            Events = events;
        }
    }
}
