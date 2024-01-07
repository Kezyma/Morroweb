namespace EspConverter.Models.TES3
{
    public class TES3_Spell
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public TES3_Spell_Effect[] effects { get; set; }
        public TES3_Spell_Data data { get; set; }
    }

}
