namespace EventProcessingService.Messages.Lights
{
    public class TurnOnCommand
    {
        public TurnOnCommand()
        {
        }

        public TurnOnCommand(int attempt)
        {
            Attempt = attempt;
        }

        public int Attempt { get; } = 1;
    }
}