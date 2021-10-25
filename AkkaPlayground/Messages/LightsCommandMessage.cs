namespace AkkaPlayground.Messages
{
    public class LightsCommandMessage
    {
        public static LightsCommandMessage TurnOn(string lightId) => new LightsCommandMessage
        {
            Action = LightAction.TurnOn,
            LightId = lightId
        };
        
        public static LightsCommandMessage TurnOff(string lightId) => new LightsCommandMessage
        {
            Action = LightAction.TurnOff,
            LightId = lightId
        };
        
        public enum LightAction
        {
            TurnOn,
            TurnOff
        }

        public LightAction Action { get; set; }
        public string LightId { get; set; }
    }
}