namespace EventProcessingService.Messages.Commands
{
    public class TurnOff
    {
        public TurnOff()
        {
        }

        public TurnOff(int attempt)
        {
            Attempt = attempt;
        }

        public int Attempt { get; } = 1;
        
        public TurnOff NewAttempt()
        {
            return new TurnOff(Attempt + 1);
        }
    }
}