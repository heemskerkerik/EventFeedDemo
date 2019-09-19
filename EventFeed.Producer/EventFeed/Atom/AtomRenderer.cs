using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;

namespace EventFeed.Producer.EventFeed.Atom
{
    internal class AtomRenderer
    {
        public async Task RenderAsync(Stream stream)
        {
            using (Initialize(stream))
            {
                await WriteMetadataAsync();

                var eventsInReverseChronologicalOrder = _page.Events.Reverse();
                foreach (var @event in eventsInReverseChronologicalOrder)
                    await WriteEventAsync(@event);
            }

            async Task WriteMetadataAsync()
            {
                await _writer.WriteId("urn:publicid:EventFeedDemo");
                await _writer.WriteUpdated(_page.Events.Select(e => e.Occurred).DefaultIfEmpty(_epoch).Max());
                await _writer.Write(
                    new SyndicationLink(
                        _page.Id == _latestEventPageId
                            ? _uriProvider.GetLatestEventsUri()
                            : _uriProvider.GetArchivedPageUri(_page.Id),
                        AtomLinkTypes.Self
                    )
                );

                if (_page.PreviousPageId != null)
                    await _writer.Write(
                        new SyndicationLink(
                            _uriProvider.GetArchivedPageUri(_page.PreviousPageId),
                            "prev-archive"
                        )
                    );
                if (_page.NextPageId != null)
                {
                    await _writer.Write(
                        new SyndicationLink(
                            _page.NextPageId != _latestEventPageId
                                ? _uriProvider.GetArchivedPageUri(_page.NextPageId)
                                : _uriProvider.GetLatestEventsUri(),
                            "next-archive"
                        )
                    );

                    if (_page.NextPageId != _latestEventPageId)
                        await _writer.Write(
                            new SyndicationLink(
                                _uriProvider.GetLatestEventsUri(),
                                "current"
                            )
                        );
                }

                if (_uriProvider.GetNotificationsUri() != null)
                    await _writer.Write(
                        new SyndicationLink(
                            _uriProvider.GetNotificationsUri(),
                            "notifications"
                        )
                    );
            }

            async Task WriteEventAsync(Event @event)
            {
                var atomEntry = ConvertEventToAtomEntry(@event);
                await _writer.Write(atomEntry);
            }
            
            AtomEntry ConvertEventToAtomEntry(Event @event)
            {
                var entry = new AtomEntry
                            {
                                Id = "urn:uuid:" + @event.Id,
                                Published = @event.Occurred,
                                LastUpdated = @event.Occurred,
                                Title = @event.Id,
                                ContentType = @event.Type,
                                Description = @event.Payload,
                            };
                entry.AddContributor(_author);

                return entry;
            }
        }

        private IDisposable Initialize(Stream stream)
        {
            var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Indent = false, });
            _writer = new AtomFeedWriter(xmlWriter);

            _latestEventPageId = _storage.GetLatestEventPageId();

            return Disposable.Create(() => xmlWriter.Dispose());
        }

        public AtomRenderer(
            EventFeedPage page,
            IReadEventStorage storage,
            IEventFeedUriProvider uriProvider
        )
        {
            _page = page;
            _storage = storage;
            _uriProvider = uriProvider;
        }

        private readonly EventFeedPage _page;
        private readonly IReadEventStorage _storage;
        private readonly IEventFeedUriProvider _uriProvider;
        private AtomFeedWriter _writer;
        private string _latestEventPageId;

        private static readonly DateTimeOffset _epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        private static readonly SyndicationPerson _author = new SyndicationPerson(
            "EventFeedDemo",
            "noreply@eventfeed-demo.eu",
            AtomContributorTypes.Author
        );
    }
}
