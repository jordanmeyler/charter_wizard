using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Covers speak the current catalog, same marks as inscriptions.
    /// Ice is Water · Earth. Vine cover speaks Plant — Vine is a
    /// spell, not a rune. Miasma is Cloud · Acid. Fog is the Cloud
    /// veil — weather, not its own rune.
    /// </summary>
    public static class CoverCatalog
    {
        public static readonly TileCover[] Spoken =
        {
            TileCover.Ice,
            TileCover.Fire,
            TileCover.Lightning,
            TileCover.Water,
            TileCover.Vine,
            TileCover.Miasma,
            TileCover.Fog,
            TileCover.Mud
        };

        public static RuneId RuneOf(TileCover cover)
        {
            switch (cover)
            {
                case TileCover.Ice: return RuneId.Ice;
                case TileCover.Fire: return RuneId.Fire;
                case TileCover.Lightning: return RuneId.Lightning;
                case TileCover.Water: return RuneId.Water;
                case TileCover.Vine: return RuneId.Plant;
                case TileCover.Miasma: return RuneId.Miasma;
                case TileCover.Fog: return RuneId.Cloud;
                case TileCover.Mud: return RuneId.Mud;
                default: return RuneId.None;
            }
        }

        public static TileCover CoverOf(RuneId rune)
        {
            switch (rune)
            {
                case RuneId.Ice:
                    return TileCover.Ice;
                case RuneId.Fire:
                case RuneId.Flame:
                case RuneId.Ember:
                    return TileCover.Fire;
                case RuneId.Lightning:
                case RuneId.Spark:
                    return TileCover.Lightning;
                case RuneId.Water:
                    return TileCover.Water;
                case RuneId.Plant:
                    return TileCover.Vine;
                case RuneId.Miasma:
                case RuneId.Poison:
                    return TileCover.Miasma;
                case RuneId.Cloud:
                    return TileCover.Fog;
                case RuneId.Mud:
                    return TileCover.Mud;
                default:
                    return TileCover.None;
            }
        }

        public static MaterialId MaterialOf(TileCover cover)
        {
            switch (cover)
            {
                case TileCover.Ice: return MaterialId.Ice;
                case TileCover.Miasma: return MaterialId.Miasma;
                case TileCover.Fog: return MaterialId.Cloud;
                case TileCover.Mud: return MaterialId.Mud;
                default: return MaterialId.None;
            }
        }

        public static void Speak(TileCover cover, ICollection<RuneId> dest)
        {
            if (dest == null)
            {
                return;
            }

            var rune = RuneOf(cover);
            if (rune == RuneId.None)
            {
                return;
            }

            var material = MaterialOf(cover);
            if (material != MaterialId.None)
            {
                var signature = MaterialCatalog.Of(material).Signature;
                for (var i = 0; i < signature.Count; i++)
                {
                    if (signature[i] != RuneId.None)
                    {
                        dest.Add(signature[i]);
                    }
                }

                return;
            }

            dest.Add(rune);
            if (!ChainBook.TryBirth(rune, out var sources) || sources == null)
            {
                return;
            }

            for (var i = 0; i < sources.Count; i++)
            {
                if (sources[i] != RuneId.None)
                {
                    dest.Add(sources[i]);
                }
            }
        }

        public static bool TryPick(Vector3 world, out RuneId rune)
        {
            rune = RuneId.None;
            var grid = Object.FindFirstObjectByType<WorldGrid>();
            var tile = grid != null ? grid.TileAtWorld(world) : null;
            if (tile == null || !tile.IsEmitting)
            {
                return false;
            }

            rune = RuneOf(tile.Cover);
            return rune != RuneId.None;
        }
    }
}
