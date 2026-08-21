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

        public void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Color color)
        {
            var minX = Mathf.Max(0, Mathf.Min(x0, Mathf.Min(x1, x2)));
            var maxX = Mathf.Min(Width - 1, Mathf.Max(x0, Mathf.Max(x1, x2)));
            var minY = Mathf.Max(0, Mathf.Min(y0, Mathf.Min(y1, y2)));
            var maxY = Mathf.Min(Height - 1, Mathf.Max(y0, Mathf.Max(y1, y2)));
            var area = Edge(x0, y0, x1, y1, x2, y2);
            if (area == 0)
            {
                Line(x0, y0, x1, y1, color);
                Line(x1, y1, x2, y2, color);
                return;
            }

            var sign = area > 0 ? 1 : -1;
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var w0 = Edge(x1, y1, x2, y2, x, y) * sign;
                    var w1 = Edge(x2, y2, x0, y0, x, y) * sign;
                    var w2 = Edge(x0, y0, x1, y1, x, y) * sign;
                    if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                    {
                        Set(x, y, color);
                    }
                }
            }
        }

        static int Edge(int ax, int ay, int bx, int by, int px, int py)
        {
            return (px - ax) * (by - ay) - (py - ay) * (bx - ax);
        }

        public Color Get(int x, int y)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            {
                return new Color(0f, 0f, 0f, 0f);
            }

            return _pixels[y * Width + x];
        }

        public void FillRounded(int x, int y, int width, int height, int radius, Color color)
        {
            var r = Mathf.Max(0, radius);
            for (var py = y; py < y + height; py++)
            {
                for (var px = x; px < x + width; px++)
                {
                    var dx = px < x + r ? x + r - px : px >= x + width - r ? px - (x + width - 1 - r) : 0;
                    var dy = py < y + r ? y + r - py : py >= y + height - r ? py - (y + height - 1 - r) : 0;
                    if (dx * dx + dy * dy <= r * r + r)
                    {
                        Set(px, py, color);
                    }
                }
            }
        }

        public void SoftCircle(int cx, int cy, int radius, Color color)
        {
            var r = Mathf.Max(1, radius);
            for (var y = -r; y <= r; y++)
            {
                for (var x = -r; x <= r; x++)
                {
                    var d = Mathf.Sqrt(x * x + y * y);
                    if (d > r)
                    {
                        continue;
                    }

                    var fade = color;
                    fade.a = color.a * Mathf.Clamp01(1f - d / r);
                    Blend(cx + x, cy + y, fade);
                }
            }
        }

        public void Shade(int x, int y, int width, int height, float amount)
        {
            for (var py = y; py < y + height; py++)
            {
                for (var px = x; px < x + width; px++)
                {
                    var under = Get(px, py);
                    if (under.a <= 0f)
                    {
                        continue;
                    }

                    Set(px, py, Color.Lerp(under, Color.black, amount));
                }
            }
        }

        public void Highlight(int x, int y, int width, int height, float amount)
        {
            for (var py = y; py < y + height; py++)
            {
                for (var px = x; px < x + width; px++)
                {
                    var under = Get(px, py);
                    if (under.a <= 0f)
                    {
                        continue;
                    }

                    Set(px, py, Color.Lerp(under, Color.white, amount));
                }
            }
        }

        public void Noise(HashRng rng, Color color, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Blend(rng.Range(0, Width), rng.Range(0, Height), color);
            }
        }

        public void DitherBand(int y0, int y1, Color color, float density)
        {
            for (var y = y0; y <= y1; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (((x + y) & 1) == 0 && ((x * 13 + y * 7) % 10) / 10f < density)
                    {
                        Blend(x, y, color);
                    }
                }
            }
        }

        public void FillEllipse(int cx, int cy, int rx, int ry, Color color)
        {
            var rx2 = Mathf.Max(1, rx * rx);
            var ry2 = Mathf.Max(1, ry * ry);
            for (var y = -ry; y <= ry; y++)
            {
                for (var x = -rx; x <= rx; x++)
                {
                    if ((x * x) * ry2 + (y * y) * rx2 <= rx2 * ry2)
                    {
                        Set(cx + x, cy + y, color);
                    }
                }
            }
        }

        public void SoftEllipse(int cx, int cy, int rx, int ry, Color color)
        {
            var rxSafe = Mathf.Max(1, rx);
            var rySafe = Mathf.Max(1, ry);
            for (var y = -rySafe; y <= rySafe; y++)
            {
                for (var x = -rxSafe; x <= rxSafe; x++)
                {
                    var nx = x / (float)rxSafe;
                    var ny = y / (float)rySafe;
                    var d = Mathf.Sqrt(nx * nx + ny * ny);
                    if (d > 1f)
                    {
                        continue;
                    }

                    var fade = color;
                    fade.a = color.a * Mathf.Clamp01(1f - d);
                    Blend(cx + x, cy + y, fade);
                }
            }
        }

        public void GroundShadow(int cx, int cy, int rx, int ry)
        {
            SoftEllipse(cx, cy, rx, ry, new Color(0f, 0f, 0f, 0.16f));
        }

        public void Outline(Color color)
        {
            var mark = new bool[_pixels.Length];
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (Get(x, y).a < 0.18f)
                    {
                        continue;
                    }

                    MarkOutline(mark, x - 1, y);
                    MarkOutline(mark, x + 1, y);
                    MarkOutline(mark, x, y + 1);
                    MarkOutline(mark, x, y - 1);
                }
            }

            for (var i = 0; i < mark.Length; i++)
            {
                if (mark[i])
                {
                    _pixels[i] = color;
                }
            }
        }

        void MarkOutline(bool[] mark, int x, int y)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            {
                return;
            }

            if (Get(x, y).a < 0.12f)
            {
                mark[y * Width + x] = true;
            }
        }

        public Texture2D ToTexture()
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(_pixels);
            texture.Apply();
            return texture;
        }

        public Sprite ToSprite(float pixelsPerUnit = 0f, Vector2? pivot = null)
        {
            var texture = ToTexture();
            var ppu = pixelsPerUnit > 0f ? pixelsPerUnit : Mathf.Max(Width, Height);
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, Width, Height),
                pivot ?? new Vector2(0.5f, 0.5f),
                ppu);
        }
    }

    public struct HashRng
    {
        uint _state;

        public HashRng(int seed)
        {
            _state = (uint)seed * 747796405u + 2891336453u;
            if (_state == 0)
            {
                _state = 1;
            }
        }

        public float Value()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (_state & 0xFFFF) / 65535f;
        }

        public int Range(int min, int maxExclusive)
        {
            if (maxExclusive <= min)
            {
                return min;
            }

            return min + (int)(Value() * (maxExclusive - min));
        }

        public Color Jitter(Color color, float amount)
        {
            var delta = (Value() - 0.5f) * amount;
            return new Color(
                Mathf.Clamp01(color.r + delta),
                Mathf.Clamp01(color.g + delta * 0.8f),
                Mathf.Clamp01(color.b + delta * 0.6f),
                color.a);
        }
    }
}
