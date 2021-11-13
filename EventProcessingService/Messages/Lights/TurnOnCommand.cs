namespace EventProcessingService.Messages.Lights
{
    public class TurnOnCommand
    {
        public TurnOnCommand(string lightId)
        {
            LightId = lightId;
        }

        public string LightId { get; }
    }
}