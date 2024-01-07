namespace EspConverter.Models.TES3
{
    public class TES3_Landscape
    {
        public string type { get; set; }
        public string flags { get; set; }
        public int[] grid { get; set; }
        public string landscape_flags { get; set; }
        public TES3_Landscape_Vertex_Normals vertex_normals { get; set; }
        public TES3_Landscape_Vertex_Heights vertex_heights { get; set; }
        public TES3_Landscape_World_Map_Data world_map_data { get; set; }
        public TES3_Landscape_Vertex_Colors vertex_colors { get; set; }
        public TES3_Landscape_Texture_Indices texture_indices { get; set; }
    }

}
