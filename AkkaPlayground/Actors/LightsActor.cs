using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using Akka.Actor;
using AkkaPlayground.Messages;

namespace AkkaPlayground.Actors
{
    public class LightsActor : UntypedActor
    {
        private static readonly HttpClient HttpClient;
        
        static LightsActor()
        {
            HttpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
            };
        }

        protected override void OnReceive(object m)
        {
            if (m is LightsCommandMessage message)
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var cancellationTokenSource = new CancellationTokenSource();

                var requestUri = $"lights/{message.LightId}/state";

                var task = message.Action switch
                {
                    LightsCommandMessage.LightAction.TurnOn => HttpClient.PutAsync(requestUri,
                        new StringContent("{ \"on\": true }", Encoding.UTF8), cancellationTokenSource.Token),
                    LightsCommandMessage.LightAction.TurnOff => HttpClient.PutAsync(requestUri,
                        new StringContent("{ \"on\": false }", Encoding.UTF8), cancellationTokenSource.Token),
                    _ => throw new ArgumentOutOfRangeException()
                };

                Console.WriteLine("[Thread {0}, Actor {1}] Request sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
                    
                task?.Wait();
                if(task != null) Console.WriteLine(task.Result.StatusCode);
            }
        }
    }
}