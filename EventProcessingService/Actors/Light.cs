using System;
using System.Net.Http;
using System.Resources;
using System.Text;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using EventProcessingService.Messages.Lights;

namespace EventProcessingService.Actors
{
    public class Light : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }
        
        private string Id { get; }
        
        private static readonly HttpClient HttpClient = new()
        {
            BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
        };
        
        public Light(string id)
        {
            Id = id;
            
            Receive<TurnOnCommand>(turnOnCommand =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] TurnOnCommand received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var cancellationTokenSource = new CancellationTokenSource();
                var requestUri = $"lights/{Id}/state";

                HttpClient.PutAsync(requestUri, new StringContent("{ \"on\": true }", Encoding.UTF8),
                    cancellationTokenSource.Token);

                Console.WriteLine("[Thread {0}, Actor {1}] Request to turn on sent", Thread.CurrentThread.ManagedThreadId, Self.Path);

                if (turnOnCommand.Attempt < 3)
                {
                    Timers.StartSingleTimer("doublecheck", new TurnOnCommand(turnOnCommand.Attempt + 1), TimeSpan.FromMilliseconds(1000));
                }
            });
            
            Receive<TurnOffCommand>(turnOffCommand =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] TurnOffCommand received", Thread.CurrentThread.ManagedThreadId, Self.Path);

                var cancellationTokenSource = new CancellationTokenSource();
                var requestUri = $"lights/{Id}/state";

                HttpClient.PutAsync(requestUri, new StringContent("{ \"on\": false }", Encoding.UTF8),
                    cancellationTokenSource.Token);

                Console.WriteLine("[Thread {0}, Actor {1}] Request to turn off sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                if (turnOffCommand.Attempt < 3)
                {
                    Timers.StartSingleTimer("doublecheck", new TurnOffCommand(turnOffCommand.Attempt + 1), TimeSpan.FromMilliseconds(1000));
                }
            });
        }
        
        public static Props Props(string id)
        {
            return Akka.Actor.Props.Create(() => new Light(id));
        }
    }

    public class DoubleCheck
    {
    }
}