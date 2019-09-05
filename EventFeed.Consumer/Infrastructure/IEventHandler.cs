namespace EventFeed.Consumer.Infrastructure
{
    public interface IEventHandler<in T>
    {
        void HandleEvent(T @event);
    }
}