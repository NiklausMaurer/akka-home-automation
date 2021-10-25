using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using Akka.Actor;
using AkkaPlayground.Messages;

namespace AkkaPlayground.Actors
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

                var requestUri = $"lights/{message.LightId}/state";

                var task = message.Action switch
                {
                    LightsCommandMessage.LightAction.TurnOn => httpClient.PutAsync(requestUri,
                        new StringContent("{ \"on\": true }", Encoding.UTF8), cancellationTokenSource.Token),
                    LightsCommandMessage.LightAction.TurnOff => httpClient.PutAsync(requestUri,
                        new StringContent("{ \"on\": false }", Encoding.UTF8), cancellationTokenSource.Token),
                    _ => throw new ArgumentOutOfRangeException()
                };

                Console.WriteLine("[Thread {0}, Actor {1}] Request sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
                    
                task?.Wait();
                if(task != null) Console.WriteLine(task.Result.StatusCode);
            });
        }
    }
}