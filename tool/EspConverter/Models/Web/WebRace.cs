using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebRace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Spells { get; set; }
        public Dictionary<string, int> SkillBonuses { get; set; }
        public Dictionary<string, int[]> Attributes { get; set; }
        public float[] Height { get; set; }
        public float[] Weight { get; set; }
        public bool Playable { get; set; }
        public bool Beast { get; set; }
    }
}
