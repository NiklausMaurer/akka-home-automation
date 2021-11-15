namespace EventProcessingService.Messages.Lights
{
    public class TurnOffCommand
    {
        public int Attempt { get; } = 1;

        public TurnOffCommand()
        {
        }
        
        public TurnOffCommand(int attempt)
        {
            Attempt = attempt;
        }
    }
}