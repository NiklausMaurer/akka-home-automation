using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using Akka.Actor;
using EventProcessingService.Messages.Lights;

namespace EventProcessingService.Actors
{
    public class Lights : ReceiveActor
    {
        public Lights()
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
            };
            
            Receive<TurnOnCommand>(command =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var cancellationTokenSource = new CancellationTokenSource();
                var requestUri = $"lights/{command.LightId}/state";

                httpClient.PutAsync(requestUri, new StringContent("{ \"on\": true }", Encoding.UTF8),
                    cancellationTokenSource.Token);

                Console.WriteLine("[Thread {0}, Actor {1}] Request sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
            });
            
            Receive<TurnOffCommand>(command =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var cancellationTokenSource = new CancellationTokenSource();
                var requestUri = $"lights/{command.LightId}/state";

                httpClient.PutAsync(requestUri, new StringContent("{ \"on\": false }", Encoding.UTF8),
                    cancellationTokenSource.Token);

                Console.WriteLine("[Thread {0}, Actor {1}] Request sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
            });
        }
    }
}