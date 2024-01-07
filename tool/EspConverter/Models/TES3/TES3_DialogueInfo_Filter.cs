namespace EspConverter.Models.TES3
{
    public class TES3_DialogueInfo_Filter
    {
        public string slot { get; set; }
        public string filter_type { get; set; }
        public string function { get; set; }
        public string comparison { get; set; }
        public string id { get; set; }
        public TES3_DialogueInfo_Value value { get; set; }
    }

}
