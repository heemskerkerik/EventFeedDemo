using EventFeed.Producer.Clicks;
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
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
