namespace EspConverter.Models.TES3
{
    public class TES3_Probe
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string script { get; set; }
        public string mesh { get; set; }
        public string icon { get; set; }
        public TES3_Probe_Data data { get; set; }
    }

}
