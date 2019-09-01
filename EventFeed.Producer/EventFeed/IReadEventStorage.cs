namespace EventFeed.Producer.EventFeed
{
    public interface IReadEventStorage
    {
        EventFeedPage GetLatestEvents();
        EventFeedPage GetArchivedEvents(string pageId);
        string GetLatestEventPageId();
    }
}
