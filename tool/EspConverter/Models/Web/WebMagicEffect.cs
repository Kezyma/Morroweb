using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebMagicEffect
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string School { get; set; }
        public float BaseCost { get; set; }
        public bool Spellmaking { get; set; }
        public bool Enchanting { get; set; }
        public float Speed { get; set; }
        public float Size { get; set; }
        public float SizeCap { get; set; }
    }
}
