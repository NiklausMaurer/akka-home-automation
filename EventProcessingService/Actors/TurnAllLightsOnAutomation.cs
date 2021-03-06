using Akka.Actor;
using EventProcessingService.Messages.Commands;
using EventProcessingService.Messages.Commands.Shared;
using EventProcessingService.Messages.Events;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOnAutomation : ReceiveActor
    {
        public TurnAllLightsOnAutomation()
        {
            Context.System.EventStream.Subscribe(Self, typeof(ButtonStateChanged));
            
            Receive<ButtonStateChanged>(ReceiveButtonEvent);
        }
        
        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new TurnAllLightsOnAutomation());
        }

        private void ReceiveButtonEvent(ButtonStateChanged buttonStateChanged)
        {
            if (buttonStateChanged.ButtonId != "10") return;
            if (buttonStateChanged.EventId != 1002) return;

            Context.ActorSelection("/user/lights")
                .Tell(new TurnLightsOn(new Selector(new[] { new Filter("id", "15") })));
        }
    }
}