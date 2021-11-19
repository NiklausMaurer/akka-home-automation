using Akka.Actor;
using EventProcessingService.Messages.Commands;
using EventProcessingService.Messages.Events;
using EventProcessingService.Models;

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
            if (buttonStateChanged.ButtonId != "9") return;
            if (buttonStateChanged.EventId != 1002) return;

            Context.ActorSelection("/user/lights")
                .Tell(new TurnLightsOff(new Selector(new[] { new Filter("id", "15") })));
        }
    }
}