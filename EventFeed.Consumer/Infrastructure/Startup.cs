using System;
using System.Collections.Generic;
using System.Net.Http;
using EventFeed.Consumer.EventFeed;
using EventFeed.Consumer.Events;
using EventFeed.Consumer.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventFeed.Consumer.Infrastructure
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = _configuration.Get<Settings>();

            services.AddMvc();
            services.AddSignalR();
            
            services.AddHttpClient();
            services.AddSingleton<IKnownEventStorage, IsolatedStorageKnownEventStorage>();
            services.AddSingleton<ICachedClickStorage, IsolatedStorageCachedClickStorage>();
            services.AddSingleton<IHostedService>(
                sp =>
                {
                    var feed = new EventFeed.EventFeed(
                        uri: settings.ProducerEventFeedUri,
                        httpClientFactory: sp.GetRequiredService<IHttpClientFactory>().CreateClient,
                        knownEventStorage: sp.GetRequiredService<IKnownEventStorage>(),
                        dispatcher: sp.GetRequiredService<IEventDispatcher>(),
                        logger: sp.GetService<ILogger<EventFeed.EventFeed>>()
                    );

                    bool enableRealTimeNotifications = _configuration.GetValue("EnableRealTimeNotifications", defaultValue: true);

                    RealTimeNotificationListener realTimeNotificationListener;

                    if (enableRealTimeNotifications)
                    {
                        var realTimeNotificationDiscoverer = new RealTimeNotificationDiscoverer(
                            uri: settings.ProducerEventFeedUri,
                            httpClientFactory: sp.GetRequiredService<IHttpClientFactory>().CreateClient,
                            logger: sp.GetService<ILogger<RealTimeNotificationDiscoverer>>()
                        );

                        realTimeNotificationListener = new RealTimeNotificationListener(
                            uri: settings.ProducerEventFeedUri,
                            discoverer: realTimeNotificationDiscoverer,
                            logger: sp.GetService<ILogger<RealTimeNotificationListener>>()
                        );
                    }
                    else
                        realTimeNotificationListener = null;

                    return new EventFeedListener(feed, realTimeNotificationListener);
                }
            );
            services.AddSingleton<IEventDispatcher>(
                sp => new EventDispatcher(
                    sp,
                    new Dictionary<string, Type>
                    {
                        { "application/vnd.eventfeeddemo.clicked+json", typeof(ClickedEvent) }
                    },
                    sp.GetService<ILogger<EventDispatcher>>()
                )
            );
            services.AddTransient<IEventHandler<ClickedEvent>, ClickedEventHandler>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSignalR(routes => routes.MapHub<ClicksHub>("/realtime/clicks"));
            app.UseMvc();
            app.UseStaticFiles();
        }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;
    }
}
