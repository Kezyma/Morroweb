using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebFaction
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string[] Ranks { get; set; }
        public Dictionary<string, int> Reactions { get; set; }
        public string[] Attributes { get; set; }
        public string[] Skills { get; set; }
        public WebFactionRequirement[] Requirements { get; set; }
    }
}

