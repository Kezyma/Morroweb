namespace EspConverter.Models.TES3
{
    public class TES3_Skill
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string skill_id { get; set; }
        public TES3_Skill_Data data { get; set; }
        public string description { get; set; }
    }

}
