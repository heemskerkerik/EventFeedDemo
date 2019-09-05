using System;

namespace EventFeed.Consumer.EventFeed.Atom
{
    internal class AtomEntry
    {
        public string Id { get; }
        public DateTimeOffset Published { get; }
        public string ContentType { get; }
        public string Payload { get; }

        public AtomEntry(string id, DateTimeOffset published, string contentType, string payload)
        {
            Id = id;
            Published = published;
            ContentType = contentType;
            Payload = payload;
        }
    }
}
