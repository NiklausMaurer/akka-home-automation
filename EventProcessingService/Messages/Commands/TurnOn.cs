namespace EventProcessingService.Messages.Commands
{
    public class TurnOn
    {
        public TurnOn()
        {
        }

        public TurnOn(int attempt)
        {
            Attempt = attempt;
        }

        public int Attempt { get; } = 1;

        public TurnOn NewAttempt()
        {
            return new TurnOn(Attempt + 1);
        }
    }
}