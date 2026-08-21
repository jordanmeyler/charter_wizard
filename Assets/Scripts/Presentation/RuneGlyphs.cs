using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Pixel marks for every rune. Each concept keeps its own silhouette
    /// so Fire, Water, and the rest still read when the tiles are small.
    /// </summary>
    public static class RuneGlyphs
    {
        const int Size = 24;

        public static Sprite Build(RuneId id)
        {
            var canvas = new PixelCanvas(Size);
            canvas.Clear(new Color(0f, 0f, 0f, 0f));
            var tone = RunePalette.Of(id);
            var ink = RunePalette.Luma(tone) > 0.78f
                ? Color.Lerp(tone, new Color(0.28f, 0.22f, 0.12f), 0.28f)
                : Color.Lerp(tone, Color.white, 0.18f);
            var hi = Color.Lerp(ink, Color.white, 0.55f);
            Paint(canvas, id, ink, hi);
            var outline = RunePalette.Luma(tone) < 0.42f
                ? new Color(0.96f, 0.92f, 0.82f, 0.9f)
                : new Color(0.06f, 0.04f, 0.05f, 0.88f);
            canvas.Outline(outline);
            return canvas.ToSprite(Size);
        }

        static void Paint(PixelCanvas c, RuneId id, Color ink, Color hi)
        {
            switch (id)
            {
                case RuneId.Fire:
                    c.FillCircle(12, 9, 7, ink);
                    c.FillTriangle(12, 21, 5, 10, 19, 10, ink);
                    c.FillCircle(12, 10, 4, hi);
                    c.Set(12, 12, Color.white);
                    break;
                case RuneId.Air:
                    c.ThickLine(5, 7, 12, 20, ink);
                    c.ThickLine(19, 7, 12, 20, ink);
                    c.Line(7, 6, 12, 15, hi);
                    c.Line(17, 6, 12, 15, hi);
                    c.FillCircle(12, 5, 2, ink);
                    break;
                case RuneId.Earth:
                    c.Fill(5, 4, 14, 10, ink);
                    c.FillTriangle(12, 20, 5, 12, 19, 12, ink);
                    c.Fill(7, 6, 4, 4, hi);
                    break;
                case RuneId.Water:
                    c.FillCircle(12, 16, 7, ink);
                    c.FillTriangle(12, 4, 6, 14, 18, 14, ink);
                    c.FillCircle(12, 16, 3, hi);
                    break;
                case RuneId.Spark:
                    Star(c, 12, 12, 9, ink);
                    c.FillCircle(12, 12, 2, hi);
                    break;
                case RuneId.Cloud:
                    c.FillCircle(8, 11, 5, ink);
                    c.FillCircle(14, 13, 6, ink);
                    c.FillCircle(17, 10, 4, ink);
                    c.Fill(7, 8, 11, 5, ink);
                    c.Set(10, 14, hi);
                    break;
                case RuneId.Mud:
                    c.FillCircle(12, 11, 7, ink);
                    c.Fill(8, 5, 8, 6, ink);
                    c.FillCircle(12, 5, 3, ink);
                    c.Set(9, 13, hi);
                    break;
                case RuneId.Lava:
                    c.FillTriangle(12, 20, 4, 8, 20, 8, ink);
                    c.Fill(10, 4, 4, 6, ink);
                    c.FillCircle(12, 4, 3, hi);
                    break;
                case RuneId.Steam:
                    c.FillCircle(12, 6, 5, ink);
                    Rise(c, 7, 10, ink);
                    Rise(c, 12, 12, hi);
                    Rise(c, 17, 10, ink);
                    break;
                case RuneId.Dust:
                    c.FillCircle(8, 14, 2, ink);
                    c.FillCircle(14, 16, 2, ink);
                    c.FillCircle(17, 10, 2, ink);
                    c.FillCircle(10, 8, 2, ink);
                    c.FillCircle(12, 12, 1, hi);
                    c.Set(6, 9, ink);
                    c.Set(18, 15, ink);
                    break;
                case RuneId.Plant:
                    c.ThickLine(12, 4, 12, 16, ink);
                    c.FillCircle(8, 14, 3, ink);
                    c.FillCircle(16, 15, 3, ink);
                    c.FillCircle(12, 18, 3, hi);
                    break;
                case RuneId.Salt:
                    c.Circle(12, 12, 8, ink);
                    c.Circle(12, 12, 7, ink);
                    c.ThickLine(5, 12, 19, 12, ink);
                    c.Set(12, 16, hi);
                    break;
                case RuneId.Mercury:
                    c.Circle(12, 14, 5, ink);
                    c.Circle(12, 14, 4, ink);
                    c.ThickLine(8, 20, 12, 16, ink);
                    c.ThickLine(16, 20, 12, 16, ink);
                    c.ThickLine(12, 4, 12, 9, ink);
                    c.ThickLine(9, 7, 15, 7, ink);
                    c.Set(12, 14, hi);
                    break;
                case RuneId.Sulphur:
                    c.FillTriangle(12, 21, 6, 12, 18, 12, ink);
                    c.ThickLine(12, 12, 12, 4, ink);
                    c.ThickLine(8, 8, 16, 8, ink);
                    c.Set(12, 16, hi);
                    break;
                case RuneId.Aether:
                    c.Circle(12, 12, 8, ink);
                    Star(c, 12, 12, 6, hi);
                    break;
                case RuneId.Vita:
                    c.ThickLine(12, 4, 12, 18, ink);
                    c.FillCircle(12, 18, 3, hi);
                    c.Line(12, 12, 6, 16, ink);
                    c.Line(12, 12, 18, 16, ink);
                    c.Line(12, 8, 7, 6, ink);
                    c.Line(12, 8, 17, 6, ink);
                    break;
                case RuneId.Mors:
                    c.ThickLine(12, 4, 12, 20, ink);
                    c.ThickLine(6, 15, 18, 15, ink);
                    c.Fill(11, 14, 3, 3, hi);
                    break;
                case RuneId.Animus:
                case RuneId.Male:
                    c.Circle(10, 10, 5, ink);
                    c.ThickLine(14, 14, 19, 19, ink);
                    c.Line(19, 19, 15, 19, ink);
                    c.Line(19, 19, 19, 15, ink);
                    break;
                case RuneId.Anima:
                case RuneId.Female:
                    c.Circle(12, 15, 5, ink);
                    c.ThickLine(12, 4, 12, 10, ink);
                    c.ThickLine(9, 7, 15, 7, ink);
                    break;
                case RuneId.Lumen:
                    Star(c, 12, 12, 10, ink);
                    c.FillCircle(12, 12, 3, hi);
                    break;
                case RuneId.Umbra:
                    c.FillCircle(12, 12, 8, ink);
                    c.FillCircle(15, 13, 6, new Color(0f, 0f, 0f, 0f));
                    c.Set(8, 14, hi);
                    break;
                case RuneId.Ice:
                    c.ThickLine(12, 3, 12, 21, ink);
                    c.ThickLine(4, 12, 20, 12, ink);
                    c.Line(6, 6, 18, 18, ink);
                    c.Line(18, 6, 6, 18, ink);
                    c.Set(12, 12, hi);
                    break;
                case RuneId.Stone:
                    c.Fill(6, 6, 12, 12, ink);
                    c.Rect(6, 6, 12, 12, Color.Lerp(ink, Color.black, 0.3f));
                    c.Fill(8, 14, 4, 3, hi);
                    break;
                case RuneId.Storm:
                    c.FillCircle(9, 15, 4, ink);
                    c.FillCircle(14, 16, 5, ink);
                    c.Fill(8, 12, 10, 4, ink);
                    Bolt(c, 11, 12, ink);
                    break;
                case RuneId.Lightning:
                    Bolt(c, 12, 21, ink);
                    c.Line(11, 14, 15, 14, hi);
                    break;
                case RuneId.Inferno:
                    c.FillTriangle(12, 21, 2, 4, 22, 4, ink);
                    c.FillTriangle(12, 17, 6, 6, 18, 6, hi);
                    c.FillTriangle(12, 13, 9, 7, 15, 7, Color.white);
                    break;
                case RuneId.Plasma:
                    c.Circle(12, 12, 8, ink);
                    Bolt(c, 12, 19, hi);
                    break;
                case RuneId.Rain:
                    c.FillCircle(9, 17, 4, ink);
                    c.FillCircle(15, 17, 4, ink);
                    c.Fill(8, 14, 9, 3, ink);
                    c.FillCircle(8, 7, 2, ink);
                    c.FillCircle(12, 5, 2, hi);
                    c.FillCircle(16, 7, 2, ink);
                    break;
                case RuneId.Snow:
                    Flake(c, 12, 12, ink, hi);
                    break;
                case RuneId.Blizzard:
                    Flake(c, 9, 14, ink, hi);
                    c.Line(16, 8, 21, 6, ink);
                    c.Line(15, 5, 20, 4, ink);
                    break;
                case RuneId.Sandstorm:
                    c.Line(4, 16, 12, 18, ink);
                    c.Line(6, 12, 18, 14, ink);
                    c.Line(8, 8, 20, 10, hi);
                    c.FillCircle(7, 6, 1, ink);
                    c.FillCircle(14, 7, 1, ink);
                    c.FillCircle(18, 16, 1, ink);
                    break;
                case RuneId.Obsidian:
                    c.FillTriangle(12, 21, 5, 4, 19, 6, ink);
                    c.Line(12, 21, 12, 4, hi);
                    break;
                case RuneId.Metal:
                    c.FillTriangle(12, 20, 4, 12, 12, 4, ink);
                    c.FillTriangle(12, 20, 20, 12, 12, 4, Color.Lerp(ink, Color.white, 0.2f));
                    c.ThickLine(4, 12, 20, 12, Color.Lerp(ink, Color.black, 0.25f));
                    c.Set(12, 16, hi);
                    break;
                case RuneId.Crystal:
                    c.FillTriangle(12, 21, 6, 12, 18, 12, ink);
                    c.FillTriangle(12, 3, 6, 12, 18, 12, Color.Lerp(ink, Color.white, 0.25f));
                    c.ThickLine(12, 21, 12, 3, hi);
                    break;
                case RuneId.Glacier:
                    c.FillTriangle(7, 18, 3, 6, 12, 6, ink);
                    c.FillTriangle(16, 20, 10, 6, 21, 6, Color.Lerp(ink, Color.white, 0.2f));
                    c.Set(16, 12, hi);
                    break;
                case RuneId.Acid:
                    c.FillCircle(12, 8, 6, ink);
                    c.FillTriangle(12, 20, 8, 12, 16, 12, ink);
                    c.Set(10, 9, hi);
                    c.Set(14, 8, Color.white);
                    break;
                case RuneId.Vine:
                    c.Line(6, 5, 9, 10, ink);
                    c.Line(9, 10, 8, 16, ink);
                    c.Line(8, 16, 14, 19, hi);
                    c.FillCircle(16, 18, 3, ink);
                    c.FillCircle(10, 12, 2, ink);
                    break;
                case RuneId.Forest:
                    Tree(c, 7, 6, 5, ink);
                    Tree(c, 16, 5, 6, ink);
                    Tree(c, 12, 8, 7, hi);
                    break;
                case RuneId.Blight:
                    c.ThickLine(12, 4, 12, 14, ink);
                    c.Line(12, 12, 7, 16, ink);
                    c.Line(12, 12, 17, 16, ink);
                    c.ThickLine(7, 8, 17, 18, Color.Lerp(ink, Color.black, 0.2f));
                    break;
                case RuneId.Ash:
                    c.Fill(6, 6, 12, 4, ink);
                    c.FillCircle(8, 10, 2, ink);
                    c.FillCircle(13, 12, 2, ink);
                    c.FillCircle(17, 9, 2, hi);
                    break;
                case RuneId.Flame:
                    c.FillCircle(12, 8, 6, ink);
                    c.FillTriangle(12, 20, 7, 10, 17, 10, ink);
                    c.FillCircle(12, 9, 3, hi);
                    break;
                case RuneId.Grove:
                    Tree(c, 12, 5, 8, ink);
                    c.FillCircle(12, 16, 2, hi);
                    break;
                case RuneId.Wind:
                    c.Line(4, 16, 18, 18, ink);
                    c.Line(5, 12, 20, 14, hi);
                    c.Line(4, 8, 17, 10, ink);
                    c.Set(19, 18, ink);
                    c.Set(21, 14, hi);
                    break;
                case RuneId.Current:
                    Wave(c, 4, 16, ink);
                    Wave(c, 5, 11, hi);
                    Wave(c, 4, 6, ink);
                    break;
                case RuneId.Ember:
                    c.FillCircle(12, 10, 6, ink);
                    c.FillCircle(12, 10, 3, hi);
                    c.Set(12, 16, Color.white);
                    break;
                case RuneId.Shade:
                    c.FillCircle(12, 11, 7, ink);
                    c.Fill(8, 4, 8, 6, ink);
                    c.FillCircle(10, 13, 1, hi);
                    c.FillCircle(14, 13, 1, hi);
                    break;
                case RuneId.Thunder:
                    Bolt(c, 12, 21, ink);
                    c.Fill(6, 4, 12, 3, ink);
                    c.Set(12, 14, hi);
                    break;
                case RuneId.Glass:
                    c.FillTriangle(12, 20, 5, 6, 19, 6, ink);
                    c.Line(12, 20, 12, 6, hi);
                    c.Set(9, 10, Color.white);
                    break;
                case RuneId.Sand:
                    c.FillTriangle(7, 8, 3, 4, 12, 4, ink);
                    c.FillTriangle(16, 10, 10, 4, 21, 4, Color.Lerp(ink, Color.white, 0.2f));
                    c.FillCircle(9, 14, 1, hi);
                    c.FillCircle(15, 16, 1, ink);
                    break;
                case RuneId.Hot:
                    c.FillCircle(12, 10, 5, ink);
                    c.Line(12, 17, 12, 21, hi);
                    c.Line(6, 15, 4, 18, ink);
                    c.Line(18, 15, 20, 18, ink);
                    break;
                case RuneId.Cold:
                    Flake(c, 12, 12, ink, hi);
                    break;
                case RuneId.Wet:
                    c.FillCircle(12, 8, 6, ink);
                    c.FillTriangle(12, 20, 7, 11, 17, 11, ink);
                    c.Set(10, 9, hi);
                    break;
                case RuneId.Dry:
                    c.Line(6, 16, 10, 10, ink);
                    c.Line(10, 10, 16, 14, ink);
                    c.Line(14, 8, 19, 6, hi);
                    break;
                default:
                    c.Circle(12, 12, 7, ink);
                    c.ThickLine(12, 8, 12, 13, ink);
                    c.Set(12, 16, hi);
                    break;
            }
        }

        static void Wave(PixelCanvas c, int x, int y, Color ink)
        {
            c.Line(x, y, x + 4, y + 2, ink);
            c.Line(x + 4, y + 2, x + 8, y, ink);
            c.Line(x + 8, y, x + 12, y + 2, ink);
            c.Line(x + 12, y + 2, x + 16, y, ink);
        }

        static void Rise(PixelCanvas c, int x, int y, Color ink)
        {
            c.Line(x, y, x - 2, y + 4, ink);
            c.Line(x - 2, y + 4, x + 1, y + 8, ink);
            c.Line(x + 1, y + 8, x - 1, y + 11, ink);
        }

        static void Star(PixelCanvas c, int cx, int cy, int reach, Color ink)
        {
            c.ThickLine(cx, cy - reach, cx, cy + reach, ink);
            c.ThickLine(cx - reach, cy, cx + reach, cy, ink);
            var slant = Mathf.Max(3, reach - 2);
            c.Line(cx - slant, cy - slant, cx + slant, cy + slant, ink);
            c.Line(cx + slant, cy - slant, cx - slant, cy + slant, ink);
        }

        static void Bolt(PixelCanvas c, int x, int top, Color ink)
        {
            c.ThickLine(x, top, x + 4, top - 7, ink);
            c.ThickLine(x + 4, top - 7, x - 3, top - 10, ink);
            c.ThickLine(x - 3, top - 10, x + 3, top - 18, ink);
        }

        static void Flake(PixelCanvas c, int cx, int cy, Color ink, Color hi)
        {
            c.ThickLine(cx, cy - 7, cx, cy + 7, ink);
            c.ThickLine(cx - 7, cy, cx + 7, cy, ink);
            c.Line(cx - 5, cy - 5, cx + 5, cy + 5, ink);
            c.Line(cx + 5, cy - 5, cx - 5, cy + 5, ink);
            c.Set(cx, cy, hi);
        }

        static void Tree(PixelCanvas c, int x, int y, int h, Color ink)
        {
            c.FillTriangle(x, y + h + 4, x - h / 2 - 1, y + 3, x + h / 2 + 1, y + 3, ink);
            c.Fill(x - 1, y, 2, 4, Color.Lerp(ink, Color.black, 0.25f));
        }
    }
}
