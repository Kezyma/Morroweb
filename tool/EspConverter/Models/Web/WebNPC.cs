using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebNPC
    {
        public WebDialogueLine[] Greetings { get; set; }
        public Dictionary<string, WebDialogueLine[]> Topics { get; set; }
    }
}
