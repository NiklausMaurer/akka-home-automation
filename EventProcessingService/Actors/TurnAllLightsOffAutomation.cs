using System.Collections.Generic;
using Akka.Actor;
using EventProcessingService.Messages.Lights;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOffAutomation : ReceiveActor
    {
        public static Props Props(ICollection<LightDto> lightDtos) =>
            Akka.Actor.Props.Create(() => new TurnAllLightsOffAutomation(lightDtos));

        public TurnAllLightsOffAutomation(ICollection<LightDto> lightDtos)
        {
            Receive<ButtonEvent>(buttonEvent =>
            {
                if (buttonEvent.ButtonId != "9") return;

                foreach (var lightDto in lightDtos)
                {
                    Context.System.EventStream.Publish(buttonEvent.EventId == 1002
                        ? new TurnOffCommand(lightDto.Id)
                        : new TurnOnCommand(lightDto.Id));
                }
            });
        }
    }

    public class ButtonEvent
    {
        public string ButtonId { get; set; }
        public int EventId { get; set; }
    }
}