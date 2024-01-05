using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebClass
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Playable { get; set; }
        public string Specialisation { get; set; }
        public string[] Attributes { get; set; }
        public string[] Minor { get; set; }
        public string[] Major { get; set; }
    }
}
