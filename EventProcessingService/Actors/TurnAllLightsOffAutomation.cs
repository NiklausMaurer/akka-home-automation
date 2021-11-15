using System.Collections.Generic;
using Akka.Actor;
using EventProcessingService.Messages.Lights;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOffAutomation : ReceiveActor
    {
        public TurnAllLightsOffAutomation(ICollection<LightDto> lights)
        {
            Lights = lights;

            Receive<ButtonEvent>(ReceiveButtonEvent);
        }

        private ICollection<LightDto> Lights { get; }

        public static Props Props(ICollection<LightDto> lights)
        {
            return Akka.Actor.Props.Create(() => new TurnAllLightsOffAutomation(lights));
        }

        private void ReceiveButtonEvent(ButtonEvent buttonEvent)
        {
            if (buttonEvent.ButtonId != "9") return;

            foreach (var light in Lights)
            {
                if (light.Id != "9") continue;

                Context.System.EventStream.Publish(buttonEvent.EventId == 1002
                    ? new TurnOffCommand(light.Id)
                    : new TurnOnCommand(light.Id));
            }
        }
    }

    public class ButtonEvent
    {
        public string ButtonId { get; set; }
        public int EventId { get; set; }
    }
}