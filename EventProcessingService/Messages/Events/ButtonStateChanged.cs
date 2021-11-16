namespace EventProcessingService.Messages.Events
{
    public class ButtonStateChanged
    {
        public string ButtonId { get; set; }
        public int EventId { get; set; }
    }
}