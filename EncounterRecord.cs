using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advanced_Combat_Tracker;
using Overlay;

namespace DeathCount_Plugin
{
    class EncounterRecord
    {
        public EncounterData ed; //kind of redundant since it's also in WhoDied, but easier access
        public List<WhoDied> deaths = new List<WhoDied>();
    }
}
