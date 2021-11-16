using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using EventProcessingService.Actors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventProcessingService
{
    public class LightDto
    {
        [JsonIgnore] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("type")] public string Type { get; set; }
    }

    public class Worker : BackgroundService
    {
        public Worker(ILogger<Worker> logger)
        {
            Logger = logger;
        }

        private ILogger<Worker> Logger { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Log(LogLevel.Trace, "Initializing Actor System");
            var system = ActorSystem.Create("playground");

            var eventDispatcher = system.ActorOf<EventDispatcher>("eventDispatcher");
            system.ActorOf(TurnAllLightsOffAutomation.Props());
            system.ActorOf(TurnAllLightsOnAutomation.Props());
            system.ActorOf(Lights.Props(stoppingToken), "lights");

            Logger.Log(LogLevel.Trace, "Connecting to WebSocket");
            using var webSocket = new ClientWebSocket();
            var cancellationTokenSource = new CancellationTokenSource();
            await webSocket.ConnectAsync(new Uri("ws://192.168.88.203:443"), cancellationTokenSource.Token);

            Logger.Log(LogLevel.Trace, "Connected. Starting to listen...");
            while (!stoppingToken.IsCancellationRequested)
            {
                var buffer = new byte[2048];
                var memory = new Memory<byte>(buffer);
                var receiveResult = await webSocket.ReceiveAsync(memory, cancellationTokenSource.Token);

                eventDispatcher.Tell(Encoding.UTF8.GetString(buffer.Take(receiveResult.Count).ToArray()));
            }
        }
    }
}