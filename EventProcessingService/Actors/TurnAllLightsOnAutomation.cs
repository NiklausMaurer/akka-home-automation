using System.Collections.Generic;
using Akka.Actor;
using EventProcessingService.Messages.Commands;
using EventProcessingService.Messages.Events;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOnAutomation : ReceiveActor
    {
        public TurnAllLightsOnAutomation(ICollection<LightDto> lights)
        {
            Context.System.EventStream.Subscribe(Self, typeof(ButtonEvent));
            
            foreach (var light in lights)
            {
                if (light.Type.Equals("On/Off plug-in unit")) continue;
                if (light.Type.Equals("Configuration tool")) continue;

                Lights[light.Id] = Context.ActorOf(Light.Props(light.Id), $"light-{light.Id}");
            }

            Receive<ButtonEvent>(ReceiveButtonEvent);
        }

        private Dictionary<string, IActorRef> Lights { get; } = new();

        public static Props Props(ICollection<LightDto> lights)
        {
            return Akka.Actor.Props.Create(() => new TurnAllLightsOnAutomation(lights));
        }

        private void ReceiveButtonEvent(ButtonEvent buttonEvent)
        {
            if (buttonEvent.ButtonId != "9") return;
            if (buttonEvent.EventId != 2002) return;

            foreach (var light in Lights)
            {
                light.Value.Tell(new TurnOnCommand());
            }
            
        }
    }
}