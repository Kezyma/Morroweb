namespace EspConverter.Models.TES3
{
    public class TES3_Faction_Data
    {
        public string[] favored_attributes { get; set; }
        public TES3_Faction_Requirement[] requirements { get; set; }
        public string[] favored_skills { get; set; }
        public string flags { get; set; }
    }

}
