namespace EspConverter.Models.TES3
{
    public class TES3_LeveledItem
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string leveled_item_flags { get; set; }
        public int chance_none { get; set; }
        public object[][] items { get; set; }
    }

}
