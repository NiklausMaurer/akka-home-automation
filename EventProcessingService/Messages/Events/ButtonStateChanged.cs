namespace EventProcessingService.Messages.Events
{
    public class ButtonStateChanged
    {
        public ButtonStateChanged(string buttonId, int eventId)
        {
            ButtonId = buttonId;
            EventId = eventId;
        }

        public string ButtonId { get; set; }
        public int EventId { get; set; }
    }
}