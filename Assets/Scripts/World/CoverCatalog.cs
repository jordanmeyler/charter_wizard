using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Covers speak the current catalog, same marks as inscriptions.
    /// Ice is Water · Earth. Fire cover only marks hunger so the
    /// weave can speak Fire — it does not kindle a hall. A stamped
    /// cover is inert until a spell or a live reaction finds it;
    /// then it uses the overlay material (ice melts, oil is fuel,
    /// metal conducts). Vine cover speaks Plant — Vine is a spell,
    /// not a rune. Miasma is Cloud · Acid. Fog is the Cloud veil.
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
                case TileCover.Fire: return MaterialId.Ember;
                case TileCover.Lightning: return MaterialId.Vein;
                case TileCover.Water: return MaterialId.Water;
                case TileCover.Vine: return MaterialId.Plant;
                case TileCover.Miasma: return MaterialId.Miasma;
                case TileCover.Fog: return MaterialId.Cloud;
                case TileCover.Mud: return MaterialId.Mud;
                default: return MaterialId.None;
            }
        }

        /// <summary>
        /// A material stamped on the Cover layer that is not just
        /// look — oil, metal, plant, ice — without starting live fire,
        /// charge, or wet on its own.
        /// </summary>
        public static bool IsOverlayMaterial(MaterialId material)
        {
            return material != MaterialId.None
                && material != MaterialId.Stone
                && material != MaterialId.Void;
        }

        public static void SpeakMaterial(MaterialId material, ICollection<RuneId> dest)
        {
            if (dest == null || material == MaterialId.None)
            {
                return;
            }

            var def = MaterialCatalog.Of(material);
            if (def.Manifestation != RuneId.None)
            {
                dest.Add(def.Manifestation);
            }

            var signature = def.Signature;
            for (var i = 0; i < signature.Count; i++)
            {
                if (signature[i] != RuneId.None)
                {
                    dest.Add(signature[i]);
                }
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
            if (tile == null)
            {
                return false;
            }

            rune = RuneOf(tile.Cover);
            if (rune != RuneId.None)
            {
                return true;
            }

            if (tile.CoverMaterial == MaterialId.None)
            {
                return false;
            }

            rune = MaterialCatalog.Of(tile.CoverMaterial).Manifestation;
            if (rune == RuneId.None && MaterialCatalog.Of(tile.CoverMaterial).Signature.Count > 0)
            {
                rune = MaterialCatalog.Of(tile.CoverMaterial).Signature[0];
            }

            return rune != RuneId.None;
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (RuneOf(TileCover.Fire) != RuneId.Fire)
            {
                broken.Add("Fire cover must speak Fire so a stamp can grant the rune");
            }

            if (MaterialOf(TileCover.Fire) != MaterialId.Ember
                || MaterialOf(TileCover.Ice) != MaterialId.Ice
                || MaterialOf(TileCover.Water) != MaterialId.Water
                || MaterialOf(TileCover.Lightning) != MaterialId.Vein
                || MaterialOf(TileCover.Vine) != MaterialId.Plant)
            {
                broken.Add("Spoken covers must name an overlay material so spells can find them");
            }

            if (!IsOverlayMaterial(MaterialId.Oil)
                || !IsOverlayMaterial(MaterialId.Metal)
                || IsOverlayMaterial(MaterialId.Stone))
            {
                broken.Add("Oil and metal covers must react; stone is the walk family, not an overlay");
            }

            if (WorldPaintTile.AuraFromCover(TileCover.Fire) != TileAura.None)
            {
                broken.Add("Fire cover is a mark — it must not map onto a kindled hall aura");
            }

            if (WorldPaintTile.CoverFromAura(TileAura.Fire) != TileCover.Fire)
            {
                broken.Add("A Fire aura still looks like fire cover");
            }
        }
    }
}
