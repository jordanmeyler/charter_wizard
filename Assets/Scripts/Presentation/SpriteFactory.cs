using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> Cache = new();

        static readonly Color Clear = new(0f, 0f, 0f, 0f);

        public static Sprite Circle(Color color, int size = 48)
        {
            return Memo($"circle:{color}:{size}", () =>
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

                var mid = (size - 1) * 0.5f;
                var radius = mid - 1f;
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var dx = x - mid;
                        var dy = y - mid;
                        var distance = Mathf.Sqrt(dx * dx + dy * dy);
                        var alpha = Mathf.Clamp01(radius - distance);
                        var rim = Mathf.Clamp01(1.6f - Mathf.Abs(distance - radius * 0.82f));
                        var pixel = Color.Lerp(color, Color.white, rim * 0.25f);
                        pixel.a = alpha;
                        texture.SetPixel(x, y, pixel);
                    }
                }

                texture.Apply();
                return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            });
        }

        public static Sprite Square(Color color, int size = 32)
        {
            return Memo($"square:{color}:{size}", () =>
            {
                var canvas = new PixelCanvas(size);
                canvas.Clear(color);
                canvas.Rect(0, 0, size, size, Color.Lerp(color, Color.black, 0.35f));
                return canvas.ToSprite();
            });
        }

        public static Sprite Floor(RuneId element)
        {
            return Memo($"floor:{element}", () =>
            {
                var canvas = new PixelCanvas(32);
                var grout = new Color(0.1f, 0.1f, 0.12f);
                var stone = FloorColor(element);
                canvas.Clear(grout);

                for (var row = 0; row < 4; row++)
                {
                    var stagger = (row % 2) * 4;
                    for (var col = -1; col < 4; col++)
                    {
                        var x = col * 8 + stagger;
                        var y = row * 8;
                        var shade = ((row + col) & 1) == 0
                            ? stone
                            : Color.Lerp(stone, Color.black, 0.12f);
                        canvas.Fill(x + 1, y + 1, 7, 7, shade);
                        canvas.Set(x + 2, y + 6, Color.Lerp(shade, Color.white, 0.16f));
                    }
                }

                PaintVein(canvas, RunePalette.Of(element == RuneId.None ? RuneId.Earth : element));
                return canvas.ToSprite(32);
            });
        }

        public static Sprite Wall(RuneId element)
        {
            return Memo($"wall:{element}", () =>
            {
                var canvas = new PixelCanvas(32);
                var mortar = new Color(0.12f, 0.1f, 0.1f);
                var brick = element == RuneId.Plant
                    ? new Color(0.42f, 0.26f, 0.14f)
                    : new Color(0.28f, 0.24f, 0.32f);
                canvas.Clear(mortar);

                for (var row = 0; row < 5; row++)
                {
                    var stagger = (row % 2) * 5;
                    for (var col = -1; col < 5; col++)
                    {
                        var x = col * 10 + stagger;
                        var y = row * 7;
                        var tone = Color.Lerp(brick, Color.black, ((row + col) & 1) * 0.1f);
                        canvas.Fill(x + 1, y + 1, 9, 6, tone);
                    }
                }

                canvas.Fill(0, 28, 32, 4, Color.Lerp(brick, Color.white, 0.12f));
                return canvas.ToSprite(32);
            });
        }

        public static Sprite Pit()
        {
            return Memo("pit", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(new Color(0.12f, 0.08f, 0.06f));
                canvas.FillCircle(16, 16, 14, new Color(0.2f, 0.12f, 0.08f));
                canvas.FillCircle(16, 16, 11, new Color(0.08f, 0.05f, 0.05f));
                canvas.FillCircle(16, 16, 8, new Color(0.03f, 0.02f, 0.03f));
                canvas.FillCircle(16, 16, 4, Color.black);
                canvas.Circle(16, 16, 14, new Color(0.38f, 0.24f, 0.14f));
                canvas.Set(8, 22, new Color(0.45f, 0.3f, 0.16f));
                canvas.Set(24, 10, new Color(0.45f, 0.3f, 0.16f));
                canvas.Line(4, 8, 8, 5, new Color(0.3f, 0.18f, 0.1f));
                canvas.Line(26, 24, 29, 20, new Color(0.3f, 0.18f, 0.1f));
                return canvas.ToSprite(32);
            });
        }

        public static Sprite Bridge()
        {
            return Memo("bridge", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(new Color(0.22f, 0.16f, 0.1f));
                var stone = new Color(0.5f, 0.36f, 0.2f);
                var dark = new Color(0.32f, 0.22f, 0.12f);
                for (var i = 0; i < 6; i++)
                {
                    var y = 2 + i * 5;
                    var offset = (i % 2) * 3;
                    canvas.Fill(1 + offset, y, 13, 4, i % 2 == 0 ? stone : dark);
                    canvas.Fill(16 + offset, y, 13, 4, i % 2 == 0 ? dark : stone);
                }

                canvas.Rect(0, 0, 32, 32, new Color(0.18f, 0.12f, 0.08f));
                return canvas.ToSprite(32);
            });
        }

        public static Sprite Door(bool open)
        {
            return Memo($"door:{open}", () =>
            {
                var canvas = new PixelCanvas(32);
                var frame = new Color(0.2f, 0.16f, 0.14f);
                canvas.Clear(frame);
                if (open)
                {
                    canvas.Fill(6, 2, 20, 28, new Color(0.04f, 0.04f, 0.06f));
                    canvas.Rect(6, 2, 20, 28, new Color(0.35f, 0.22f, 0.12f));
                    canvas.Fill(22, 4, 6, 24, new Color(0.4f, 0.24f, 0.12f));
                }
                else
                {
                    var wood = new Color(0.46f, 0.28f, 0.14f);
                    canvas.Fill(5, 2, 22, 28, wood);
                    canvas.Fill(5, 8, 22, 3, new Color(0.22f, 0.18f, 0.16f));
                    canvas.Fill(5, 20, 22, 3, new Color(0.22f, 0.18f, 0.16f));
                    canvas.Line(16, 2, 16, 29, new Color(0.28f, 0.16f, 0.08f));
                    canvas.FillCircle(22, 16, 2, new Color(0.75f, 0.62f, 0.28f));
                    canvas.Rect(5, 2, 22, 28, new Color(0.18f, 0.1f, 0.06f));
                }

                return canvas.ToSprite(32);
            });
        }

        public static Sprite Adept()
        {
            return Memo("adept", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var cloak = new Color(0.42f, 0.3f, 0.68f);
                var hood = new Color(0.28f, 0.18f, 0.48f);
                var lining = new Color(0.62f, 0.48f, 0.86f);
                var skin = new Color(0.93f, 0.8f, 0.68f);
                var staff = new Color(0.38f, 0.24f, 0.12f);

                canvas.Fill(18, 6, 12, 16, cloak);
                canvas.Fill(14, 8, 20, 12, cloak);
                canvas.Fill(16, 4, 16, 6, hood);
                canvas.FillCircle(24, 22, 7, hood);
                canvas.FillCircle(24, 21, 4, skin);
                canvas.Set(23, 22, new Color(0.2f, 0.12f, 0.16f));
                canvas.Set(26, 22, new Color(0.2f, 0.12f, 0.16f));
                canvas.Fill(12, 10, 6, 4, lining);
                canvas.Fill(30, 10, 6, 4, lining);
                canvas.Fill(36, 8, 3, 22, staff);
                canvas.FillCircle(37, 31, 3, new Color(0.75f, 0.55f, 0.95f));
                canvas.Fill(17, 4, 5, 8, cloak);
                canvas.Fill(26, 4, 5, 8, cloak);
                return canvas.ToSprite(40);
            });
        }

        public static Sprite AshMite()
        {
            return Memo("ash-mite", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var body = new Color(0.86f, 0.28f, 0.1f);
                var ember = new Color(0.98f, 0.72f, 0.2f);
                var leg = new Color(0.18f, 0.08f, 0.06f);

                canvas.ThickLine(8, 16, 14, 22, leg);
                canvas.ThickLine(8, 24, 14, 24, leg);
                canvas.ThickLine(8, 32, 14, 26, leg);
                canvas.ThickLine(40, 16, 34, 22, leg);
                canvas.ThickLine(40, 24, 34, 24, leg);
                canvas.ThickLine(40, 32, 34, 26, leg);

                canvas.FillCircle(24, 24, 11, body);
                canvas.FillCircle(24, 26, 6, ember);
                canvas.FillCircle(20, 28, 2, Color.white);
                canvas.FillCircle(28, 28, 2, Color.white);
                canvas.Set(20, 28, Color.black);
                canvas.Set(28, 28, Color.black);
                canvas.Line(18, 20, 16, 17, ember);
                canvas.Line(30, 20, 32, 17, ember);
                return canvas.ToSprite(36);
            });
        }

        public static Sprite Torch(bool lit)
        {
            return Memo($"torch:{lit}", () =>
            {
                var canvas = new PixelCanvas(32, 48);
                canvas.Clear(Clear);
                var pole = new Color(0.4f, 0.24f, 0.12f);
                canvas.Fill(14, 4, 4, 22, pole);
                canvas.Fill(11, 24, 10, 6, new Color(0.28f, 0.22f, 0.2f));
                canvas.Rect(11, 24, 10, 6, new Color(0.16f, 0.12f, 0.1f));
                if (lit)
                {
                    canvas.FillCircle(16, 36, 7, new Color(0.95f, 0.45f, 0.1f, 0.95f));
                    canvas.FillCircle(16, 38, 5, new Color(1f, 0.78f, 0.2f));
                    canvas.FillCircle(16, 40, 2, new Color(1f, 0.95f, 0.7f));
                }
                else
                {
                    canvas.Fill(13, 30, 6, 4, new Color(0.16f, 0.12f, 0.1f));
                    canvas.Line(15, 34, 15, 38, new Color(0.28f, 0.2f, 0.14f));
                }

                return canvas.ToSprite(32);
            });
        }

        public static Sprite LightningRod(bool charged)
        {
            return Memo($"rod:{charged}", () =>
            {
                var canvas = new PixelCanvas(32, 48);
                canvas.Clear(Clear);
                var metal = new Color(0.62f, 0.64f, 0.68f);
                var copper = new Color(0.78f, 0.46f, 0.18f);
                canvas.Fill(10, 4, 12, 4, new Color(0.28f, 0.24f, 0.22f));
                canvas.Fill(14, 8, 4, 22, metal);
                canvas.Rect(12, 18, 8, 10, copper);
                canvas.Fill(13, 19, 6, 8, new Color(0.9f, 0.58f, 0.22f));
                canvas.FillCircle(16, 34, 5, charged ? new Color(0.98f, 0.9f, 0.35f) : metal);
                if (charged)
                {
                    var bolt = new Color(0.85f, 0.95f, 1f);
                    canvas.ThickLine(16, 40, 10, 44, bolt);
                    canvas.ThickLine(10, 44, 20, 46, bolt);
                    canvas.ThickLine(20, 46, 14, 47, bolt);
                }

                return canvas.ToSprite(32);
            });
        }

        public static Sprite Charm()
        {
            return Memo("charm", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var gold = new Color(0.9f, 0.62f, 0.18f);
                var core = new Color(1f, 0.42f, 0.12f);
                canvas.FillCircle(16, 16, 10, gold);
                canvas.FillCircle(16, 16, 7, core);
                canvas.Fill(15, 24, 2, 6, gold);
                canvas.FillCircle(16, 16, 3, new Color(1f, 0.86f, 0.4f));
                return canvas.ToSprite(28);
            });
        }

        public static Sprite Plaque()
        {
            return Memo("plaque", () =>
            {
                var canvas = new PixelCanvas(32, 16);
                canvas.Clear(Clear);
                canvas.Fill(1, 1, 30, 14, new Color(0.42f, 0.28f, 0.14f));
                canvas.Rect(1, 1, 30, 14, new Color(0.22f, 0.14f, 0.08f));
                canvas.Fill(3, 3, 26, 3, new Color(0.32f, 0.2f, 0.1f));
                return canvas.ToSprite(24);
            });
        }

        static Color FloorColor(RuneId element)
        {
            switch (element)
            {
                case RuneId.Fire: return new Color(0.42f, 0.22f, 0.16f);
                case RuneId.Plant: return new Color(0.46f, 0.3f, 0.16f);
                case RuneId.Spark: return new Color(0.36f, 0.36f, 0.28f);
                case RuneId.Air: return new Color(0.3f, 0.34f, 0.38f);
                case RuneId.Water: return new Color(0.2f, 0.28f, 0.4f);
                default: return new Color(0.28f, 0.28f, 0.32f);
            }
        }

        static void PaintVein(PixelCanvas canvas, Color vein)
        {
            vein.a = 0.55f;
            canvas.Blend(4, 18, vein);
            canvas.Blend(5, 19, vein);
            canvas.Blend(6, 20, vein);
            canvas.Blend(22, 8, vein);
            canvas.Blend(23, 9, vein);
            canvas.Blend(14, 27, vein);
        }

        static Sprite Memo(string key, System.Func<Sprite> build)
        {
            if (Cache.TryGetValue(key, out var sprite))
            {
                return sprite;
            }

            sprite = build();
            Cache[key] = sprite;
            return sprite;
        }
    }
}
