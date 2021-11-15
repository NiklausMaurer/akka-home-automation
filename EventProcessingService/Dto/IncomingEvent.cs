using EventProcessingService.Actors;
using Newtonsoft.Json;

namespace EventProcessingService.Dto
{
    public class IncomingEvent
    {
        [JsonProperty("t")] public string MessageType { get; set; }

        [JsonProperty("e")] public string EventType { get; set; }

        [JsonProperty("r")] public string ResourceType { get; set; }

        [JsonProperty("id")] public string ResourceId { get; set; }

        [JsonProperty("state")] public IncomingEventState State { get; set; }
    }
}