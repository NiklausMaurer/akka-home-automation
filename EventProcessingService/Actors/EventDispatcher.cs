using System;
using System.Text.Json;
using System.Threading;
using Akka.Actor;
using EventProcessingService.Messages;

namespace EventProcessingService.Actors
{
    public class EventDispatcher : ReceiveActor
    {
        public EventDispatcher()
        {
            Receive<string>(message =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var messageDocument = JsonDocument.Parse(message);

                ButtonEvent buttonEvent = new ButtonEvent
                {
                    MessageType = messageDocument.RootElement.GetProperty("t").GetString(),
                    EventType = messageDocument.RootElement.GetProperty("e").GetString(),
                    ResourceType = messageDocument.RootElement.GetProperty("r").GetString(),
                    ResourceId = messageDocument.RootElement.GetProperty("id").GetString(),
                };

                if (!messageDocument.RootElement.TryGetProperty("state", out var stateDocument)) return;
                if (!stateDocument.TryGetProperty("buttonevent", out var buttonEventText)) return;

                buttonEvent.Event = buttonEventText.GetInt64();

                Context.System.EventStream.Publish(buttonEvent);
                Console.WriteLine("[Thread {0}, Actor {1}] Message sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
            });
        }
    }
}