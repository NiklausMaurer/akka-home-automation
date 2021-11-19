using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventProcessingService
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) => { 
                    services.AddHostedService<WebSocketListener>();
                    services.AddHttpClient("deconz", client =>
                    {
                        client.BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/");
                        client.Timeout = TimeSpan.FromMilliseconds(100);
                    });
                });
        }
    }
}