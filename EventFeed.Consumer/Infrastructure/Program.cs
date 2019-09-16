using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace EventFeed.Consumer.Infrastructure
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                        .MinimumLevel.Is(LogEventLevel.Debug)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .WriteTo.Console(outputTemplate:"[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                        .CreateLogger();

            try
            {
                await WebHost.CreateDefaultBuilder<Startup>(args)
                             .UseSerilog(logger)
                             .UseUrls("http://localhost:5001")
                             .Build()
                             .RunAsync();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                logger.Dispose();
            }
        }
    }
}
