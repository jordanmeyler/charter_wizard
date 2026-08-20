using UnityEngine;

namespace RuneMagic
{
    public sealed class PixelCanvas
    {
        public int Width { get; }
        public int Height { get; }

        readonly Color[] _pixels;

        public PixelCanvas(int size) : this(size, size)
        {
        }

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new Color[width * height];
        }

        public void Clear(Color color)
        {
            for (var i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = color;
            }
        }

        public void Set(int x, int y, Color color)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            {
                return;
            }

            _pixels[y * Width + x] = color;
        }

        public void Blend(int x, int y, Color color)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            {
                return;
            }

            var i = y * Width + x;
            var under = _pixels[i];
            var a = color.a;
            _pixels[i] = new Color(
                Mathf.Lerp(under.r, color.r, a),
                Mathf.Lerp(under.g, color.g, a),
                Mathf.Lerp(under.b, color.b, a),
                Mathf.Lerp(under.a, 1f, a));
        }

        public void Fill(int x, int y, int width, int height, Color color)
        {
            for (var py = y; py < y + height; py++)
            {
                for (var px = x; px < x + width; px++)
                {
                    Set(px, py, color);
                }
            }
        }

        public void Rect(int x, int y, int width, int height, Color color)
        {
            for (var px = x; px < x + width; px++)
            {
                Set(px, y, color);
                Set(px, y + height - 1, color);
            }

            for (var py = y; py < y + height; py++)
            {
                Set(x, py, color);
                Set(x + width - 1, py, color);
            }
        }

        public void FillCircle(int cx, int cy, int radius, Color color)
        {
            var r2 = radius * radius;
            for (var y = -radius; y <= radius; y++)
            {
                for (var x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= r2)
                    {
                        Set(cx + x, cy + y, color);
                    }
                }
            }
        }

        public void Circle(int cx, int cy, int radius, Color color)
        {
            var r2 = radius * radius;
            var inner = (radius - 1) * (radius - 1);
            for (var y = -radius; y <= radius; y++)
            {
                for (var x = -radius; x <= radius; x++)
                {
                    var d = x * x + y * y;
                    if (d <= r2 && d >= inner)
                    {
                        Set(cx + x, cy + y, color);
                    }
                }
            }
        }

        public void Line(int x0, int y0, int x1, int y1, Color color)
        {
            var dx = Mathf.Abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;
            var dy = -Mathf.Abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;
            var err = dx + dy;

            while (true)
            {
                Set(x0, y0, color);
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                var e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void ThickLine(int x0, int y0, int x1, int y1, Color color)
        {
            Line(x0, y0, x1, y1, color);
            Line(x0 + 1, y0, x1 + 1, y1, color);
            Line(x0, y0 + 1, x1, y1 + 1, color);
        }

        public Sprite ToSprite(float pixelsPerUnit = 0f)
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(_pixels);
            texture.Apply();
            var ppu = pixelsPerUnit > 0f ? pixelsPerUnit : Mathf.Max(Width, Height);
            return Sprite.Create(texture, new Rect(0f, 0f, Width, Height), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
