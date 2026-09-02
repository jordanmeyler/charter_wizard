using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// What the camera can see. Play draws from the weave; Develop still
    /// lists the eleven on the wall. Only runes in this vicinity can be
    /// strung in Play.
    /// </summary>
    public static class RuneField
    {
        public const float PerceptionRadius = RuneTapestry.PerceptionRadius;

        public static readonly RuneId[] StartingStream = RuneCatalog.BasicRunes;

        public static List<RuneId> Perceive(Vector3 origin, WorldGrid grid, ISpellLock[] locks)
        {
            return RuneTapestry.Perceive(origin, grid, locks);
        }
    }
}
