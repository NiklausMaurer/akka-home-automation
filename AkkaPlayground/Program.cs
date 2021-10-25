using System;
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
    public class EventActor : ReceiveActor
    {
        public EventActor()
        {
            Receive<EventMessage>(message =>
            {
                Console.WriteLine("[Thread {0}] Message received", Thread.CurrentThread.ManagedThreadId);
                
                if (message.MessageType != "event" || message.EventType != "changed" ||
                    message.ResourceType != "sensors" ||
                    message.ResourceId != "9") return;

                using var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
                };

                var cancellationTokenSource = new CancellationTokenSource();

                if (message.ButtonEvent == 1002)
                {
                    var task = httpClient.PutAsync("lights/15/state", new StringContent("{ \"on\": false }", Encoding.UTF8),
                        cancellationTokenSource.Token);
                    task.Wait();
                }
                else
                {
                    var task = httpClient.PutAsync("lights/15/state", new StringContent("{ \"on\": true }", Encoding.UTF8),
                        cancellationTokenSource.Token);
                    task.Wait();
                }
            });
            
            Console.WriteLine("[Thread {0}] Constructor of EventActor terminated", Thread.CurrentThread.ManagedThreadId);
        }
    }

    public class EventMessage
    {
        public string MessageType { get; set; }
        public string EventType { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public long ButtonEvent { get; set; }
    }

    internal static class Program
    {
        private static async Task Main()
        {
            using var webSocket = new ClientWebSocket();

            var cancellationTokenSource = new CancellationTokenSource();
            await webSocket.ConnectAsync(new Uri("ws://192.168.88.203:443"), cancellationTokenSource.Token);

            var system = ActorSystem.Create("playground");
            var eventActor = system.ActorOf<EventActor>("eventActor");

            while (true)
            {
                var buffer = new byte[2048];
                var memory = new Memory<byte>(buffer);
                var receiveResult = await webSocket.ReceiveAsync(memory, cancellationTokenSource.Token);

                var httpMessage = Encoding.UTF8.GetString(buffer.Take(receiveResult.Count).ToArray());
                var document = JsonDocument.Parse(httpMessage);

                EventMessage message = new EventMessage
                {
                    MessageType = document.RootElement.GetProperty("t").GetString(),
                    EventType = document.RootElement.GetProperty("e").GetString(),
                    ResourceType = document.RootElement.GetProperty("r").GetString(),
                    ResourceId = document.RootElement.GetProperty("id").GetString(),
                };

                if (!document.RootElement.TryGetProperty("state", out var stateDocument)) continue;
                if (!stateDocument.TryGetProperty("buttonevent", out var buttonEvent)) continue;

                message.ButtonEvent = buttonEvent.GetInt64();

                //system.EventStream.Publish(message);
                Console.WriteLine("[Thread {0}] Forwarding message", Thread.CurrentThread.ManagedThreadId);
                eventActor.Tell(message);
            }
        }
    }
}