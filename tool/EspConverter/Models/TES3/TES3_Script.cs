namespace EspConverter.Models.TES3
{
    public class TES3_Script
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public TES3_Script_Header header { get; set; }
        public string variables { get; set; }
        public string bytecode { get; set; }
        public string text { get; set; }
    }

}
