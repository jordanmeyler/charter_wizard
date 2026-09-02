using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Covers speak the current catalog, same marks as inscriptions.
    /// Ice is Water · Earth. Fire cover is the live hunger layer:
    /// it can catch and interact once a spell starts work, and it
    /// always puts Fire in the weave so it can be drawn. It does
    /// not kindle a hall. Ember cover is coals: it provides fire
    /// and stays on the walk. Floor-Fire / Wall-Fire are rest matter.
    /// When fuel is spent, fire cover wears off and a plant or
    /// timber walk swaps to leftover dirt (look and stamp). Vine
    /// cover speaks Plant — Vine is a spell, not a rune.
    /// Miasma is Cloud · Acid, a hanging fog wind must take.
    /// Poison is a liquid slick yield washes. Fog is the Cloud veil.
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
            TileCover.Poison,
            TileCover.Fog,
            TileCover.Mud,
            TileCover.Ash,
            TileCover.Ember
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
                case TileCover.Poison: return RuneId.Poison;
                case TileCover.Fog: return RuneId.Cloud;
                case TileCover.Mud: return RuneId.Mud;
                case TileCover.Ash: return RuneId.Ash;
                case TileCover.Ember: return RuneId.Ember;
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
                    return TileCover.Fire;
                case RuneId.Ember:
                    return TileCover.Ember;
                case RuneId.Lightning:
                case RuneId.Spark:
                    return TileCover.Lightning;
                case RuneId.Water:
                    return TileCover.Water;
                case RuneId.Plant:
                    return TileCover.Vine;
                case RuneId.Miasma:
                    return TileCover.Miasma;
                case RuneId.Poison:
                case RuneId.Acid:
                    return TileCover.Poison;
                case RuneId.Cloud:
                    return TileCover.Fog;
                case RuneId.Mud:
                    return TileCover.Mud;
                case RuneId.Ash:
                    return TileCover.Ash;
                default:
                    return TileCover.None;
            }
        }

        public static MaterialId MaterialOf(TileCover cover)
        {
            switch (cover)
            {
                case TileCover.Ice: return MaterialId.Ice;
                case TileCover.Fire: return MaterialId.Fire;
                case TileCover.Lightning: return MaterialId.Vein;
                case TileCover.Water: return MaterialId.Water;
                case TileCover.Vine: return MaterialId.Plant;
                case TileCover.Miasma: return MaterialId.Miasma;
                case TileCover.Poison: return MaterialId.Acid;
                case TileCover.Fog: return MaterialId.Cloud;
                case TileCover.Mud: return MaterialId.Mud;
                case TileCover.Ash: return MaterialId.Ash;
                case TileCover.Ember: return MaterialId.Ember;
                default: return MaterialId.None;
            }
        }

        /// <summary>
        /// One sheen per spoken cover. Ice-shot, ice-wall over water,
        /// and a stamped ice mark all draw the same ice sheet.
        /// </summary>
        public static string SheenId(TileCover cover)
        {
            switch (cover)
            {
                case TileCover.Ice: return "cover-ice";
                case TileCover.Fire: return "cover-fire";
                case TileCover.Lightning: return "cover-lightning";
                case TileCover.Vine: return "cover-vine";
                case TileCover.Miasma: return "tile-poison";
                case TileCover.Poison: return "tile-wet";
                case TileCover.Fog: return "tile-fog";
                case TileCover.Mud: return "floor-mud";
                case TileCover.Ash: return "floor-ash";
                case TileCover.Ember: return "fx-ember";
                default: return null;
            }
        }

        public static Sprite Sheen(TileCover cover)
        {
            var id = SheenId(cover);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (LookLibrary.TryAuthored(id, out var authored) && authored != null)
            {
                return authored;
            }

            return TileAtlas.TryGet(id, out var sprite) && sprite != null
                ? sprite
                : null;
        }

        /// <summary>
        /// What the walk becomes when fuel is spent. Plant, timber,
        /// oil, and grit become dirt (look and stamp). A timber or
        /// plant wall falls to that leftover dirt. Masonry stays
        /// stone. This is a swap, not an ash covering. Water, ice,
        /// lava, and void are not leftover walks.
        /// </summary>
        public static MaterialId RestAfterBurn(MaterialId walk)
        {
            switch (walk)
            {
                case MaterialId.Plant:
                case MaterialId.Timber:
                case MaterialId.Moss:
                case MaterialId.Grove:
                case MaterialId.Oil:
                case MaterialId.Dust:
                    return MaterialId.Dirt;
                case MaterialId.Dirt:
                case MaterialId.Sand:
                case MaterialId.Stone:
                case MaterialId.Hearth:
                case MaterialId.Damp:
                case MaterialId.Scoured:
                case MaterialId.SaltCrust:
                case MaterialId.Wardstone:
                case MaterialId.Fire:
                case MaterialId.Ember:
                    return walk;
                default:
                    return MaterialId.None;
            }
        }

        /// <summary>
        /// A spent burnable floor leaves dirt. The stamp and the
        /// floor tile both swap. Covers and spells may sit on that
        /// leftover; burn-out does not draw ash over the old tile.
        /// </summary>
        public static MaterialId LeftoverFloor(MaterialId walk)
        {
            return RestAfterBurn(walk);
        }

        public static bool Speaks(TileCover cover, RuneId rune)
        {
            if (rune == RuneId.None)
            {
                return false;
            }

            var dest = new HashSet<RuneId>();
            Speak(cover, dest);
            return dest.Contains(rune);
        }

        public static void AshAt(Vector3 world)
        {
            var grid = Object.FindFirstObjectByType<WorldGrid>();
            grid?.TileAtWorld(world)?.BurnOut();
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

            dest.Add(rune);
            var material = MaterialOf(cover);
            if (material != MaterialId.None)
            {
                SpeakMaterial(material, dest);
                return;
            }

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

            if (RuneOf(TileCover.Fire) != RuneId.Fire
                || !Speaks(TileCover.Fire, RuneId.Fire)
                || !Speaks(TileCover.Ice, RuneId.Ice)
                || !Speaks(TileCover.Ash, RuneId.Ash)
                || !Speaks(TileCover.Vine, RuneId.Plant)
                || !Speaks(TileCover.Ember, RuneId.Ember)
                || !Speaks(TileCover.Ember, RuneId.Fire))
            {
                broken.Add("A spoken cover must put its own rune in the weave so it can be drawn");
            }

            if (RestAfterBurn(MaterialId.Plant) != MaterialId.Dirt
                || RestAfterBurn(MaterialId.Timber) != MaterialId.Dirt
                || RestAfterBurn(MaterialId.Oil) != MaterialId.Dirt
                || RestAfterBurn(MaterialId.Stone) != MaterialId.Stone
                || RestAfterBurn(MaterialId.Dirt) != MaterialId.Dirt
                || RestAfterBurn(MaterialId.Fire) != MaterialId.Fire
                || RestAfterBurn(MaterialId.Water) != MaterialId.None)
            {
                broken.Add("Spent fuel becomes dirt; masonry and fire-rest stay; water is not leftover walk");
            }

            if (LeftoverFloor(MaterialId.Plant) != MaterialId.Dirt
                || LeftoverFloor(MaterialId.Timber) != MaterialId.Dirt
                || LeftoverFloor(MaterialId.Stone) != MaterialId.Stone
                || LeftoverFloor(MaterialId.Dirt) != MaterialId.Dirt)
            {
                broken.Add("A spent plant or timber floor swaps to dirt (look and stamp), not an ash covering");
            }

            if (LeftoverFloor(MaterialId.Fire) != MaterialId.Fire
                || LeftoverFloor(MaterialId.Hearth) != MaterialId.Hearth
                || LeftoverFloor(MaterialId.Ember) != MaterialId.Ember)
            {
                broken.Add("Fire, hearth, and ember marks stay; they are not leftover dirt");
            }

            var emberSpeak = new HashSet<RuneId>();
            SpeakMaterial(MaterialId.Ember, emberSpeak);
            if (!emberSpeak.Contains(RuneId.Fire)
                || MaterialCatalog.Of(MaterialId.Ember).Manifestation != RuneId.Fire)
            {
                broken.Add("Ember must speak Fire and stay embered — it is not leftover dirt");
            }

            if (CoverOf(RuneId.Ember) != TileCover.Ember
                || RuneOf(TileCover.Ember) != RuneId.Ember
                || MaterialOf(TileCover.Ember) != MaterialId.Ember
                || SheenId(TileCover.Ember) != "fx-ember")
            {
                broken.Add("Ember cover is coals on the walk — not fire cover that wears off");
            }

            if (MaterialOf(TileCover.Fire) != MaterialId.Fire
                || MaterialOf(TileCover.Ice) != MaterialId.Ice
                || MaterialOf(TileCover.Water) != MaterialId.Water
                || MaterialOf(TileCover.Lightning) != MaterialId.Vein
                || MaterialOf(TileCover.Vine) != MaterialId.Plant
                || MaterialOf(TileCover.Poison) != MaterialId.Acid
                || MaterialOf(TileCover.Miasma) != MaterialId.Miasma)
            {
                broken.Add("Spoken covers must name an overlay material so spells can find them");
            }

            if (CoverOf(RuneId.Poison) != TileCover.Poison
                || CoverOf(RuneId.Miasma) != TileCover.Miasma
                || CoverOf(RuneId.Acid) != TileCover.Poison)
            {
                broken.Add("Poison is a liquid slick; miasma is the airborne cloud");
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

            if (WorldPaintTile.AutomaticOpacity(TileCover.Fire) >= 0.95f)
            {
                broken.Add("Fire cover is a sheen over the walk tile, not an opaque replacement");
            }

            if (WorldPaintTile.CoverFromAura(TileAura.Fire) != TileCover.Fire)
            {
                broken.Add("A Fire aura still looks like fire cover");
            }

            if (SheenId(TileCover.Ice) != "cover-ice"
                || SheenId(TileCover.Fire) != "cover-fire"
                || SheenId(TileCover.Lightning) != "cover-lightning"
                || SheenId(TileCover.Miasma) != "tile-poison"
                || SheenId(TileCover.Poison) != "tile-wet")
            {
                broken.Add("Each spoken cover must use one sheen so ice-shot and ice-wall match");
            }
        }
    }
}
