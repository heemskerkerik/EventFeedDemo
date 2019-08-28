using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace EventFeed.Producer.Infrastructure
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            return WebHost.CreateDefaultBuilder<Startup>(args)
                          .Build()
                          .RunAsync();
        }
    }
}
