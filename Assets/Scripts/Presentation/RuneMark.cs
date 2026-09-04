using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Work-signs for both sights. The eleven basics keep the old
    /// language — triangles, the salt circle, mercury, the sulphur
    /// cross. Joins use their own strokes, the way Lightning is a
    /// bolt and not another triangle, so a wrought rune never reads
    /// as a root.
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
                    Triangle(canvas, c, 6, 10, 26, 38, 26, ink, false);
                    canvas.ThickLine(c, 10, c, 20, ink);
                    canvas.ThickLine(c, 26, c, 43, ink);
                    canvas.ThickLine(13, 36, 35, 36, ink);
                    canvas.ThickLine(13, 36, 13, 40, ink);
                    canvas.ThickLine(35, 36, 35, 40, ink);
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
                    Triangle(canvas, c, 14, 34, c, 14, c, ink, true);
                    Triangle(canvas, c, 34, 34, c, 14, c, ink, true);
                    canvas.ThickLine(c, 14, c, 6, ink);
                    canvas.ThickLine(34, c, 42, c, ink);
                    canvas.ThickLine(c, 34, c, 42, ink);
                    canvas.ThickLine(14, c, 6, c, ink);
                    break;
                case RuneId.Lightning:
                    canvas.ThickLine(18, 7, 32, 7, ink);
                    canvas.ThickLine(32, 7, 16, 23, ink);
                    canvas.ThickLine(16, 23, 30, 23, ink);
                    canvas.ThickLine(30, 23, 14, 42, ink);
                    canvas.ThickLine(14, 42, 22, 42, ink);
                    canvas.ThickLine(22, 32, 34, 40, ink);
                    break;
                case RuneId.Thunder:
                    canvas.ThickLine(20, 6, 30, 6, ink);
                    canvas.ThickLine(30, 6, 18, 18, ink);
                    canvas.ThickLine(18, 18, 28, 18, ink);
                    canvas.ThickLine(28, 18, 22, 28, ink);
                    canvas.ThickLine(c, 28, c, 38, ink);
                    canvas.Fill(8, 38, 32, 5, ink);
                    canvas.ThickLine(10, 32, 6, 26, ink);
                    canvas.ThickLine(38, 32, 42, 26, ink);
                    break;
                case RuneId.Cloud:
                    canvas.Circle(15, 26, 8, ink);
                    canvas.Circle(24, 18, 9, ink);
                    canvas.Circle(33, 26, 8, ink);
                    canvas.ThickLine(10, 33, 38, 33, ink);
                    break;
                case RuneId.Storm:
                    canvas.Circle(15, 16, 7, ink);
                    canvas.Circle(24, 12, 8, ink);
                    canvas.Circle(33, 16, 7, ink);
                    canvas.FillCircle(c, 30, 2, ink);
                    canvas.ThickLine(c, 24, 20, 36, ink);
                    canvas.ThickLine(20, 36, 28, 34, ink);
                    canvas.ThickLine(16, 30, 32, 30, ink);
                    break;
                case RuneId.Plant:
                    canvas.ThickLine(c, 42, c, 18, ink);
                    canvas.ThickLine(c, 26, 12, 16, ink);
                    canvas.ThickLine(c, 26, 36, 16, ink);
                    canvas.Circle(c, 12, 4, ink);
                    canvas.ThickLine(16, 38, 32, 38, ink);
                    break;
                case RuneId.Grove:
                    canvas.ThickLine(14, 42, 14, 22, ink);
                    canvas.ThickLine(c, 42, c, 16, ink);
                    canvas.ThickLine(34, 42, 34, 22, ink);
                    canvas.Circle(14, 18, 3, ink);
                    canvas.Circle(c, 12, 4, ink);
                    canvas.Circle(34, 18, 3, ink);
                    break;
                case RuneId.Flame:
                    canvas.ThickLine(c, 6, 14, 22, ink);
                    canvas.ThickLine(14, 22, 16, 34, ink);
                    canvas.ThickLine(c, 6, 34, 22, ink);
                    canvas.ThickLine(34, 22, 32, 34, ink);
                    canvas.ThickLine(16, 34, 32, 34, ink);
                    canvas.ThickLine(c, 14, 20, 26, ink);
                    canvas.ThickLine(20, 26, c, 30, ink);
                    canvas.Circle(c, 40, 5, ink);
                    canvas.ThickLine(16, 40, 32, 40, ink);
                    break;
                case RuneId.Ice:
                    canvas.ThickLine(c, 8, c, 40, ink);
                    canvas.ThickLine(10, c, 38, c, ink);
                    canvas.ThickLine(14, 14, 34, 34, ink);
                    canvas.ThickLine(34, 14, 14, 34, ink);
                    break;
                case RuneId.Stone:
                    canvas.Rect(12, 16, 24, 20, ink);
                    canvas.ThickLine(12, 16, c, 8, ink);
                    canvas.ThickLine(36, 16, c, 8, ink);
                    canvas.ThickLine(12, 26, 36, 26, ink);
                    break;
                case RuneId.Ash:
                    canvas.Circle(c, c, 11, ink);
                    canvas.ThickLine(14, 14, 34, 34, ink);
                    canvas.ThickLine(34, 14, 14, 34, ink);
                    break;
                case RuneId.Lava:
                    canvas.Fill(8, 34, 32, 8, ink);
                    canvas.ThickLine(10, 34, 14, 18, ink);
                    canvas.ThickLine(14, 18, 20, 28, ink);
                    canvas.ThickLine(20, 28, 26, 12, ink);
                    canvas.ThickLine(26, 12, 32, 24, ink);
                    canvas.ThickLine(32, 24, 38, 16, ink);
                    break;
                case RuneId.Steam:
                    canvas.ThickLine(14, 36, 34, 36, ink);
                    canvas.ThickLine(14, 36, 18, 42, ink);
                    canvas.ThickLine(34, 36, 30, 42, ink);
                    canvas.ThickLine(18, 42, 30, 42, ink);
                    canvas.ThickLine(16, 32, 14, 16, ink);
                    canvas.ThickLine(14, 16, 18, 10, ink);
                    canvas.ThickLine(c, 30, c, 8, ink);
                    canvas.ThickLine(32, 32, 34, 16, ink);
                    canvas.ThickLine(34, 16, 30, 10, ink);
                    break;
                case RuneId.Dust:
                    canvas.FillCircle(14, 16, 2, ink);
                    canvas.FillCircle(28, 12, 2, ink);
                    canvas.FillCircle(36, 20, 2, ink);
                    canvas.FillCircle(18, 28, 2, ink);
                    canvas.FillCircle(32, 32, 2, ink);
                    canvas.FillCircle(12, 36, 2, ink);
                    canvas.ThickLine(20, 18, 26, 16, ink);
                    canvas.ThickLine(30, 24, 38, 28, ink);
                    canvas.ThickLine(16, 34, 24, 38, ink);
                    break;
                case RuneId.Mud:
                    canvas.FillCircle(16, 30, 8, ink);
                    canvas.FillCircle(c, 28, 10, ink);
                    canvas.FillCircle(32, 30, 8, ink);
                    canvas.Fill(10, 34, 28, 8, ink);
                    break;
                case RuneId.Rain:
                    canvas.Circle(16, 16, 6, ink);
                    canvas.Circle(c, 12, 7, ink);
                    canvas.Circle(32, 16, 6, ink);
                    canvas.ThickLine(16, 26, 14, 40, ink);
                    canvas.ThickLine(c, 24, c, 42, ink);
                    canvas.ThickLine(32, 26, 34, 40, ink);
                    break;
                case RuneId.Wind:
                    canvas.ThickLine(8, 14, 28, 14, ink);
                    canvas.ThickLine(28, 14, 36, 10, ink);
                    canvas.ThickLine(8, 24, 32, 24, ink);
                    canvas.ThickLine(32, 24, 42, 18, ink);
                    canvas.ThickLine(8, 34, 26, 34, ink);
                    canvas.ThickLine(26, 34, 34, 30, ink);
                    break;
                case RuneId.Current:
                    canvas.ThickLine(8, 18, 16, 12, ink);
                    canvas.ThickLine(16, 12, 24, 20, ink);
                    canvas.ThickLine(24, 20, 32, 14, ink);
                    canvas.ThickLine(32, 14, 40, 20, ink);
                    canvas.ThickLine(8, 32, 16, 26, ink);
                    canvas.ThickLine(16, 26, 24, 34, ink);
                    canvas.ThickLine(24, 34, 32, 28, ink);
                    canvas.ThickLine(32, 28, 40, 34, ink);
                    break;
                case RuneId.Ember:
                    canvas.FillCircle(c, 20, 7, ink);
                    canvas.Circle(c, 20, 10, ink);
                    canvas.ThickLine(c, 10, 20, 6, ink);
                    canvas.ThickLine(16, 20, 32, 20, ink);
                    canvas.ThickLine(c, 30, c, 42, ink);
                    canvas.ThickLine(16, 38, 32, 38, ink);
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
                    canvas.ThickLine(c, 42, c, 28, ink);
                    canvas.ThickLine(c, 28, 14, 20, ink);
                    canvas.ThickLine(14, 20, 20, 12, ink);
                    canvas.ThickLine(20, 12, 34, 10, ink);
                    canvas.Circle(36, 10, 3, ink);
                    break;
                case RuneId.Crystal:
                    Triangle(canvas, c, 8, 14, 24, 34, 24, ink, false);
                    Triangle(canvas, c, 40, 14, 24, 34, 24, ink, false);
                    canvas.ThickLine(c, 8, c, 40, ink);
                    break;
                case RuneId.Metal:
                    canvas.ThickLine(12, 16, 36, 16, ink);
                    canvas.ThickLine(36, 16, 40, 32, ink);
                    canvas.ThickLine(40, 32, 8, 32, ink);
                    canvas.ThickLine(8, 32, 12, 16, ink);
                    canvas.ThickLine(12, 24, 36, 24, ink);
                    break;
                case RuneId.Obsidian:
                    Triangle(canvas, 16, 6, 40, 18, 10, 42, ink, true);
                    canvas.ThickLine(18, 16, 14, 34, ink);
                    break;
                case RuneId.Acid:
                    canvas.ThickLine(12, 14, 12, 26, ink);
                    canvas.ThickLine(12, 26, 36, 26, ink);
                    canvas.ThickLine(36, 26, 36, 14, ink);
                    canvas.FillCircle(18, 34, 2, ink);
                    canvas.FillCircle(28, 40, 3, ink);
                    canvas.FillCircle(34, 32, 2, ink);
                    break;
                case RuneId.Glass:
                    canvas.Rect(14, 10, 20, 28, ink);
                    canvas.ThickLine(16, 14, 24, 22, ink);
                    canvas.ThickLine(16, 22, 18, 26, ink);
                    break;
                case RuneId.Sand:
                    canvas.ThickLine(8, 36, 20, 20, ink);
                    canvas.ThickLine(20, 20, 28, 28, ink);
                    canvas.ThickLine(28, 28, 40, 16, ink);
                    canvas.Fill(8, 36, 32, 6, ink);
                    canvas.FillCircle(16, 30, 1, ink);
                    canvas.FillCircle(30, 24, 1, ink);
                    break;
                case RuneId.Inferno:
                    canvas.ThickLine(12, 40, 12, 20, ink);
                    canvas.ThickLine(12, 20, 8, 10, ink);
                    canvas.ThickLine(c, 42, c, 8, ink);
                    canvas.ThickLine(c, 8, 18, 16, ink);
                    canvas.ThickLine(c, 8, 30, 16, ink);
                    canvas.ThickLine(36, 40, 36, 20, ink);
                    canvas.ThickLine(36, 20, 40, 10, ink);
                    canvas.ThickLine(8, 40, 40, 40, ink);
                    break;
                case RuneId.Plasma:
                    canvas.Circle(c, c, 12, ink);
                    canvas.ThickLine(20, 10, 30, 10, ink);
                    canvas.ThickLine(30, 10, 16, 24, ink);
                    canvas.ThickLine(16, 24, 32, 24, ink);
                    canvas.ThickLine(32, 24, 18, 38, ink);
                    break;
                case RuneId.Snow:
                    canvas.ThickLine(c, 10, c, 38, ink);
                    canvas.ThickLine(12, 18, 36, 30, ink);
                    canvas.ThickLine(36, 18, 12, 30, ink);
                    canvas.FillCircle(c, 10, 2, ink);
                    canvas.FillCircle(c, 38, 2, ink);
                    canvas.FillCircle(12, 18, 2, ink);
                    canvas.FillCircle(36, 18, 2, ink);
                    canvas.FillCircle(12, 30, 2, ink);
                    canvas.FillCircle(36, 30, 2, ink);
                    break;
                case RuneId.Blizzard:
                    canvas.ThickLine(c, 12, c, 36, ink);
                    canvas.ThickLine(14, 18, 34, 30, ink);
                    canvas.ThickLine(34, 18, 14, 30, ink);
                    canvas.ThickLine(8, 10, 18, 8, ink);
                    canvas.ThickLine(30, 8, 40, 12, ink);
                    break;
                case RuneId.Sandstorm:
                    canvas.FillCircle(14, 18, 2, ink);
                    canvas.FillCircle(26, 14, 2, ink);
                    canvas.FillCircle(36, 20, 2, ink);
                    canvas.FillCircle(18, 30, 2, ink);
                    canvas.ThickLine(8, 16, 22, 12, ink);
                    canvas.ThickLine(10, 26, 28, 22, ink);
                    canvas.ThickLine(16, 36, 40, 28, ink);
                    break;
                case RuneId.Glacier:
                    canvas.ThickLine(c, 8, 8, 36, ink);
                    canvas.ThickLine(c, 8, 40, 36, ink);
                    canvas.ThickLine(8, 36, 40, 36, ink);
                    canvas.ThickLine(c, 8, c, 36, ink);
                    canvas.Fill(8, 36, 32, 5, ink);
                    break;
                case RuneId.Blight:
                case RuneId.Poison:
                    canvas.ThickLine(c, 40, c, 20, ink);
                    canvas.ThickLine(c, 24, 12, 14, ink);
                    canvas.ThickLine(c, 24, 36, 18, ink);
                    canvas.ThickLine(12, 14, 10, 20, ink);
                    canvas.Circle(c, 12, 3, ink);
                    canvas.ThickLine(16, 16, 32, 32, ink);
                    break;
                case RuneId.Oil:
                    canvas.FillCircle(c, 28, 10, ink);
                    canvas.ThickLine(c, 18, c, 8, ink);
                    canvas.ThickLine(16, 36, 32, 36, ink);
                    canvas.FillCircle(18, 14, 2, ink);
                    break;
                case RuneId.Explosion:
                    canvas.FillCircle(c, c, 4, ink);
                    canvas.ThickLine(c, 8, c, 18, ink);
                    canvas.ThickLine(c, 30, c, 40, ink);
                    canvas.ThickLine(8, c, 18, c, ink);
                    canvas.ThickLine(30, c, 40, c, ink);
                    canvas.ThickLine(14, 14, 20, 20, ink);
                    canvas.ThickLine(28, 28, 34, 34, ink);
                    canvas.ThickLine(34, 14, 28, 20, ink);
                    canvas.ThickLine(20, 28, 14, 34, ink);
                    break;
                case RuneId.Miasma:
                    canvas.Circle(16, 18, 7, ink);
                    canvas.Circle(c, 14, 8, ink);
                    canvas.Circle(32, 18, 7, ink);
                    canvas.FillCircle(18, 34, 2, ink);
                    canvas.FillCircle(28, 38, 2, ink);
                    canvas.FillCircle(34, 32, 2, ink);
                    break;
                case RuneId.Hot:
                    canvas.ThickLine(14, 40, 16, 18, ink);
                    canvas.ThickLine(16, 18, 12, 10, ink);
                    canvas.ThickLine(c, 40, c, 8, ink);
                    canvas.ThickLine(34, 40, 32, 18, ink);
                    canvas.ThickLine(32, 18, 36, 10, ink);
                    break;
                case RuneId.Cold:
                    canvas.ThickLine(c, 8, c, 40, ink);
                    canvas.ThickLine(16, 16, 32, 16, ink);
                    canvas.ThickLine(14, 28, 34, 28, ink);
                    canvas.ThickLine(18, 40, 30, 40, ink);
                    break;
                case RuneId.Wet:
                    canvas.Circle(c, 16, 6, ink);
                    canvas.FillCircle(c, 16, 2, ink);
                    canvas.ThickLine(c, 22, 16, 40, ink);
                    canvas.ThickLine(c, 22, 32, 40, ink);
                    canvas.ThickLine(16, 40, 32, 40, ink);
                    break;
                case RuneId.Dry:
                    canvas.Circle(c, c, 13, ink);
                    canvas.ThickLine(16, 16, 32, 32, ink);
                    canvas.ThickLine(20, 32, 28, 20, ink);
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
