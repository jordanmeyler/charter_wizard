using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum RuneSourceKind
    {
        Tile,
        Creature,
        String,
        Presence
    }

    /// <summary>
    /// Anything that speaks runes into the room's Charter weave. Tiles
    /// first; later the adept's place, world-strings, and other living details.
    /// </summary>
    public interface IRuneSource
    {
        bool IsEmitting { get; }
        Vector3 WorldOrigin { get; }
        float VoiceRadius { get; }
        float VoiceWeight { get; }
        RuneSourceKind SourceKind { get; }

        void Collect(List<RuneId> buffer);
    }
}
