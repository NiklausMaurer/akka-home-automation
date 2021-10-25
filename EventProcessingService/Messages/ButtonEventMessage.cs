namespace EventProcessingService.Messages
{
    public class ButtonEventMessage
    {
        public string MessageType { get; set; }
        public string EventType { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public long ButtonEvent { get; set; }
    }
}