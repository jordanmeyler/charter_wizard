using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Saturated, well-separated colours for every named rune. Related
    /// joins stay in the same family, but no two neighbours share a hue
    /// and value close enough to read as the same tile.
    /// </summary>
    public static class RunePalette
    {
        public static Color Of(RuneId id)
        {
            switch (id)
            {
                case RuneId.Fire: return new Color(1f, 0.34f, 0.08f);
                case RuneId.Air: return new Color(0.38f, 0.86f, 0.96f);
                case RuneId.Earth: return new Color(0.76f, 0.5f, 0.16f);
                case RuneId.Water: return new Color(0.12f, 0.4f, 0.96f);
                case RuneId.Spark: return new Color(1f, 0.96f, 0.72f);
                case RuneId.Cloud: return new Color(0.58f, 0.66f, 0.86f);
                case RuneId.Mud: return new Color(0.5f, 0.32f, 0.1f);
                case RuneId.Lava: return new Color(0.96f, 0.16f, 0.04f);
                case RuneId.Steam: return new Color(0.9f, 0.78f, 0.82f);
                case RuneId.Dust: return new Color(0.84f, 0.7f, 0.36f);
                case RuneId.Plant: return new Color(0.28f, 0.72f, 0.2f);
                case RuneId.Salt: return new Color(0.96f, 0.92f, 0.78f);
                case RuneId.Mercury: return new Color(0.16f, 0.82f, 0.68f);
                case RuneId.Sulphur: return new Color(0.92f, 0.56f, 0.08f);
                case RuneId.Aether: return new Color(0.72f, 0.4f, 0.98f);
                case RuneId.Vita: return new Color(0.12f, 0.9f, 0.4f);
                case RuneId.Mors: return new Color(0.68f, 0.1f, 0.3f);
                case RuneId.Animus: return new Color(0.96f, 0.36f, 0.32f);
                case RuneId.Anima: return new Color(0.58f, 0.36f, 0.92f);
                case RuneId.Lumen: return new Color(1f, 0.94f, 0.38f);
                case RuneId.Umbra: return new Color(0.28f, 0.14f, 0.5f);
                case RuneId.Ice: return new Color(0.5f, 0.88f, 1f);
                case RuneId.Stone: return new Color(0.56f, 0.54f, 0.5f);
                case RuneId.Storm: return new Color(0.36f, 0.4f, 0.74f);
                case RuneId.Lightning: return new Color(0.82f, 0.88f, 1f);
                case RuneId.Inferno: return new Color(1f, 0.06f, 0.04f);
                case RuneId.Plasma: return new Color(0.48f, 0.96f, 1f);
                case RuneId.Rain: return new Color(0.18f, 0.36f, 0.78f);
                case RuneId.Snow: return new Color(0.94f, 0.97f, 1f);
                case RuneId.Blizzard: return new Color(0.68f, 0.8f, 0.94f);
                case RuneId.Sandstorm: return new Color(0.84f, 0.6f, 0.18f);
                case RuneId.Obsidian: return new Color(0.16f, 0.08f, 0.2f);
                case RuneId.Metal: return new Color(0.7f, 0.74f, 0.8f);
                case RuneId.Crystal: return new Color(0.55f, 0.94f, 0.9f);
                case RuneId.Glacier: return new Color(0.38f, 0.66f, 0.84f);
                case RuneId.Acid: return new Color(0.62f, 0.96f, 0.1f);
                case RuneId.Vine: return new Color(0.14f, 0.62f, 0.28f);
                case RuneId.Forest: return new Color(0.08f, 0.38f, 0.16f);
                case RuneId.Blight: return new Color(0.54f, 0.5f, 0.08f);
                case RuneId.Ash: return new Color(0.5f, 0.48f, 0.46f);
                case RuneId.Flame: return new Color(0.78f, 0.42f, 1f);
                case RuneId.Grove: return new Color(0.18f, 0.58f, 0.26f);
                case RuneId.Wind: return new Color(0.68f, 0.96f, 0.86f);
                case RuneId.Current: return new Color(0.06f, 0.62f, 0.92f);
                case RuneId.Ember: return new Color(0.74f, 0.2f, 0.06f);
                case RuneId.Shade: return new Color(0.22f, 0.1f, 0.32f);
                case RuneId.Thunder: return new Color(0.64f, 0.52f, 0.96f);
                case RuneId.Glass: return new Color(0.78f, 0.9f, 0.92f);
                case RuneId.Sand: return new Color(0.9f, 0.76f, 0.38f);
                case RuneId.Hot: return new Color(1f, 0.4f, 0.16f);
                case RuneId.Cold: return new Color(0.52f, 0.76f, 1f);
                case RuneId.Wet: return new Color(0.22f, 0.54f, 0.86f);
                case RuneId.Dry: return new Color(0.86f, 0.7f, 0.44f);
                case RuneId.Male: return new Color(0.96f, 0.36f, 0.32f);
                case RuneId.Female: return new Color(0.58f, 0.36f, 0.92f);
                default: return new Color(0.7f, 0.7f, 0.75f);
            }
        }

        public static float Luma(Color color) =>
            color.r * 0.3f + color.g * 0.59f + color.b * 0.11f;

        public static Color Card(RuneId id, bool available)
        {
            var tone = Of(id);
            var wash = Luma(tone) > 0.78f ? 0.42f : 0.28f;
            var fill = Color.Lerp(tone, new Color(0.06f, 0.05f, 0.07f), available ? wash : 0.78f);
            fill.a = available ? 0.94f : 0.4f;
            return fill;
        }

        public static Color Ink(RuneId id, bool available)
        {
            if (!available)
            {
                return new Color(0.58f, 0.58f, 0.62f, 0.5f);
            }

            return Luma(Of(id)) < 0.5f
                ? new Color(0.98f, 0.96f, 0.9f)
                : new Color(0.1f, 0.08f, 0.06f);
        }

        public static Color MarkInk(RuneId id, bool available = true)
        {
            var tone = Of(id);
            var ink = Luma(tone) > 0.78f
                ? Color.Lerp(tone, new Color(0.32f, 0.26f, 0.16f), 0.28f)
                : Color.Lerp(tone, Color.white, 0.18f);
            if (!available)
            {
                ink = Color.Lerp(ink, new Color(0.34f, 0.34f, 0.38f), 0.72f);
                ink.a = 0.42f;
            }

            return ink;
        }

        public static Color Caption(RuneId id, bool available)
        {
            if (!available)
            {
                return new Color(0.48f, 0.48f, 0.52f, 0.55f);
            }

            return Luma(Of(id)) < 0.55f
                ? new Color(0.98f, 0.94f, 0.86f)
                : new Color(0.12f, 0.08f, 0.06f);
        }
    }
}
