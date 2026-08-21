using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Abstract marks for Play sight. Not letters, not names.
    /// The eleven use old work-signs; the rest are distinct strokes
    /// so a join never looks like a root.
    /// </summary>
    public static class RuneMark
    {
        const int Size = 48;

        static readonly Dictionary<int, Texture2D> Cache = new();
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
                    Triangle(canvas, c, 8, 8, 38, 40, 38, ink, true);
                    break;
                case RuneId.Air:
                    Triangle(canvas, c, 8, 8, 38, 40, 38, ink, false);
                    canvas.ThickLine(12, 24, 36, 24, ink);
                    break;
                case RuneId.Earth:
                    Triangle(canvas, c, 40, 8, 10, 40, 10, ink, false);
                    canvas.ThickLine(12, 24, 36, 24, ink);
                    break;
                case RuneId.Water:
                    Triangle(canvas, c, 40, 8, 10, 40, 10, ink, true);
                    break;
                case RuneId.Salt:
                    canvas.Circle(c, c, 13, ink);
                    canvas.ThickLine(11, c, 37, c, ink);
                    break;
                case RuneId.Mercury:
                    canvas.Circle(c, 18, 9, ink);
                    canvas.ThickLine(c, 27, c, 40, ink);
                    canvas.ThickLine(16, 36, 32, 36, ink);
                    canvas.ThickLine(16, 8, c, 16, ink);
                    canvas.ThickLine(32, 8, c, 16, ink);
                    break;
                case RuneId.Sulphur:
                    Triangle(canvas, c, 8, 10, 28, 38, 28, ink, false);
                    canvas.ThickLine(c, 28, c, 42, ink);
                    canvas.ThickLine(16, 36, 32, 36, ink);
                    break;
                case RuneId.Vita:
                    canvas.Circle(c, 28, 10, ink);
                    canvas.ThickLine(c, 18, c, 6, ink);
                    canvas.ThickLine(c, 10, 14, 16, ink);
                    canvas.ThickLine(c, 10, 34, 16, ink);
                    break;
                case RuneId.Mors:
                    canvas.Circle(c, 16, 10, ink);
                    canvas.ThickLine(c, 26, c, 42, ink);
                    canvas.ThickLine(14, 36, 34, 36, ink);
                    break;
                case RuneId.Lumen:
                    canvas.Circle(c, c, 6, ink);
                    for (var i = 0; i < 8; i++)
                    {
                        var a = i * Mathf.PI * 0.25f;
                        var x0 = c + Mathf.RoundToInt(Mathf.Cos(a) * 10f);
                        var y0 = c + Mathf.RoundToInt(Mathf.Sin(a) * 10f);
                        var x1 = c + Mathf.RoundToInt(Mathf.Cos(a) * 20f);
                        var y1 = c + Mathf.RoundToInt(Mathf.Sin(a) * 20f);
                        canvas.ThickLine(x0, y0, x1, y1, ink);
                    }

                    break;
                case RuneId.Umbra:
                    canvas.FillCircle(c, c, 13, ink);
                    canvas.FillCircle(c + 5, c - 2, 8, Clear);
                    canvas.Circle(c, c, 15, ink);
                    break;
                case RuneId.Spark:
                    Triangle(canvas, c, 10, 14, 30, 34, 30, ink, true);
                    canvas.ThickLine(c, 30, 18, 40, ink);
                    canvas.ThickLine(18, 40, 30, 42, ink);
                    break;
                case RuneId.Lightning:
                    canvas.ThickLine(20, 8, 30, 8, ink);
                    canvas.ThickLine(30, 8, 18, 24, ink);
                    canvas.ThickLine(18, 24, 30, 24, ink);
                    canvas.ThickLine(30, 24, 16, 42, ink);
                    break;
                case RuneId.Cloud:
                    canvas.Circle(16, 26, 8, ink);
                    canvas.Circle(24, 20, 9, ink);
                    canvas.Circle(34, 26, 8, ink);
                    canvas.ThickLine(12, 32, 38, 32, ink);
                    break;
                case RuneId.Plant:
                    canvas.ThickLine(c, 40, c, 16, ink);
                    canvas.ThickLine(c, 22, 12, 14, ink);
                    canvas.ThickLine(c, 22, 36, 14, ink);
                    canvas.Circle(c, 12, 4, ink);
                    break;
                case RuneId.Flame:
                    Triangle(canvas, c, 10, 10, 36, 38, 36, ink, false);
                    canvas.ThickLine(14, 40, 34, 40, ink);
                    break;
                case RuneId.Ice:
                    canvas.ThickLine(c, 8, c, 40, ink);
                    canvas.ThickLine(10, c, 38, c, ink);
                    canvas.ThickLine(14, 14, 34, 34, ink);
                    canvas.ThickLine(34, 14, 14, 34, ink);
                    break;
                case RuneId.Stone:
                    canvas.Rect(12, 14, 24, 22, ink);
                    canvas.ThickLine(12, 14, 24, 8, ink);
                    canvas.ThickLine(36, 14, 24, 8, ink);
                    break;
                case RuneId.Ash:
                    canvas.Circle(c, c, 11, ink);
                    canvas.ThickLine(14, 14, 34, 34, ink);
                    canvas.ThickLine(34, 14, 14, 34, ink);
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
            canvas.Circle(c, c, 12, ink);
            var ticks = 3 + (id % 5);
            for (var i = 0; i < ticks; i++)
            {
                var a = (id * 0.7f + i * (Mathf.PI * 2f / ticks));
                var x0 = c + Mathf.RoundToInt(Mathf.Cos(a) * 12f);
                var y0 = c + Mathf.RoundToInt(Mathf.Sin(a) * 12f);
                var x1 = c + Mathf.RoundToInt(Mathf.Cos(a) * 19f);
                var y1 = c + Mathf.RoundToInt(Mathf.Sin(a) * 19f);
                canvas.ThickLine(x0, y0, x1, y1, ink);
            }

            if ((id & 1) == 0)
            {
                canvas.FillCircle(c, c, 3, ink);
            }
            else
            {
                canvas.ThickLine(c - 5, c, c + 5, c, ink);
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
