using Advanced_Combat_Tracker;

namespace Overlay
{
    /// <summary>
    /// Data about a death.
    /// </summary>
    public class WhoDied
    {
        /// <summary>
        /// Character name
        /// </summary>
        public string who;
        /// <summary>
        /// Encounter index for the death
        /// </summary>
        public int killedAtIndex;
        /// <summary>
        /// Number of deaths so far
        /// </summary>
        public int deathCount;
        /// <summary>
        /// Encounter data that includes the death
        /// </summary>
        public EncounterData ed;

        /// <summary>
        /// Display when set as a listbox Item.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{deathCount}. {who}";
        }
    }
}
