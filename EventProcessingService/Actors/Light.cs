using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using Akka.Actor;
using EventProcessingService.Messages.Commands;

namespace EventProcessingService.Actors
{
    public class Light : ReceiveActor, IWithTimers
    {
        private static readonly HttpClient HttpClient = new()
        {
            BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
        };

        public Light(string id)
        {
            Id = id;

            Receive<TurnOnCommand>(TurnOn);
            Receive<TurnOff>(TurnOff);
        }

        private string Id { get; }


        public ITimerScheduler Timers { get; set; }

        private void TurnOn(TurnOnCommand turnOnCommand)
        {
            HttpPut($"lights/{Id}/state", "{ \"on\": true }");

            if (turnOnCommand.Attempt < 3)
                Timers.StartSingleTimer("doublecheck", turnOnCommand.NewAttempt(),
                    TimeSpan.FromMilliseconds(1000));
        }

        private void TurnOff(TurnOff turnOff)
        {
            HttpPut($"lights/{Id}/state", "{ \"on\": false }");

            if (turnOff.Attempt < 3)
                Timers.StartSingleTimer("doublecheck", turnOff.NewAttempt(),
                    TimeSpan.FromMilliseconds(1000));
        }

        private void HttpPut(string uri, string body)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            HttpClient.PutAsync(uri, new StringContent(body, Encoding.UTF8),
                cancellationTokenSource.Token);
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