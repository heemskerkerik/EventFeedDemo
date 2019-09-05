using System;

namespace EventFeed.Consumer.EventFeed
{
    public class Event
    {
        public string Id { get; }
        public DateTimeOffset Occurred { get; }
        public string Type { get; }
        public string Payload { get; }
        
        public Event(string id, DateTimeOffset occurred, string type, string payload)
        {
            Id = id;
            Occurred = occurred;
            Type = type;
            Payload = payload;
        }
    }
}