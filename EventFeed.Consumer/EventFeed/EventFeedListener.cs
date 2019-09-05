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
            _subscription = Observable.Interval(TimeSpan.FromSeconds(5))
                                      .Select(_ => (string) null)
                                      .Merge(_realTimeNotificationListener.EventNotification)
                                      .SubscribeOn(NewThreadScheduler.Default)
                                      .Subscribe(PollForChanges);
            return _realTimeNotificationListener.StartAsync();
        }

        private void PollForChanges(string id)
        {
            Task.Run(() => _eventFeed.PollForChangesAsync()).GetAwaiter().GetResult();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription.Dispose();
            return _realTimeNotificationListener.StopAsync(cancellationToken);
        }

        public EventFeedListener(
            EventFeed eventFeed,
            RealTimeNotificationListener realTimeNotificationListener
        )
        {
            _eventFeed = eventFeed;
            _realTimeNotificationListener = realTimeNotificationListener;
        }

        private readonly EventFeed _eventFeed;
        private readonly RealTimeNotificationListener _realTimeNotificationListener;
        private IDisposable _subscription;
    }
}
