namespace EventFeed.Consumer.EventFeed
{
    public interface IKnownEventStorage
    {
        string GetLastKnownEventId();
        void StoreLastKnownEventId(string id);
    }
}