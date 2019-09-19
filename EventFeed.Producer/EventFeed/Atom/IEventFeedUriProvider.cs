using System;

namespace EventFeed.Producer.EventFeed.Atom
{
    public interface IEventFeedUriProvider
    {
        Uri GetLatestEventsUri();
        Uri GetArchivedPageUri(string pageId);
        Uri GetNotificationsUri();
    }
}
