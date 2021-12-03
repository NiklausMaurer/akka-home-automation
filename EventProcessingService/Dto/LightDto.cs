using Newtonsoft.Json;

namespace EventProcessingService.Dto
{
    public class LightDto
    {
        public LightDto(string id, string name, string type)
        {
            Id = id;
            Name = name;
            Type = type;
        }

        [JsonIgnore] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("type")] public string Type { get; set; }
    }
}