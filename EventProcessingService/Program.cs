using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventProcessingService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => { 
                    services.AddHostedService<WebSocketListener>();
                    services.AddHttpClient("deconz", client =>
                    {
                        string apiKey = context.Configuration["deconz-api-key"];
                        client.BaseAddress = new Uri($"http://192.168.88.203:9080/api/{apiKey}/");
                        client.Timeout = TimeSpan.FromMilliseconds(200);
                    });
                });
        }
    }
}