using System.Collections.Generic;
using System.Linq;

namespace EventProcessingService.Messages.Commands.Shared
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

        public IEnumerable<Light> ApplyTo(IEnumerable<Light> lights)
        {
            return lights.Where(l => l.HasLabel(Key, Value));
        }
    }
}