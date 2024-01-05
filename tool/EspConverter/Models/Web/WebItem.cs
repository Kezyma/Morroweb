using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebItem
    {
        public string Name { get; set; }
        public string Icon {  get; set; }
        public string Type { get; set; }
        public float Weight { get; set; }
        public int Value { get; set; }
        public string Flags { get; set; }
    }
}
