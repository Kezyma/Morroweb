using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebSkill
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int Attribute { get; set; }
        public int Specialisation { get; set; }
        public float[] Actions { get; set; }

    }
}
