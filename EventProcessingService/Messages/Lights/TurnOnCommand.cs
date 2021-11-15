namespace EventProcessingService.Messages.Lights
{
    public class TurnOnCommand
    {
        public int Attempt { get; } = 1;

        public TurnOnCommand()
        {
        }
        
        public TurnOnCommand(int attempt)
        {
            Attempt = attempt;
        }
    }
}