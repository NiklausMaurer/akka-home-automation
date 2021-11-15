namespace EventProcessingService.Messages.Lights
{
    public class TurnOffCommand
    {
        public TurnOffCommand()
        {
        }

        public TurnOffCommand(int attempt)
        {
            Attempt = attempt;
        }

        public int Attempt { get; } = 1;
    }
}