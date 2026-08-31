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
            AddDirected(RuneId.Water, RuneId.Earth, RuneId.Ice, BlendKind.Stable, "Yield meeting rest. Water · Earth → Ice.");
            AddDirected(RuneId.Earth, RuneId.Water, RuneId.Mud, BlendKind.Stable, "Rest meeting yield. Earth · Water → Mud.");
            Add(RuneId.Fire, RuneId.Earth, RuneId.Lava, BlendKind.Stable, "Hunger meeting rest. Fire · Earth → Lava.");

            Add(RuneId.Fire, RuneId.Water, RuneId.Steam, BlendKind.Violent, "Hunger forced through yield. Fire · Water → Steam.");
            Add(RuneId.Air, RuneId.Earth, RuneId.Dust, BlendKind.Violent, "Breath forced through rest. Air · Earth → Dust. The same grit as sand.");

            Add(RuneId.Spark, RuneId.Air, RuneId.Lightning, BlendKind.Stable, "The seed stretched through more breath. Spark · Air → Lightning.");
            Add(RuneId.Stone, RuneId.Water, RuneId.Crystal, BlendKind.Stable, "Stone grown with yield. Stone · Water → Crystal.");
            Add(RuneId.Ice, RuneId.Stone, RuneId.Glacier, BlendKind.Stable, "Still water given stone. Ice · Stone → Glacier.");
            Add(RuneId.Steam, RuneId.Metal, RuneId.Acid, BlendKind.Violent, "Steam forced through Metal → Acid.");
            Add(RuneId.Fire, RuneId.Plant, RuneId.Ash, BlendKind.Stable, "Hunger leaves a vegetable body as Ash.");
            Add(RuneId.Ash, RuneId.Earth, RuneId.Oil, BlendKind.Stable, "Pressed vegetable hunger given rest. Ash · Earth → Oil.");
            Add(RuneId.Plant, RuneId.Mors, RuneId.Poison, BlendKind.Stable, "The vegetable body, then the grave. Plant · Death → Poison.");
            Add(RuneId.Flame, RuneId.Lightning, RuneId.Plasma, BlendKind.Violent, "Witchfire joined to the bolt. Flame · Lightning → Plasma.");
            Add(RuneId.Cloud, RuneId.Acid, RuneId.Miasma, BlendKind.Violent, "The hanging veil forced through acid. Cloud · Acid → Miasma.");
        }

        public static IReadOnlyList<(RuneId Left, RuneId Right, BlendResult Result)> All => Canonical;

        static void Add(RuneId left, RuneId right, RuneId result, BlendKind kind, string note)
        {
            AddDirected(left, right, result, kind, note);
            if (left != right)
            {
                Blends[(right, left)] = new BlendResult(result, kind, note);
            }
        }

        static void AddDirected(RuneId left, RuneId right, RuneId result, BlendKind kind, string note)
        {
            var blend = new BlendResult(result, kind, note);
            Canonical.Add((left, right, blend));
            Blends[(left, right)] = blend;
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
