namespace EspConverter.Models.TES3
{
    public class TES3_Race
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string[] spells { get; set; }
        public string description { get; set; }
        public TES3_Race_Data data { get; set; }
    }

}
