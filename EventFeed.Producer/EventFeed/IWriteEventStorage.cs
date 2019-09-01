namespace EventFeed.Producer.EventFeed
{
    public interface IWriteEventStorage
    {
        void StoreEvent(object @event);
    }
}
