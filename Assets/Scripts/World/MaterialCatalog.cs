using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// What a tile is made of. Maps paint with this, not with a root rune.
    /// Each material keeps a full signature — roots plus the manifestation
    /// the world has already become.
    /// </summary>
    public enum MaterialId
    {
        None = 0,
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
        Void,
        Ice,
        Sand,
        Mud,
        Lava,
        Steam,
        Dust,
        Glass,
        Crystal,
        Obsidian,
        Grove,
        Cloud,
        Rain,
        Snow,
        Glacier,
        Acid,
        Water,
        Plant
    }

    public enum MaterialPaint
    {
        Cobble,
        Planks,
        Ash,
        Hearth,
        Ember,
        Damp,
        Vein,
        Scoured,
        Moss,
        Metal,
        Salt,
        Ice,
        Sand,
        Mud,
        Lava,
        Steam,
        Dust,
        Glass,
        Crystal,
        Obsidian,
        Grove,
        Cloud,
        Rain,
        Snow,
        Glacier,
        Acid,
        Water,
        Plant,
        Void
    }

    /// <summary>
    /// A world substance you can stamp on a tile. Later maps apply
    /// <see cref="MaterialId"/> directly; the sanctum is only a first slice.
    /// </summary>
    public sealed class WorldMaterial
    {
        public WorldMaterial(
            MaterialId id,
            string name,
            string note,
            RuneId manifestation,
            MaterialPaint paint,
            Color floor,
            Color wall,
            bool tearsTheWeave,
            params RuneId[] signature)
        {
            Id = id;
            Name = name;
            Note = note;
            Manifestation = manifestation;
            Paint = paint;
            FloorTone = floor;
            WallTone = wall;
            TearsTheWeave = tearsTheWeave;
            Signature = signature ?? System.Array.Empty<RuneId>();
        }

        public MaterialId Id { get; }
        public string Name { get; }
        public string Note { get; }
        public RuneId Manifestation { get; }
        public MaterialPaint Paint { get; }
        public Color FloorTone { get; }
        public Color WallTone { get; }
        public bool TearsTheWeave { get; }
        public IReadOnlyList<RuneId> Signature { get; }

        /// <summary>
        /// Negative puts nearby fire out. Zero will not burn. Positive
        /// is how readily hunger takes it, and how far it runs.
        /// </summary>
        public float Flammability { get; internal set; }

        /// <summary>
        /// How freely a spark may travel this body. Zero is an insulator.
        /// </summary>
        public float Conductivity { get; internal set; }

        public RuneId Primary
        {
            get
            {
                if (Manifestation != RuneId.None)
                {
                    return Manifestation;
                }

                return Signature.Count > 0 ? Signature[0] : RuneId.None;
            }
        }

        public string SignatureText()
        {
            if (Signature.Count == 0)
            {
                return TearsTheWeave ? "— (tear)" : "—";
            }

            var parts = new string[Signature.Count];
            for (var i = 0; i < Signature.Count; i++)
            {
                parts[i] = RuneCatalog.GlyphOf(Signature[i]);
            }

            return string.Join(" · ", parts);
        }

        public string SignatureNames()
        {
            if (Signature.Count == 0)
            {
                return TearsTheWeave ? "a tear — no sentence" : "silent";
            }

            var parts = new string[Signature.Count];
            for (var i = 0; i < Signature.Count; i++)
            {
                parts[i] = RuneCatalog.NameOf(Signature[i]);
            }

            return string.Join(" · ", parts);
        }
    }

    public static class MaterialCatalog
    {
        static readonly Dictionary<MaterialId, WorldMaterial> ById;
        static readonly WorldMaterial[] AllMaterials;
        static readonly RuneId[] Empty = System.Array.Empty<RuneId>();

        static MaterialCatalog()
        {
            AllMaterials = new[]
            {
                new WorldMaterial(MaterialId.Stone, "stone",
                    "Rest given a body. The ordinary floor of the sanctum.",
                    RuneId.Stone, MaterialPaint.Cobble,
                    new Color(0.28f, 0.28f, 0.32f), new Color(0.28f, 0.24f, 0.32f), false,
                    RuneId.Earth, RuneId.Salt, RuneId.Stone),

                new WorldMaterial(MaterialId.Ash, "ash",
                    "What hunger leaves of a vegetable body.",
                    RuneId.Ash, MaterialPaint.Ash,
                    new Color(0.22f, 0.18f, 0.16f), new Color(0.22f, 0.18f, 0.16f), false,
                    RuneId.Fire, RuneId.Plant, RuneId.Ash),

                new WorldMaterial(MaterialId.Timber, "timber",
                    "Yield and rest given a vegetable body. Not living until Life.",
                    RuneId.Plant, MaterialPaint.Planks,
                    new Color(0.46f, 0.3f, 0.16f), new Color(0.42f, 0.26f, 0.14f), false,
                    RuneId.Water, RuneId.Earth, RuneId.Salt, RuneId.Plant),

                new WorldMaterial(MaterialId.Hearth, "hearthstone",
                    "Hunger given a body and asked to stay in stone.",
                    RuneId.Flame, MaterialPaint.Hearth,
                    new Color(0.42f, 0.22f, 0.16f), new Color(0.36f, 0.2f, 0.16f), false,
                    RuneId.Fire, RuneId.Salt, RuneId.Earth, RuneId.Flame),

                new WorldMaterial(MaterialId.Ember, "ember bed",
                    "Hunger after the motion has gone. Ash that still wants.",
                    RuneId.Ember, MaterialPaint.Ember,
                    new Color(0.2f, 0.1f, 0.08f), new Color(0.24f, 0.12f, 0.1f), false,
                    RuneId.Fire, RuneId.Ash, RuneId.Ember),

                new WorldMaterial(MaterialId.Damp, "damp stone",
                    "Rest holding yield. Not yet mud.",
                    RuneId.Water, MaterialPaint.Damp,
                    new Color(0.2f, 0.28f, 0.4f), new Color(0.22f, 0.26f, 0.34f), false,
                    RuneId.Water, RuneId.Earth),

                new WorldMaterial(MaterialId.Vein, "spark-veined stone",
                    "Hunger given breath, then asked to rest in the wall.",
                    RuneId.Spark, MaterialPaint.Vein,
                    new Color(0.32f, 0.3f, 0.24f), new Color(0.3f, 0.28f, 0.22f), false,
                    RuneId.Fire, RuneId.Air, RuneId.Spark, RuneId.Earth),

                new WorldMaterial(MaterialId.Scoured, "wind-scoured stone",
                    "Breath forced through rest. The grit still hangs.",
                    RuneId.Dust, MaterialPaint.Scoured,
                    new Color(0.3f, 0.34f, 0.38f), new Color(0.28f, 0.3f, 0.34f), false,
                    RuneId.Air, RuneId.Earth, RuneId.Dust),

                new WorldMaterial(MaterialId.Moss, "moss",
                    "A vegetable body marked living. Soft grove-work on stone.",
                    RuneId.Grove, MaterialPaint.Moss,
                    new Color(0.28f, 0.26f, 0.16f), new Color(0.24f, 0.28f, 0.18f), false,
                    RuneId.Water, RuneId.Earth, RuneId.Salt, RuneId.Plant, RuneId.Vita, RuneId.Grove),

                new WorldMaterial(MaterialId.Metal, "iron plate",
                    "Hungry earth given more rest, then stilled.",
                    RuneId.Metal, MaterialPaint.Metal,
                    new Color(0.42f, 0.44f, 0.48f), new Color(0.34f, 0.36f, 0.4f), false,
                    RuneId.Fire, RuneId.Earth, RuneId.Lava, RuneId.Metal),

                new WorldMaterial(MaterialId.SaltCrust, "salt crust",
                    "A body laid over rest. White on the floor.",
                    RuneId.Salt, MaterialPaint.Salt,
                    new Color(0.42f, 0.4f, 0.38f), new Color(0.38f, 0.36f, 0.34f), false,
                    RuneId.Salt, RuneId.Earth),

                new WorldMaterial(MaterialId.Void, "void",
                    "No floor. The weave tears. Nothing speaks.",
                    RuneId.None, MaterialPaint.Void,
                    new Color(0.03f, 0.02f, 0.03f), new Color(0.06f, 0.05f, 0.06f), true),

                new WorldMaterial(MaterialId.Ice, "ice",
                    "Yield given a body and asked to rest. It will thaw.",
                    RuneId.Ice, MaterialPaint.Ice,
                    new Color(0.62f, 0.78f, 0.9f), new Color(0.48f, 0.62f, 0.76f), false,
                    RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Ice),

                new WorldMaterial(MaterialId.Sand, "sand",
                    "Mud given breath until it dries. Grit given a body.",
                    RuneId.Sand, MaterialPaint.Sand,
                    new Color(0.72f, 0.6f, 0.38f), new Color(0.58f, 0.48f, 0.3f), false,
                    RuneId.Water, RuneId.Earth, RuneId.Mud, RuneId.Air, RuneId.Sand),

                new WorldMaterial(MaterialId.Mud, "mud",
                    "Yield meeting rest. Soft ground.",
                    RuneId.Mud, MaterialPaint.Mud,
                    new Color(0.32f, 0.22f, 0.14f), new Color(0.28f, 0.2f, 0.14f), false,
                    RuneId.Water, RuneId.Earth, RuneId.Mud),

                new WorldMaterial(MaterialId.Lava, "lava",
                    "Hunger meeting rest. Earth that cannot stay earth.",
                    RuneId.Lava, MaterialPaint.Lava,
                    new Color(0.55f, 0.16f, 0.06f), new Color(0.28f, 0.1f, 0.08f), false,
                    RuneId.Fire, RuneId.Earth, RuneId.Lava),

                new WorldMaterial(MaterialId.Steam, "steam",
                    "Hunger forced through yield. Water that cannot stay water.",
                    RuneId.Steam, MaterialPaint.Steam,
                    new Color(0.7f, 0.74f, 0.78f), new Color(0.5f, 0.54f, 0.58f), false,
                    RuneId.Fire, RuneId.Water, RuneId.Steam),

                new WorldMaterial(MaterialId.Dust, "dust",
                    "Breath forced through rest. Rest that has lost its weight.",
                    RuneId.Dust, MaterialPaint.Dust,
                    new Color(0.52f, 0.46f, 0.38f), new Color(0.4f, 0.36f, 0.3f), false,
                    RuneId.Air, RuneId.Earth, RuneId.Dust),

                new WorldMaterial(MaterialId.Glass, "glass",
                    "Grains meet hunger, then rest. Sand · Flame · Earth.",
                    RuneId.Glass, MaterialPaint.Glass,
                    new Color(0.28f, 0.42f, 0.46f), new Color(0.22f, 0.32f, 0.36f), false,
                    RuneId.Sand, RuneId.Flame, RuneId.Earth, RuneId.Glass),

                new WorldMaterial(MaterialId.Crystal, "crystal",
                    "Stone grown with yield.",
                    RuneId.Crystal, MaterialPaint.Crystal,
                    new Color(0.55f, 0.42f, 0.72f), new Color(0.4f, 0.3f, 0.54f), false,
                    RuneId.Earth, RuneId.Salt, RuneId.Stone, RuneId.Water, RuneId.Crystal),

                new WorldMaterial(MaterialId.Obsidian, "obsidian",
                    "Hungry earth quenched and given a body.",
                    RuneId.Obsidian, MaterialPaint.Obsidian,
                    new Color(0.08f, 0.06f, 0.1f), new Color(0.1f, 0.08f, 0.12f), false,
                    RuneId.Fire, RuneId.Earth, RuneId.Lava, RuneId.Water, RuneId.Salt, RuneId.Obsidian),

                new WorldMaterial(MaterialId.Grove, "grove",
                    "The vegetable body marked living. Plant · Life.",
                    RuneId.Grove, MaterialPaint.Grove,
                    new Color(0.16f, 0.32f, 0.14f), new Color(0.14f, 0.26f, 0.12f), false,
                    RuneId.Water, RuneId.Earth, RuneId.Salt, RuneId.Plant, RuneId.Vita, RuneId.Grove),

                new WorldMaterial(MaterialId.Cloud, "cloud",
                    "Breath holding yield. A hanging veil.",
                    RuneId.Cloud, MaterialPaint.Cloud,
                    new Color(0.72f, 0.76f, 0.82f), new Color(0.56f, 0.6f, 0.66f), false,
                    RuneId.Air, RuneId.Water, RuneId.Cloud),

                new WorldMaterial(MaterialId.Rain, "rain-slick stone",
                    "The hanging veil drawn down.",
                    RuneId.Rain, MaterialPaint.Rain,
                    new Color(0.22f, 0.3f, 0.4f), new Color(0.2f, 0.26f, 0.34f), false,
                    RuneId.Air, RuneId.Water, RuneId.Cloud, RuneId.Earth, RuneId.Rain),

                new WorldMaterial(MaterialId.Snow, "snow",
                    "The hanging veil given ice’s story.",
                    RuneId.Snow, MaterialPaint.Snow,
                    new Color(0.88f, 0.9f, 0.94f), new Color(0.7f, 0.74f, 0.8f), false,
                    RuneId.Air, RuneId.Water, RuneId.Cloud, RuneId.Ice, RuneId.Snow),

                new WorldMaterial(MaterialId.Glacier, "glacier",
                    "Ice given Stone. Still water that will not thaw easily.",
                    RuneId.Glacier, MaterialPaint.Glacier,
                    new Color(0.7f, 0.82f, 0.88f), new Color(0.48f, 0.58f, 0.66f), false,
                    RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Ice, RuneId.Stone, RuneId.Glacier),

                new WorldMaterial(MaterialId.Acid, "acid slick",
                    "Steam forced through Metal.",
                    RuneId.Acid, MaterialPaint.Acid,
                    new Color(0.55f, 0.72f, 0.18f), new Color(0.36f, 0.48f, 0.14f), false,
                    RuneId.Fire, RuneId.Water, RuneId.Steam, RuneId.Metal, RuneId.Acid),

                new WorldMaterial(MaterialId.Water, "standing water",
                    "Yield, still holding a vessel. A pool, not weather.",
                    RuneId.Water, MaterialPaint.Water,
                    new Color(0.18f, 0.38f, 0.62f), new Color(0.16f, 0.3f, 0.48f), false,
                    RuneId.Water, RuneId.Salt),

                new WorldMaterial(MaterialId.Plant, "living plant",
                    "Vegetable body before Life marks a grove. Green cover.",
                    RuneId.Plant, MaterialPaint.Plant,
                    new Color(0.22f, 0.42f, 0.16f), new Color(0.18f, 0.34f, 0.14f), false,
                    RuneId.Water, RuneId.Earth, RuneId.Salt, RuneId.Plant)
            };

            ById = new Dictionary<MaterialId, WorldMaterial>(AllMaterials.Length);
            foreach (var material in AllMaterials)
            {
                ById[material.Id] = material;
            }

            Flag(MaterialId.Stone, 0f, 0f);
            Flag(MaterialId.Ash, 0.05f, 0f);
            Flag(MaterialId.Timber, 1.2f, 0f);
            Flag(MaterialId.Hearth, 0f, 0.1f);
            Flag(MaterialId.Ember, 0.35f, 0.15f);
            Flag(MaterialId.Damp, -0.7f, 0.35f);
            Flag(MaterialId.Vein, 0f, 0.85f);
            Flag(MaterialId.Scoured, 0f, 0f);
            Flag(MaterialId.Moss, 1.05f, 0.1f);
            Flag(MaterialId.Metal, 0f, 1.6f);
            Flag(MaterialId.SaltCrust, -0.15f, 0.2f);
            Flag(MaterialId.Void, 0f, 0f);
            Flag(MaterialId.Ice, -0.85f, 0.15f);
            Flag(MaterialId.Sand, 0f, 0f);
            Flag(MaterialId.Mud, -0.35f, 0.25f);
            Flag(MaterialId.Lava, 0.2f, 0.3f);
            Flag(MaterialId.Steam, 0f, 0.1f);
            Flag(MaterialId.Dust, 0.55f, 0f);
            Flag(MaterialId.Glass, 0f, 0.05f);
            Flag(MaterialId.Crystal, 0f, 0.35f);
            Flag(MaterialId.Obsidian, 0f, 0.1f);
            Flag(MaterialId.Grove, 1.35f, 0.1f);
            Flag(MaterialId.Cloud, 0f, 0.2f);
            Flag(MaterialId.Rain, -1.1f, 0.7f);
            Flag(MaterialId.Snow, -0.65f, 0.1f);
            Flag(MaterialId.Glacier, -0.9f, 0.12f);
            Flag(MaterialId.Acid, 0.15f, 0.45f);
            Flag(MaterialId.Water, -1.6f, 1.25f);
            Flag(MaterialId.Plant, 1.5f, 0.05f);
        }

        static void Flag(MaterialId id, float flammability, float conductivity)
        {
            if (ById.TryGetValue(id, out var material))
            {
                material.Flammability = flammability;
                material.Conductivity = conductivity;
            }
        }

        public static IReadOnlyList<WorldMaterial> All => AllMaterials;

        public static WorldMaterial Of(MaterialId id)
        {
            return ById.TryGetValue(id, out var material) ? material : ById[MaterialId.Stone];
        }

        public static IReadOnlyList<RuneId> SignatureOf(MaterialId id)
        {
            return ById.TryGetValue(id, out var material) ? material.Signature : Empty;
        }

        public static bool TearsTheWeave(MaterialId id)
        {
            return ById.TryGetValue(id, out var material) && material.TearsTheWeave;
        }

        public static MaterialId FromLegacy(TileSubstance substance)
        {
            switch (substance)
            {
                case TileSubstance.Ash: return MaterialId.Ash;
                case TileSubstance.Timber: return MaterialId.Timber;
                case TileSubstance.Hearth: return MaterialId.Hearth;
                case TileSubstance.Ember: return MaterialId.Ember;
                case TileSubstance.Damp: return MaterialId.Damp;
                case TileSubstance.Vein: return MaterialId.Vein;
                case TileSubstance.Scoured: return MaterialId.Scoured;
                case TileSubstance.Moss: return MaterialId.Moss;
                case TileSubstance.Metal: return MaterialId.Metal;
                case TileSubstance.SaltCrust: return MaterialId.SaltCrust;
                case TileSubstance.Void: return MaterialId.Void;
                default: return MaterialId.Stone;
            }
        }

        public static TileSubstance ToLegacy(MaterialId id)
        {
            switch (id)
            {
                case MaterialId.Ash: return TileSubstance.Ash;
                case MaterialId.Timber: return TileSubstance.Timber;
                case MaterialId.Hearth: return TileSubstance.Hearth;
                case MaterialId.Ember: return TileSubstance.Ember;
                case MaterialId.Damp: return TileSubstance.Damp;
                case MaterialId.Vein: return TileSubstance.Vein;
                case MaterialId.Scoured: return TileSubstance.Scoured;
                case MaterialId.Moss: return TileSubstance.Moss;
                case MaterialId.Metal: return TileSubstance.Metal;
                case MaterialId.SaltCrust: return TileSubstance.SaltCrust;
                case MaterialId.Void: return TileSubstance.Void;
                default: return TileSubstance.Stone;
            }
        }

        public static MaterialId FromElement(RuneId element)
        {
            switch (element)
            {
                case RuneId.Fire: return MaterialId.Hearth;
                case RuneId.Air: return MaterialId.Scoured;
                case RuneId.Water: return MaterialId.Water;
                case RuneId.Earth: return MaterialId.Stone;
                case RuneId.Plant: return MaterialId.Timber;
                case RuneId.Spark: return MaterialId.Vein;
                case RuneId.Ash: return MaterialId.Ash;
                case RuneId.Metal: return MaterialId.Metal;
                case RuneId.Salt: return MaterialId.SaltCrust;
                case RuneId.Vita:
                case RuneId.Grove: return MaterialId.Grove;
                case RuneId.Dust: return MaterialId.Dust;
                case RuneId.Ice: return MaterialId.Ice;
                case RuneId.Sand: return MaterialId.Sand;
                case RuneId.Mud: return MaterialId.Mud;
                case RuneId.Lava: return MaterialId.Lava;
                case RuneId.Steam: return MaterialId.Steam;
                case RuneId.Glass: return MaterialId.Glass;
                case RuneId.Crystal: return MaterialId.Crystal;
                case RuneId.Obsidian: return MaterialId.Obsidian;
                case RuneId.Cloud: return MaterialId.Cloud;
                case RuneId.Rain: return MaterialId.Rain;
                case RuneId.Snow: return MaterialId.Snow;
                case RuneId.Glacier: return MaterialId.Glacier;
                case RuneId.Acid: return MaterialId.Acid;
                case RuneId.Flame: return MaterialId.Hearth;
                case RuneId.Ember: return MaterialId.Ember;
                case RuneId.Stone: return MaterialId.Stone;
                case RuneId.None: return MaterialId.Void;
                default: return MaterialId.Stone;
            }
        }

        public static string DisplayName(TileKind kind, MaterialId material)
        {
            var def = Of(material);
            switch (kind)
            {
                case TileKind.Pit:
                    return "a tear — no floor";
                case TileKind.Bridge:
                    return "earth bridge";
                case TileKind.Door:
                    return material == MaterialId.Timber ? "timber door"
                        : material == MaterialId.Metal ? "iron door"
                        : "stone door";
                case TileKind.Wall:
                    return material == MaterialId.Timber ? "timber wall"
                        : material == MaterialId.Metal ? "iron wall"
                        : material == MaterialId.Ice ? "ice wall"
                        : def.Name + " wall";
                default:
                    return def.Name;
            }
        }
    }
}
