using System;
using Akka.Actor;
using EventProcessingService.Dto;
using EventProcessingService.Messages.Events;
using Newtonsoft.Json.Linq;

namespace EventProcessingService.Actors
{
    public class EventDispatcher : ReceiveActor
    {
        public EventDispatcher()
        {
            Receive<string>(message =>
            {
                var incomingEvent = JObject.Parse(message).ToObject<IncomingEvent>();
                if (incomingEvent == null) throw new Exception($"Parsing of message {message} failed.");

                if (incomingEvent.MessageType != "event" ||
                    incomingEvent.EventType != "changed" ||
                    incomingEvent.State is null) return;

                if (incomingEvent.State.ButtonEvent.HasValue)
                {
                    Context.System.EventStream.Publish(new ButtonStateChanged
                    {
                        ButtonId = incomingEvent.ResourceId,
                        EventId = incomingEvent.State.ButtonEvent.Value
                    });
                }
            });
        }
    }
}