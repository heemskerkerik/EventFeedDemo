using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Polly;

namespace EventFeed.Consumer.EventFeed
{
    public class RealTimeNotificationListener
    {
        public IObservable<string> EventNotification => _eventNotificationSubject;

        public Task StartAsync()
        {
            _connectTask = StartInBackgroundAsync();
            return Task.CompletedTask;
        }

        private async Task StartInBackgroundAsync()
        {
            _notificationsUri = await DiscoverNotificationUriAsync();

            if (_notificationsUri == null)
            {
                _logger.LogInformation(
                    "Event feed at {Uri} does not expose real-time notifications.",
                    _uri
                );
                return;
            }

            _connection = new HubConnectionBuilder()
                         .WithUrl(_notificationsUri)
                         .Build();

            _connection.On<string>("Notify", OnNotification);
            _connection.Closed += OnConnectionClosed;

            await OpenConnectionAsync();

            Task<Uri> DiscoverNotificationUriAsync()
            {
                return _discoverer.DiscoverNotificationUriAsync(_stoppingTokenSource.Token);
            }
            
            void OnNotification(string id)
            {
                _logger.LogDebug("Received real-time notification: {Id}", id);
                _eventNotificationSubject.OnNext(id);
            }

            Task OnConnectionClosed(Exception error)
            {
                _logger.LogWarning(error, "Lost connection to {Uri}", _notificationsUri);
                _connectTask = OpenConnectionAsync();
                return Task.CompletedTask;
            }

            Task OpenConnectionAsync()
            {
                return _connectPolicy.ExecuteAsync(
                    ConnectAsync,
                    _stoppingTokenSource.Token
                );
            }

            async Task ConnectAsync(CancellationToken cancellation)
            {
                await _connection.StartAsync(cancellation);
                _logger.LogInformation("Connected to {Uri}", _notificationsUri);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingTokenSource.Cancel();

            if (_connection == null || _connectTask == null)
                return;

            await _connection.StopAsync(cancellationToken);
            await _connectTask;
        }

        public RealTimeNotificationListener(
            Uri uri,
            RealTimeNotificationDiscoverer discoverer,
            ILogger<RealTimeNotificationListener> logger
        )
        {
            _uri = uri;
            _discoverer = discoverer;
            _logger = logger;

            _connectPolicy = Policy.Handle<Exception>()
                                   .WaitAndRetryForeverAsync(
                                        _ => TimeSpan.FromSeconds(5),
                                        (ex, sleep) => _logger.LogDebug(
                                            ex,
                                            "Failed to connect to {Uri}; will retry in {SleepTimeMs} ms.",
                                            _notificationsUri,
                                            sleep.TotalMilliseconds
                                        )
                                    );
        }

        private readonly Uri _uri;
        private readonly RealTimeNotificationDiscoverer _discoverer;
        private readonly ILogger<RealTimeNotificationListener> _logger;
        private readonly Subject<string> _eventNotificationSubject = new Subject<string>();
        private HubConnection _connection;
        private Task _connectTask;
        private Uri _notificationsUri;
        private readonly IAsyncPolicy _connectPolicy;
        private readonly CancellationTokenSource _stoppingTokenSource = new CancellationTokenSource();
    }
}
