using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace AkkaPlayground
{
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