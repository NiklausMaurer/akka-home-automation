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
            Receive<string>(webSocketMessage =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var document = JsonDocument.Parse(webSocketMessage);

                ButtonEventMessage message = new ButtonEventMessage
                {
                    MessageType = document.RootElement.GetProperty("t").GetString(),
                    EventType = document.RootElement.GetProperty("e").GetString(),
                    ResourceType = document.RootElement.GetProperty("r").GetString(),
                    ResourceId = document.RootElement.GetProperty("id").GetString(),
                };

                if (!document.RootElement.TryGetProperty("state", out var stateDocument)) return;
                if (!stateDocument.TryGetProperty("buttonevent", out var buttonEvent)) return;

                message.ButtonEvent = buttonEvent.GetInt64();

                Context.System.EventStream.Publish(message);
                Console.WriteLine("[Thread {0}, Actor {1}] Message sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
            });
        }
    }
}