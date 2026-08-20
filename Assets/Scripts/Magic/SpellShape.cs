using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum SpellShape
    {
        None = 0,
        Shot,
        Pillar,
        Spread,
        Remote,
        Self
    }

    public readonly struct FormationDef
    {
        public FormationDef(SpellShape shape, string name, string verb, string hint, float range, float lockRadius)
        {
            Shape = shape;
            Name = name;
            Verb = verb;
            Hint = hint;
            Range = range;
            LockRadius = lockRadius;
        }

        public SpellShape Shape { get; }
        public string Name { get; }
        public string Verb { get; }
        public string Hint { get; }
        public float Range { get; }
        public float LockRadius { get; }
    }

    /// <summary>
    /// How a spell is placed. Chosen at cast time, not implied by the aspect.
    /// Availability is sparse on purpose — not every material can take every form.
    /// </summary>
    public static class SpellFormations
    {
        public static readonly FormationDef Shot = new(
            SpellShape.Shot, "Shot", "aim a line",
            "Click through a point. The spell flies that way.", 8.4f, 1.45f);

        public static readonly FormationDef Pillar = new(
            SpellShape.Pillar, "Pillar", "raise a column",
            "Click the ground. A column rises there.", 6.2f, 1.45f);

        public static readonly FormationDef Spread = new(
            SpellShape.Spread, "Spread", "release at your feet",
            "Click to confirm. It wells from where you stand.", 2.4f, 2.4f);

        public static readonly FormationDef Remote = new(
            SpellShape.Remote, "Remote", "place at a distance",
            "Click a distant point. The spell forms there, not here.", 9.2f, 1.2f);

        public static readonly FormationDef Self = new(
            SpellShape.Self, "Self", "keep it on you",
            "Click to confirm. The spell stays on the caster.", 0.8f, 2.4f);

        static readonly FormationDef[] AllDefs = { Shot, Pillar, Spread, Remote, Self };

        public static IReadOnlyList<FormationDef> All => AllDefs;

        public static FormationDef Get(SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Pillar: return Pillar;
                case SpellShape.Spread: return Spread;
                case SpellShape.Remote: return Remote;
                case SpellShape.Self: return Self;
                default: return Shot;
            }
        }

        public static string NameOf(SpellShape shape) =>
            shape == SpellShape.None ? "—" : Get(shape).Name;

        public static bool MakesSense(RuneId material, RuneId aspect, SpellShape shape)
        {
            if (material == RuneId.None || !RuneCatalog.IsFormAspect(aspect) || shape == SpellShape.None)
            {
                return false;
            }

            switch (aspect)
            {
                case RuneId.Salt:
                    return SaltSense(material, shape);
                case RuneId.Mercury:
                    return MercurySense(material, shape);
                case RuneId.Sulphur:
                    return SulphurSense(material, shape);
                case RuneId.Vita:
                    return VitaSense(material, shape);
                case RuneId.Mors:
                    return MorsSense(material, shape);
                case RuneId.Lumen:
                    return LumenSense(material, shape);
                case RuneId.Umbra:
                    return UmbraSense(material, shape);
                case RuneId.Animus:
                    return AnimusSense(material, shape);
                case RuneId.Anima:
                    return AnimaSense(material, shape);
                default:
                    return false;
            }
        }

        public static List<SpellShape> Available(RuneId material, RuneId aspect)
        {
            var list = new List<SpellShape>(4);
            foreach (var def in AllDefs)
            {
                if (MakesSense(material, aspect, def.Shape))
                {
                    list.Add(def.Shape);
                }
            }

            return list;
        }

        public static Vector3 ClampPoint(SpellShape shape, Vector3 origin, Vector3 requested, float rangeScale = 1f)
        {
            var def = Get(shape);
            if (shape == SpellShape.Spread || shape == SpellShape.Self)
            {
                return origin;
            }

            var delta = requested - origin;
            delta.z = 0f;
            if (delta.sqrMagnitude < 0.04f)
            {
                delta = Vector3.right * 0.8f;
            }

            var range = def.Range * (rangeScale <= 0f ? 1f : rangeScale);
            if (delta.magnitude > range)
            {
                delta = delta.normalized * range;
            }

            return origin + delta;
        }

        static bool SaltSense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Pillar:
                    return IsAny(material, RuneId.Fire, RuneId.Earth, RuneId.Water, RuneId.Ice,
                        RuneId.Stone, RuneId.Plant, RuneId.Mud, RuneId.Lava, RuneId.Spark);
                case SpellShape.Spread:
                    return IsAny(material, RuneId.Fire, RuneId.Water, RuneId.Spark, RuneId.Plant, RuneId.Mud);
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Earth, RuneId.Water, RuneId.Ice, RuneId.Stone);
                case SpellShape.Shot:
                    return IsAny(material, RuneId.Earth, RuneId.Ice, RuneId.Stone);
                default:
                    return false;
            }
        }

        static bool MercurySense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Shot:
                    return IsAny(material, RuneId.Fire, RuneId.Water, RuneId.Earth, RuneId.Air,
                        RuneId.Spark, RuneId.Steam, RuneId.Dust, RuneId.Ice, RuneId.Lava);
                case SpellShape.Spread:
                    return IsAny(material, RuneId.Water, RuneId.Air, RuneId.Dust, RuneId.Steam);
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Air, RuneId.Spark, RuneId.Steam);
                default:
                    return false;
            }
        }

        static bool SulphurSense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Spread:
                    return IsAny(material, RuneId.Fire, RuneId.Air, RuneId.Spark);
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Water, RuneId.Earth, RuneId.Spark);
                case SpellShape.Shot:
                    return IsAny(material, RuneId.Fire, RuneId.Spark);
                default:
                    return false;
            }
        }

        static bool VitaSense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Spread:
                    return IsAny(material, RuneId.Water, RuneId.Earth, RuneId.Mud, RuneId.Plant);
                case SpellShape.Pillar:
                    return IsAny(material, RuneId.Earth, RuneId.Plant, RuneId.Mud);
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Water, RuneId.Plant);
                default:
                    return false;
            }
        }

        static bool MorsSense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Shot:
                    return IsAny(material, RuneId.Fire, RuneId.Spark);
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Water, RuneId.Fire);
                case SpellShape.Spread:
                    return material == RuneId.Earth;
                case SpellShape.Pillar:
                    return material == RuneId.Earth || material == RuneId.Water;
                default:
                    return false;
            }
        }

        static bool LumenSense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Shot:
                    return IsAny(material, RuneId.Fire, RuneId.Spark, RuneId.Air);
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Fire, RuneId.Water, RuneId.Air);
                case SpellShape.Spread:
                    return material == RuneId.Air;
                case SpellShape.Pillar:
                    return material == RuneId.Fire;
                default:
                    return false;
            }
        }

        static bool UmbraSense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Earth, RuneId.Air, RuneId.Fire);
                case SpellShape.Spread:
                    return IsAny(material, RuneId.Air, RuneId.Earth, RuneId.Water);
                case SpellShape.Shot:
                    return material == RuneId.Fire;
                default:
                    return false;
            }
        }

        static bool AnimusSense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Shot:
                    return IsAny(material, RuneId.Fire, RuneId.Air, RuneId.Spark);
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Fire, RuneId.Spark);
                default:
                    return false;
            }
        }

        static bool AnimaSense(RuneId material, SpellShape shape)
        {
            switch (shape)
            {
                case SpellShape.Remote:
                    return IsAny(material, RuneId.Water, RuneId.Earth, RuneId.Plant);
                case SpellShape.Spread:
                    return material == RuneId.Water;
                case SpellShape.Pillar:
                    return material == RuneId.Water;
                default:
                    return false;
            }
        }

        static bool IsAny(RuneId value, params RuneId[] options)
        {
            for (var i = 0; i < options.Length; i++)
            {
                if (options[i] == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
