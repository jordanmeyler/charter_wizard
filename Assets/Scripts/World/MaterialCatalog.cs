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
        Plant,
        Dirt,
        Oil,
        Miasma,
        Wardstone,
        Aegis,
        Fire
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
        Oil,
        Miasma,
        Wardstone,
        Aegis,
        Void,
        Dirt,
        Fire
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
        /// How readily hunger takes this body once it is allowed to
        /// catch. Positive is catch strength. Zero will not catch.
        /// Negative is a leftover that tracks <see cref="Quench"/>
        /// (about −grade × 0.16). Water’s −1.6 is quench 10.
        /// </summary>
        public float Flammability { get; internal set; }

        /// <summary>
        /// How freely a spark may travel this body. Positive conducts.
        /// Zero is neutral — it may hold a charge but will not pass it.
        /// Negative is an insulator and breaks the path.
        /// </summary>
        public float Conductivity { get; internal set; }

        /// <summary>
        /// How long a full fire lasts on this body, in seconds.
        /// Fuel lives on a one-to-five second clock. Zero is not fuel.
        /// </summary>
        public float BurnSeconds { get; internal set; }

        /// <summary>
        /// Clock leftover <c>5 − seconds</c>. Oil and wood last;
        /// plant and grove burn out sooner. Spread itself uses
        /// <see cref="Hunger"/>, not this leftover.
        /// </summary>
        public float BurnRate { get; internal set; }

        /// <summary>
        /// 0–10 hunger grade. 0 is neutral; 1–6 catch from a strong
        /// source (7+) and do not run; 7–10 are strong sources and
        /// may walk fire to equal-or-weaker fuel out to hunger − 6.
        /// </summary>
        public int Hunger { get; internal set; }

        /// <summary>
        /// 0–10 quench grade. 0 is dry — it leaves a fire alone.
        /// 3–8 suppress neighbor fire. 9–10 put it out.
        /// </summary>
        public int Quench { get; internal set; }

        /// <summary>
        /// 0–10 conduct grade. 0 is an insulator (wood, plants).
        /// 1–6 hold a spark from a bolt or live-floor but will not
        /// pass it. 7–10 are conductors and walk the spark.
        /// </summary>
        public int Conduct { get; internal set; }

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
                    "Yield given a body, then rest. A vegetable body. Not living until Life.",
                    RuneId.Plant, MaterialPaint.Planks,
                    new Color(0.46f, 0.3f, 0.16f), new Color(0.42f, 0.26f, 0.14f), false,
                    RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Plant),

                new WorldMaterial(MaterialId.Hearth, "hearthstone",
                    "Hunger given a body and asked to stay in stone.",
                    RuneId.Fire, MaterialPaint.Hearth,
                    new Color(0.42f, 0.22f, 0.16f), new Color(0.36f, 0.2f, 0.16f), false,
                    RuneId.Fire, RuneId.Salt, RuneId.Earth),

                new WorldMaterial(MaterialId.Ember, "ember bed",
                    "Coals. Speak Fire. Provide fire and let hunger sit and walk across. The tile underneath stays embered.",
                    RuneId.Fire, MaterialPaint.Ember,
                    new Color(0.2f, 0.1f, 0.08f), new Color(0.24f, 0.12f, 0.1f), false,
                    RuneId.Fire),

                new WorldMaterial(MaterialId.Damp, "damp stone",
                    "Wet rest. Not ice, not mud.",
                    RuneId.Water, MaterialPaint.Damp,
                    new Color(0.2f, 0.28f, 0.4f), new Color(0.22f, 0.26f, 0.34f), false,
                    RuneId.Water, RuneId.Stone),

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
                    "A vegetable body marked living. Soft cover on stone.",
                    RuneId.Plant, MaterialPaint.Moss,
                    new Color(0.28f, 0.26f, 0.16f), new Color(0.24f, 0.28f, 0.18f), false,
                    RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Plant, RuneId.Vita),

                new WorldMaterial(MaterialId.Metal, "iron plate",
                    "Hungry earth given spark, then stilled. It conducts heat and the bolt.",
                    RuneId.Metal, MaterialPaint.Metal,
                    new Color(0.42f, 0.44f, 0.48f), new Color(0.34f, 0.36f, 0.4f), false,
                    RuneId.Fire, RuneId.Earth, RuneId.Lava, RuneId.Spark, RuneId.Metal),

                new WorldMaterial(MaterialId.SaltCrust, "salt crust",
                    "A body laid over rest. White on the floor.",
                    RuneId.Salt, MaterialPaint.Salt,
                    new Color(0.42f, 0.4f, 0.38f), new Color(0.38f, 0.36f, 0.34f), false,
                    RuneId.Salt, RuneId.Earth),

                new WorldMaterial(MaterialId.Void, "void",
                    "No floor. The weave tears. Dark — withheld.",
                    RuneId.None, MaterialPaint.Void,
                    new Color(0.03f, 0.02f, 0.03f), new Color(0.06f, 0.05f, 0.06f), true),

                new WorldMaterial(MaterialId.Ice, "ice",
                    "Yield meeting rest. Hard water. It will thaw.",
                    RuneId.Ice, MaterialPaint.Ice,
                    new Color(0.62f, 0.78f, 0.9f), new Color(0.48f, 0.62f, 0.76f), false,
                    RuneId.Water, RuneId.Earth, RuneId.Ice),

                new WorldMaterial(MaterialId.Sand, "sand",
                    "The same grit as dust. Breath forced through rest. Air · Earth.",
                    RuneId.Dust, MaterialPaint.Sand,
                    new Color(0.72f, 0.6f, 0.38f), new Color(0.58f, 0.48f, 0.3f), false,
                    RuneId.Air, RuneId.Earth, RuneId.Dust),

                new WorldMaterial(MaterialId.Mud, "mud",
                    "Rest meeting yield. Soft ground. Earth · Water.",
                    RuneId.Mud, MaterialPaint.Mud,
                    new Color(0.32f, 0.22f, 0.14f), new Color(0.28f, 0.2f, 0.14f), false,
                    RuneId.Earth, RuneId.Water, RuneId.Mud),

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
                    "Breath forced through rest. The same grit as sand.",
                    RuneId.Dust, MaterialPaint.Dust,
                    new Color(0.52f, 0.46f, 0.38f), new Color(0.4f, 0.36f, 0.3f), false,
                    RuneId.Air, RuneId.Earth, RuneId.Dust),

                new WorldMaterial(MaterialId.Glass, "glass",
                    "Grains meet witchfire, then rest. Dust · Flame · Earth.",
                    RuneId.Glass, MaterialPaint.Glass,
                    new Color(0.28f, 0.42f, 0.46f), new Color(0.22f, 0.32f, 0.36f), false,
                    RuneId.Dust, RuneId.Flame, RuneId.Earth, RuneId.Glass),

                new WorldMaterial(MaterialId.Crystal, "crystal",
                    "Stone grown with yield.",
                    RuneId.Crystal, MaterialPaint.Crystal,
                    new Color(0.55f, 0.42f, 0.72f), new Color(0.4f, 0.3f, 0.54f), false,
                    RuneId.Earth, RuneId.Salt, RuneId.Stone, RuneId.Water, RuneId.Crystal),

                new WorldMaterial(MaterialId.Obsidian, "obsidian",
                    "Hungry earth quenched and given a body. Melt, Shatter, and hunger's thaw will not take it. Lava · Salt · Water.",
                    RuneId.Obsidian, MaterialPaint.Obsidian,
                    new Color(0.08f, 0.06f, 0.1f), new Color(0.1f, 0.08f, 0.12f), false,
                    RuneId.Fire, RuneId.Earth, RuneId.Lava, RuneId.Salt, RuneId.Water, RuneId.Obsidian),

                new WorldMaterial(MaterialId.Grove, "grove",
                    "Living plant as a mass. Tree-work, not a rune.",
                    RuneId.Plant, MaterialPaint.Grove,
                    new Color(0.16f, 0.32f, 0.14f), new Color(0.14f, 0.26f, 0.12f), false,
                    RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Plant, RuneId.Vita),

                new WorldMaterial(MaterialId.Cloud, "cloud",
                    "Breath holding yield. A hanging veil.",
                    RuneId.Cloud, MaterialPaint.Cloud,
                    new Color(0.72f, 0.76f, 0.82f), new Color(0.56f, 0.6f, 0.66f), false,
                    RuneId.Air, RuneId.Water, RuneId.Cloud),

                new WorldMaterial(MaterialId.Rain, "rain-slick stone",
                    "The hanging veil drawn down. Weather, not a rune.",
                    RuneId.Cloud, MaterialPaint.Rain,
                    new Color(0.22f, 0.3f, 0.4f), new Color(0.2f, 0.26f, 0.34f), false,
                    RuneId.Air, RuneId.Water, RuneId.Cloud),

                new WorldMaterial(MaterialId.Snow, "snow",
                    "The hanging veil given ice’s story. Weather, not a rune.",
                    RuneId.Ice, MaterialPaint.Snow,
                    new Color(0.88f, 0.9f, 0.94f), new Color(0.7f, 0.74f, 0.8f), false,
                    RuneId.Air, RuneId.Water, RuneId.Cloud, RuneId.Ice),

                new WorldMaterial(MaterialId.Glacier, "glacier",
                    "Ice given logos and its own perpetuity. Ordinary fire cannot take it. Witchfire can.",
                    RuneId.Glacier, MaterialPaint.Glacier,
                    new Color(0.7f, 0.82f, 0.88f), new Color(0.48f, 0.58f, 0.66f), false,
                    RuneId.Water, RuneId.Earth, RuneId.Ice, RuneId.Animus, RuneId.Glacier),

                new WorldMaterial(MaterialId.Acid, "acid slick",
                    "Steam forced through Metal.",
                    RuneId.Acid, MaterialPaint.Acid,
                    new Color(0.55f, 0.72f, 0.18f), new Color(0.36f, 0.48f, 0.14f), false,
                    RuneId.Fire, RuneId.Water, RuneId.Steam, RuneId.Metal, RuneId.Acid),

                new WorldMaterial(MaterialId.Water, "standing water",
                    "Yield, still holding a vessel. A pool, not weather. It drowns until ice gives it a body.",
                    RuneId.Water, MaterialPaint.Water,
                    new Color(0.18f, 0.38f, 0.62f), new Color(0.16f, 0.3f, 0.48f), false,
                    RuneId.Water, RuneId.Salt),

                new WorldMaterial(MaterialId.Plant, "living plant",
                    "Vegetable body before Life marks a forest. Green cover.",
                    RuneId.Plant, MaterialPaint.Plant,
                    new Color(0.22f, 0.42f, 0.16f), new Color(0.18f, 0.34f, 0.14f), false,
                    RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Plant),

                new WorldMaterial(MaterialId.Dirt, "dirt",
                    "Loose rest, thrown. Earth speaks here. It smothers ground-fire.",
                    RuneId.Earth, MaterialPaint.Dirt,
                    new Color(0.42f, 0.32f, 0.2f), new Color(0.34f, 0.26f, 0.16f), false,
                    RuneId.Earth),

                new WorldMaterial(MaterialId.Oil, "oil",
                    "A vegetable body pressed with hunger and rest. It holds flame. It floats: a film on water still burns and flashes. Connected oil flashes; a geyser, once lit, keeps burning.",
                    RuneId.Oil, MaterialPaint.Oil,
                    new Color(0.18f, 0.14f, 0.08f), new Color(0.14f, 0.1f, 0.06f), false,
                    RuneId.Plant, RuneId.Fire, RuneId.Earth, RuneId.Oil),

                new WorldMaterial(MaterialId.Miasma, "miasma",
                    "The hanging veil forced through acid. Foul breath on the floor.",
                    RuneId.Miasma, MaterialPaint.Miasma,
                    new Color(0.28f, 0.42f, 0.12f), new Color(0.22f, 0.32f, 0.1f), false,
                    RuneId.Cloud, RuneId.Acid, RuneId.Miasma),

                new WorldMaterial(MaterialId.Wardstone, "wardstone",
                    "Rest given a body, then the mind holds it. Mostly spell-proof.",
                    RuneId.Stone, MaterialPaint.Wardstone,
                    new Color(0.42f, 0.36f, 0.52f), new Color(0.32f, 0.26f, 0.4f), false,
                    RuneId.Earth, RuneId.Salt, RuneId.Sulphur, RuneId.Stone),

                new WorldMaterial(MaterialId.Aegis, "aegis",
                    "Shown steel. Light seated in metal. Mostly spell-proof.",
                    RuneId.Metal, MaterialPaint.Aegis,
                    new Color(0.72f, 0.7f, 0.42f), new Color(0.56f, 0.54f, 0.3f), false,
                    RuneId.Metal, RuneId.Lumen),

                new WorldMaterial(MaterialId.Fire, "fire",
                    "Hunger seated in the walk. At rest until a spell finds it.",
                    RuneId.Fire, MaterialPaint.Fire,
                    new Color(0.86f, 0.28f, 0.08f), new Color(0.62f, 0.16f, 0.06f), false,
                    RuneId.Fire)
            };

            ById = new Dictionary<MaterialId, WorldMaterial>(AllMaterials.Length);
            foreach (var material in AllMaterials)
            {
                ById[material.Id] = material;
            }

            Flag(MaterialId.Stone, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Ash, 0.05f, 0f, VitalLaw.TinderBurnSeconds, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Timber, 1.6f, -0.9f, VitalLaw.TimberBurnSeconds, VitalLaw.HungerTimber, VitalLaw.QuenchDry, VitalLaw.ConductInsulator);
            Flag(MaterialId.Hearth, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Ember, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Damp, -0.7f, 0.35f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDamp, VitalLaw.ConductDamp);
            Flag(MaterialId.Vein, 0f, 0.85f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductVein);
            Flag(MaterialId.Scoured, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Moss, 1.05f, -0.7f, VitalLaw.PlantBurnSeconds, VitalLaw.HungerMoss, VitalLaw.QuenchDry, VitalLaw.ConductInsulator);
            Flag(MaterialId.Metal, 0f, 1.6f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductMetal);
            Flag(MaterialId.SaltCrust, -0.15f, 0.2f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchSalt, VitalLaw.ConductSalt);
            Flag(MaterialId.Void, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductInsulator);
            Flag(MaterialId.Ice, -0.85f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchIce, VitalLaw.ConductPoor);
            Flag(MaterialId.Sand, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Mud, -0.35f, 0.25f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchMud, VitalLaw.ConductSalt);
            Flag(MaterialId.Lava, 0.2f, 0.3f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductDamp);
            Flag(MaterialId.Steam, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Dust, 0.55f, 0f, VitalLaw.GroveBurnSeconds, VitalLaw.HungerTinder, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Glass, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Crystal, 0f, 0.35f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductDamp);
            Flag(MaterialId.Obsidian, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Grove, 0.85f, -1.2f, VitalLaw.GroveBurnSeconds, VitalLaw.HungerSoft, VitalLaw.QuenchDry, VitalLaw.ConductInsulator);
            Flag(MaterialId.Cloud, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Rain, -1.1f, 0.7f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchRain, VitalLaw.ConductRain);
            Flag(MaterialId.Snow, -0.65f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchIce, VitalLaw.ConductPoor);
            Flag(MaterialId.Glacier, -0.9f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchGlacier, VitalLaw.ConductPoor);
            Flag(MaterialId.Acid, 0.15f, 0.45f, VitalLaw.TinderBurnSeconds, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductAcid);
            Flag(MaterialId.Water, -1.6f, 1.25f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchWater, VitalLaw.ConductWater);
            Flag(MaterialId.Plant, 1.1f, -1.1f, VitalLaw.PlantBurnSeconds, VitalLaw.HungerPlant, VitalLaw.QuenchDry, VitalLaw.ConductInsulator);
            Flag(MaterialId.Dirt, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Oil, 2.2f, -0.25f, VitalLaw.OilBurnSeconds, VitalLaw.HungerOil, VitalLaw.QuenchDry, VitalLaw.ConductInsulator);
            Flag(MaterialId.Miasma, 0.1f, 0f, VitalLaw.TinderBurnSeconds, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Wardstone, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
            Flag(MaterialId.Aegis, 0f, 1.1f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductVein);
            Flag(MaterialId.Fire, 0f, 0f, 0f, VitalLaw.HungerNeutral, VitalLaw.QuenchDry, VitalLaw.ConductPoor);
        }

        /// <summary>
        /// Stamp catch, leftover conductivity, burn clock, the 0–10
        /// Hunger grade, the 0–10 Quench grade, and the 0–10 Conduct
        /// grade. Omit hunger and it stays 0. Omit quench and it
        /// stays dry. Omit conduct and the leftover conductivity
        /// number is read into a grade.
        /// </summary>
        static void Flag(
            MaterialId id,
            float flammability,
            float conductivity,
            float burnSeconds = 0f,
            int hunger = VitalLaw.HungerNeutral,
            int quench = VitalLaw.QuenchDry,
            int conduct = -1)
        {
            if (ById.TryGetValue(id, out var material))
            {
                material.Flammability = flammability;
                material.Conductivity = conductivity;
                material.BurnSeconds = burnSeconds;
                material.BurnRate = VitalLaw.FireRun(burnSeconds);
                material.Hunger = Mathf.Clamp(hunger, VitalLaw.HungerNeutral, VitalLaw.HungerMax);
                material.Quench = Mathf.Clamp(quench, VitalLaw.QuenchDry, VitalLaw.QuenchMax);
                material.Conduct = conduct >= 0
                    ? Mathf.Clamp(conduct, VitalLaw.ConductInsulator, VitalLaw.ConductMax)
                    : InferConduct(conductivity);
            }
        }

        /// <summary>
        /// Leftover conductivity → 0–10 when a new body omits the
        /// grade. Negative is wood. Zero is stone. Strong leftover
        /// is metal.
        /// </summary>
        public static int InferConduct(float leftover)
        {
            if (leftover < 0f)
            {
                return VitalLaw.ConductInsulator;
            }

            if (leftover < 0.2f)
            {
                return VitalLaw.ConductPoor;
            }

            if (leftover < 0.3f)
            {
                return VitalLaw.ConductSalt;
            }

            if (leftover < 0.5f)
            {
                return VitalLaw.ConductDamp;
            }

            if (leftover < 0.65f)
            {
                return VitalLaw.ConductAcid;
            }

            if (leftover < 0.8f)
            {
                return VitalLaw.ConductRain;
            }

            if (leftover < 1.05f)
            {
                return VitalLaw.ConductVein;
            }

            if (leftover < 1.4f)
            {
                return VitalLaw.ConductWater;
            }

            return VitalLaw.ConductMetal;
        }

        public static IReadOnlyList<WorldMaterial> All => AllMaterials;

        public static bool IsStampable(MaterialId id) =>
            id != MaterialId.None;

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
                case RuneId.Earth: return MaterialId.Dirt;
                case RuneId.Plant: return MaterialId.Timber;
                case RuneId.Spark: return MaterialId.Vein;
                case RuneId.Ash: return MaterialId.Ash;
                case RuneId.Metal: return MaterialId.Metal;
                case RuneId.Salt: return MaterialId.SaltCrust;
                case RuneId.Vita: return MaterialId.Grove;
                case RuneId.Dust: return MaterialId.Dust;
                case RuneId.Ice: return MaterialId.Ice;
                case RuneId.Sand: return MaterialId.Dust;
                case RuneId.Mud: return MaterialId.Mud;
                case RuneId.Lava: return MaterialId.Lava;
                case RuneId.Steam: return MaterialId.Steam;
                case RuneId.Glass: return MaterialId.Glass;
                case RuneId.Crystal: return MaterialId.Crystal;
                case RuneId.Obsidian: return MaterialId.Obsidian;
                case RuneId.Cloud: return MaterialId.Cloud;
                case RuneId.Glacier: return MaterialId.Glacier;
                case RuneId.Acid: return MaterialId.Acid;
                case RuneId.Oil: return MaterialId.Oil;
                case RuneId.Miasma: return MaterialId.Miasma;
                case RuneId.Poison: return MaterialId.Acid;
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
                        : material == MaterialId.Hearth ? "flame wall"
                        : material == MaterialId.Fire ? "fire wall"
                        : material == MaterialId.Grove ? "vine wall"
                        : def.Name + " wall";
                default:
                    return def.Name;
            }
        }
    }
}
