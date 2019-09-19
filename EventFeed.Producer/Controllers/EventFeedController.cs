using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EventFeed.Producer.EventFeed;
using EventFeed.Producer.EventFeed.Atom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace EventFeed.Producer.Controllers
{
    [ApiController]
    public class EventFeedController: ControllerBase, IEventFeedUriProvider
    {
        [HttpGet("events/latest")]
        public async Task<IActionResult> GetLatestEventsAsync()
        {
            if (!ClientAcceptsAtomResponse())
                return StatusCode(StatusCodes.Status406NotAcceptable);

            var page = _storage.GetLatestEvents();
            return await RenderFeedAsync(page);
        }

        private bool ClientAcceptsAtomResponse()
        {
            var acceptHeader = Request.GetTypedHeaders().Accept;
            return acceptHeader?.Any(h => _atomMediaType.IsSubsetOf(h)) ?? true;
        }

        private async Task<IActionResult> RenderFeedAsync(EventFeedPage page)
        {
            var renderer = new AtomRenderer(page, _storage, this);
            var stream = new MemoryStream();

            await renderer.RenderAsync(stream);

            stream.Position = 0;
            return File(stream, contentType: AtomContentType);
        }

        [HttpGet("events/{pageId}")]
        public async Task<IActionResult> GetArchivedEventsAsync(string pageId)
        {
            if (!ClientAcceptsAtomResponse())
                return StatusCode(StatusCodes.Status406NotAcceptable);
            
            EventFeedPage page;
            try
            {
                page = _storage.GetArchivedEvents(pageId);
            }
            catch (EventFeedPageNotFoundException)
            {
                return NotFound();
            }

            return await RenderFeedAsync(page);
        }

        [NonAction]
        public Uri GetLatestEventsUri() =>
            new Uri(
                Url.Action(
                    action: "GetLatestEventsAsync",
                    controller: "EventFeed",
                    values: null,
                    protocol: Request.Scheme
                )
            );

        [NonAction]
        public Uri GetArchivedPageUri(string pageId) =>
            new Uri(
                Url.Action(
                    action: "GetArchivedEventsAsync",
                    controller: "EventFeed",
                    values: new { pageId = pageId },
                    protocol: Request.Scheme
                )
            );

        [NonAction]
        public Uri GetNotificationsUri()
        {
            return _settings.Value.EnableSignalR
                       ? new Uri(new Uri(Request.GetEncodedUrl()), Url.Content("~/events/notification"))
                       : null;
        }

        public EventFeedController(
            IReadEventStorage storage,
            IOptions<Settings> settings
        )
        {
            _storage = storage;
            _settings = settings;
        }

        private readonly IReadEventStorage _storage;
        private readonly IOptions<Settings> _settings;

        private const string AtomContentType = "application/atom+xml";
        private static readonly MediaTypeHeaderValue _atomMediaType = new MediaTypeHeaderValue(AtomContentType);
    }
}
