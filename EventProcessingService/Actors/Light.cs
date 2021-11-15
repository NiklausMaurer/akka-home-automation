using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using Akka.Actor;
using EventProcessingService.Messages.Lights;

namespace EventProcessingService.Actors
{
    public class Light : ReceiveActor
    {
        private string Id { get; }

        private static readonly HttpClient HttpClient = new()
        {
            BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
        };
        
        public Light(string id)
        {
            Id = id;
            
            Receive<TurnOnCommand>(_ =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var cancellationTokenSource = new CancellationTokenSource();
                var requestUri = $"lights/{Id}/state";

                HttpClient.PutAsync(requestUri, new StringContent("{ \"on\": true }", Encoding.UTF8),
                    cancellationTokenSource.Token);

                Console.WriteLine("[Thread {0}, Actor {1}] Request sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
            });
            
            Receive<TurnOffCommand>(_ =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var cancellationTokenSource = new CancellationTokenSource();
                var requestUri = $"lights/{Id}/state";

                HttpClient.PutAsync(requestUri, new StringContent("{ \"on\": false }", Encoding.UTF8),
                    cancellationTokenSource.Token);

                Console.WriteLine("[Thread {0}, Actor {1}] Request sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
            });
        }
        
        public static Props Props(string id)
        {
            return Akka.Actor.Props.Create(() => new Light(id));
        }
    }
}