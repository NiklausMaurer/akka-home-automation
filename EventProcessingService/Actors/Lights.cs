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

            Receive<LightsAction>(ReceiveLightsAction);
        }

        private Dictionary<string, IActorRef> LightRefs { get; } = new();

        public static Props Props(ICollection<LightDto> lights)
        {
            return Akka.Actor.Props.Create(() => new Lights(lights));
        }

        private void ReceiveLightsAction(LightsAction action)
        {
            if (action.Type == LightsActionType.TurnOff)
            {
                foreach (var light in LightRefs)
                {
                    light.Value.Tell(new TurnOff());
                }
            }
            else
            {
                foreach (var light in LightRefs)
                {
                    light.Value.Tell(new TurnOnCommand());
                }
            }
        }
    }

    public enum LightsActionType
    {
        TurnOn,
        TurnOff
    }
    
    public class LightsAction
    {
        public LightsActionType Type;
    }
}