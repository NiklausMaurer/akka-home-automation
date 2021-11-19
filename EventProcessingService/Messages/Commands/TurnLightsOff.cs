using EventProcessingService.Models;

namespace EventProcessingService.Messages.Commands
{
    public class TurnLightsOff
    {
        public TurnLightsOff(Selector selector)
        {
            Selector = selector;
        }

        public Selector Selector { get; set; }
    }
}