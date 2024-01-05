using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Tes3
{
    public class Tes3Object
    {
        public string type { get; set; }
        public string flags { get; set; }
        public float version { get; set; }
        public string file_type { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public int num_objects { get; set; }
        public object[] masters { get; set; }
        public string id { get; set; }
        public object value { get; set; }
        public string global_type { get; set; }
        public string name { get; set; }
        public Tes3Data data { get; set; }
        public string[] rank_names { get; set; }
        public Tes3Reaction[] reactions { get; set; }
        public string[] spells { get; set; }
        public string sound_path { get; set; }
        public string sound_gen_type { get; set; }
        public string creature { get; set; }
        public string sound { get; set; }
        public string skill_id { get; set; }
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
        public Tes3Header header { get; set; }
        public string variables { get; set; }
        public string bytecode { get; set; }
        public string text { get; set; }
        public Tes3WeatherChances weather_chances { get; set; }
        public string sleep_creature { get; set; }
        public int[] map_color { get; set; }
        public object[][] sounds { get; set; }
        public int index { get; set; }
        public string file_name { get; set; }
        public Tes3Effect[] effects { get; set; }
        public string mesh { get; set; }
        public string script { get; set; }
        public string open_sound { get; set; }
        public string close_sound { get; set; }
        public string enchanting { get; set; }
        public float encumbrance { get; set; }
        public string container_flags { get; set; }
        public object[][] inventory { get; set; }
        public Tes3AiData ai_data { get; set; }
        public Tes3AiPackages[] ai_packages { get; set; }
        public Tes3TravelDestinations[] travel_destinations { get; set; }
        public string creature_flags { get; set; }
        public int blood_type { get; set; }
        public string race { get; set; }
        public string _class { get; set; }
        public string faction { get; set; }
        public string head { get; set; }
        public string hair { get; set; }
        public string npc_flags { get; set; }
        public Tes3BipedObjects[] biped_objects { get; set; }
        public string leveled_item_flags { get; set; }
        public int chance_none { get; set; }
        public object[][] items { get; set; }
        public string leveled_creature_flags { get; set; }
        public object[][] creatures { get; set; }
        public string region { get; set; }
        public Tes3Reference[] references { get; set; }
        public float water_height { get; set; }
        public Tes3AtmosphereData atmosphere_data { get; set; }
        public int[] grid { get; set; }
        public string landscape_flags { get; set; }
        public Tes3VertexNormals vertex_normals { get; set; }
        public Tes3VertexHeights vertex_heights { get; set; }
        public Tes3WorldMapData world_map_data { get; set; }
        public Tes3VertexColors vertex_colors { get; set; }
        public Tes3TextureIndices texture_indices { get; set; }
        public string cell { get; set; }
        public Tes3Point[] points { get; set; }
        public string connections { get; set; }
        public string dialogue_type { get; set; }
        public string prev_id { get; set; }
        public string next_id { get; set; }
        public string speaker_id { get; set; }
        public string speaker_race { get; set; }
        public string speaker_class { get; set; }
        public string speaker_faction { get; set; }
        public string speaker_cell { get; set; }
        public string player_faction { get; set; }
        public Tes3Filter[] filters { get; set; }
        public string script_text { get; set; }
    }
}
