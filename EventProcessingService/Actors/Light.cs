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
        public Light(string id, IHttpClientFactory httpClientFactory)
        {
            Id = id;
            HttpClientFactory = httpClientFactory;

            Receive<TurnOn>(TurnOn);
            Receive<TurnOff>(TurnOff);
        }

        private string Id { get; }
        private IHttpClientFactory HttpClientFactory { get; }
        
        public ITimerScheduler? Timers { get; set; }

        private void TurnOn(TurnOn turnOn)
        {
            HttpPut($"lights/{Id}/state", "{ \"on\": true }");

            if (turnOn.Attempt < 3)
                Timers?.StartSingleTimer("doublecheck", turnOn.NewAttempt(),
                    TimeSpan.FromMilliseconds(1000));
        }

        private void TurnOff(TurnOff turnOff)
        {
            HttpPut($"lights/{Id}/state", "{ \"on\": false }");

            if (turnOff.Attempt < 3)
                Timers?.StartSingleTimer("doublecheck", turnOff.NewAttempt(),
                    TimeSpan.FromMilliseconds(1000));
        }

        private void HttpPut(string uri, string body)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var client = HttpClientFactory.CreateClient("deconz");
            client.PutAsync(uri, new StringContent(body, Encoding.UTF8),
                cancellationTokenSource.Token);
        }

        public static Props Props(string id, IHttpClientFactory httpClientFactory)
        {
            return Akka.Actor.Props.Create(() => new Light(id, httpClientFactory));
        }
    }
}