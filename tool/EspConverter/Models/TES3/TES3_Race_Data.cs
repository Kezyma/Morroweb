namespace EspConverter.Models.TES3
{
    public class TES3_Race_Data
    {
        public TES3_Race_Skill_Bonuses skill_bonuses { get; set; }
        public int[] strength { get; set; }
        public int[] intelligence { get; set; }
        public int[] willpower { get; set; }
        public int[] agility { get; set; }
        public int[] speed { get; set; }
        public int[] endurance { get; set; }
        public int[] personality { get; set; }
        public int[] luck { get; set; }
        public float[] height { get; set; }
        public float[] weight { get; set; }
        public string flags { get; set; }
    }

}
