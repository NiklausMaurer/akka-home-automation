namespace AkkaPlayground.Messages
{
    public class LightsCommandMessage
    {
        public enum LightAction
        {
            TurnOn,
            TurnOff
        }
        
        public LightAction Action { get; private set; }
        public string LightId { get; private set; }
        
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
    }
}