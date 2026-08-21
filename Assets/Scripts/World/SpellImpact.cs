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
            float potency)
        {
            var verb = SpellVerb.Of(spell, shape);
            var notes = new List<string>(4);
            var hits = new List<ISpellLock>();
            var radius = SpellVerb.RadiusOf(spell, shape, potency);

            if (verb.Target == SpellTarget.Self)
            {
                var adept = AdeptAvatar.Find();
                if (adept != null && verb.Status != StatusId.None)
                {
                    var host = StatusHost.On(adept) ?? adept.gameObject.AddComponent<StatusHost>();
                    notes.Add(host.Apply(verb.Status, verb.StatusSeconds));
                }

                ApplyTiles(grid, verb, origin, radius, notes);
                return new SpellImpactResult(First(notes), hits);
            }

            if (verb.Target == SpellTarget.Area || shape == SpellShape.Spread)
            {
                var center = shape == SpellShape.Spread ? origin : aim;
                CollectLocks(locks, center, radius, hits, verb);
                ApplyHosts(hits, origin, verb, notes);
                var self = AdeptAvatar.Find();
                if (self != null && Vector2.Distance(self.transform.position, center) <= radius && verb.Status != StatusId.None)
                {
                    var host = StatusHost.On(self);
                    if (host != null && !ElementalLaw.IsWard(verb.Status))
                    {
                        notes.Add(host.Apply(verb.Status, verb.StatusSeconds * 0.45f));
                    }
                }

                ApplyTiles(grid, verb, center, radius, notes);
                return new SpellImpactResult(First(notes), hits);
            }

            var single = Nearest(locks, aim, Mathf.Max(radius, 1.55f), verb);
            if (single != null)
            {
                hits.Add(single);
                ApplyHosts(hits, origin, verb, notes);
            }

            ApplyTiles(grid, verb, aim, Mathf.Max(0.8f, radius), notes);
            return new SpellImpactResult(First(notes), hits);
        }

        static void CollectLocks(
            IReadOnlyList<ISpellLock> locks,
            Vector3 center,
            float radius,
            List<ISpellLock> hits,
            SpellVerb verb = default)
        {
            if (locks == null)
            {
                return;
            }

            for (var i = 0; i < locks.Count; i++)
            {
                var encounter = locks[i];
                if (encounter is MonoBehaviour body && body != null && !encounter.Resolved &&
                    CanTake(encounter, verb) &&
                    Vector2.Distance(center, encounter.WorldPosition) <= radius)
                {
                    hits.Add(encounter);
                }
            }
        }

        static ISpellLock Nearest(IReadOnlyList<ISpellLock> locks, Vector3 point, float radius, SpellVerb verb = default)
        {
            ISpellLock best = null;
            var bestDistance = radius;
            if (locks == null)
            {
                return null;
            }

            for (var i = 0; i < locks.Count; i++)
            {
                var encounter = locks[i];
                if (encounter is not MonoBehaviour body || body == null || encounter.Resolved || !CanTake(encounter, verb))
                {
                    continue;
                }

                var distance = Vector2.Distance(point, encounter.WorldPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = encounter;
                }
            }

            return best;
        }

        static bool CanTake(ISpellLock encounter, SpellVerb verb)
        {
            if (!StatusSpec.IsMindAilment(verb.Status))
            {
                return true;
            }

            return encounter is MonoBehaviour body && body != null && StatusHost.On(body) != null;
        }

        static void ApplyHosts(List<ISpellLock> hits, Vector3 from, SpellVerb verb, List<string> notes)
        {
            if (verb.Status == StatusId.None)
            {
                return;
            }

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

                notes.Add(host.Apply(verb.Status, verb.StatusSeconds));
            }
        }

        static void ApplyTiles(WorldGrid grid, SpellVerb verb, Vector3 center, float radius, List<string> notes)
        {
            if (grid == null || verb.Tiles == TileVerb.None)
            {
                return;
            }

            var cells = grid.TilesInRadius(center, Mathf.Max(0.6f, radius));
            var changed = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = cells[i];
                switch (verb.Tiles)
                {
                    case TileVerb.Ignite:
                        tile.Ignite(0.85f);
                        changed++;
                        break;
                    case TileVerb.Douse:
                    case TileVerb.Wet:
                        tile.Drench(1f);
                        if (tile.IsPlantish)
                        {
                            tile.Grow(1);
                            if (grid.SpreadPlant(tile))
                            {
                                changed++;
                            }
                        }

                        changed++;
                        break;
                    case TileVerb.Grow:
                        if (tile.CanTakePlant)
                        {
                            tile.PlantHere();
                            changed++;
                        }
                        else if (tile.IsPlantish)
                        {
                            tile.Grow(1);
                            grid.SpreadPlant(tile);
                            changed++;
                        }

                        break;
                    case TileVerb.Charge:
                        if (tile.Conductivity > 0.05f)
                        {
                            tile.ChargeAt(1f);
                            changed++;
                        }

                        break;
                    case TileVerb.Freeze:
                        tile.Drench(0.4f);
                        changed++;
                        break;
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
                    notes.Add("Yield fills a small hollow. Water · Salt stands as a floor.");
                }
            }

            if (changed > 0)
            {
                notes.Add(verb.Tiles == TileVerb.Grow
                    ? "The vegetable body drinks."
                    : verb.Tiles == TileVerb.Ignite
                        ? "Hunger finds the floor."
                        : verb.Tiles == TileVerb.Charge
                            ? "The seed runs where it can."
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
