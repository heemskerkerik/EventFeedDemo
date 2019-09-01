using EventFeed.Producer.Clicks;
using EventFeed.Producer.EventFeed;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EventFeed.Producer.Infrastructure
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddTransient<IClickService, ClickService>();
            services.AddSingleton<IClickStorage, IsolatedStorageClickStorage>();
            services.AddSingleton<IWriteEventStorage, IsolatedStorageEventStorage>();
            services.AddSingleton(sp => (IReadEventStorage) sp.GetService<IWriteEventStorage>());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
