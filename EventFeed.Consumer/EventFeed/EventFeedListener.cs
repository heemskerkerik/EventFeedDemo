using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace EventFeed.Consumer.EventFeed
{
    internal class EventFeedListener: IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var realTimeEventNotification = _realTimeNotificationListener?.EventNotification
                                         ?? Observable.Never<string>();

            _subscription = Observable.Interval(_pollingInterval)
                                      .Select(_ => (string?) null)
                                      .Merge(realTimeEventNotification)
                                      .SubscribeOn(NewThreadScheduler.Default)
                                      .Subscribe(PollForChanges);

            return _realTimeNotificationListener != null
                       ? _realTimeNotificationListener.StartAsync()
                       : Task.CompletedTask;
        }

        private void PollForChanges(string? id) =>
            Task.Run(() => _eventFeed.PollForChangesAsync()).GetAwaiter().GetResult();

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription?.Dispose();
            return _realTimeNotificationListener?.StopAsync(cancellationToken)
                ?? Task.CompletedTask;
        }

        public EventFeedListener(
            EventFeed eventFeed,
            RealTimeNotificationListener? realTimeNotificationListener,
            TimeSpan pollingInterval
        )
        {
            _eventFeed = eventFeed;
            _realTimeNotificationListener = realTimeNotificationListener;
            _pollingInterval = pollingInterval;
        }

        private readonly EventFeed _eventFeed;
        private readonly RealTimeNotificationListener? _realTimeNotificationListener;
        private readonly TimeSpan _pollingInterval;
        private IDisposable? _subscription;
    }
}
