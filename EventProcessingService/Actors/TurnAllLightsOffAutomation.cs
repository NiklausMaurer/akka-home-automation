using Akka.Actor;
using EventProcessingService.Messages.Events;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOffAutomation : ReceiveActor
    {
        public TurnAllLightsOffAutomation()
        {
            Context.System.EventStream.Subscribe(Self, typeof(ButtonEvent));
            
            Receive<ButtonEvent>(ReceiveButtonEvent);
        }
        
        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new TurnAllLightsOffAutomation());
        }

        private void ReceiveButtonEvent(ButtonEvent buttonEvent)
        {
            if (buttonEvent.ButtonId != "9") return;
            if (buttonEvent.EventId != 1002) return;

            Context.ActorSelection("/user/lights").Tell(new LightsAction
            {
                Type = LightsActionType.TurnOff
            });
        }
    }
}