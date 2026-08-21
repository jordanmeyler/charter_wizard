using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Work-signs for both sights. The four roots stay triangles in the
    /// old language, but each mark is cut with its own notches and
    /// inner strokes so they do not read as the same blank shape.
    /// </summary>
    public static class RuneMark
    {
        const int Size = 48;

        static readonly Dictionary<int, Texture2D> Cache = new();
        static readonly Dictionary<int, Sprite> Sprites = new();
        static readonly Color Clear = new(0f, 0f, 0f, 0f);

        public static Texture2D Of(RuneId rune, Color ink)
        {
            var key = ((int)rune * 397) ^ ColorKey(ink);
            if (Cache.TryGetValue(key, out var texture) && texture != null)
            {
                return texture;
            }

            texture = Draw(rune, ink);
            Cache[key] = texture;
            return texture;
        }

        public static Sprite AsSprite(RuneId rune, Color ink)
        {
            var key = ((int)rune * 397) ^ ColorKey(ink);
            if (Sprites.TryGetValue(key, out var sprite) && sprite != null)
            {
                return sprite;
            }

            var texture = Of(rune, ink);
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                Size);
            Sprites[key] = sprite;
            return sprite;
        }

        public static void DrawGui(Rect rect, RuneId rune, Color ink)
        {
            var texture = Of(rune, ink);
            var size = Mathf.Min(rect.width, rect.height) * 0.78f;
            var x = rect.x + (rect.width - size) * 0.5f;
            var y = rect.y + (rect.height - size) * 0.5f;
            GUI.DrawTexture(new Rect(x, y, size, size), texture, ScaleMode.ScaleToFit, true);
        }

        static int ColorKey(Color color)
        {
            return (Mathf.RoundToInt(color.r * 31f) << 15)
                ^ (Mathf.RoundToInt(color.g * 31f) << 10)
                ^ (Mathf.RoundToInt(color.b * 31f) << 5)
                ^ Mathf.RoundToInt(color.a * 31f);
        }

        static Texture2D Draw(RuneId rune, Color ink)
        {
            var canvas = new PixelCanvas(Size);
            canvas.Clear(Clear);
            Stroke(canvas, rune, ink);
            return canvas.ToTexture();
        }

        static void Stroke(PixelCanvas canvas, RuneId rune, Color ink)
        {
            const int c = 24;
            switch (rune)
            {
                case RuneId.Fire:
                    Triangle(canvas, c, 6, 6, 41, 42, 41, ink, true);
                    Triangle(canvas, c, 16, 14, 34, 34, 34, ink, false);
                    canvas.ThickLine(c, 6, c, 14, ink);
                    canvas.ThickLine(16, 41, 32, 41, ink);
                    canvas.ThickLine(14, 28, 20, 18, ink);
                    canvas.ThickLine(20, 18, 26, 26, ink);
                    canvas.ThickLine(26, 26, 30, 16, ink);
                    canvas.FillCircle(c, 22, 2, ink);
                    break;
                case RuneId.Air:
                    Triangle(canvas, c, 7, 8, 39, 40, 39, ink, false);
                    canvas.ThickLine(11, 23, 18, 23, ink);
                    canvas.ThickLine(30, 23, 37, 23, ink);
                    canvas.ThickLine(14, 19, 14, 15, ink);
                    canvas.ThickLine(34, 19, 34, 15, ink);
                    break;
                case RuneId.Earth:
                    Triangle(canvas, c, 41, 8, 10, 40, 10, ink, false);
                    canvas.ThickLine(12, 24, 36, 24, ink);
                    canvas.Fill(10, 38, 28, 5, ink);
                    canvas.ThickLine(8, 10, 12, 10, ink);
                    canvas.ThickLine(36, 10, 40, 10, ink);
                    break;
                case RuneId.Water:
                    Triangle(canvas, c, 41, 6, 8, 42, 8, ink, true);
                    Triangle(canvas, c, 32, 14, 16, 34, 16, ink, false);
                    canvas.ThickLine(c, 41, c, 34, ink);
                    canvas.ThickLine(14, 12, 34, 12, ink);
                    canvas.ThickLine(12, 22, 20, 18, ink);
                    canvas.ThickLine(20, 18, 28, 24, ink);
                    canvas.ThickLine(28, 24, 36, 20, ink);
                    canvas.Circle(c, 28, 3, ink);
                    break;
                case RuneId.Salt:
                    canvas.Circle(c, c, 14, ink);
                    canvas.Circle(c, c, 11, ink);
                    canvas.ThickLine(8, c, 40, c, ink);
                    canvas.ThickLine(c, 20, c, 28, ink);
                    break;
                case RuneId.Mercury:
                    canvas.Circle(c, 20, 9, ink);
                    canvas.Circle(c, 20, 5, ink);
                    canvas.ThickLine(c, 29, c, 42, ink);
                    canvas.ThickLine(15, 37, 33, 37, ink);
                    canvas.ThickLine(14, 8, c, 18, ink);
                    canvas.ThickLine(34, 8, c, 18, ink);
                    canvas.ThickLine(14, 8, 18, 8, ink);
                    canvas.ThickLine(30, 8, 34, 8, ink);
                    break;
                case RuneId.Sulphur:
                    Triangle(canvas, c, 6, 10, 26, 38, 26, ink, true);
                    Triangle(canvas, c, 13, 16, 22, 32, 22, ink, false);
                    canvas.ThickLine(c, 26, c, 43, ink);
                    canvas.ThickLine(15, 36, 33, 36, ink);
                    break;
                case RuneId.Vita:
                    canvas.Circle(c, 16, 9, ink);
                    canvas.Circle(c, 16, 4, ink);
                    canvas.ThickLine(c, 25, c, 42, ink);
                    canvas.ThickLine(14, 32, 34, 32, ink);
                    canvas.ThickLine(14, 32, 14, 36, ink);
                    canvas.ThickLine(34, 32, 34, 36, ink);
                    canvas.ThickLine(c, 7, c, 3, ink);
                    canvas.FillCircle(c, 16, 2, ink);
                    break;
                case RuneId.Mors:
                    canvas.Circle(c, 16, 10, ink);
                    canvas.ThickLine(16, 16, 32, 16, ink);
                    canvas.ThickLine(c, 26, c, 43, ink);
                    canvas.ThickLine(13, 37, 35, 37, ink);
                    canvas.ThickLine(13, 37, 13, 41, ink);
                    canvas.ThickLine(35, 37, 35, 41, ink);
                    break;
                case RuneId.Lumen:
                    canvas.FillCircle(c, c, 5, ink);
                    canvas.Circle(c, c, 8, ink);
                    for (var i = 0; i < 8; i++)
                    {
                        var a = i * Mathf.PI * 0.25f;
                        var inner = (i & 1) == 0 ? 11f : 10f;
                        var outer = (i & 1) == 0 ? 21f : 16f;
                        var x0 = c + Mathf.RoundToInt(Mathf.Cos(a) * inner);
                        var y0 = c + Mathf.RoundToInt(Mathf.Sin(a) * inner);
                        var x1 = c + Mathf.RoundToInt(Mathf.Cos(a) * outer);
                        var y1 = c + Mathf.RoundToInt(Mathf.Sin(a) * outer);
                        canvas.ThickLine(x0, y0, x1, y1, ink);
                    }

                    break;
                case RuneId.Umbra:
                    canvas.Circle(c, c, 15, ink);
                    canvas.FillCircle(c, c, 12, ink);
                    canvas.FillCircle(c + 6, c - 2, 8, Clear);
                    canvas.Circle(c + 6, c - 2, 8, ink);
                    break;
                case RuneId.Spark:
                    Triangle(canvas, c, 10, 12, 32, 36, 32, ink, true);
                    canvas.ThickLine(14, 22, 34, 22, ink);
                    canvas.ThickLine(c, 32, 18, 42, ink);
                    canvas.ThickLine(18, 42, 30, 40, ink);
                    break;
                case RuneId.Lightning:
                    canvas.ThickLine(18, 7, 32, 7, ink);
                    canvas.ThickLine(32, 7, 16, 23, ink);
                    canvas.ThickLine(16, 23, 30, 23, ink);
                    canvas.ThickLine(30, 23, 14, 42, ink);
                    canvas.ThickLine(14, 42, 22, 42, ink);
                    break;
                case RuneId.Thunder:
                    canvas.ThickLine(18, 8, 32, 8, ink);
                    canvas.ThickLine(32, 8, 16, 22, ink);
                    canvas.ThickLine(16, 22, 30, 22, ink);
                    canvas.ThickLine(30, 22, 16, 36, ink);
                    canvas.Fill(10, 40, 28, 4, ink);
                    break;
                case RuneId.Cloud:
                    Triangle(canvas, c, 12, 10, 22, 38, 22, ink, false);
                    canvas.ThickLine(14, 22, 20, 22, ink);
                    canvas.ThickLine(28, 22, 34, 22, ink);
                    Triangle(canvas, c, 42, 12, 28, 36, 28, ink, false);
                    break;
                case RuneId.Storm:
                    Triangle(canvas, c, 10, 10, 20, 38, 20, ink, false);
                    canvas.ThickLine(14, 20, 34, 20, ink);
                    Triangle(canvas, c, 40, 12, 26, 36, 26, ink, false);
                    canvas.ThickLine(c, 26, 18, 38, ink);
                    canvas.ThickLine(18, 38, 28, 36, ink);
                    break;
                case RuneId.Plant:
                    Triangle(canvas, c, 40, 10, 20, 38, 20, ink, false);
                    canvas.ThickLine(14, 28, 34, 28, ink);
                    canvas.ThickLine(c, 20, c, 8, ink);
                    canvas.Circle(c, 8, 3, ink);
                    break;
                case RuneId.Grove:
                    Triangle(canvas, c, 40, 10, 22, 38, 22, ink, false);
                    canvas.ThickLine(14, 28, 34, 28, ink);
                    canvas.Circle(c, 12, 6, ink);
                    canvas.ThickLine(c, 18, c, 8, ink);
                    break;
                case RuneId.Flame:
                    Triangle(canvas, c, 8, 10, 36, 38, 36, ink, true);
                    canvas.Circle(c, 38, 8, ink);
                    canvas.ThickLine(14, 38, 34, 38, ink);
                    break;
                case RuneId.Ice:
                    Triangle(canvas, c, 40, 10, 12, 38, 12, ink, false);
                    canvas.ThickLine(c, 12, c, 40, ink);
                    canvas.ThickLine(14, 26, 34, 26, ink);
                    canvas.ThickLine(16, 18, 32, 34, ink);
                    break;
                case RuneId.Stone:
                    Triangle(canvas, c, 40, 8, 12, 40, 12, ink, false);
                    canvas.ThickLine(12, 24, 36, 24, ink);
                    canvas.Rect(14, 28, 20, 10, ink);
                    break;
                case RuneId.Ash:
                    Triangle(canvas, c, 8, 12, 22, 36, 22, ink, false);
                    canvas.Circle(c, 30, 8, ink);
                    canvas.ThickLine(16, 24, 32, 36, ink);
                    canvas.ThickLine(32, 24, 16, 36, ink);
                    break;
                case RuneId.Lava:
                    Triangle(canvas, c, 8, 10, 22, 38, 22, ink, true);
                    Triangle(canvas, c, 42, 10, 26, 38, 26, ink, false);
                    canvas.ThickLine(14, 32, 34, 32, ink);
                    break;
                case RuneId.Steam:
                    Triangle(canvas, c, 10, 10, 22, 38, 22, ink, true);
                    Triangle(canvas, c, 42, 10, 26, 38, 26, ink, false);
                    canvas.ThickLine(c, 18, c, 30, ink);
                    break;
                case RuneId.Dust:
                    Triangle(canvas, c, 8, 10, 22, 38, 22, ink, false);
                    canvas.ThickLine(14, 22, 20, 22, ink);
                    canvas.ThickLine(28, 22, 34, 22, ink);
                    Triangle(canvas, c, 42, 10, 26, 38, 26, ink, false);
                    canvas.ThickLine(14, 32, 34, 32, ink);
                    break;
                case RuneId.Mud:
                    Triangle(canvas, c, 40, 8, 14, 40, 14, ink, true);
                    canvas.ThickLine(12, 26, 36, 26, ink);
                    canvas.Fill(10, 36, 28, 5, ink);
                    break;
                case RuneId.Rain:
                    Triangle(canvas, c, 14, 10, 20, 38, 20, ink, false);
                    canvas.ThickLine(14, 20, 20, 20, ink);
                    canvas.ThickLine(28, 20, 34, 20, ink);
                    Triangle(canvas, c, 36, 12, 24, 36, 24, ink, true);
                    canvas.ThickLine(c, 36, c, 42, ink);
                    break;
                case RuneId.Wind:
                    Triangle(canvas, c, 8, 10, 22, 38, 22, ink, false);
                    canvas.ThickLine(12, 22, 18, 22, ink);
                    canvas.ThickLine(30, 22, 36, 22, ink);
                    canvas.ThickLine(c, 22, c, 40, ink);
                    canvas.ThickLine(16, 36, 32, 36, ink);
                    canvas.ThickLine(16, 8, c, 18, ink);
                    canvas.ThickLine(32, 8, c, 18, ink);
                    break;
                case RuneId.Current:
                    Triangle(canvas, c, 40, 8, 12, 40, 12, ink, true);
                    canvas.ThickLine(c, 12, c, 40, ink);
                    canvas.ThickLine(16, 36, 32, 36, ink);
                    canvas.ThickLine(16, 8, c, 16, ink);
                    canvas.ThickLine(32, 8, c, 16, ink);
                    break;
                case RuneId.Ember:
                    Triangle(canvas, c, 8, 10, 34, 38, 34, ink, true);
                    canvas.Circle(c, 16, 8, ink);
                    canvas.ThickLine(16, 16, 32, 16, ink);
                    canvas.ThickLine(c, 24, c, 40, ink);
                    break;
                case RuneId.Shade:
                    canvas.FillCircle(c, c, 12, ink);
                    canvas.FillCircle(c + 6, c - 2, 8, Clear);
                    canvas.Circle(c, c, 14, ink);
                    canvas.ThickLine(14, 38, 34, 38, ink);
                    canvas.Circle(c, 38, 6, ink);
                    break;
                case RuneId.Forest:
                    Triangle(canvas, 16, 36, 8, 18, 24, 18, ink, false);
                    Triangle(canvas, 32, 40, 22, 16, 42, 16, ink, false);
                    canvas.ThickLine(16, 18, 16, 12, ink);
                    canvas.ThickLine(32, 16, 32, 10, ink);
                    canvas.ThickLine(12, 28, 20, 28, ink);
                    canvas.ThickLine(26, 30, 38, 30, ink);
                    break;
                case RuneId.Vine:
                    Triangle(canvas, c, 40, 12, 22, 36, 22, ink, false);
                    canvas.ThickLine(c, 22, 14, 10, ink);
                    canvas.ThickLine(14, 10, 34, 8, ink);
                    canvas.Circle(34, 8, 3, ink);
                    break;
                case RuneId.Crystal:
                    Triangle(canvas, c, 8, 14, 24, 34, 24, ink, false);
                    Triangle(canvas, c, 40, 14, 24, 34, 24, ink, false);
                    canvas.ThickLine(c, 8, c, 40, ink);
                    break;
                case RuneId.Metal:
                    Triangle(canvas, c, 8, 10, 24, 38, 24, ink, false);
                    canvas.ThickLine(10, 24, 38, 24, ink);
                    canvas.ThickLine(c, 24, c, 40, ink);
                    canvas.ThickLine(16, 36, 32, 36, ink);
                    break;
                case RuneId.Obsidian:
                    Triangle(canvas, c, 8, 10, 22, 38, 22, ink, true);
                    Triangle(canvas, c, 40, 12, 24, 36, 24, ink, false);
                    canvas.ThickLine(c, 8, c, 40, ink);
                    break;
                case RuneId.Acid:
                    Triangle(canvas, c, 10, 12, 22, 36, 22, ink, true);
                    Triangle(canvas, c, 40, 12, 26, 36, 26, ink, true);
                    canvas.ThickLine(16, 24, 32, 24, ink);
                    canvas.ThickLine(c, 24, c, 40, ink);
                    break;
                case RuneId.Aether:
                    canvas.Circle(c, c, 14, ink);
                    Triangle(canvas, c, 12, 16, 22, 32, 22, ink, false);
                    Triangle(canvas, c, 36, 16, 26, 32, 26, ink, false);
                    break;
                case RuneId.Animus:
                case RuneId.Male:
                    canvas.Circle(18, 18, 8, ink);
                    canvas.ThickLine(24, 24, 38, 38, ink);
                    canvas.ThickLine(38, 38, 30, 38, ink);
                    canvas.ThickLine(38, 38, 38, 30, ink);
                    break;
                case RuneId.Anima:
                case RuneId.Female:
                    canvas.Circle(c, 18, 9, ink);
                    canvas.ThickLine(c, 27, c, 42, ink);
                    canvas.ThickLine(16, 36, 32, 36, ink);
                    break;
                default:
                    Fallback(canvas, rune, ink);
                    break;
            }
        }

        static void Fallback(PixelCanvas canvas, RuneId rune, Color ink)
        {
            const int c = 24;
            var id = (int)rune;
            canvas.Circle(c, c, 13, ink);
            var ticks = 3 + (id % 5);
            for (var i = 0; i < ticks; i++)
            {
                var a = id * 0.7f + i * (Mathf.PI * 2f / ticks);
                var x0 = c + Mathf.RoundToInt(Mathf.Cos(a) * 13f);
                var y0 = c + Mathf.RoundToInt(Mathf.Sin(a) * 13f);
                var x1 = c + Mathf.RoundToInt(Mathf.Cos(a) * 20f);
                var y1 = c + Mathf.RoundToInt(Mathf.Sin(a) * 20f);
                canvas.ThickLine(x0, y0, x1, y1, ink);
            }

            if ((id & 1) == 0)
            {
                canvas.FillCircle(c, c, 3, ink);
            }
            else
            {
                canvas.ThickLine(c - 6, c, c + 6, c, ink);
            }
        }

        static void Triangle(
            PixelCanvas canvas,
            int x0, int y0,
            int x1, int y1,
            int x2, int y2,
            Color ink,
            bool fill)
        {
            canvas.ThickLine(x0, y0, x1, y1, ink);
            canvas.ThickLine(x1, y1, x2, y2, ink);
            canvas.ThickLine(x2, y2, x0, y0, ink);
            if (!fill)
            {
                return;
            }

            var minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            var maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            var minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            var maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (Inside(x, y, x0, y0, x1, y1, x2, y2))
                    {
                        canvas.Set(x, y, ink);
                    }
                }
            }
        }

        static bool Inside(int x, int y, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            var d = (x1 - x0) * (y2 - y0) - (x2 - x0) * (y1 - y0);
            if (d == 0)
            {
                return false;
            }

            var a = ((x1 - x) * (y2 - y) - (x2 - x) * (y1 - y)) / (float)d;
            var b = ((x2 - x) * (y0 - y) - (x0 - x) * (y2 - y)) / (float)d;
            var c = 1f - a - b;
            return a >= 0f && b >= 0f && c >= 0f;
        }
    }
}
