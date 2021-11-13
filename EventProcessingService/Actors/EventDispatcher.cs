using System;
using System.Threading;
using Akka.Actor;
using EventProcessingService.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventProcessingService.Actors
{
    public class IncomingEvent
    {
        [JsonProperty("t")]
        public string MessageType { get; set; }
        
        [JsonProperty("e")]
        public string EventType { get; set; }
        
        [JsonProperty("r")]
        public string ResourceType { get; set; }
        
        [JsonProperty("id")]
        public string ResourceId { get; set; }
        
        [JsonProperty("state")]
        public IncomingEventState State { get; set; }
    }

    public class IncomingEventState
    {
        [JsonProperty("buttonevent")]
        public int? ButtonEvent { get; set; }
        
        [JsonProperty("lastupdated")]
        public DateTime? LastUpdated { get; set; }
    }
    
    public class EventDispatcher : ReceiveActor
    {
        public EventDispatcher()
        {
            Receive<string>(message =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                var incomingEvent =  JObject.Parse(message).ToObject<IncomingEvent>();
                if (incomingEvent == null) throw new Exception($"Parsing of message {message} failed.");
                
                if (incomingEvent.MessageType != "event" || incomingEvent.EventType != "changed" ||
                    incomingEvent.ResourceType != "sensors" ||
                    incomingEvent.ResourceId != "9" || incomingEvent.State is null) return;

                Context.System.EventStream.Publish(incomingEvent.State.ButtonEvent == 1002
                    ? LightsCommandMessage.TurnOff("15")
                    : LightsCommandMessage.TurnOn("15"));
                
                Console.WriteLine("[Thread {0}, Actor {1}] Message sent", Thread.CurrentThread.ManagedThreadId, Self.Path);
            });
        }
    }
}