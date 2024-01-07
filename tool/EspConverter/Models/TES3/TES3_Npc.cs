namespace EspConverter.Models.TES3
{
    public class TES3_Npc
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string script { get; set; }
        public string mesh { get; set; }
        public object[][] inventory { get; set; }
        public string[] spells { get; set; }
        public TES3_Npc_Ai_Data ai_data { get; set; }
        public TES3_Npc_Ai_Packages[] ai_packages { get; set; }
        public TES3_Npc_Travel_Destinations[] travel_destinations { get; set; }
        public string race { get; set; }
        public string _class { get; set; }
        public string faction { get; set; }
        public string head { get; set; }
        public string hair { get; set; }
        public string npc_flags { get; set; }
        public int blood_type { get; set; }
        public TES3_Npc_Data data { get; set; }
    }

}
