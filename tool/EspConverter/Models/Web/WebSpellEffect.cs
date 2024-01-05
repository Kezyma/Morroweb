namespace EspConverter.Models.Web
{
    public class WebSpellEffect
    {
        public string Effect { get; set; }
        public string Skill { get; set; }
        public string Attribute { get; set; }
        public string Range { get; set; }
        public int Area { get; set; }
        public int Duration { get; set; }
        public int[] Magnitude { get; set; }
     } 
}
