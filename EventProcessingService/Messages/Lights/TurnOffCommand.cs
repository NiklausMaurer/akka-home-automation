namespace EventProcessingService.Messages.Lights
{
    public class TurnOffCommand
    {
        public TurnOffCommand(string lightId)
        {
            LightId = lightId;
        }

        public string LightId { get; }
    }
}