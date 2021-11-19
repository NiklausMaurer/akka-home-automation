using System;
using System.Collections.Generic;
using System.Linq;

namespace EventProcessingService.Models
{
    public class Light
    {
        public Light(string id)
        {
            Id = id;
        }

        public void AddLabel(string key, string value)
        {
            Labels.Add(key, value);
        }

        public bool HasLabel(string key, string value)
        {
            return Labels.Keys.Contains(key) &&
                   Labels[key].Equals(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public IEnumerable<KeyValuePair<string, string>> GetLabels()
        {
            return Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public string Id { get; set; }
        private Dictionary<string, string> Labels { get; set; } = new();
    }
}