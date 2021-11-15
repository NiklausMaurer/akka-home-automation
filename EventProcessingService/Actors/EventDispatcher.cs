using System;
using System.Threading;
using Akka.Actor;
using EventProcessingService.Dto;
using Newtonsoft.Json.Linq;

namespace EventProcessingService.Actors
{
    public class EventDispatcher : ReceiveActor
    {
        public EventDispatcher()
        {
            Receive<string>(message =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId,
                    Self.Path);

                var incomingEvent = JObject.Parse(message).ToObject<IncomingEvent>();
                if (incomingEvent == null) throw new Exception($"Parsing of message {message} failed.");

                if (incomingEvent.MessageType != "event" ||
                    incomingEvent.EventType != "changed" ||
                    incomingEvent.State is null) return;

                if (incomingEvent.State.ButtonEvent.HasValue)
                {
                    Context.System.EventStream.Publish(new ButtonEvent
                    {
                        ButtonId = incomingEvent.ResourceId,
                        EventId = incomingEvent.State.ButtonEvent.Value
                    });

                    Console.WriteLine(
                        $"[Thread {Thread.CurrentThread.ManagedThreadId}, Actor {Self.Path}] Buttonevent published.");
                }

                if (incomingEvent.ResourceType.Equals("lights") && incomingEvent.State.IsOn.HasValue)
                {
                    Context.System.EventStream.Publish(new LightStateChanged
                    {
                        LightId = incomingEvent.ResourceId,
                        IsOn = incomingEvent.State.IsOn.Value
                    });

                    Console.WriteLine("[Thread {0}, Actor {1}] LightStateChanged published.",
                        Thread.CurrentThread.ManagedThreadId, Self.Path);
                }
            });
        }
    }

    public class LightStateChanged
    {
        public string LightId { get; set; }
        public bool IsOn { get; set; }
    }
}