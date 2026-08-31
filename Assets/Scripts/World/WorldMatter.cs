using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A world body that can come apart. Stones and artifacts stay.
    /// Ice, timber, and marked props yield to the opposed work.
    /// </summary>
    public interface IWorldMatter
    {
        Vector3 WorldPosition { get; }
        Essence Matter { get; }
        bool Fragile { get; }
        bool Available { get; }
        bool YieldsTo(SpellId spell);
        string Unmake(SpellId spell);
    }

    public static class WorldMatter
    {
        static readonly List<IWorldMatter> Live = new();

        public static void Register(IWorldMatter body)
        {
            if (body != null && !Live.Contains(body))
            {
                Live.Add(body);
            }
        }

        public static void Unregister(IWorldMatter body)
        {
            if (body != null)
            {
                Live.Remove(body);
            }
        }

        public static Essence Parse(string matter)
        {
            if (string.IsNullOrEmpty(matter))
            {
                return Essence.None;
            }

            switch (matter.Trim().ToLowerInvariant())
            {
                case "fire":
                case "flame":
                case "hearth":
                    return Essence.Fire;
                case "water":
                case "ice":
                case "snow":
                case "glacier":
                    return Essence.Water;
                case "glass":
                    return Essence.Earth;
                case "earth":
                case "stone":
                case "rock":
                    return Essence.Earth;
                case "air":
                case "fog":
                case "wind":
                    return Essence.Air;
                case "poison":
                case "blight":
                case "miasma":
                    return Essence.Poison;
                case "wood":
                case "plant":
                case "timber":
                    return Essence.Physical;
                default:
                    return Essence.None;
            }
        }

        public static string Smash(SpellSweep sweep)
        {
            if (sweep.Spell == SpellId.None)
            {
                return string.Empty;
            }

            var broken = 0;
            var note = string.Empty;
            for (var i = Live.Count - 1; i >= 0; i--)
            {
                var body = Live[i];
                if (body == null || body is Object vanished && vanished == null)
                {
                    Live.RemoveAt(i);
                    continue;
                }

                if (!body.Available)
                {
                    continue;
                }

                if (CellVolume.SegmentDistance(sweep.From, sweep.To, body.WorldPosition) > sweep.Width + WorldPhysics.BodyRadius)
                {
                    continue;
                }

                if (WorldWork.IsFireWork(sweep.Spell) && body is Component host)
                {
                    var burnable = Burnable.On(host);
                    if (burnable != null && burnable.Ignite())
                    {
                        broken++;
                        if (string.IsNullOrEmpty(note))
                        {
                            note = "Hunger finds the timber. It will burn to ash.";
                        }

                        continue;
                    }
                }

                if (!body.Fragile || !body.YieldsTo(sweep.Spell))
                {
                    continue;
                }

                var flavor = body.Unmake(sweep.Spell);
                broken++;
                if (string.IsNullOrEmpty(note))
                {
                    note = flavor;
                }
            }

            if (broken == 0)
            {
                return string.Empty;
            }

            return string.IsNullOrEmpty(note)
                ? (broken == 1 ? "The stood thing comes apart." : "Breath and hunger find what would not stay.")
                : note;
        }
    }
}
