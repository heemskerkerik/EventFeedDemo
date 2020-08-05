using System.Reactive.Subjects;
using EventFeed.Producer.Clicks;
using EventFeed.Producer.EventFeed;
using EventFeed.Producer.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventFeed.Producer.Infrastructure
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSignalR();
            services.AddTransient<IClickService, ClickService>();
            services.AddSingleton<IClickStorage, IsolatedStorageClickStorage>();
            services.AddSingleton<IWriteEventStorage, IsolatedStorageEventStorage>();
            services.AddSingleton(sp => (IReadEventStorage) sp.GetService<IWriteEventStorage>());
            services.AddHostedService<EventNotificationBroadcaster>();
            services.AddSingleton<Subject<string>>();
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            bool enableSignalR = _configuration.GetValue("EnableSignalR", defaultValue: true);

            app.UseRouting();
            app.UseEndpoints(
                r =>
                {
                    if (enableSignalR)
                        r.MapHub<EventNotificationHub>("/events/notification");
                    else
                        logger.LogWarning("Real-time notifications of events have been disabled by configuration");

                    r.MapControllers();
                }
            );
        }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;
    }
}
