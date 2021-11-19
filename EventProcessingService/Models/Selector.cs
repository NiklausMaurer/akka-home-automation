using System.Collections.Generic;
using System.Linq;

namespace EventProcessingService.Models
{
    public class Selector
    {
        public Selector(IEnumerable<Filter> filters)
        {
            Filters.AddRange(filters);
        }

        private List<Filter> Filters { get; } = new();

        public IEnumerable<Light> Select(IEnumerable<Light> lights)
        {
            return Filters.Aggregate(lights, (current, filter) => filter.Apply(current));
        }
    }
}