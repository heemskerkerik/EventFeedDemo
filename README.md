# Event Feed Demo

This application demonstrates how events can flow reliably from one application to another using an Atom-based event feed.

It consists of a 'producer', counting the number of times users click a button, and a 'consumer', replaying events from the producer to reach the same count.

## Running

Running is easiest from the terminal. Open two separate terminal windows. In the one, navigate to the `EventFeed.Producer` directory and run `dotnet run`. In the other, navigate to the `EventFeed.Consumer` directory and run `dotnet run`.

Open two browser tabs, ideally side by side, and navigate to `http://localhost:5000` and `http://localhost:5001`. The former one will be the 'producer' side (with the 'Click!' button), the latter one will be the 'consumer' side.

When you click a button in the producer, the click count will increase on both sides. Behind the scenes, the producer broadcasts a notification via SignalR, which prompts the consumer to poll the producer's event feed for changes.

The consumer should be pretty resilient against the producer being unreachable. See what happens when the producer is not there for a while or if the event feed endpoint is incorrectly configured.

## Configuration

You shouldn't have to configure anything; it just works :tm:. However, there are several things you can configure. Set them as environment variables, or change the projects' `appsettings.json` files.

### Producer configuration settings

| Setting | Description |
| ------------- | -------------|
| `ASPNETCORE_URLS` | The URLs the application listens on (defaults to `http://localhost:5000`) |
| `EnableSignalR` | Enable the SignalR endpoint to broadcast event notifications (defaults to `true`) |

### Consumer 

| Setting | Description |
| ------------- | -------------|
| `ASPNETCORE_URLS` | The URLs the application listens on (defaults to `http://localhost:5001`) |
| `ProducerEventFeedUri` | The URI of the producer's event feed endpoint (defaults to `http://localhost:5000/events/latest`) |
| `EnableRealTimeNotifications` | Whether to subscribe to real-time notifications from the producer using SignalR (defaults to `true`) |
| `PollingIntervalSeconds` | The interval in seconds between the event feed being polled, regardless of event notifications (defaults to `5`) |

## Navigating the code

Here are some of the interesting starting points to navigate the code:

* `EventFeed.Producer/Controllers/HomeController.cs`
* `EventFeed.Producer/Controllers/EventFeedController.cs`
* `EventFeed.Consumer/EventFeed/EventFeedListener.cs`
