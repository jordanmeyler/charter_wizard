using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// What a tile is made of. The world and the spell book share materials.
    /// Each substance speaks a short rune signature into the tapestry.
    /// </summary>
    public enum TileSubstance
    {
        Stone,
        Ash,
        Timber,
        Hearth,
        Ember,
        Damp,
        Vein,
        Scoured,
        Moss,
        Metal,
        SaltCrust,
        Void
    }

    public readonly struct SubstanceDef
    {
        public SubstanceDef(TileSubstance id, string name, RuneId primary, params RuneId[] emission)
        {
            Id = id;
            Name = name;
            Primary = primary;
            Emission = emission;
        }

        public TileSubstance Id { get; }
        public string Name { get; }
        public RuneId Primary { get; }
        public IReadOnlyList<RuneId> Emission { get; }
    }

    public static class TileSubstances
    {
        static readonly Dictionary<TileSubstance, SubstanceDef> ById;
        static readonly RuneId[] Empty = System.Array.Empty<RuneId>();

        static TileSubstances()
        {
            var defs = new[]
            {
                new SubstanceDef(TileSubstance.Stone, "stone", RuneId.Earth, RuneId.Earth, RuneId.Salt),
                new SubstanceDef(TileSubstance.Ash, "ash", RuneId.Ash, RuneId.Ash, RuneId.Fire),
                new SubstanceDef(TileSubstance.Timber, "timber", RuneId.Plant, RuneId.Plant),
                new SubstanceDef(TileSubstance.Hearth, "hearthstone", RuneId.Fire, RuneId.Fire, RuneId.Earth),
                new SubstanceDef(TileSubstance.Ember, "ember bed", RuneId.Fire, RuneId.Fire, RuneId.Ash),
                new SubstanceDef(TileSubstance.Damp, "damp stone", RuneId.Water, RuneId.Water, RuneId.Earth),
                new SubstanceDef(TileSubstance.Vein, "spark-veined stone", RuneId.Spark, RuneId.Spark, RuneId.Earth),
                new SubstanceDef(TileSubstance.Scoured, "wind-scoured stone", RuneId.Air, RuneId.Air, RuneId.Dust),
                new SubstanceDef(TileSubstance.Moss, "moss", RuneId.Plant, RuneId.Plant, RuneId.Vita),
                new SubstanceDef(TileSubstance.Metal, "iron plate", RuneId.Metal, RuneId.Metal, RuneId.Earth),
                new SubstanceDef(TileSubstance.SaltCrust, "salt crust", RuneId.Salt, RuneId.Salt, RuneId.Earth),
                new SubstanceDef(TileSubstance.Void, "void", RuneId.None)
            };

            ById = new Dictionary<TileSubstance, SubstanceDef>(defs.Length);
            foreach (var def in defs)
            {
                ById[def.Id] = def;
            }
        }

        public static SubstanceDef Get(TileSubstance substance) => ById[substance];

        public static RuneId Primary(TileSubstance substance) => ById[substance].Primary;

        public static IReadOnlyList<RuneId> EmissionOf(TileSubstance substance)
        {
            return ById.TryGetValue(substance, out var def) ? def.Emission : Empty;
        }

        public static TileSubstance FromElement(RuneId element)
        {
            switch (element)
            {
                case RuneId.Fire: return TileSubstance.Hearth;
                case RuneId.Air: return TileSubstance.Scoured;
                case RuneId.Water: return TileSubstance.Damp;
                case RuneId.Plant: return TileSubstance.Timber;
                case RuneId.Spark: return TileSubstance.Vein;
                case RuneId.Ash: return TileSubstance.Ash;
                case RuneId.Metal: return TileSubstance.Metal;
                case RuneId.Salt: return TileSubstance.SaltCrust;
                case RuneId.Vita: return TileSubstance.Moss;
                case RuneId.Dust: return TileSubstance.Scoured;
                case RuneId.None: return TileSubstance.Void;
                default: return TileSubstance.Stone;
            }
        }

        public static string DisplayName(TileKind kind, TileSubstance substance)
        {
            switch (kind)
            {
                case TileKind.Pit:
                    return "a tear — no floor";
                case TileKind.Bridge:
                    return "earth bridge";
                case TileKind.Door:
                    return substance == TileSubstance.Timber ? "timber door"
                        : substance == TileSubstance.Metal ? "iron door"
                        : "stone door";
                case TileKind.Wall:
                    return substance == TileSubstance.Timber ? "timber wall"
                        : substance == TileSubstance.Metal ? "iron wall"
                        : "stone wall";
                default:
                    return Get(substance).Name;
            }
        }
    }
}
