using System;

namespace EventFeed.Consumer.EventFeed
{
    public interface IEventDispatcher
    {
        void Dispatch(Event @event, Action markEventAsProcessed);
    }
}