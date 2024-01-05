using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspConverter.Models.Web
{
    public class WebDialogueLine
    {
        public string Id { get; set; }
        public string PrevId { get; set; }
        public string NextId { get; set; }

        public string NPCSex { get; set; }
        public string NPCId { get; set; }
        public string NPCRace { get; set; }
        public string NPCClass { get; set; }
        public string NPCFaction { get; set; }
        public int? NPCRank { get; set; }
        public string NPCCell { get; set; }

        public int Disposition { get; set; }
        public string PCFaction { get; set; }
        public int? PCRank { get; set; }

        public string Text { get; set; }
        public string[] Scripts { get; set; }
        public WebDialogueFilter[] Filters { get; set; }
    }
}
