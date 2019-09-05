using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Subjects;
using EventFeed.Producer.Clicks;
using EventFeed.Producer.EventFeed;
using Newtonsoft.Json;

namespace EventFeed.Producer.Infrastructure
{
    internal class IsolatedStorageEventStorage: IWriteEventStorage, IReadEventStorage
    {
        private readonly Subject<string> _notifySubject;

        public void StoreEvent(object @event)
        {
            EventEnvelope envelope;
            
            lock (this)
            {
                envelope = CreateEnvelope();
                string pageId = DeterminePageId();
                AppendEvent(pageId, envelope);
            }

            _notifySubject.OnNext(envelope.Id);

            string DeterminePageId()
            {
                string pageId = GetMostRecentPageId();
                string currentPageId = GetCurrentPageId();

                if (!ShouldCreateNewPage())
                    return pageId;
                
                AppendPage(currentPageId);
                return currentPageId;

                bool ShouldCreateNewPage() =>
                    // when pageId is null -> true
                    // when pageId is 'less' than currentPageId -> true
                    // when they are equal -> false
                    // when pageId is 'greater' than currentPageId -> false
                    // in the last case we should continue to use that most recent page, instead of
                    // creating an 'archived' page which we will then mutate
                    string.CompareOrdinal(pageId, currentPageId) < 0;
            }

            EventEnvelope CreateEnvelope()
            {
                return new EventEnvelope(
                    id: Guid.NewGuid().ToString("N"),
                    occurred: DateTimeOffset.Now,
                    eventType: GetEventType(),
                    payload: SerializeEvent()
                );
            }

            string GetEventType()
            {
                switch (@event.GetType())
                {
                    case Type t when t == typeof(ClickedEvent): return "application/vnd.eventfeeddemo.clicked+json";
                    default: throw new ArgumentException($"Unrecognized event type '{@event.GetType()}'");                        
                }
            }

            string SerializeEvent() => JsonConvert.SerializeObject(@event);

            void AppendPage(string pageId)
            {
                _storage.CreateDirectory(Path.GetDirectoryName(PageListFileName));

                var allPages = GetPageList();
                var newPages = allPages.Add(pageId);

                string json = JsonConvert.SerializeObject(newPages);

                using (var writer = new StreamWriter(
                    _storage.OpenFile(
                        PageListFileName,
                        FileMode.Create,
                        FileAccess.Write
                    )
                ))
                {
                    writer.Write(json);
                }
            }

            void AppendEvent(string pageId, EventEnvelope eventEnvelope)
            {
                var events = ReadEventsFromPage(pageId);
                var newEvents = events.Add(eventEnvelope);
                
                StorePage(pageId, newEvents);
            }

            void StorePage(string pageId, IReadOnlyCollection<EventEnvelope> events)
            {
                string fileName = GetPageFileName(pageId);

                using (var writer = new StreamWriter(
                    _storage.OpenFile(
                        fileName,
                        FileMode.Create,
                        FileAccess.Write
                    )
                ))
                {
                    string json = JsonConvert.SerializeObject(events);
                    writer.Write(json);
                }
            }
        }
        
        private string GetMostRecentPageId()
        {
            return GetPageList().LastOrDefault();
        }

        private ImmutableList<string> GetPageList()
        {
            if (!_storage.FileExists(PageListFileName))
                return ImmutableList<string>.Empty;

            using (var reader = new StreamReader(
                _storage.OpenFile(
                    PageListFileName,
                    FileMode.Open,
                    FileAccess.Read
                )
            ))
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<ImmutableList<string>>(json);
            }
        }

        private string GetCurrentPageId()
        {
            return RoundDownToMinute(DateTimeOffset.UtcNow).ToString("yyyyMMdd'T'HHmmss");
            
            DateTimeOffset RoundDownToMinute(DateTimeOffset d) =>
                new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0, d.Offset);
        }

        private ImmutableList<EventEnvelope> ReadEventsFromPage(string pageId)
        {
            string fileName = GetPageFileName(pageId);

            if (!_storage.FileExists(fileName))
                return ImmutableList<EventEnvelope>.Empty;

            using (var reader = new StreamReader(
                _storage.OpenFile(
                    fileName,
                    FileMode.Open,
                    FileAccess.Read
                )
            ))
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<EventEnvelope[]>(json).ToImmutableList();
            }
        }

        private string GetPageFileName(string pageId) => PageFileNamePrefix + pageId + ".dat";

        public EventFeedPage GetLatestEvents()
        {
            lock (this)
            {
                var allPages = GetPageList();

                if (!allPages.Any())
                    return new EventFeedPage(
                        id: null,
                        previousPageId: null,
                        nextPageId: null,
                        events: Array.Empty<Event>()
                    );

                string pageId = allPages.Last();
                return GetEventFeedPage(pageId);
            }
        }

        private Event ConvertEventEnvelopeToEvent(EventEnvelope envelope)
        {
            return new Event(
                id: envelope.Id,
                occurred: envelope.Occurred,
                type: envelope.EventType,
                payload: envelope.Payload
            );
        }

        public EventFeedPage GetArchivedEvents(string pageId)
        {
            lock (this)
            {
                return GetEventFeedPage(pageId);
            }
        }

        public string GetLatestEventPageId()
        {
            lock (this)
            {
                return GetPageList().LastOrDefault();
            }
        }

        private EventFeedPage GetEventFeedPage(string pageId)
        {
            var allPages = GetPageList();
            int index = allPages.IndexOf(pageId);

            if (index == -1)
                throw new EventFeedPageNotFoundException();

            string previousPageId = index > 0
                                        ? allPages[index - 1]
                                        : null;
            string nextPageId = index < allPages.Count - 1
                                    ? allPages[index + 1]
                                    : null;
            
            var events = ReadEventsFromPage(pageId).Select(ConvertEventEnvelopeToEvent).ToList();

            return new EventFeedPage(
                id: pageId,
                previousPageId: previousPageId,
                nextPageId: nextPageId,
                events: events
            );
        }

        public IsolatedStorageEventStorage(Subject<string> notifySubject)
        {
            _notifySubject = notifySubject;
        }
        
        private readonly IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForApplication();

        private const string PageListFileName = "Events/pages.dat";
        private const string PageFileNamePrefix = "Events/";

        private class EventEnvelope
        {
            public string Id { get; }
            public DateTimeOffset Occurred { get; }
            public string EventType { get; }
            public string Payload { get; }

            public EventEnvelope(string id, DateTimeOffset occurred, string eventType, string payload)
            {
                Id = id;
                Occurred = occurred;
                EventType = eventType;
                Payload = payload;
            }
        }
    }
}
