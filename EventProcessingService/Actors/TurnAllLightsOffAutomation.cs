using Akka.Actor;
using EventProcessingService.Messages.Commands;
using EventProcessingService.Messages.Commands.Shared;
using EventProcessingService.Messages.Events;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOffAutomation : ReceiveActor
    {
        public TurnAllLightsOffAutomation()
        {
            Context.System.EventStream.Subscribe(Self, typeof(ButtonStateChanged));
            
            Receive<ButtonStateChanged>(ReceiveButtonEvent);
        }
        
        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new TurnAllLightsOffAutomation());
        }

        private void ReceiveButtonEvent(ButtonStateChanged buttonStateChanged)
        {
            if (buttonStateChanged.ButtonId != "10") return;
            if (buttonStateChanged.EventId != 1001) return;

            Context.ActorSelection("/user/lights")
                .Tell(new TurnLightsOff(new Selector(new[] { new Filter("id", "15") })));
        }
    }
}