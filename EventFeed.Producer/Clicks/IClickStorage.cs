namespace EventFeed.Producer.Clicks
{
    public interface IClickStorage
    {
        void IncrementClickCount();
        int GetClickCount();
    }
}
