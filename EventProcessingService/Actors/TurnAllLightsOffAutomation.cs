using Akka.Actor;
using EventProcessingService.Messages.Lights;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOffAutomation : ReceiveActor
    {
        public TurnAllLightsOffAutomation()
        {
            Receive<ButtonEvent>(buttonEvent =>
            {
                if (buttonEvent.ButtonId != "9") return;
                
                 Context.System.EventStream.Publish(buttonEvent.EventId == 1002
                    ? new TurnOffCommand("15")
                    : new TurnOnCommand("15"));
            });
        }
    }

    public class ButtonEvent
    {
        public string ButtonId { get; set; }
        public int EventId { get; set; }
    }
}