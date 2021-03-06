using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using Akka.DependencyInjection;
using EventProcessingService.Dto;
using EventProcessingService.Messages.Commands;
using Newtonsoft.Json.Linq;

namespace EventProcessingService.Actors
{
    public class Lights : ReceiveActor
    {
        private IHttpClientFactory HttpClientFactory { get; }

        public Lights(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
            
            var lightDtos = FetchLights(CancellationToken.None);

            foreach (var light in lightDtos)
            {
                 var lightModel = new Messages.Commands.Shared.Light(light.Id);
                 lightModel.AddLabel("id", light.Id);
                 if(light.Type.ToLower().Contains("light")) lightModel.AddLabel("type", "light");
                 if(light.Type.ToLower().Contains("plug")) lightModel.AddLabel("type", "plug");
                 LightModels.Add(lightModel);
                 
                 var props = DependencyResolver.For(Context.System).Props<Light>(light.Id);
                 LightRefs[light.Id] = Context.ActorOf(props, $"light-{light.Id}");
            }

            Receive<TurnLightsOn>(TurnLightsOn);
            Receive<TurnLightsOff>(TurnLightsOff);
        }

        private Dictionary<string, IActorRef> LightRefs { get; } = new();
        private List<Messages.Commands.Shared.Light> LightModels { get; } = new();

        public static Props Props(IHttpClientFactory httpClientFactory)
        {
            return Akka.Actor.Props.Create(() => new Lights( httpClientFactory));
        }

        private void TurnLightsOn(TurnLightsOn turnOn)
        {
            foreach (var light in turnOn.Selector.Select(LightModels))
            {
                LightRefs[light.Id].Tell(new TurnOn());
            }
        }

        private void TurnLightsOff(TurnLightsOff turnOff)
        {
            foreach (var light in turnOff.Selector.Select(LightModels))
            {   
                LightRefs[light.Id].Tell(new TurnOff());
            }
        }
        
        private Collection<LightDto> FetchLights(CancellationToken stoppingToken)
        {
            var client = HttpClientFactory.CreateClient("deconz");
            var getAsyncTask = client.GetAsync("lights", stoppingToken);
            var response = getAsyncTask .GetAwaiter().GetResult();
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception("Getting lights info failed");

            var readAsyncTask = response.Content.ReadAsStringAsync(stoppingToken);
            var content = readAsyncTask.GetAwaiter().GetResult();
            
            var lightsDocument = JObject.Parse(content);
            var lightDict = lightsDocument.ToObject<Dictionary<string, LightDto>>();
            if (lightDict is null) throw new Exception("Lights could not be parsed.");

            foreach (var keyValuePair in lightDict) keyValuePair.Value.Id = keyValuePair.Key;

            var lights = new Collection<LightDto>(lightDict.Values.ToList());

            if (lightDict is null) throw new Exception("Parsing of lghts failed");
            return lights;
        }
    }
}