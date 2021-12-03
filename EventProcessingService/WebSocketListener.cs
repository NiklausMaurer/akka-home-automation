using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.DependencyInjection;
using EventProcessingService.Actors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventProcessingService
{
    public class WebSocketListener : BackgroundService
    {
        public WebSocketListener(IServiceProvider serviceProvider, ILogger<WebSocketListener> logger)
        {
            ServiceProvider = serviceProvider;
            Logger = logger;
        }

        private IServiceProvider ServiceProvider { get; }
        private ILogger<WebSocketListener> Logger { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Log(LogLevel.Trace, "Initializing Actor System");
            
            var di = DependencyResolverSetup.Create(ServiceProvider);
            var system = ActorSystem.Create("akkaHomeAutomation", BootstrapSetup.Create().And(di));
            system.CreateActor<TurnAllLightsOffAutomation>();
            system.CreateActor<TurnAllLightsOnAutomation>();
            system.CreateActor<Lights>("lights");
            
            var eventDispatcher = system.CreateActor<EventDispatcher>("eventDispatcher");
            
            var webSocket = await CreateWebSocket(stoppingToken);
            var buffer = new byte[2048];
            var memory = new Memory<byte>(buffer);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var receiveResult = await webSocket.ReceiveAsync(memory, stoppingToken);
                var messageBytes = buffer.Take(receiveResult.Count).ToArray();
                var message = Encoding.UTF8.GetString(messageBytes);
                
                eventDispatcher.Tell(message);
            }

            await system.Terminate();
        }

        private async Task<ClientWebSocket> CreateWebSocket(CancellationToken stoppingToken)
        {
            Logger.Log(LogLevel.Trace, "Connecting to WebSocket");
            using var webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri("ws://192.168.88.203:443"), stoppingToken);

            Logger.Log(LogLevel.Trace, "Connected. Starting to listen...");
            return webSocket;
        }
    }
    
    public static class ActorSystemExtensions
    {
        public static IActorRef CreateActor<T>(this ActorSystem system, string name = null!) where T : ActorBase
        {
            var props = DependencyResolver.For(system).Props<T>();
            return system.ActorOf(props, name);
        }
    }
}