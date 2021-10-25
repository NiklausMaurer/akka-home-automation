namespace AkkaPlayground.Messages
{
    public class LightsCommandMessage
    {
        public static LightsCommandMessage TurnOn(string lightId) => new LightsCommandMessage
        {
            Command = LightsCommand.TurnOn,
            LightId = lightId
        };
        
        public static LightsCommandMessage TurnOff(string lightId) => new LightsCommandMessage
        {
            Command = LightsCommand.TurnOff,
            LightId = lightId
        };
        
        public enum LightsCommand
        {
            TurnOn,
            TurnOff
        }

        public LightsCommand Command { get; set; }
        public string LightId { get; set; }
    }
}