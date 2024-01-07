namespace EspConverter.Models.TES3
{
    public class TES3_GameSetting
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public TES3_GameSetting_Value value { get; set; }
    }

}
