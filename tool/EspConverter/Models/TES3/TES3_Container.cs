namespace EspConverter.Models.TES3
{
    public class TES3_Container
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string script { get; set; }
        public string mesh { get; set; }
        public float encumbrance { get; set; }
        public string container_flags { get; set; }
        public object[][] inventory { get; set; }
    }

}
