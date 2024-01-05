using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebDialogueType
    {
        public string Name { get; set; }
        public WebDialogueTopic[] Topics { get; set; }
    }
}
