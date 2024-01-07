namespace EspConverter.Models.TES3
{
    public class TES3_Pathgrid
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string cell { get; set; }
        public TES3_Pathgrid_Data data { get; set; }
        public TES3_Pathgrid_Point[] points { get; set; }
        public string connections { get; set; }
    }

}
