using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// After the sentence is written, this is what the world does:
    /// tiles react, bodies take conditions, and every lock in the area
    /// is offered the key.
    /// </summary>
    public readonly struct SpellImpactResult
    {
        public SpellImpactResult(string note, IReadOnlyList<ISpellLock> locks)
        {
            Note = note;
            Locks = locks;
        }

        public string Note { get; }
        public IReadOnlyList<ISpellLock> Locks { get; }
    }

    public static class SpellImpact
    {
        public static SpellImpactResult Apply(
            WorldGrid grid,
            IReadOnlyList<ISpellLock> locks,
            SpellId spell,
            SpellShape shape,
            Vector3 origin,
            Vector3 aim,
            float potency,
            Composition composition = default)
        {
            var verb = SpellVerb.Of(spell, shape);
            var notes = new List<string>(4);
            var hits = new List<ISpellLock>();
            var radius = SpellVerb.RadiusOf(spell, shape, potency);
            var sweep = WorldPhysics.Build(grid, spell, shape, origin, origin, aim, potency);
            var heldRunes = FocusLaw.UsedRunes(spell, composition);
            WorldPhysics.BlowFog(locks, sweep);

            if (verb.Target == SpellTarget.Self)
            {
                var adept = AdeptAvatar.Find();
                if (adept != null && verb.Status != StatusId.None)
                {
                    var host = StatusHost.On(adept) ?? adept.gameObject.AddComponent<StatusHost>();
                    notes.Add(host.Apply(verb.Status, verb.StatusSeconds, adept, heldRunes, spell));
                }

                WorldPhysics.Collect(locks, sweep, hits, verb, grid);
                ApplyTiles(grid, spell, verb, origin, radius, notes);
                return new SpellImpactResult(First(notes), hits);
            }

            if (verb.Target == SpellTarget.Area || shape == SpellShape.Spread || WorldPhysics.SweepsPath(spell, shape))
            {
                var center = shape == SpellShape.Spread ? origin : aim;
                WorldPhysics.Collect(locks, sweep, hits, verb, grid);
                ApplyHosts(hits, origin, verb, notes, heldRunes, spell);
                var self = AdeptAvatar.Find();
                if (self != null && Vector2.Distance(self.transform.position, center) <= radius && verb.Status != StatusId.None)
                {
                    var host = StatusHost.On(self);
                    if (host != null && !ElementalLaw.IsWard(verb.Status) && verb.Status != StatusId.Poisoned)
                    {
                        notes.Add(host.Apply(verb.Status, verb.StatusSeconds * 0.45f, self, heldRunes, spell));
                    }
                }

                ApplyTiles(grid, spell, verb, center, Mathf.Max(radius, sweep.Width), notes);
                return new SpellImpactResult(First(notes), hits);
            }

            if (WorldWork.IsSinglePillar(spell) && (WorldWork.IsFireWork(spell) || spell == SpellId.LavaPillar))
            {
                WorldPhysics.Collect(locks, sweep, hits, verb, grid);
            }

            if (hits.Count == 0)
            {
                var single = WorldPhysics.Nearest(locks, aim, Mathf.Max(radius, sweep.Width, 1.55f), verb, grid, spell, origin);
                if (single != null)
                {
                    hits.Add(single);
                }
            }

            ApplyHosts(hits, origin, verb, notes, heldRunes, spell);
            ApplyTiles(grid, spell, verb, aim, Mathf.Max(0.8f, radius, sweep.Width), notes);
            return new SpellImpactResult(First(notes), hits);
        }

        static void ApplyHosts(
            List<ISpellLock> hits,
            Vector3 from,
            SpellVerb verb,
            List<string> notes,
            IReadOnlyList<RuneId> heldRunes,
            SpellId spell)
        {
            for (var i = 0; i < hits.Count; i++)
            {
                if (hits[i] is not MonoBehaviour body || body == null)
                {
                    continue;
                }

                var host = StatusHost.On(body);
                if (host == null)
                {
                    continue;
                }

                if (verb.Status != StatusId.None)
                {
                    notes.Add(host.Apply(verb.Status, verb.StatusSeconds, AdeptAvatar.Find(), heldRunes, spell));
                }

                if (WorldWork.IsOilWork(spell) && host.Nature == CreatureNature.Fire)
                {
                    var actor = body.GetComponent<CombatActor>();
                    actor?.FeedOil();
                    notes.Add("The hungering body drinks the fuel and grows.");
                }
            }
        }

        static void ApplyTiles(WorldGrid grid, SpellId spell, SpellVerb verb, Vector3 center, float radius, List<string> notes)
        {
            if (grid == null || verb.Tiles == TileVerb.None)
            {
                return;
            }

            var cells = grid.TilesInRadius(center, Mathf.Max(0.6f, radius));
            var changed = 0;
            var frozen = 0;
            var spreadLeft = PlantLaw.MaxSpread(spell);
            var growBy = PlantLaw.GrowSteps(spell);
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = cells[i];
                switch (verb.Tiles)
                {
                    case TileVerb.Ignite:
                        tile.Ignite(0.85f);
                        if (tile.MeltWith(spell))
                        {
                            changed++;
                        }

                        changed++;
                        break;
                    case TileVerb.Douse:
                    case TileVerb.Wet:
                        tile.Drench(1f);
                        if (tile.IsPlantish || tile.HasPlantCover || tile.HasPlantishDetail)
                        {
                            if (tile.IsPlantish && growBy > 0)
                            {
                                tile.Grow(growBy);
                            }

                            if (spreadLeft > 0)
                            {
                                var took = grid.SpreadPlant(tile, spreadLeft);
                                spreadLeft -= took;
                                if (took > 0)
                                {
                                    changed++;
                                }
                            }
                        }

                        changed++;
                        break;
                    case TileVerb.Grow:
                        if (PlantLaw.PlantsNewBodies(spell) && tile.CanTakePlant)
                        {
                            tile.PlantHere();
                            changed++;
                        }
                        else if (tile.IsPlantish || tile.HasPlantCover || tile.HasPlantishDetail)
                        {
                            if (tile.IsPlantish && growBy > 0)
                            {
                                tile.Grow(growBy);
                            }

                            changed++;
                        }

                        if (spreadLeft > 0
                            && !PlantLaw.FillsVisibleWater(spell)
                            && (tile.HasWaterSource || tile.IsPlantish || tile.HasPlantCover))
                        {
                            var took = grid.SpreadPlant(tile, spreadLeft);
                            spreadLeft -= took;
                            if (took > 0)
                            {
                                changed++;
                            }
                        }

                        break;
                    case TileVerb.Charge:
                        if (ChargeLaw.AcceptsDirectCharge(tile.Conductivity))
                        {
                            tile.ChargeAt(1f);
                            changed++;
                        }

                        break;
                    case TileVerb.Freeze:
                        if (tile.FreezeSolid())
                        {
                            frozen++;
                            changed++;
                        }
                        else
                        {
                            tile.Drench(0.4f);
                        }

                        break;
                    case TileVerb.Cloak:
                        tile.Cloak(1f);
                        changed++;
                        break;
                    case TileVerb.Foul:
                        tile.Foul(1f);
                        changed++;
                        break;
                    case TileVerb.Vent:
                        if (tile.Vent(spell))
                        {
                            changed++;
                        }

                        break;
                    case TileVerb.Dirt:
                        if (tile.LayDirt())
                        {
                            changed++;
                        }

                        break;
                    case TileVerb.Slick:
                        if (tile.SlickOil())
                        {
                            changed++;
                        }

                        break;
                    case TileVerb.Vine:
                        if (tile.LayVine())
                        {
                            changed++;
                        }

                        break;
                }
            }

            if (PlantLaw.FillsVisibleWater(spell) && spreadLeft > 0)
            {
                var seed = grid.TileAtWorld(center) ?? (cells.Count > 0 ? cells[0] : null);
                if (seed != null)
                {
                    var took = grid.SpreadPlant(seed, spreadLeft, visibleOnly: true);
                    if (took > 0)
                    {
                        changed++;
                    }
                }
            }

            if (verb.Tiles == TileVerb.Douse || verb.Tiles == TileVerb.Wet)
            {
                var seeds = new List<Vector2Int>(cells.Count);
                for (var i = 0; i < cells.Count; i++)
                {
                    seeds.Add(cells[i].Coord);
                }

                var filled = WorldWork.FillSmallPits(grid, seeds);
                if (filled > 0)
                {
                    notes.Add("Yield fills a small hollow. The water has no floor. Freeze it, or fall.");
                }
            }

            if (frozen > 0)
            {
                notes.Add(frozen == 1
                    ? "Yield given a body. That water is ice."
                    : "Hard water stands. The pool will hold you.");
            }

            if (changed > 0 && frozen == 0)
            {
                notes.Add(verb.Tiles == TileVerb.Grow
                    ? spell == SpellId.Forest
                        ? "A living plant opens to every water you can see."
                        : "The vegetable body drinks."
                    : verb.Tiles == TileVerb.Ignite
                        ? "Hunger finds the floor."
                        : verb.Tiles == TileVerb.Charge
                            ? "The seed runs where it can."
                            : verb.Tiles == TileVerb.Vent
                                ? "Breath finds the tile."
                                : verb.Tiles == TileVerb.Cloak
                                    ? "The hanging veil is given a body."
                                    : verb.Tiles == TileVerb.Foul
                                        ? "A sick mist stands on the floor."
                                        : verb.Tiles == TileVerb.Dirt
                                            ? "Loose rest lands. Ground-fire dies. Earth speaks here."
                                            : verb.Tiles == TileVerb.Vine
                                                ? "The vegetable body climbs. Hunger can run this line as a wick."
                                                : "Yield finds the floor.");
            }
        }

        static string First(List<string> notes)
        {
            for (var i = 0; i < notes.Count; i++)
            {
                if (!string.IsNullOrEmpty(notes[i]))
                {
                    return notes[i];
                }
            }

            return string.Empty;
        }
    }
}
