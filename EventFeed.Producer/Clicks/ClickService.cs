namespace EventFeed.Producer.Clicks
{
    public class ClickService: IClickService
    {
        public void RegisterClick()
        {
            _storage.IncrementClickCount();
        }

        public ClickService(IClickStorage storage)
        {
            _storage = storage;
        }

        private readonly IClickStorage _storage;
    }
}
