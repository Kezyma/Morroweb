namespace EspConverter.Models.TES3
{
    public class TES3_Faction
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string[] rank_names { get; set; }
        public TES3_Faction_Reaction[] reactions { get; set; }
        public TES3_Faction_Data data { get; set; }
    }

}
