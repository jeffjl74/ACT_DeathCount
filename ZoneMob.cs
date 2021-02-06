using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathCount_Plugin
{
    class ZoneMob
    {
        public string mobName;
        public string zoneName;
        public int? warningLevel;
        public int? maxDeaths;

        public ZoneMob(string mob, string zone, int? warning, int? deaths)
        {
            mobName = mob;
            zoneName = zone;
            warningLevel = warning;
            maxDeaths = deaths;
        }

        /// <summary>
        /// Provide a class summary while debugging
        /// </summary>
        public override string ToString()
        {
            return $"{mobName}:{zoneName}";
        }
    }
}
