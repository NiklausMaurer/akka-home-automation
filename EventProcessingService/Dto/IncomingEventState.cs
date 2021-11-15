using System;
using Newtonsoft.Json;

namespace EventProcessingService.Dto
{
    public class IncomingEventState
    {
        [JsonProperty("buttonevent")] public int? ButtonEvent { get; set; }

        [JsonProperty("on")] public bool? IsOn { get; set; }

        [JsonProperty("lastupdated")] public DateTime? LastUpdated { get; set; }
    }
}