using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Hosting;

namespace EventProcessingService
{
    public class WebSocketListener : BackgroundService
    {
        public WebSocketListener(ActorSystem actorSystem)
        {
            ActorSystem = actorSystem;
        }

        private ActorSystem ActorSystem { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri("ws://192.168.88.203:443"), stoppingToken);
            var buffer = new byte[2048];
            var memory = new Memory<byte>(buffer);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var receiveResult = await webSocket.ReceiveAsync(memory, stoppingToken);
                var messageBytes = buffer.Take(receiveResult.Count).ToArray();
                var message = Encoding.UTF8.GetString(messageBytes);
                ActorSystem.ActorSelection("/user/eventDispatcher").Tell(message);
            }

            await ActorSystem.Terminate();
        }
    }
}