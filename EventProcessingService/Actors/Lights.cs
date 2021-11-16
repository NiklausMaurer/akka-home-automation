using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using EventProcessingService.Messages.Commands;
using Newtonsoft.Json.Linq;

namespace EventProcessingService.Actors
{
    public class Lights : ReceiveActor
    {
        private static HttpClient _httpClient;

        public Lights(CancellationToken cancellationToken)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
            };
            
            var lights = FetchLightDtos(cancellationToken);
            
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

        public static Props Props(CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new Lights(cancellationToken));
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
        
        private Collection<LightDto> FetchLightDtos(CancellationToken stoppingToken)
        {
            var getAsyncTask = _httpClient.GetAsync("lights", stoppingToken);
            var response = getAsyncTask .GetAwaiter().GetResult();
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception("Getting lights info failed");

            var readAsyncTask = response.Content.ReadAsStringAsync(stoppingToken);
            var content = readAsyncTask.GetAwaiter().GetResult();
            
            var lightsDocument = JObject.Parse(content);
            var lightDict = lightsDocument.ToObject<Dictionary<string, LightDto>>();
            if (lightDict is null) throw new Exception("Lights could not be parsed.");

            foreach (var keyValuePair in lightDict) keyValuePair.Value.Id = keyValuePair.Key;

            var lightDtos = new Collection<LightDto>(lightDict.Values.ToList());

            if (lightDict is null) throw new Exception("Parsing of lghts failed");
            return lightDtos;
        }
    }
}