namespace EspConverter.Models.TES3
{
    public class TES3_Enchanting
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public TES3_Enchanting_Effect[] effects { get; set; }
        public TES3_Enchanting_Data data { get; set; }
    }

}
