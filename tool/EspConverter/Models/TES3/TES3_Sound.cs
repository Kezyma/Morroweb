namespace EspConverter.Models.TES3
{
    public class TES3_Sound
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string sound_path { get; set; }
        public TES3_Sound_Data data { get; set; }
    }

}
