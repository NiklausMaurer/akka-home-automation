using Akka.Actor;
using Akka.DependencyInjection;

namespace EventProcessingService.Extensions
{
    public static class ActorSystemExtensions
    {
        public static void CreateActor<T>(this ActorSystem system, string name = null!) where T : ActorBase
        {
            var props = DependencyResolver.For(system).Props<T>();
            system.ActorOf(props, name);
        }
    }
}