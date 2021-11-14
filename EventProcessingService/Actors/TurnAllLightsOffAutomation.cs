using System.Collections.Generic;
using Akka.Actor;
using EventProcessingService.Messages.Lights;

namespace EventProcessingService.Actors
{
    public class TurnAllLightsOffAutomation : ReceiveActor
    {
        public static Props Props(Dictionary<string, LightDto> lightDtos) =>
            Akka.Actor.Props.Create(() => new TurnAllLightsOffAutomation(lightDtos));

        public TurnAllLightsOffAutomation(Dictionary<string, LightDto> lightDtos)
        {
            Receive<ButtonEvent>(buttonEvent =>
            {
                if (buttonEvent.ButtonId != "9") return;

                foreach (KeyValuePair<string,LightDto> keyValuePair in lightDtos)
                {
                    Context.System.EventStream.Publish(buttonEvent.EventId == 1002
                        ? new TurnOffCommand(keyValuePair.Key)
                        : new TurnOnCommand(keyValuePair.Key));
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