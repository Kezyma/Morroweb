namespace EspConverter.Models.TES3
{
    public class TES3_DialogueInfo
    {
        public string type { get; set; }
        public string flags { get; set; }
        public string id { get; set; }
        public string prev_id { get; set; }
        public string next_id { get; set; }
        public TES3_DialogueInfo_Data data { get; set; }
        public string speaker_id { get; set; }
        public string speaker_race { get; set; }
        public string speaker_class { get; set; }
        public string speaker_faction { get; set; }
        public string speaker_cell { get; set; }
        public string player_faction { get; set; }
        public string sound_path { get; set; }
        public string text { get; set; }
        public TES3_DialogueInfo_Filter[] filters { get; set; }
        public string script_text { get; set; }
    }

}
