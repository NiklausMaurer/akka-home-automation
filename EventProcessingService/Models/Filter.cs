using System.Collections.Generic;
using System.Linq;

namespace EventProcessingService.Models
{
    public class Filter
    {
        public Filter(string key, string value)
        {
            Key = key;
            Value = value;
        }

        private string Key { get; set; }
        private string Value { get; set; }

        public IEnumerable<Light> Apply(IEnumerable<Light> lights)
        {
            return lights.Where(l => l.HasLabel(Key, Value));
        }
    }
}