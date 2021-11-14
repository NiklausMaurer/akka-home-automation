using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using EventProcessingService.Actors;
using EventProcessingService.Messages.Lights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventProcessingService
{
    public class LightDto
    {
        [JsonIgnore]
        public string Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
    
    public class Worker : BackgroundService
    {
        private ILogger<Worker> Logger { get; }
        private HttpClient _httpClient;
        
        public Worker(ILogger<Worker> logger)
        {
            Logger = logger;
            
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var response = await _httpClient.GetAsync("lights", stoppingToken);
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception("Getting lights info failed");

            var content = await response.Content.ReadAsStringAsync(stoppingToken);
            var lightsDocument = JObject.Parse(content);
            var lightDict = lightsDocument.ToObject<Dictionary<string, LightDto>>();
            if(lightDict is null)  throw new Exception("Lights could not be parsed.");
            
            foreach (var keyValuePair in lightDict)
            {
                keyValuePair.Value.Id = keyValuePair.Key;
            }

            var lightDtos = new Collection<LightDto>(lightDict.Values.ToList());
            
            if (lightDict is null) throw new Exception("Parsing of lghts failed");
            
            Logger.Log(LogLevel.Trace, "Initializing Actor System");
            var system = ActorSystem.Create("playground");
            
            var eventDispatcher = system.ActorOf<EventDispatcher>("eventDispatcher");
            var lights = system.ActorOf<Lights>("lights");
            var automation = system.ActorOf(TurnAllLightsOffAutomation.Props(lightDtos));
            
            system.EventStream.Subscribe(lights, typeof(TurnOnCommand));
            system.EventStream.Subscribe(lights, typeof(TurnOffCommand));
            system.EventStream.Subscribe(automation, typeof(ButtonEvent));
            
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