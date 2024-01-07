namespace EspConverter.Models.TES3
{
    public class TES3_Creature
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string script { get; set; }
        public string mesh { get; set; }
        public object[][] inventory { get; set; }
        public string[] spells { get; set; }
        public TES3_Creature_Ai_Data ai_data { get; set; }
        public TES3_Creature_Ai_Packages[] ai_packages { get; set; }
        public object[] travel_destinations { get; set; }
        public string sound { get; set; }
        public string creature_flags { get; set; }
        public int blood_type { get; set; }
        public TES3_Creature_Data data { get; set; }
    }

}
