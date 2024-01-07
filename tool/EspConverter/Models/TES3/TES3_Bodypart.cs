namespace EspConverter.Models.TES3
{
    public class TES3_Bodypart
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string race { get; set; }
        public string mesh { get; set; }
        public TES3_Bodypart_Data data { get; set; }
    }

}
