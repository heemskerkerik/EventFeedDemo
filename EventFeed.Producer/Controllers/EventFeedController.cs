using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using EventFeed.Producer.EventFeed;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;

namespace EventFeed.Producer.Controllers
{
    [ApiController]
    public class EventFeedController: ControllerBase
    {
        [HttpGet("events/latest")]
        public async Task<IActionResult> GetLatestEventsAsync()
        {
            var page = _storage.GetLatestEvents();
            var stream = new MemoryStream();

            await RenderPageToStreamAsync(page, stream);

            return File(stream, "application/atom+xml");
        }

        private async Task RenderPageToStreamAsync(EventFeedPage page, Stream stream)
        {
            string latestEventPageId = _storage.GetLatestEventPageId();

            using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Indent = false, }))
            {
                var writer = new AtomFeedWriter(xmlWriter);

                await writer.WriteId("urn:publicid:EventFeedDemo");
                await writer.WriteUpdated(page.Events.Select(e => e.Occurred).DefaultIfEmpty(_epoch).Max());
                await writer.Write(
                    new SyndicationLink(
                        page.Id == latestEventPageId
                            ? GetLatestEventsUri()
                            : GetArchivedEventsUri(page.Id),
                        AtomLinkTypes.Self
                    )
                );

                if (page.PreviousPageId != null)
                    await writer.Write(
                        new SyndicationLink(
                            GetArchivedEventsUri(page.PreviousPageId),
                            "prev-archive"
                        )
                    );
                if (page.NextPageId != null)
                    await writer.Write(
                        new SyndicationLink(
                            page.NextPageId != _storage.GetLatestEventPageId()
                                ? GetArchivedEventsUri(page.NextPageId)
                                : GetLatestEventsUri(),
                            "next-archive"
                        )
                    );

                foreach (var @event in page.Events.Reverse())
                {
                    var entry = ConvertEventToAtomEntry(@event);
                    await writer.Write(entry);
                }
            }

            stream.Position = 0;

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

            Uri GetLatestEventsUri()
            {
                return new Uri(
                    Url.Action(
                        action: "GetLatestEventsAsync",
                        controller: "EventFeed",
                        values: null,
                        protocol: Request.Scheme
                    )
                );
            }

            Uri GetArchivedEventsUri(string pageId)
            {
                return new Uri(
                    Url.Action(
                        action: "GetArchivedEventsAsync",
                        controller: "EventFeed",
                        values: new { pageId = pageId },
                        protocol: Request.Scheme
                    )
                );
            }
        }

        [HttpGet("events/{pageId}")]
        public async Task<IActionResult> GetArchivedEventsAsync(string pageId)
        {
            EventFeedPage page;
            try
            {
                page = _storage.GetArchivedEvents(pageId);
            }
            catch (EventFeedPageNotFoundException)
            {
                return NotFound();
            }

            var stream = new MemoryStream();

            await RenderPageToStreamAsync(page, stream);

            return File(stream, "application/atom+xml");
        }

        public EventFeedController(IReadEventStorage storage)
        {
            _storage = storage;
        }

        private readonly IReadEventStorage _storage;

        private static readonly DateTimeOffset _epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private static readonly SyndicationPerson _author = new SyndicationPerson(
            "EventFeedDemo",
            "noreply@eventfeed-demo.eu",
            AtomContributorTypes.Author
        );
    }
}
