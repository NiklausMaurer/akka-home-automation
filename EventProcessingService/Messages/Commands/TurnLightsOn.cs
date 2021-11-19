using EventProcessingService.Models;

namespace EventProcessingService.Messages.Commands
{
    public class TurnLightsOn
    {
        public TurnLightsOn(Selector selector)
        {
            Selector = selector;
        }

        public Selector Selector { get; set; }
    }
}