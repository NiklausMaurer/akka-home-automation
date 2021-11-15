using System.Collections.Generic;
using Akka.Actor;
using EventProcessingService.Messages.Lights;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOffAutomation : ReceiveActor
    {
        public TurnAllLightsOffAutomation(ICollection<LightDto> lights)
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
            return Akka.Actor.Props.Create(() => new TurnAllLightsOffAutomation(lights));
        }

        private void ReceiveButtonEvent(ButtonEvent buttonEvent)
        {
            if (buttonEvent.ButtonId != "9") return;

            foreach (var light in Lights)
                light.Value.Tell(buttonEvent.EventId == 1002 ? new TurnOffCommand() : new TurnOnCommand());
        }
    }

    public class ButtonEvent
    {
        public string ButtonId { get; set; }
        public int EventId { get; set; }
    }
}