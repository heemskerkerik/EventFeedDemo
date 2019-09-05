using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventFeed.Consumer.EventFeed.Atom;
using Microsoft.Extensions.Logging;

namespace EventFeed.Consumer.EventFeed
{
    internal class EventFeed
    {
        public async Task PollForChangesAsync()
        {
            await HandleExceptionsAsync(PollAsync);

            async Task HandleExceptionsAsync(Func<Task> func)
            {
                try
                {
                    await func();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error polling for changes in {Uri}", _uri);
                }
            }

            async Task PollAsync()
            {
                var newEntries = await GetNewEntriesAsync();

                if (!newEntries.Any())
                    return;

                DispatchEvents(newEntries);
            }

            async Task<IReadOnlyCollection<AtomEntry>> GetNewEntriesAsync()
            {
                string lastKnownEventId = _knownEventStorage.GetLastKnownEventId();

                return await _navigator.GetEntriesSinceAsync(lastKnownEventId);
            }

            void DispatchEvents(IReadOnlyCollection<AtomEntry> newEntries)
            {
                foreach (var entry in newEntries)
                {
                    DispatchEvent(entry);
                }
            }

            void DispatchEvent(AtomEntry atomEntry)
            {
                var @event = ConvertAtomEntryToEvent(atomEntry);
                _dispatcher.Dispatch(@event, MarkEventAsProcessed);

                void MarkEventAsProcessed() =>
                    _knownEventStorage.StoreLastKnownEventId(@event.Id);
            }

            Event ConvertAtomEntryToEvent(AtomEntry entry) =>
                new Event(
                    id: entry.Id,
                    occurred: entry.Published,
                    type: entry.ContentType,
                    payload: entry.Payload
                );
        }

        public EventFeed(
            Uri uri,
            Func<HttpClient> httpClientFactory,
            IKnownEventStorage knownEventStorage,
            IEventDispatcher dispatcher,
            ILogger<EventFeed> logger
        )
        {
            _navigator = new AtomFeedNavigator(uri, httpClientFactory);
            _uri = uri;
            _knownEventStorage = knownEventStorage;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        private readonly AtomFeedNavigator _navigator;
        private readonly Uri _uri;
        private readonly IKnownEventStorage _knownEventStorage;
        private readonly IEventDispatcher _dispatcher;
        private readonly ILogger<EventFeed> _logger;
    }
}
