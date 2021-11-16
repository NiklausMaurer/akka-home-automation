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

            Receive<TurnOn>(TurnOn);
            Receive<TurnOff>(TurnOff);
        }

        private Dictionary<string, IActorRef> LightRefs { get; } = new();

        public static Props Props(ICollection<LightDto> lights)
        {
            return Akka.Actor.Props.Create(() => new Lights(lights));
        }

        private void TurnOn(TurnOn turnOn)
        {
            foreach (var light in LightRefs)
            {
                light.Value.Tell(new TurnOn());
            }
        }

        private void TurnOff(TurnOff turnOff)
        {
            foreach (var light in LightRefs)
            {
                light.Value.Tell(new TurnOff());
            }
        }
    }
}