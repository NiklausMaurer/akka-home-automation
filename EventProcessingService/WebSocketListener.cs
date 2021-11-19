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
using Newtonsoft.Json;

namespace EventProcessingService
{
    public class LightDto
    {
        public LightDto(string id, string name, string type)
        {
            Id = id;
            Name = name;
            Type = type;
        }

        [JsonIgnore] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("type")] public string Type { get; set; }
    }

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
            var system = ActorSystem.Create("playground", BootstrapSetup.Create().And(di));

            var eventDispatcher = system.ActorOf<EventDispatcher>("eventDispatcher");
            system.ActorOf(TurnAllLightsOffAutomation.Props());
            system.ActorOf(TurnAllLightsOnAutomation.Props());

            var props = DependencyResolver.For(system).Props<Lights>();
            system.ActorOf(props, "lights");

            Logger.Log(LogLevel.Trace, "Connecting to WebSocket");
            using var webSocket = new ClientWebSocket();
            var cancellationTokenSource = new CancellationTokenSource();
            await webSocket.ConnectAsync(new Uri("ws://192.168.88.203:443"), cancellationTokenSource.Token);

            Logger.Log(LogLevel.Trace, "Connected. Starting to listen...");
            
            var buffer = new byte[2048];
            var memory = new Memory<byte>(buffer);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var receiveResult = await webSocket.ReceiveAsync(memory, cancellationTokenSource.Token);
                var messageBytes = buffer.Take(receiveResult.Count).ToArray();
                var message = Encoding.UTF8.GetString(messageBytes);
                
                eventDispatcher.Tell(message);
            }
        }
    }
}