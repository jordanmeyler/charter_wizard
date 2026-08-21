using System.Collections.Generic;

namespace RuneMagic
{
    public enum BlendKind
    {
        Stable,
        Violent
    }

    public readonly struct BlendResult
    {
        public BlendResult(RuneId result, BlendKind kind, string note)
        {
            Result = result;
            Kind = kind;
            Note = note;
        }

        public RuneId Result { get; }
        public BlendKind Kind { get; }
        public string Note { get; }
    }

    /// <summary>
    /// Quality square and material blends from the design reference.
    /// Deeper nodes stay additive so the tree can grow without rewriting callers.
    /// </summary>
    public static class MaterialTree
    {
        static readonly Dictionary<(RuneId, RuneId), BlendResult> Blends = new();
        static readonly List<(RuneId Left, RuneId Right, BlendResult Result)> Canonical = new();

        static MaterialTree()
        {
            Add(RuneId.Fire, RuneId.Air, RuneId.Spark, BlendKind.Stable, "Hunger given breath. Fire · Air → Spark.");
            Add(RuneId.Air, RuneId.Water, RuneId.Cloud, BlendKind.Stable, "Breath holding yield. Air · Water → Cloud.");
            Add(RuneId.Water, RuneId.Earth, RuneId.Mud, BlendKind.Stable, "Yield meeting rest. Water · Earth → Mud.");
            Add(RuneId.Fire, RuneId.Earth, RuneId.Lava, BlendKind.Stable, "Hunger meeting rest. Fire · Earth → Lava.");

            Add(RuneId.Fire, RuneId.Water, RuneId.Steam, BlendKind.Violent, "Hunger forced through yield. Fire · Water → Steam.");
            Add(RuneId.Air, RuneId.Earth, RuneId.Dust, BlendKind.Violent, "Breath forced through rest. Air · Earth → Dust.");

            Add(RuneId.Spark, RuneId.Air, RuneId.Lightning, BlendKind.Stable, "The seed stretched through more breath. Spark · Air → Lightning.");
            Add(RuneId.Lightning, RuneId.Earth, RuneId.Thunder, BlendKind.Stable, "The arc meeting rest. Lightning · Earth → Thunder.");
            Add(RuneId.Spark, RuneId.Cloud, RuneId.Storm, BlendKind.Stable, "The seed inside the hanging veil. Spark · Cloud → Storm.");
            Add(RuneId.Cloud, RuneId.Earth, RuneId.Rain, BlendKind.Stable, "The hanging veil drawn down. Cloud · Earth → Rain.");
            Add(RuneId.Cloud, RuneId.Water, RuneId.Rain, BlendKind.Stable, "The hanging veil yields more. Cloud · Water → Rain.");
            Add(RuneId.Cloud, RuneId.Ice, RuneId.Snow, BlendKind.Stable, "The hanging veil given ice’s story. Cloud · Ice → Snow.");
            Add(RuneId.Wind, RuneId.Snow, RuneId.Blizzard, BlendKind.Violent, "Wind driving Snow. Wind · Snow → Blizzard.");
            Add(RuneId.Lava, RuneId.Water, RuneId.Obsidian, BlendKind.Violent, "Hungry earth quenched. Lava · Water → Obsidian.");
            Add(RuneId.Lava, RuneId.Earth, RuneId.Metal, BlendKind.Stable, "Hungry earth given more rest. Lava · Earth → Metal.");
            Add(RuneId.Mud, RuneId.Air, RuneId.Sand, BlendKind.Stable, "Mud given breath until it dries. Mud · Air → Sand.");
            Add(RuneId.Sand, RuneId.Fire, RuneId.Glass, BlendKind.Stable, "Grains meet hunger, then rest. Sand · Flame · Earth → Glass.");
            Add(RuneId.Stone, RuneId.Water, RuneId.Crystal, BlendKind.Stable, "Stone grown with yield. Stone · Water → Crystal.");
            Add(RuneId.Ice, RuneId.Stone, RuneId.Glacier, BlendKind.Stable, "Still water given stone. Ice · Stone → Glacier.");
            Add(RuneId.Steam, RuneId.Metal, RuneId.Acid, BlendKind.Violent, "Steam forced through Metal → Acid.");
            Add(RuneId.Fire, RuneId.Plant, RuneId.Ash, BlendKind.Stable, "Hunger leaves a vegetable body as Ash.");
            Add(RuneId.Plant, RuneId.Vita, RuneId.Forest, BlendKind.Stable, "The vegetable body waking. Plant · Life → Grove.");
            Add(RuneId.Plant, RuneId.Mors, RuneId.Blight, BlendKind.Stable, "Death-work. A living plant, then the grave. Grove · Death → Blight.");
        }

        public static IReadOnlyList<(RuneId Left, RuneId Right, BlendResult Result)> All => Canonical;

        static void Add(RuneId left, RuneId right, RuneId result, BlendKind kind, string note)
        {
            var blend = new BlendResult(result, kind, note);
            Canonical.Add((left, right, blend));
            Blends[(left, right)] = blend;
            if (left != right)
            {
                Blends[(right, left)] = blend;
            }
        }

        public static bool TryBlend(RuneId left, RuneId right, out BlendResult result)
        {
            if (left == RuneId.None || right == RuneId.None || left == right)
            {
                result = default;
                return false;
            }

            return Blends.TryGetValue((left, right), out result);
        }

        public static bool TryFold(IReadOnlyList<RuneId> materials, out RuneId result, out BlendResult? lastBlend)
        {
            lastBlend = null;
            result = RuneId.None;
            if (materials == null || materials.Count == 0)
            {
                return false;
            }

            result = materials[0];
            for (var i = 1; i < materials.Count; i++)
            {
                if (!TryBlend(result, materials[i], out var blend))
                {
                    result = RuneId.None;
                    lastBlend = null;
                    return false;
                }

                result = blend.Result;
                lastBlend = blend;
            }

            return true;
        }

        public static bool TryFindSources(RuneId material, out RuneId left, out RuneId right)
        {
            foreach (var entry in Canonical)
            {
                if (entry.Result.Result == material)
                {
                    left = entry.Left;
                    right = entry.Right;
                    return true;
                }
            }

            left = RuneId.None;
            right = RuneId.None;
            return false;
        }

        public static string DescribePair(RuneId left, RuneId right)
        {
            if (!TryBlend(left, right, out var blend))
            {
                return $"{RuneCatalog.NameOf(left)} and {RuneCatalog.NameOf(right)} have no recorded join yet.";
            }

            var tone = blend.Kind == BlendKind.Violent ? "violently" : "stably";
            return $"{RuneCatalog.NameOf(left)} and {RuneCatalog.NameOf(right)} join {tone} into {RuneCatalog.NameOf(blend.Result)}.";
        }
    }
}
