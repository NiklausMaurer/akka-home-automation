using System.Collections.Generic;
using Akka.Actor;
using EventProcessingService.Messages.Commands;

namespace EventProcessingService.Actors
{
    public class Lights : ReceiveActor
    {
        public Lights(ICollection<LightDto> lights)
        {
            foreach (var light in lights)
            {
                if (light.Type.Equals("On/Off plug-in unit")) continue;
                if (light.Type.Equals("Configuration tool")) continue;

                LightRefs[light.Id] = Context.ActorOf(Light.Props(light.Id), $"light-{light.Id}");
            }

            Receive<TurnLightsOn>(TurnLightsOn);
            Receive<TurnLightsOff>(TurnLightsOff);
        }

        private Dictionary<string, IActorRef> LightRefs { get; } = new();

        public static Props Props(ICollection<LightDto> lights)
        {
            return Akka.Actor.Props.Create(() => new Lights(lights));
        }

        private void TurnLightsOn(TurnLightsOn turnOn)
        {
            foreach (var light in LightRefs)
            {
                light.Value.Tell(new TurnOn());
            }
        }

        private void TurnLightsOff(TurnLightsOff turnOff)
        {
            foreach (var light in LightRefs)
            {
                light.Value.Tell(new TurnOff());
            }
        }
    }
}