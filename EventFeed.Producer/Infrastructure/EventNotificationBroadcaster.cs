using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using EventFeed.Producer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventFeed.Producer.Infrastructure
{
    public class EventNotificationBroadcaster: IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = _notifySubject
                           .Select(id => "urn:uuid:" + id)
                           .Do(id => _logger.LogDebug("Broadcasting notification for event {Id}", id))
                           .SelectMany(
                                async id =>
                                {
                                    await NotifyViaHubAsync(id);
                                    return id;
                                }
                            )
                           .Subscribe();
            return Task.CompletedTask;
        }

        private Task NotifyViaHubAsync(string id) => _hubContext.Clients.All.SendAsync("Notify", id);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription?.Dispose();
            return Task.CompletedTask;
        }

        public EventNotificationBroadcaster(
            Subject<string> notifySubject,
            IHubContext<EventNotificationHub> hubContext,
            ILogger<EventNotificationBroadcaster> logger
        )
        {
            _notifySubject = notifySubject;
            _hubContext = hubContext;
            _logger = logger;
        }

        private readonly Subject<string> _notifySubject;
        private readonly IHubContext<EventNotificationHub> _hubContext;
        private readonly ILogger<EventNotificationBroadcaster> _logger;
        private IDisposable? _subscription;
    }
}
