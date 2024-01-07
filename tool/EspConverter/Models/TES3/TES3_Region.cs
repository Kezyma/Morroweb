namespace EspConverter.Models.TES3
{
    public class TES3_Region
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public TES3_Region_Weather_Chances weather_chances { get; set; }
        public string sleep_creature { get; set; }
        public int[] map_color { get; set; }
        public object[][] sounds { get; set; }
    }

}
