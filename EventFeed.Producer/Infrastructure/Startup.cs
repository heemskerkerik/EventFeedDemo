using System.Reactive.Subjects;
using EventFeed.Producer.Clicks;
using EventFeed.Producer.EventFeed;
using EventFeed.Producer.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddHostedService<EventNotificationMultiplexer>();
            services.AddSingleton<Subject<string>>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSignalR(routes => routes.MapHub<EventNotificationHub>("/events/notification"));
            app.UseMvc();
        }
    }
}
