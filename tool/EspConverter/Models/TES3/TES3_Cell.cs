namespace EspConverter.Models.TES3
{
    public class TES3_Cell
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string name { get; set; }
        public TES3_Cell_Data data { get; set; }
        public string region { get; set; }
        public TES3_Cell_Reference[] references { get; set; }
        public int[] map_color { get; set; }
        public float water_height { get; set; }
        public TES3_Cell_Atmosphere_Data atmosphere_data { get; set; }
    }

}
