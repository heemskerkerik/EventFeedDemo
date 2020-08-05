using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace EventFeed.Consumer.Infrastructure
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                        .WriteTo.Console(outputTemplate:"[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj} {NewLine}{Exception}")
                        .CreateLogger();

            try
            {
                await Host.CreateDefaultBuilder(args)
                          .ConfigureWebHostDefaults(
                               h => h.UseStartup<Startup>()
                                     .ConfigureLogging(
                                          l => l.ClearProviders()
                                                .SetMinimumLevel(LogLevel.Trace)
                                                .AddSerilog(logger)
                                      )
                                     .UseUrls("http://localhost:5001")
                           )
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
