using System;
using Akka.Actor;
using Akka.DependencyInjection;
using EventProcessingService.Actors;
using EventProcessingService.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceProvider = Microsoft.Extensions.DependencyInjection.ServiceProvider;

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
                    services.AddSingleton<ActorSystem>(serviceProvider =>
                    {
                        var di = DependencyResolverSetup.Create(serviceProvider);
                        var system = ActorSystem.Create("akkaHomeAutomation", BootstrapSetup.Create().And(di));
            
                        system.CreateActor<TurnAllLightsOffAutomation>();
                        system.CreateActor<TurnAllLightsOnAutomation>();
                        system.CreateActor<Lights>("lights");
                        system.CreateActor<EventDispatcher>("eventDispatcher");

                        return system;
                    });
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