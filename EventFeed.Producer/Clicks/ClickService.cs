using EventFeed.Producer.EventFeed;

namespace EventFeed.Producer.Clicks
{
    public class ClickService: IClickService
    {
        public void RegisterClick()
        {
            _storage.IncrementClickCount();
            _eventStorage.StoreEvent(new ClickedEvent());
        }

        public ClickService(IClickStorage storage, IWriteEventStorage eventStorage)
        {
            _storage = storage;
            _eventStorage = eventStorage;
        }

        private readonly IClickStorage _storage;
        private readonly IWriteEventStorage _eventStorage;
    }
}
