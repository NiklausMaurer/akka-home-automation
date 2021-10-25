﻿using System;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace AkkaPlayground
{
    public class LightsActor : ReceiveActor
    {
        public LightsActor()
        {
            Receive<LightsCommandMessage>(message =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);

                using var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
                };

                var cancellationTokenSource = new CancellationTokenSource();
                Task<HttpResponseMessage> task;

                var requestUri = $"lights/{message.LightId}/state";

                task = message.Command switch
                {
                    LightsCommandMessage.LightsCommand.TurnOn => httpClient.PutAsync(requestUri,
                        new StringContent("{ \"on\": true }", Encoding.UTF8), cancellationTokenSource.Token),
                    LightsCommandMessage.LightsCommand.TurnOff => httpClient.PutAsync(requestUri,
                        new StringContent("{ \"on\": false }", Encoding.UTF8), cancellationTokenSource.Token),
                    _ => throw new ArgumentOutOfRangeException()
                };

                Console.WriteLine("[Thread {0}, Actor {1}] Request sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
                    
                task?.Wait();
                if(task != null) Console.WriteLine(task.Result.StatusCode);
            });
        }
    }

    public class LightsCommandMessage
    {
        public static LightsCommandMessage TurnOn(string lightId) => new LightsCommandMessage
        {
            Command = LightsCommand.TurnOn,
            LightId = lightId
        };
        
        public static LightsCommandMessage TurnOff(string lightId) => new LightsCommandMessage
        {
            Command = LightsCommand.TurnOff,
            LightId = lightId
        };
        
        public enum LightsCommand
        {
            TurnOn,
            TurnOff
        }

        public LightsCommand Command { get; set; }
        public string LightId { get; set; }
    }

    public class EventActor : ReceiveActor
    {
        public EventActor()
        {
            Receive<ButtonEventMessage>(message =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                if (message.MessageType != "event" || message.EventType != "changed" ||
                    message.ResourceType != "sensors" ||
                    message.ResourceId != "9") return;

                Context.System.EventStream.Publish(message.ButtonEvent == 1002
                    ? LightsCommandMessage.TurnOff("15")
                    : LightsCommandMessage.TurnOn("15"));
            });
        }
    }

    public class WebSocketMessageActor : ReceiveActor
    {
        public WebSocketMessageActor()
        {
            Receive<WebSocketMessage>(webSocketMessage =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var document = JsonDocument.Parse(webSocketMessage.MessageText);

                ButtonEventMessage message = new ButtonEventMessage
                {
                    MessageType = document.RootElement.GetProperty("t").GetString(),
                    EventType = document.RootElement.GetProperty("e").GetString(),
                    ResourceType = document.RootElement.GetProperty("r").GetString(),
                    ResourceId = document.RootElement.GetProperty("id").GetString(),
                };

                if (!document.RootElement.TryGetProperty("state", out var stateDocument)) return;
                if (!stateDocument.TryGetProperty("buttonevent", out var buttonEvent)) return;

                message.ButtonEvent = buttonEvent.GetInt64();

                Context.System.EventStream.Publish(message);
                Console.WriteLine("[Thread {0}, Actor {1}] Message sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
            });
        }
    }

    public class ButtonEventMessage
    {
        public string MessageType { get; set; }
        public string EventType { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public long ButtonEvent { get; set; }
    }
    
    public class WebSocketMessage   
    {
        public string MessageText { get; set; }
    }

    internal static class Program
    {
        private static async Task Main()
        {
            var system = ActorSystem.Create("playground");
            
            var webSocketMessageActor = system.ActorOf<WebSocketMessageActor>("webSocketMessageActor");
            var eventActor = system.ActorOf<EventActor>("eventActor");
            var lightsActor = system.ActorOf<LightsActor>("lightsActor");

            system.EventStream.Subscribe(webSocketMessageActor, typeof(WebSocketMessage));
            system.EventStream.Subscribe(eventActor, typeof(ButtonEventMessage));
            system.EventStream.Subscribe(lightsActor, typeof(LightsCommandMessage));
            
            using var webSocket = new ClientWebSocket();
            var cancellationTokenSource = new CancellationTokenSource();
            await webSocket.ConnectAsync(new Uri("ws://192.168.88.203:443"), cancellationTokenSource.Token);
            
            while (true)
            {
                var buffer = new byte[2048];
                var memory = new Memory<byte>(buffer);
                var receiveResult = await webSocket.ReceiveAsync(memory, cancellationTokenSource.Token);

                var webSocketMessage = new WebSocketMessage()
                {
                    MessageText = Encoding.UTF8.GetString(buffer.Take(receiveResult.Count).ToArray())
                };
                
                system.EventStream.Publish(webSocketMessage);

                if (cancellationTokenSource.IsCancellationRequested) break;
            }
        }
    }
}