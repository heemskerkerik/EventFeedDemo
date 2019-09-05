namespace EventFeed.Consumer.Events
{
    public interface ICachedClickStorage
    {
        int GetClickCount();
        void StoreClickCount(int count);
    }
}