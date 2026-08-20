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

        static MaterialTree()
        {
            Add(RuneId.Fire, RuneId.Air, RuneId.Spark, BlendKind.Stable, "Share Hot. Fire + Air → Spark.");
            Add(RuneId.Air, RuneId.Water, RuneId.Cloud, BlendKind.Stable, "Share Wet. Air + Water → Cloud.");
            Add(RuneId.Water, RuneId.Earth, RuneId.Mud, BlendKind.Stable, "Share Cold. Water + Earth → Mud.");
            Add(RuneId.Fire, RuneId.Earth, RuneId.Lava, BlendKind.Stable, "Share Dry. Fire + Earth → Lava.");

            Add(RuneId.Fire, RuneId.Water, RuneId.Steam, BlendKind.Violent, "Opposed diagonal. Fire + Water → Steam.");
            Add(RuneId.Air, RuneId.Earth, RuneId.Dust, BlendKind.Violent, "Opposed diagonal. Air + Earth → Dust.");

            Add(RuneId.Spark, RuneId.Air, RuneId.Storm, BlendKind.Stable, "Spark driven into Air → Storm.");
            Add(RuneId.Spark, RuneId.Water, RuneId.Storm, BlendKind.Stable, "Spark driven into Water → Storm.");
            Add(RuneId.Water, RuneId.Cold, RuneId.Ice, BlendKind.Stable, "Water drawn toward Cold → Ice.");
            Add(RuneId.Lava, RuneId.Cold, RuneId.Stone, BlendKind.Stable, "Lava cooled → Stone.");
            Add(RuneId.Mud, RuneId.Air, RuneId.Sand, BlendKind.Stable, "Mud dried by Air → Sand.");
        }

        static void Add(RuneId left, RuneId right, RuneId result, BlendKind kind, string note)
        {
            var blend = new BlendResult(result, kind, note);
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
