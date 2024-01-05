using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebSpell
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public WebSpellEffect[] Effects { get; set; }
    }
}
