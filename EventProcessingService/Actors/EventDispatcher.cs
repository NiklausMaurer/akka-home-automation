using System;
using Akka.Actor;
using EventProcessingService.Dto;
using EventProcessingService.Messages.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EventProcessingService.Actors
{
    public class EventDispatcher : ReceiveActor
    {
        private ILogger<EventDispatcher> Logger { get; }

        public EventDispatcher(ILogger<EventDispatcher> logger)
        {
            Logger = logger;
            
            Receive<string>(message =>
            {
                var incomingEvent = JObject.Parse(message).ToObject<IncomingEvent>();
                if (incomingEvent == null) throw new Exception($"Parsing of message {message} failed.");

                if (incomingEvent.MessageType != "event" ||
                    incomingEvent.EventType != "changed" ||
                    incomingEvent.State is null ||
                    !incomingEvent.State.ButtonEvent.HasValue) return;

                if (incomingEvent.State.ButtonEvent.HasValue)
                {
                    var msg = new ButtonStateChanged(incomingEvent.ResourceId, incomingEvent.State.ButtonEvent.Value);
                    var msgJson = JsonSerializer.Serialize(msg);
                    
                    logger.LogInformation("Publishing button event: {MsgJson}", msgJson);
                    
                    Context.System.EventStream.Publish(msg);
                }
            });
        }
        
        public static Props Props(ILogger<EventDispatcher> logger)
        {
            return Akka.Actor.Props.Create(() => new EventDispatcher(logger));
        }
    }
}