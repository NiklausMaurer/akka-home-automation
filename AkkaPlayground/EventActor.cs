using System;
using System.Threading;
using Akka.Actor;

namespace AkkaPlayground
{
    public class EventActor : ReceiveActor
    {
        public EventActor()
        {
            Receive<ButtonEventMessage>(message =>
            {
                Console.WriteLine("[Thread {0}, Actor {1}] Message received", Thread.CurrentThread.ManagedThreadId, Self.Path);
                
                if (message.MessageType != "event" || message.EventType != "changed" ||
                    message.ResourceType != "sensors" ||
                    message.ResourceId != "9") return;

                Context.System.EventStream.Publish(message.ButtonEvent == 1002
                    ? LightsCommandMessage.TurnOff("15")
                    : LightsCommandMessage.TurnOn("15"));
            });
        }
    }
}