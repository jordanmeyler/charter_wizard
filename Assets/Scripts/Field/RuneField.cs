using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Perception helper for the Charter wall. The living picture of the
    /// field is <see cref="RuneTapestry"/> — this only lists what can be strung.
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
