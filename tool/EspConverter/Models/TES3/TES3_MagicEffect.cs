namespace EspConverter.Models.TES3
{
    public class TES3_MagicEffect
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string effect_id { get; set; }
        public string icon { get; set; }
        public string texture { get; set; }
        public string bolt_sound { get; set; }
        public string cast_sound { get; set; }
        public string hit_sound { get; set; }
        public string area_sound { get; set; }
        public string cast_visual { get; set; }
        public string bolt_visual { get; set; }
        public string hit_visual { get; set; }
        public string area_visual { get; set; }
        public string description { get; set; }
        public TES3_MagicEffect_Data data { get; set; }
    }

}
