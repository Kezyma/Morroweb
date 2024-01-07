namespace EspConverter.Models.TES3
{
    public class TES3_LeveledCreature
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string leveled_creature_flags { get; set; }
        public int chance_none { get; set; }
        public object[][] creatures { get; set; }
    }

}
