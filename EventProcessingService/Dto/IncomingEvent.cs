using EventProcessingService.Actors;
using Newtonsoft.Json;

namespace EventProcessingService.Dto
{
    public class IncomingEvent
    {
        public IncomingEvent(string messageType, string eventType, string resourceType, string resourceId, IncomingEventState state)
        {
            MessageType = messageType;
            EventType = eventType;
            ResourceType = resourceType;
            ResourceId = resourceId;
            State = state;
        }

        [JsonProperty("t")] public string MessageType { get; set; }

        [JsonProperty("e")] public string EventType { get; set; }

        [JsonProperty("r")] public string ResourceType { get; set; }

        [JsonProperty("id")] public string ResourceId { get; set; }

        [JsonProperty("state")] public IncomingEventState State { get; set; }
    }
}