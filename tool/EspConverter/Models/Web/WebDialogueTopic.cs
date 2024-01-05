using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebDialogueTopic
    {
        public string Text { get; set; }
        public List<WebDialogueLine> Lines { get; set; }
    }
}
