using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> Cache = new();

        static readonly Color Clear = new(0f, 0f, 0f, 0f);

        public static Sprite Named(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Square(new Color(0.4f, 0.38f, 0.42f));
            }

            if (CatalogBook.TrySprite(id, out var custom) && custom != null)
            {
                return custom;
            }

            var painted = SpriteActors.Still(id);
            if (painted != null)
            {
                return painted;
            }

            switch (id.Trim().ToLowerInvariant())
            {
                case "adept": return Adept();
                case "ash-mite":
                case "mite": return AshMite();
                case "torch":
                case "torch-unlit": return Torch(false);
                case "torch-lit": return Torch(true);
                case "rod":
                case "rod-idle": return LightningRod(false);
                case "rod-live": return LightningRod(true);
                case "charm": return Charm();
                case "ice-block": return IceBlock();
                case "flame-curtain": return FlameCurtain();
                case "arrow-rack": return ArrowRack();
                case "poison-veil":
                case "poison-fog": return PoisonVeil();
                case "rope": return Rope();
                case "socket-gate":
                case "gate": return SocketGateSprite();
                case "stone-fire": return ElementStone("stone-fire", new Color(0.95f, 0.32f, 0.08f), new Color(1f, 0.78f, 0.28f));
                case "stone-water": return ElementStone("stone-water", new Color(0.18f, 0.52f, 0.92f), new Color(0.7f, 0.9f, 1f));
                case "stone-earth": return ElementStone("stone-earth", new Color(0.48f, 0.32f, 0.14f), new Color(0.72f, 0.82f, 0.28f));
                case "stone-air": return ElementStone("stone-air", new Color(0.72f, 0.86f, 0.92f), new Color(1f, 1f, 1f));
                case "stone-body": return ElementStone("stone-body", new Color(0.88f, 0.84f, 0.72f), new Color(0.98f, 0.96f, 0.88f));
                case "stone-spirit": return ElementStone("stone-spirit", new Color(0.55f, 0.42f, 0.82f), new Color(0.82f, 0.88f, 1f));
                case "stone-mind": return ElementStone("stone-mind", new Color(0.92f, 0.62f, 0.12f), new Color(1f, 0.92f, 0.45f));
                case "ice-thing": return IceThing();
                case "fire-golem": return FireGolem();
                case "stone-man": return StoneMan();
                case "warden": return Warden();
                case "spawn-crystal": return SpawnCrystalSprite();
                case "arrow-shot": return ArrowShot();
                case "fireball-shot": return FireballShot();
                case "tile-fire": return TileWash(new Color(1f, 0.4f, 0.08f, 0.7f));
                case "tile-wet": return TileWash(new Color(0.25f, 0.55f, 0.95f, 0.55f));
                case "tile-charge": return TileWash(new Color(0.75f, 0.9f, 1f, 0.65f));
                case "tile-grow": return TileWash(new Color(0.3f, 0.7f, 0.22f, 0.5f));
                case "nature-fire": return NatureOf(RuneId.Fire);
                case "nature-water": return NatureOf(RuneId.Water);
                case "nature-earth": return NatureOf(RuneId.Earth);
                case "nature-air": return NatureOf(RuneId.Air);
                case "nature-body":
                case "nature-salt": return NatureOf(RuneId.Salt);
                case "nature-spirit":
                case "nature-mercury": return NatureOf(RuneId.Mercury);
                case "nature-mind":
                case "nature-sulphur": return NatureOf(RuneId.Sulphur);
                case "altar": return AltarBase();
                case "inscription": return FloorCarve();
                case "plaque": return Plaque();
                case "pit": return Pit();
                case "bridge": return Bridge();
                case "door": return Door(false);
                case "door-open": return Door(true);
                case "target": return TargetRing();
                default: return Square(new Color(0.45f, 0.4f, 0.48f));
            }
        }

        public static Sprite[] Clip(string id)
        {
            var clip = SpriteActors.Clip(id);
            if (clip != null && clip.Length > 0)
            {
                return clip;
            }

            return new[] { Named(id) };
        }

        public static bool HasClip(string id)
        {
            var clip = SpriteActors.Clip(id);
            return clip != null && clip.Length > 1;
        }

        public static Sprite MemoPublic(string key, System.Func<Sprite> build) => Memo(key, build);

        public static Sprite RuneMark(RuneId id)
        {
            return Memo($"rune-mark:{id}", () => RuneGlyphs.Build(id));
        }

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

        public static Sprite Floor(RuneId element) => Floor(MaterialCatalog.FromElement(element));

        public static Sprite Floor(TileSubstance substance) =>
            Floor(MaterialCatalog.FromLegacy(substance));

        public static Sprite Floor(MaterialId material)
        {
            return Floor(material, 0, 0);
        }

        public static Sprite Floor(MaterialId material, int x, int y, int frame = 0)
        {
            var seed = Hash(x, y, (int)material);
            var wave = Animates(material) ? frame & 3 : 0;
            return Memo($"floor:{material}:{seed}:{wave}", () => PaintFloor(MaterialCatalog.Of(material), seed, wave));
        }

        public static bool Animates(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Water:
                case MaterialId.Lava:
                case MaterialId.Ember:
                case MaterialId.Ice:
                case MaterialId.Steam:
                case MaterialId.Cloud:
                case MaterialId.Rain:
                    return true;
                default:
                    return false;
            }
        }

        public static Sprite Wall(RuneId element) => Wall(MaterialCatalog.FromElement(element));

        public static Sprite Wall(TileSubstance substance) =>
            Wall(MaterialCatalog.FromLegacy(substance));

        public static Sprite Wall(MaterialId material)
        {
            return Wall(material, 0, 0);
        }

        public static Sprite Wall(MaterialId material, int x, int y)
        {
            var seed = Hash(x, y, (int)material + 17);
            return Memo($"wall:{material}:{seed}", () => PaintWall(MaterialCatalog.Of(material), seed));
        }

        public static Sprite Pit()
        {
            return Pit(0, 0);
        }

        public static Sprite Pit(int x, int y)
        {
            var seed = Hash(x, y, 91);
            return Memo($"pit:{seed}", () =>
            {
                var canvas = new PixelCanvas(32);
                var rng = new HashRng(seed);
                canvas.Clear(new Color(0.07f, 0.04f, 0.05f));
                canvas.Fill(0, 0, 32, 32, new Color(0.16f, 0.1f, 0.08f));
                canvas.SoftCircle(16, 18, 16, new Color(0.04f, 0.02f, 0.03f, 0.95f));
                canvas.FillCircle(16, 15, 12, new Color(0.05f, 0.03f, 0.04f));
                canvas.FillCircle(16, 14, 9, new Color(0.02f, 0.01f, 0.02f));
                canvas.FillCircle(16, 13, 5, Color.black);
                canvas.Circle(16, 16, 14, new Color(0.42f, 0.26f, 0.16f));
                canvas.Circle(16, 16, 13, new Color(0.28f, 0.16f, 0.1f));
                canvas.DitherBand(2, 8, new Color(0.32f, 0.2f, 0.12f, 0.35f), 0.4f);
                canvas.Noise(rng, new Color(0.45f, 0.28f, 0.14f, 0.55f), 8);
                canvas.Line(3, 10, 7, 6, new Color(0.34f, 0.2f, 0.12f));
                canvas.Line(25, 24, 30, 19, new Color(0.34f, 0.2f, 0.12f));
                return canvas.ToSprite(32);
            });
        }

        public static Sprite Bridge()
        {
            return Memo("bridge-v2", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(new Color(0.1f, 0.06f, 0.05f));
                var stone = new Color(0.56f, 0.4f, 0.24f);
                var dark = new Color(0.32f, 0.22f, 0.13f);
                for (var i = 0; i < 5; i++)
                {
                    var y = 3 + i * 5;
                    var offset = (i % 2) * 2;
                    canvas.FillRounded(1 + offset, y, 14, 4, 1, i % 2 == 0 ? stone : dark);
                    canvas.FillRounded(16 + offset, y, 14, 4, 1, i % 2 == 0 ? dark : stone);
                    canvas.Highlight(2 + offset, y + 1, 8, 1, 0.12f);
                }

                canvas.Fill(0, 0, 32, 3, new Color(0.2f, 0.12f, 0.08f));
                canvas.Fill(0, 29, 32, 3, new Color(0.16f, 0.1f, 0.07f));
                canvas.Rect(0, 0, 32, 32, new Color(0.14f, 0.09f, 0.06f));
                return canvas.ToSprite(32);
            });
        }

        public static Sprite Door(bool open)
        {
            return Memo($"door-v2:{open}", () =>
            {
                var canvas = new PixelCanvas(32, 40);
                var frame = new Color(0.18f, 0.13f, 0.11f);
                canvas.Clear(frame);
                canvas.Fill(2, 2, 28, 36, new Color(0.26f, 0.18f, 0.14f));
                canvas.Rect(2, 2, 28, 36, new Color(0.12f, 0.08f, 0.06f));
                if (open)
                {
                    canvas.Fill(7, 5, 18, 30, new Color(0.03f, 0.03f, 0.05f));
                    canvas.SoftCircle(16, 18, 8, new Color(0.08f, 0.05f, 0.04f, 0.7f));
                    canvas.FillRounded(20, 6, 7, 28, 1, new Color(0.42f, 0.24f, 0.12f));
                    canvas.Line(21, 8, 21, 32, new Color(0.28f, 0.16f, 0.08f));
                }
                else
                {
                    var wood = new Color(0.5f, 0.3f, 0.15f);
                    canvas.FillRounded(6, 5, 20, 30, 1, wood);
                    canvas.Fill(6, 12, 20, 2, new Color(0.22f, 0.16f, 0.12f));
                    canvas.Fill(6, 24, 20, 2, new Color(0.22f, 0.16f, 0.12f));
                    canvas.Line(16, 5, 16, 34, new Color(0.28f, 0.16f, 0.08f));
                    canvas.FillCircle(22, 20, 2, new Color(0.82f, 0.68f, 0.28f));
                    canvas.Set(22, 20, new Color(0.95f, 0.88f, 0.5f));
                    canvas.Rect(6, 5, 20, 30, new Color(0.16f, 0.09f, 0.05f));
                }

                return canvas.ToSprite(32, new Vector2(0.5f, 0.4f));
            });
        }

        public static Sprite Adept()
        {
            var still = SpriteActors.Still("adept");
            return still != null ? still : Square(new Color(0.28f, 0.12f, 0.48f));
        }

        public static Sprite Glow(Color color, int size = 64)
        {
            return Memo($"glow:{color}:{size}", () =>
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

                var mid = (size - 1) * 0.5f;
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var dx = (x - mid) / mid;
                        var dy = (y - mid) / mid;
                        var distance = Mathf.Sqrt(dx * dx + dy * dy);
                        var alpha = Mathf.Clamp01(1.05f - distance);
                        alpha *= alpha;
                        var pixel = color;
                        pixel.a = alpha * color.a;
                        texture.SetPixel(x, y, pixel);
                    }
                }

                texture.Apply();
                return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            });
        }

        public static Sprite Bolt(Color color)
        {
            return Memo($"bolt:{color}", () =>
            {
                var canvas = new PixelCanvas(48, 20);
                canvas.Clear(Clear);
                var core = Color.Lerp(color, Color.white, 0.45f);
                canvas.Fill(6, 7, 36, 6, color);
                canvas.Fill(4, 8, 40, 4, core);
                canvas.FillCircle(42, 10, 6, color);
                canvas.FillCircle(42, 10, 3, Color.white);
                canvas.FillCircle(8, 10, 4, core);
                return canvas.ToSprite(28);
            });
        }

        public static Sprite Pillar(Color color)
        {
            return Memo($"pillar:{color}", () =>
            {
                var canvas = new PixelCanvas(28, 56);
                canvas.Clear(Clear);
                var rim = Color.Lerp(color, Color.white, 0.35f);
                canvas.Fill(8, 2, 12, 50, color);
                canvas.Fill(10, 4, 8, 46, rim);
                canvas.FillCircle(14, 50, 8, color);
                canvas.FillCircle(14, 50, 4, Color.white);
                canvas.Fill(4, 2, 20, 6, rim);
                return canvas.ToSprite(28);
            });
        }

        public static Sprite Burst(Color color)
        {
            return Memo($"burst:{color}", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                canvas.FillCircle(24, 24, 18, color);
                canvas.FillCircle(24, 24, 11, Color.Lerp(color, Color.white, 0.4f));
                canvas.FillCircle(24, 24, 5, Color.white);
                canvas.ThickLine(24, 4, 24, 12, Color.white);
                canvas.ThickLine(24, 36, 24, 44, Color.white);
                canvas.ThickLine(4, 24, 12, 24, Color.white);
                canvas.ThickLine(36, 24, 44, 24, Color.white);
                return canvas.ToSprite(32);
            });
        }

        public static Sprite Ember(Color color)
        {
            return Memo($"ember:{color}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var core = Color.Lerp(color, Color.yellow, 0.45f);
                canvas.SoftCircle(16, 14, 12, new Color(color.r, color.g, color.b, 0.55f));
                canvas.FillCircle(16, 13, 7, color);
                canvas.FillCircle(16, 14, 4, core);
                canvas.FillCircle(16, 15, 2, Color.white);
                canvas.SoftCircle(16, 22, 6, new Color(1f, 0.55f, 0.15f, 0.45f));
                return canvas.ToSprite(28);
            });
        }

        public static Sprite Droplet(Color color)
        {
            return Memo($"droplet:{color}", () =>
            {
                var canvas = new PixelCanvas(24, 32);
                canvas.Clear(Clear);
                var core = Color.Lerp(color, Color.white, 0.4f);
                canvas.FillCircle(12, 12, 8, color);
                canvas.FillCircle(12, 18, 6, color);
                canvas.Fill(10, 8, 5, 10, color);
                canvas.FillCircle(12, 22, 4, core);
                canvas.Set(10, 20, Color.white);
                canvas.Set(9, 19, Color.white);
                return canvas.ToSprite(24);
            });
        }

        public static Sprite Shard(Color color)
        {
            return Memo($"shard:{color}", () =>
            {
                var canvas = new PixelCanvas(24, 32);
                canvas.Clear(Clear);
                var rim = Color.Lerp(color, Color.white, 0.55f);
                canvas.ThickLine(12, 2, 6, 28, color);
                canvas.ThickLine(12, 2, 18, 28, color);
                canvas.ThickLine(6, 28, 18, 28, rim);
                canvas.ThickLine(12, 2, 12, 28, rim);
                canvas.Set(12, 8, Color.white);
                return canvas.ToSprite(22);
            });
        }

        public static Sprite Pebble(Color color)
        {
            return Memo($"pebble:{color}", () =>
            {
                var canvas = new PixelCanvas(28);
                canvas.Clear(Clear);
                var dark = Color.Lerp(color, Color.black, 0.35f);
                canvas.FillCircle(14, 14, 10, dark);
                canvas.FillCircle(14, 15, 8, color);
                canvas.FillCircle(11, 17, 3, Color.Lerp(color, Color.white, 0.25f));
                canvas.Shade(16, 10, 6, 6, 0.2f);
                return canvas.ToSprite(24);
            });
        }

        public static Sprite Arc(Color color)
        {
            return Memo($"arc:{color}", () =>
            {
                var canvas = new PixelCanvas(48, 20);
                canvas.Clear(Clear);
                var core = Color.Lerp(color, Color.white, 0.7f);
                canvas.ThickLine(2, 10, 14, 6, color);
                canvas.ThickLine(14, 6, 22, 14, core);
                canvas.ThickLine(22, 14, 34, 5, color);
                canvas.ThickLine(34, 5, 46, 11, core);
                canvas.Line(3, 10, 45, 10, new Color(1f, 1f, 1f, 0.55f));
                return canvas.ToSprite(26);
            });
        }

        public static Sprite Wisp(Color color)
        {
            return Memo($"wisp:{color}", () =>
            {
                var canvas = new PixelCanvas(40);
                canvas.Clear(Clear);
                var soft = color;
                soft.a = 0.55f;
                canvas.SoftCircle(16, 18, 12, soft);
                canvas.SoftCircle(24, 22, 10, new Color(color.r, color.g, color.b, 0.4f));
                canvas.SoftCircle(20, 14, 8, new Color(1f, 1f, 1f, 0.18f));
                return canvas.ToSprite(28);
            });
        }

        public static Sprite Leaf(Color color)
        {
            return Memo($"leaf:{color}", () =>
            {
                var canvas = new PixelCanvas(24, 32);
                canvas.Clear(Clear);
                var vein = Color.Lerp(color, Color.black, 0.35f);
                canvas.FillCircle(12, 18, 8, color);
                canvas.FillCircle(12, 12, 6, color);
                canvas.Line(12, 6, 12, 26, vein);
                canvas.Line(12, 16, 6, 20, vein);
                canvas.Line(12, 16, 18, 20, vein);
                canvas.Highlight(9, 20, 3, 2, 0.2f);
                return canvas.ToSprite(22);
            });
        }

        public static Sprite Column(MaterialId material)
        {
            return Column(material, 0, 0);
        }

        public static Sprite Column(MaterialId material, int x, int y)
        {
            var seed = Hash(x, y, (int)material + 41);
            return Memo($"column:{material}:{seed}", () => PaintColumn(MaterialCatalog.Of(material), seed));
        }

        public static Sprite TargetRing()
        {
            return Memo("target-ring", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var gold = new Color(1f, 0.86f, 0.32f);
                canvas.Circle(24, 24, 18, gold);
                canvas.Circle(24, 24, 17, Color.white);
                canvas.Circle(24, 24, 16, gold);
                canvas.Fill(22, 2, 4, 8, gold);
                canvas.Fill(22, 38, 4, 8, gold);
                canvas.Fill(2, 22, 8, 4, gold);
                canvas.Fill(38, 22, 8, 4, gold);
                return canvas.ToSprite(28);
            });
        }

        public static Sprite AshMite()
        {
            return Memo("ash-mite-v2", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var body = new Color(0.82f, 0.24f, 0.1f);
                var ember = new Color(0.98f, 0.72f, 0.22f);
                var leg = new Color(0.16f, 0.07f, 0.05f);

                canvas.SoftCircle(24, 22, 16, new Color(0.95f, 0.35f, 0.08f, 0.35f));
                canvas.ThickLine(7, 15, 15, 22, leg);
                canvas.ThickLine(6, 24, 15, 24, leg);
                canvas.ThickLine(8, 33, 15, 27, leg);
                canvas.ThickLine(41, 15, 33, 22, leg);
                canvas.ThickLine(42, 24, 33, 24, leg);
                canvas.ThickLine(40, 33, 33, 27, leg);
                canvas.FillCircle(24, 24, 12, new Color(0.2f, 0.06f, 0.04f));
                canvas.FillCircle(24, 24, 11, body);
                canvas.FillCircle(24, 26, 7, ember);
                canvas.FillCircle(24, 27, 3, new Color(1f, 0.92f, 0.55f));
                canvas.FillCircle(20, 29, 2, Color.white);
                canvas.FillCircle(28, 29, 2, Color.white);
                canvas.Set(20, 29, Color.black);
                canvas.Set(28, 29, Color.black);
                canvas.Line(17, 20, 14, 16, ember);
                canvas.Line(31, 20, 34, 16, ember);
                canvas.SoftCircle(24, 24, 8, new Color(1f, 0.55f, 0.12f, 0.25f));
                return canvas.ToSprite(36);
            });
        }

        public static Sprite Torch(bool lit)
        {
            return Memo($"torch-v2:{lit}", () =>
            {
                var canvas = new PixelCanvas(32, 48);
                canvas.Clear(Clear);
                var pole = new Color(0.42f, 0.24f, 0.12f);
                canvas.FillRounded(14, 4, 4, 24, 1, pole);
                canvas.Highlight(15, 6, 1, 18, 0.16f);
                canvas.FillRounded(10, 24, 12, 7, 2, new Color(0.3f, 0.22f, 0.18f));
                canvas.Rect(10, 24, 12, 7, new Color(0.14f, 0.1f, 0.08f));
                if (lit)
                {
                    canvas.SoftCircle(16, 37, 10, new Color(1f, 0.45f, 0.08f, 0.55f));
                    canvas.FillCircle(16, 36, 7, new Color(0.95f, 0.42f, 0.08f));
                    canvas.FillCircle(16, 38, 5, new Color(1f, 0.78f, 0.22f));
                    canvas.FillCircle(16, 40, 2, new Color(1f, 0.96f, 0.72f));
                }
                else
                {
                    canvas.FillRounded(13, 30, 6, 5, 1, new Color(0.14f, 0.1f, 0.08f));
                    canvas.Line(16, 34, 16, 39, new Color(0.28f, 0.2f, 0.14f));
                    canvas.Set(16, 39, new Color(0.2f, 0.14f, 0.1f));
                }

                return canvas.ToSprite(32, new Vector2(0.5f, 0.22f));
            });
        }

        public static Sprite LightningRod(bool charged)
        {
            return Memo($"rod-v2:{charged}", () =>
            {
                var canvas = new PixelCanvas(32, 48);
                canvas.Clear(Clear);
                var metal = new Color(0.66f, 0.68f, 0.74f);
                var copper = new Color(0.8f, 0.48f, 0.2f);
                canvas.FillRounded(9, 3, 14, 5, 1, new Color(0.24f, 0.2f, 0.18f));
                canvas.Fill(14, 8, 4, 22, metal);
                canvas.Highlight(15, 10, 1, 18, 0.28f);
                canvas.FillRounded(11, 17, 10, 12, 2, copper);
                canvas.Fill(13, 19, 6, 8, new Color(0.92f, 0.6f, 0.24f));
                canvas.FillCircle(16, 34, 5, charged ? new Color(0.98f, 0.92f, 0.4f) : metal);
                if (charged)
                {
                    canvas.SoftCircle(16, 36, 9, new Color(0.75f, 0.9f, 1f, 0.45f));
                    var bolt = new Color(0.88f, 0.96f, 1f);
                    canvas.ThickLine(16, 40, 10, 44, bolt);
                    canvas.ThickLine(10, 44, 20, 46, bolt);
                    canvas.ThickLine(20, 46, 14, 47, bolt);
                }

                return canvas.ToSprite(32, new Vector2(0.5f, 0.22f));
            });
        }

        public static Sprite Charm()
        {
            return Memo("charm-v2", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var gold = new Color(0.92f, 0.66f, 0.2f);
                var core = new Color(1f, 0.4f, 0.1f);
                canvas.SoftCircle(16, 16, 13, new Color(1f, 0.55f, 0.12f, 0.4f));
                canvas.FillCircle(16, 16, 10, gold);
                canvas.FillCircle(16, 16, 7, core);
                canvas.Fill(15, 24, 2, 6, gold);
                canvas.FillCircle(16, 16, 3, new Color(1f, 0.88f, 0.45f));
                canvas.Set(14, 19, new Color(1f, 0.95f, 0.7f));
                return canvas.ToSprite(28);
            });
        }

        public static Sprite IceBlock()
        {
            return Memo("ice-block", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 14, 14, new Color(0.55f, 0.85f, 1f, 0.35f));
                canvas.FillRounded(5, 4, 22, 24, 3, new Color(0.62f, 0.84f, 0.95f));
                canvas.FillRounded(7, 6, 18, 20, 2, new Color(0.78f, 0.92f, 1f));
                canvas.Fill(8, 18, 16, 8, new Color(0.42f, 0.68f, 0.86f, 0.55f));
                canvas.Line(8, 22, 14, 8, new Color(1f, 1f, 1f, 0.7f));
                canvas.Line(18, 24, 24, 10, new Color(0.85f, 0.95f, 1f, 0.55f));
                canvas.Set(11, 20, Color.white);
                canvas.Rect(5, 4, 22, 24, new Color(0.28f, 0.5f, 0.68f));
                return canvas.ToSprite(28);
            });
        }

        public static Sprite FlameCurtain()
        {
            return Memo("flame-curtain", () =>
            {
                var canvas = new PixelCanvas(32, 48);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 28, 16, new Color(1f, 0.35f, 0.05f, 0.4f));
                canvas.FillRounded(6, 4, 20, 10, 2, new Color(0.22f, 0.1f, 0.06f));
                var ember = new Color(0.95f, 0.38f, 0.06f);
                var gold = new Color(1f, 0.78f, 0.2f);
                canvas.FillCircle(10, 22, 7, ember);
                canvas.FillCircle(16, 28, 8, ember);
                canvas.FillCircle(22, 24, 7, ember);
                canvas.FillCircle(16, 34, 6, gold);
                canvas.FillCircle(12, 30, 4, gold);
                canvas.FillCircle(20, 32, 4, gold);
                canvas.FillCircle(16, 38, 3, new Color(1f, 0.95f, 0.7f));
                canvas.FillCircle(10, 26, 2, new Color(1f, 0.95f, 0.7f));
                return canvas.ToSprite(32, new Vector2(0.5f, 0.2f));
            });
        }

        public static Sprite ArrowRack()
        {
            return Memo("arrow-rack", () =>
            {
                var canvas = new PixelCanvas(32, 40);
                canvas.Clear(Clear);
                var wood = new Color(0.5f, 0.3f, 0.14f);
                var dark = new Color(0.28f, 0.16f, 0.08f);
                canvas.FillRounded(4, 3, 24, 6, 1, dark);
                canvas.Fill(6, 8, 3, 18, wood);
                canvas.Fill(23, 8, 3, 18, wood);
                canvas.FillRounded(5, 24, 22, 4, 1, wood);
                var shaft = new Color(0.82f, 0.78f, 0.62f);
                var head = new Color(0.7f, 0.74f, 0.78f);
                for (var i = 0; i < 4; i++)
                {
                    var x = 9 + i * 4;
                    canvas.Fill(x, 12, 2, 20, shaft);
                    canvas.Fill(x - 1, 30, 4, 3, head);
                    canvas.Set(x, 33, new Color(0.95f, 0.25f, 0.18f));
                    canvas.Line(x, 12, x - 2, 8, new Color(0.85f, 0.2f, 0.15f));
                    canvas.Line(x + 1, 12, x + 3, 8, new Color(0.85f, 0.2f, 0.15f));
                }

                return canvas.ToSprite(32, new Vector2(0.5f, 0.22f));
            });
        }

        public static Sprite PoisonVeil()
        {
            return Memo("poison-veil", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var mist = new Color(0.35f, 0.82f, 0.22f, 0.55f);
                canvas.SoftCircle(10, 12, 10, mist);
                canvas.SoftCircle(22, 16, 11, new Color(0.55f, 0.95f, 0.28f, 0.45f));
                canvas.SoftCircle(16, 22, 9, new Color(0.22f, 0.55f, 0.12f, 0.5f));
                canvas.FillCircle(12, 18, 3, new Color(0.7f, 1f, 0.35f, 0.7f));
                canvas.FillCircle(20, 14, 2, new Color(0.85f, 1f, 0.45f, 0.8f));
                canvas.Line(6, 8, 10, 20, new Color(0.45f, 0.9f, 0.3f, 0.45f));
                canvas.Line(24, 10, 18, 24, new Color(0.45f, 0.9f, 0.3f, 0.4f));
                return canvas.ToSprite(28);
            });
        }

        public static Sprite Rope()
        {
            return Memo("rope", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var coil = new Color(0.72f, 0.48f, 0.2f);
                var dark = new Color(0.42f, 0.26f, 0.1f);
                canvas.Circle(16, 16, 10, coil);
                canvas.Circle(16, 16, 9, dark);
                canvas.Circle(16, 16, 7, coil);
                canvas.Circle(16, 16, 6, dark);
                canvas.Circle(16, 16, 4, coil);
                canvas.FillCircle(16, 16, 2, dark);
                canvas.FillRounded(22, 14, 8, 4, 1, coil);
                canvas.Line(24, 15, 28, 8, dark);
                return canvas.ToSprite(28);
            });
        }

        public static Sprite SocketGateSprite()
        {
            return Memo("socket-gate", () =>
            {
                var canvas = new PixelCanvas(32, 40);
                canvas.Clear(Clear);
                var stone = new Color(0.42f, 0.38f, 0.34f);
                var gold = new Color(0.92f, 0.74f, 0.28f);
                canvas.FillRounded(4, 2, 24, 34, 3, new Color(0.22f, 0.18f, 0.16f));
                canvas.FillRounded(7, 6, 18, 26, 2, stone);
                canvas.Fill(10, 10, 12, 18, new Color(0.08f, 0.06f, 0.07f));
                canvas.FillCircle(16, 28, 3, gold);
                canvas.FillCircle(12, 20, 2, gold);
                canvas.FillCircle(20, 20, 2, gold);
                canvas.FillCircle(16, 14, 2, gold);
                canvas.Highlight(8, 30, 14, 2, 0.16f);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.28f));
            });
        }

        public static Sprite IceThing()
        {
            return Memo("ice-thing", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var body = new Color(0.55f, 0.82f, 0.95f);
                var core = new Color(0.82f, 0.95f, 1f);
                var spike = new Color(0.75f, 0.92f, 1f);
                canvas.SoftCircle(24, 22, 16, new Color(0.45f, 0.8f, 1f, 0.35f));
                canvas.FillCircle(24, 22, 11, new Color(0.22f, 0.4f, 0.55f));
                canvas.FillCircle(24, 22, 10, body);
                canvas.FillCircle(24, 24, 6, core);
                canvas.ThickLine(24, 32, 24, 42, spike);
                canvas.ThickLine(14, 30, 10, 40, spike);
                canvas.ThickLine(34, 30, 38, 40, spike);
                canvas.FillCircle(20, 26, 2, Color.white);
                canvas.FillCircle(28, 26, 2, Color.white);
                canvas.Set(20, 26, new Color(0.08f, 0.2f, 0.35f));
                canvas.Set(28, 26, new Color(0.08f, 0.2f, 0.35f));
                canvas.Line(18, 18, 14, 12, spike);
                canvas.Line(30, 18, 34, 12, spike);
                return canvas.ToSprite(36);
            });
        }

        public static Sprite FireGolem()
        {
            return Memo("fire-golem", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var body = new Color(0.82f, 0.22f, 0.06f);
                var ember = new Color(1f, 0.55f, 0.12f);
                canvas.SoftCircle(24, 20, 16, new Color(1f, 0.3f, 0.04f, 0.4f));
                canvas.FillRounded(12, 8, 24, 22, 4, new Color(0.28f, 0.08f, 0.04f));
                canvas.FillRounded(14, 10, 20, 18, 3, body);
                canvas.FillRounded(16, 28, 16, 10, 3, body);
                canvas.FillCircle(24, 34, 7, body);
                canvas.FillCircle(24, 22, 6, ember);
                canvas.FillCircle(20, 36, 2, new Color(1f, 0.92f, 0.4f));
                canvas.FillCircle(28, 36, 2, new Color(1f, 0.92f, 0.4f));
                canvas.Set(20, 36, Color.black);
                canvas.Set(28, 36, Color.black);
                canvas.FillRounded(8, 12, 6, 10, 2, ember);
                canvas.FillRounded(34, 12, 6, 10, 2, ember);
                canvas.FillCircle(24, 40, 3, new Color(1f, 0.85f, 0.3f));
                return canvas.ToSprite(36);
            });
        }

        public static Sprite StoneMan()
        {
            return Memo("stone-man", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var rock = new Color(0.48f, 0.46f, 0.42f);
                var dark = new Color(0.22f, 0.2f, 0.18f);
                canvas.FillRounded(16, 6, 16, 20, 2, dark);
                canvas.FillRounded(18, 8, 12, 16, 2, rock);
                canvas.FillRounded(14, 24, 20, 8, 2, rock);
                canvas.FillRounded(18, 30, 12, 10, 2, rock);
                canvas.Fill(20, 36, 3, 3, dark);
                canvas.Fill(26, 36, 3, 3, dark);
                canvas.Line(20, 14, 28, 12, new Color(0.7f, 0.68f, 0.62f));
                canvas.Line(18, 20, 30, 22, dark);
                canvas.FillRounded(10, 16, 6, 8, 1, rock);
                canvas.FillRounded(32, 16, 6, 8, 1, rock);
                return canvas.ToSprite(36);
            });
        }

        public static Sprite Warden()
        {
            return Memo("warden", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var cloak = new Color(0.28f, 0.16f, 0.48f);
                var metal = new Color(0.72f, 0.7f, 0.62f);
                var gold = new Color(0.92f, 0.74f, 0.28f);
                canvas.SoftCircle(24, 22, 14, new Color(0.55f, 0.35f, 0.9f, 0.3f));
                canvas.FillRounded(16, 8, 16, 22, 4, new Color(0.12f, 0.06f, 0.2f));
                canvas.FillRounded(18, 10, 12, 18, 3, cloak);
                canvas.FillRounded(20, 26, 8, 10, 2, cloak);
                canvas.FillCircle(24, 34, 6, metal);
                canvas.Fill(22, 36, 2, 2, new Color(0.15f, 0.08f, 0.2f));
                canvas.Fill(26, 36, 2, 2, new Color(0.15f, 0.08f, 0.2f));
                canvas.Fill(34, 8, 3, 28, new Color(0.42f, 0.24f, 0.12f));
                canvas.Fill(32, 34, 7, 6, metal);
                canvas.FillCircle(35, 40, 3, gold);
                canvas.FillRounded(14, 18, 6, 5, 1, gold);
                canvas.FillRounded(28, 18, 6, 5, 1, gold);
                return canvas.ToSprite(36);
            });
        }

        public static Sprite SpawnCrystalSprite()
        {
            return Memo("spawn-crystal", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var gem = new Color(0.72f, 0.52f, 1f);
                var rim = new Color(0.95f, 0.88f, 1f);
                canvas.SoftCircle(16, 16, 12, new Color(0.7f, 0.5f, 1f, 0.4f));
                canvas.FillCircle(16, 16, 8, gem);
                canvas.Fill(14, 4, 4, 24, rim);
                canvas.Fill(6, 14, 20, 4, rim);
                canvas.FillCircle(16, 16, 3, Color.white);
                return canvas.ToSprite(28);
            });
        }

        public static Sprite ArrowShot()
        {
            return Memo("arrow-shot", () =>
            {
                var canvas = new PixelCanvas(32, 12);
                canvas.Clear(Clear);
                var shaft = new Color(0.55f, 0.32f, 0.12f);
                var head = new Color(0.72f, 0.74f, 0.78f);
                canvas.Fill(2, 5, 22, 2, shaft);
                canvas.Fill(22, 3, 8, 6, head);
                canvas.Fill(24, 4, 6, 4, Color.white);
                canvas.Fill(1, 3, 4, 6, new Color(0.85f, 0.45f, 0.15f));
                return canvas.ToSprite(24, new Vector2(0.8f, 0.5f));
            });
        }

        public static Sprite FireballShot()
        {
            return Memo("fireball-shot", () =>
            {
                var canvas = new PixelCanvas(24);
                canvas.Clear(Clear);
                canvas.SoftCircle(12, 12, 10, new Color(1f, 0.35f, 0.05f, 0.55f));
                canvas.FillCircle(12, 12, 7, new Color(1f, 0.45f, 0.08f));
                canvas.FillCircle(12, 12, 4, new Color(1f, 0.85f, 0.3f));
                canvas.FillCircle(13, 11, 2, Color.white);
                return canvas.ToSprite(22);
            });
        }

        public static Sprite NatureOf(RuneId rune)
        {
            switch (rune)
            {
                case RuneId.Fire: return NatureFire();
                case RuneId.Water: return NatureWater();
                case RuneId.Earth: return NatureEarth();
                case RuneId.Air: return NatureAir();
                case RuneId.Salt: return NatureBody();
                case RuneId.Mercury: return NatureSpirit();
                case RuneId.Sulphur: return NatureMind();
                default: return Burst(RunePalette.Of(rune));
            }
        }

        public static bool HasNature(RuneId rune)
        {
            switch (rune)
            {
                case RuneId.Fire:
                case RuneId.Water:
                case RuneId.Earth:
                case RuneId.Air:
                case RuneId.Salt:
                case RuneId.Mercury:
                case RuneId.Sulphur:
                    return true;
                default:
                    return false;
            }
        }

        public static Sprite AltarBase()
        {
            return Memo("altar-base", () =>
            {
                var canvas = new PixelCanvas(40, 22);
                canvas.Clear(Clear);
                var stone = new Color(0.38f, 0.36f, 0.34f);
                var dark = new Color(0.2f, 0.18f, 0.16f);
                var rim = new Color(0.62f, 0.58f, 0.5f);
                canvas.FillRounded(2, 2, 36, 16, 2, stone);
                canvas.Rect(2, 2, 36, 16, dark);
                canvas.Fill(4, 14, 32, 3, rim);
                canvas.Fill(8, 4, 24, 3, dark);
                canvas.Highlight(6, 15, 10, 1, 0.2f);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.15f));
            });
        }

        public static Sprite FloorCarve()
        {
            return Memo("floor-carve", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var slab = new Color(0.28f, 0.26f, 0.24f, 0.92f);
                var seam = new Color(0.14f, 0.12f, 0.1f, 0.95f);
                canvas.FillRounded(1, 1, 30, 30, 3, slab);
                canvas.Rect(1, 1, 30, 30, seam);
                canvas.Rect(3, 3, 26, 26, new Color(0.4f, 0.36f, 0.3f, 0.55f));
                return canvas.ToSprite(32);
            });
        }

        public static Sprite AspectColumn()
        {
            return Memo("aspect-column", () =>
            {
                var canvas = new PixelCanvas(28, 56);
                canvas.Clear(Clear);
                var stone = new Color(0.42f, 0.4f, 0.38f);
                var dark = new Color(0.22f, 0.2f, 0.18f);
                var rim = new Color(0.7f, 0.66f, 0.56f);
                canvas.Fill(8, 2, 12, 46, stone);
                canvas.Fill(10, 4, 8, 42, Color.Lerp(stone, Color.white, 0.12f));
                canvas.Fill(4, 2, 20, 6, dark);
                canvas.Fill(6, 44, 16, 6, rim);
                canvas.FillCircle(14, 50, 7, stone);
                canvas.FillCircle(14, 50, 4, rim);
                canvas.Shade(16, 8, 6, 36, 0.18f);
                return canvas.ToSprite(28, new Vector2(0.5f, 0.08f));
            });
        }

        static Sprite NatureFire()
        {
            return Memo("nature-fire", () =>
            {
                var canvas = new PixelCanvas(32, 40);
                canvas.Clear(Clear);
                var coal = new Color(0.22f, 0.08f, 0.04f);
                var ember = new Color(0.95f, 0.32f, 0.05f);
                var gold = new Color(1f, 0.78f, 0.18f);
                var white = new Color(1f, 0.96f, 0.75f);
                canvas.FillRounded(6, 2, 20, 6, 2, coal);
                canvas.SoftCircle(16, 16, 14, new Color(1f, 0.35f, 0.05f, 0.35f));
                canvas.FillCircle(10, 14, 6, ember);
                canvas.FillCircle(22, 14, 6, ember);
                canvas.FillCircle(16, 20, 8, ember);
                canvas.FillCircle(16, 26, 6, gold);
                canvas.FillCircle(12, 22, 4, gold);
                canvas.FillCircle(20, 22, 4, gold);
                canvas.FillCircle(16, 32, 4, white);
                canvas.FillCircle(16, 18, 3, white);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.12f));
            });
        }

        static Sprite NatureWater()
        {
            return Memo("nature-water", () =>
            {
                var canvas = new PixelCanvas(32, 36);
                canvas.Clear(Clear);
                var deep = new Color(0.12f, 0.32f, 0.72f);
                var mid = new Color(0.28f, 0.58f, 0.92f);
                var foam = new Color(0.82f, 0.92f, 1f);
                canvas.FillRounded(3, 4, 26, 12, 4, deep);
                canvas.Fill(4, 8, 24, 6, mid);
                canvas.ThickLine(4, 14, 12, 18, foam);
                canvas.ThickLine(12, 18, 20, 12, foam);
                canvas.ThickLine(20, 12, 28, 16, foam);
                canvas.FillCircle(16, 26, 6, mid);
                canvas.FillCircle(16, 30, 4, deep);
                canvas.Fill(14, 20, 4, 8, mid);
                canvas.Set(14, 28, Color.white);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.18f));
            });
        }

        static Sprite NatureEarth()
        {
            return Memo("nature-earth", () =>
            {
                var canvas = new PixelCanvas(36, 28);
                canvas.Clear(Clear);
                var rock = new Color(0.48f, 0.36f, 0.22f);
                var dark = new Color(0.28f, 0.2f, 0.12f);
                var moss = new Color(0.42f, 0.55f, 0.22f);
                canvas.FillCircle(10, 10, 8, dark);
                canvas.FillCircle(10, 11, 7, rock);
                canvas.FillCircle(24, 10, 7, dark);
                canvas.FillCircle(24, 11, 6, Color.Lerp(rock, Color.white, 0.08f));
                canvas.FillCircle(17, 16, 9, dark);
                canvas.FillCircle(17, 17, 8, rock);
                canvas.FillCircle(14, 20, 3, moss);
                canvas.Highlight(12, 18, 4, 2, 0.18f);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.2f));
            });
        }

        static Sprite NatureAir()
        {
            return Memo("nature-air", () =>
            {
                var canvas = new PixelCanvas(36, 32);
                canvas.Clear(Clear);
                var wind = new Color(0.78f, 0.9f, 0.96f);
                var core = new Color(1f, 1f, 1f, 0.85f);
                canvas.SoftCircle(18, 16, 14, new Color(0.7f, 0.85f, 0.95f, 0.28f));
                canvas.ThickLine(4, 10, 18, 6, wind);
                canvas.ThickLine(18, 6, 32, 12, core);
                canvas.ThickLine(3, 16, 20, 14, core);
                canvas.ThickLine(20, 14, 33, 18, wind);
                canvas.ThickLine(6, 24, 16, 20, wind);
                canvas.ThickLine(16, 20, 30, 26, core);
                canvas.Circle(18, 16, 5, wind);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.35f));
            });
        }

        static Sprite NatureBody()
        {
            return Memo("nature-body", () =>
            {
                var canvas = new PixelCanvas(28, 40);
                canvas.Clear(Clear);
                var flesh = new Color(0.86f, 0.78f, 0.62f);
                var dark = new Color(0.42f, 0.34f, 0.24f);
                canvas.FillRounded(6, 2, 16, 6, 2, dark);
                canvas.Fill(10, 8, 8, 16, flesh);
                canvas.Fill(4, 18, 6, 10, flesh);
                canvas.Fill(18, 18, 6, 10, flesh);
                canvas.Fill(10, 22, 3, 12, flesh);
                canvas.Fill(15, 22, 3, 12, flesh);
                canvas.FillCircle(14, 30, 6, flesh);
                canvas.Circle(14, 30, 6, dark);
                canvas.Fill(12, 8, 4, 3, dark);
                return canvas.ToSprite(28, new Vector2(0.5f, 0.08f));
            });
        }

        static Sprite NatureSpirit()
        {
            return Memo("nature-spirit", () =>
            {
                var canvas = new PixelCanvas(36, 32);
                canvas.Clear(Clear);
                var path = new Color(0.55f, 0.78f, 0.7f);
                var glow = new Color(0.82f, 0.95f, 0.88f);
                canvas.Fill(4, 8, 6, 16, Color.Lerp(path, Color.black, 0.35f));
                canvas.Fill(26, 8, 6, 16, Color.Lerp(path, Color.black, 0.35f));
                canvas.ThickLine(10, 16, 26, 16, path);
                canvas.ThickLine(10, 16, 16, 22, glow);
                canvas.ThickLine(16, 22, 26, 16, path);
                canvas.SoftCircle(18, 16, 7, new Color(0.7f, 0.95f, 0.85f, 0.4f));
                canvas.FillCircle(18, 16, 3, glow);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.3f));
            });
        }

        static Sprite NatureMind()
        {
            return Memo("nature-mind", () =>
            {
                var canvas = new PixelCanvas(32, 36);
                canvas.Clear(Clear);
                var gold = new Color(0.93f, 0.78f, 0.22f);
                var dark = new Color(0.35f, 0.26f, 0.08f);
                canvas.FillCircle(16, 14, 9, dark);
                canvas.FillCircle(16, 15, 8, gold);
                canvas.FillCircle(16, 16, 4, new Color(1f, 0.95f, 0.7f));
                canvas.ThickLine(16, 23, 16, 30, gold);
                canvas.ThickLine(12, 28, 20, 28, dark);
                canvas.SoftCircle(16, 32, 5, new Color(1f, 0.9f, 0.35f, 0.55f));
                canvas.FillCircle(16, 32, 2, Color.white);
                return canvas.ToSprite(28, new Vector2(0.5f, 0.18f));
            });
        }

        static Sprite TileWash(Color color)
        {
            return Memo($"tile-wash:{color}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 16, 13, color);
                return canvas.ToSprite(32);
            });
        }

        static Sprite ElementStone(string key, Color gem, Color rim)
        {
            return Memo(key, () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 16, 12, new Color(gem.r, gem.g, gem.b, 0.4f));
                canvas.FillCircle(16, 15, 9, Color.Lerp(gem, Color.black, 0.35f));
                canvas.FillCircle(16, 16, 8, gem);
                canvas.FillCircle(16, 17, 5, Color.Lerp(gem, rim, 0.55f));
                canvas.FillCircle(16, 18, 2, Color.Lerp(rim, Color.white, 0.5f));
                canvas.Set(13, 20, Color.white);
                canvas.Circle(16, 16, 8, rim);
                canvas.Fill(15, 6, 2, 5, rim);
                canvas.FillCircle(16, 6, 2, rim);
                return canvas.ToSprite(28);
            });
        }

        public static Sprite Plaque()
        {
            return Memo("plaque-v2", () =>
            {
                var canvas = new PixelCanvas(32, 16);
                canvas.Clear(Clear);
                canvas.FillRounded(1, 1, 30, 14, 2, new Color(0.44f, 0.28f, 0.14f));
                canvas.Rect(1, 1, 30, 14, new Color(0.22f, 0.14f, 0.08f));
                canvas.Fill(3, 3, 26, 3, new Color(0.32f, 0.2f, 0.1f));
                canvas.Highlight(4, 10, 8, 1, 0.12f);
                return canvas.ToSprite(24);
            });
        }

        public static Sprite WallShadow()
        {
            return Memo("wall-shadow", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                for (var y = 20; y < 32; y++)
                {
                    var a = (y - 20) / 12f * 0.38f;
                    canvas.Fill(0, y, 32, 1, new Color(0f, 0f, 0f, a));
                }

                return canvas.ToSprite(32);
            });
        }

        public static Sprite PitRim(int mask)
        {
            return Memo($"pit-rim:{mask}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var rim = new Color(0.4f, 0.24f, 0.14f, 0.85f);
                var inner = new Color(0.08f, 0.04f, 0.05f, 0.55f);
                if ((mask & 1) != 0)
                {
                    canvas.Fill(0, 28, 32, 4, rim);
                    canvas.Fill(0, 26, 32, 2, inner);
                }

                if ((mask & 2) != 0)
                {
                    canvas.Fill(0, 0, 32, 4, rim);
                    canvas.Fill(0, 4, 32, 2, inner);
                }

                if ((mask & 4) != 0)
                {
                    canvas.Fill(0, 0, 4, 32, rim);
                    canvas.Fill(4, 0, 2, 32, inner);
                }

                if ((mask & 8) != 0)
                {
                    canvas.Fill(28, 0, 4, 32, rim);
                    canvas.Fill(26, 0, 2, 32, inner);
                }

                return canvas.ToSprite(32);
            });
        }

        static Sprite PaintFloor(WorldMaterial material, int seed, int frame = 0)
        {
            var canvas = new PixelCanvas(32);
            var tone = material.FloorTone;
            var rng = new HashRng(seed == 0 ? 1 : seed);
            switch (material.Paint)
            {
                case MaterialPaint.Planks:
                    PaintPlanks(canvas, tone, Color.Lerp(tone, Color.black, 0.35f), rng);
                    break;
                case MaterialPaint.Ash:
                    PaintCobble(canvas, tone, rng);
                    canvas.Noise(rng, new Color(0.55f, 0.28f, 0.12f, 0.7f), 16);
                    canvas.Noise(rng, new Color(0.08f, 0.07f, 0.07f, 0.8f), 12);
                    break;
                case MaterialPaint.Hearth:
                    PaintCobble(canvas, tone, rng);
                    PaintVein(canvas, new Color(0.92f, 0.38f, 0.16f, 0.7f), rng);
                    canvas.SoftCircle(rng.Range(8, 24), rng.Range(8, 24), 4, new Color(0.95f, 0.4f, 0.12f, 0.28f));
                    break;
                case MaterialPaint.Ember:
                    PaintCobble(canvas, tone, rng);
                    canvas.Noise(rng, new Color(0.95f, 0.42f, 0.12f, 0.85f), 14);
                    PaintVein(canvas, new Color(0.85f, 0.22f, 0.08f, 0.65f), rng);
                    canvas.SoftCircle(8 + (frame * 3) % 16, 10 + (frame * 5) % 14, 5 + (frame & 1), new Color(1f, 0.35f, 0.05f, 0.22f + frame * 0.04f));
                    break;
                case MaterialPaint.Damp:
                    PaintCobble(canvas, tone, rng);
                    canvas.SoftCircle(rng.Range(6, 20), rng.Range(8, 22), 5, new Color(0.45f, 0.7f, 0.9f, 0.28f));
                    canvas.Line(4, 16, 12, 14, new Color(0.4f, 0.62f, 0.85f, 0.4f));
                    break;
                case MaterialPaint.Vein:
                    PaintCobble(canvas, tone, rng);
                    canvas.ThickLine(4, 6, 14, 18, new Color(0.98f, 0.86f, 0.28f, 0.75f));
                    canvas.ThickLine(14, 18, 26, 10, new Color(0.98f, 0.86f, 0.28f, 0.75f));
                    canvas.ThickLine(8, 24, 22, 28, new Color(0.85f, 0.7f, 0.2f, 0.55f));
                    canvas.SoftCircle(16, 16, 4, new Color(1f, 0.9f, 0.4f, 0.2f));
                    break;
                case MaterialPaint.Scoured:
                    PaintCobble(canvas, tone, rng);
                    canvas.Line(2, 8, 28, 6, new Color(0.8f, 0.88f, 0.95f, 0.28f));
                    canvas.Line(3, 18, 30, 16, new Color(0.8f, 0.88f, 0.95f, 0.22f));
                    canvas.Line(1, 26, 26, 24, new Color(0.75f, 0.84f, 0.92f, 0.2f));
                    canvas.Noise(rng, new Color(0.85f, 0.88f, 0.92f, 0.25f), 10);
                    break;
                case MaterialPaint.Moss:
                    PaintCobble(canvas, tone, rng);
                    canvas.FillCircle(rng.Range(6, 14), rng.Range(8, 16), 4, new Color(0.28f, 0.52f, 0.22f, 0.8f));
                    canvas.FillCircle(rng.Range(16, 26), rng.Range(14, 24), 5, new Color(0.22f, 0.46f, 0.2f, 0.75f));
                    canvas.FillCircle(rng.Range(10, 20), rng.Range(6, 14), 3, new Color(0.34f, 0.58f, 0.26f, 0.7f));
                    break;
                case MaterialPaint.Metal:
                    canvas.Clear(new Color(0.18f, 0.18f, 0.2f));
                    canvas.Fill(2, 2, 13, 13, new Color(0.42f, 0.44f, 0.48f));
                    canvas.Fill(17, 2, 13, 13, new Color(0.36f, 0.38f, 0.42f));
                    canvas.Fill(2, 17, 13, 13, new Color(0.36f, 0.38f, 0.42f));
                    canvas.Fill(17, 17, 13, 13, new Color(0.42f, 0.44f, 0.48f));
                    canvas.FillCircle(6, 6, 1, new Color(0.7f, 0.7f, 0.72f));
                    canvas.FillCircle(26, 6, 1, new Color(0.7f, 0.7f, 0.72f));
                    canvas.FillCircle(6, 26, 1, new Color(0.7f, 0.7f, 0.72f));
                    canvas.FillCircle(26, 26, 1, new Color(0.7f, 0.7f, 0.72f));
                    canvas.Rect(0, 0, 32, 32, new Color(0.12f, 0.12f, 0.14f));
                    break;
                case MaterialPaint.Salt:
                    PaintCobble(canvas, tone, rng);
                    canvas.Noise(rng, new Color(0.95f, 0.95f, 0.9f, 0.8f), 18);
                    canvas.Set(rng.Range(4, 28), rng.Range(4, 28), Color.white);
                    break;
                case MaterialPaint.Ice:
                    PaintCobble(canvas, tone, rng);
                    canvas.Line(4 + frame, 8, 26 + frame, 6, new Color(1f, 1f, 1f, 0.4f + frame * 0.04f));
                    canvas.Line(6, 20 - frame, 28, 18 - frame, new Color(0.85f, 0.95f, 1f, 0.35f));
                    canvas.Noise(rng, new Color(1f, 1f, 1f, 0.55f), 10);
                    canvas.Highlight(2, 22, 12, 2, 0.18f);
                    break;
                case MaterialPaint.Sand:
                    canvas.Clear(Color.Lerp(tone, Color.black, 0.18f));
                    PaintCobble(canvas, tone, rng);
                    canvas.Noise(rng, new Color(0.9f, 0.78f, 0.45f, 0.7f), 20);
                    break;
                case MaterialPaint.Mud:
                    PaintCobble(canvas, tone, rng);
                    canvas.SoftCircle(rng.Range(6, 20), rng.Range(8, 22), 6, new Color(0.18f, 0.12f, 0.08f, 0.4f));
                    canvas.Line(4, 18, 16, 16, new Color(0.15f, 0.1f, 0.06f, 0.5f));
                    break;
                case MaterialPaint.Lava:
                    PaintCobble(canvas, Color.Lerp(tone, Color.black, 0.45f), rng);
                    PaintVein(canvas, new Color(1f, 0.45f, 0.08f, 0.85f), rng);
                    canvas.Noise(rng, new Color(1f, 0.7f, 0.15f, 0.8f), 12);
                    canvas.SoftCircle(10 + frame * 4, 12 + frame * 3, 6, new Color(1f, 0.4f, 0.05f, 0.28f + frame * 0.05f));
                    canvas.SoftCircle(20 - frame * 2, 22 - frame, 5, new Color(1f, 0.55f, 0.1f, 0.22f));
                    break;
                case MaterialPaint.Steam:
                    PaintCobble(canvas, tone, rng);
                    canvas.Noise(rng, new Color(1f, 1f, 1f, 0.35f), 16);
                    canvas.Line(3 + frame, 10 + frame, 12 + frame, 4 + frame, new Color(1f, 1f, 1f, 0.25f));
                    canvas.Line(18, 22 - frame, 28, 14 - frame, new Color(1f, 1f, 1f, 0.2f));
                    break;
                case MaterialPaint.Dust:
                    PaintCobble(canvas, tone, rng);
                    canvas.Noise(rng, new Color(0.7f, 0.62f, 0.48f, 0.55f), 18);
                    break;
                case MaterialPaint.Glass:
                    canvas.Clear(Color.Lerp(tone, Color.black, 0.35f));
                    canvas.Fill(3, 3, 12, 12, Color.Lerp(tone, Color.white, 0.25f));
                    canvas.Fill(17, 3, 12, 12, tone);
                    canvas.Fill(3, 17, 12, 12, tone);
                    canvas.Fill(17, 17, 12, 12, Color.Lerp(tone, Color.white, 0.18f));
                    canvas.Set(6, 6, Color.white);
                    canvas.Set(24, 22, new Color(1f, 1f, 1f, 0.7f));
                    canvas.Rect(0, 0, 32, 32, Color.Lerp(tone, Color.black, 0.4f));
                    break;
                case MaterialPaint.Crystal:
                    PaintCobble(canvas, tone, rng);
                    canvas.ThickLine(8, 4, 16, 22, new Color(0.9f, 0.8f, 1f, 0.7f));
                    canvas.ThickLine(20, 6, 12, 26, new Color(0.75f, 0.55f, 0.95f, 0.55f));
                    canvas.Set(16, 12, Color.white);
                    break;
                case MaterialPaint.Obsidian:
                    PaintCobble(canvas, tone, rng);
                    canvas.Line(4, 10, 14, 6, new Color(0.45f, 0.3f, 0.6f, 0.45f));
                    canvas.Line(18, 24, 28, 18, new Color(0.35f, 0.22f, 0.5f, 0.4f));
                    canvas.Noise(rng, new Color(0.2f, 0.16f, 0.28f, 0.7f), 10);
                    break;
                case MaterialPaint.Grove:
                    canvas.Clear(Color.Lerp(tone, Color.black, 0.25f));
                    canvas.FillCircle(rng.Range(6, 14), rng.Range(8, 16), 6, new Color(0.2f, 0.42f, 0.16f, 0.9f));
                    canvas.FillCircle(rng.Range(16, 26), rng.Range(12, 22), 7, new Color(0.16f, 0.36f, 0.14f, 0.85f));
                    canvas.FillCircle(rng.Range(10, 20), rng.Range(6, 14), 4, new Color(0.28f, 0.5f, 0.2f, 0.75f));
                    canvas.Noise(rng, new Color(0.4f, 0.65f, 0.22f, 0.7f), 12);
                    break;
                case MaterialPaint.Cloud:
                    canvas.Clear(Color.Lerp(tone, Color.white, 0.15f));
                    canvas.FillCircle(10 + frame, 16, 8, new Color(1f, 1f, 1f, 0.45f));
                    canvas.FillCircle(20 - frame, 14 + (frame & 1), 9, new Color(0.92f, 0.94f, 0.98f, 0.4f));
                    canvas.FillCircle(16, 20 - frame, 6, new Color(0.8f, 0.84f, 0.9f, 0.35f));
                    break;
                case MaterialPaint.Rain:
                    PaintCobble(canvas, tone, rng);
                    canvas.Line(6, 26 - frame * 3, 4, 6 - frame * 3, new Color(0.55f, 0.75f, 0.95f, 0.45f));
                    canvas.Line(14, 28 - frame * 2, 12, 8 - frame * 2, new Color(0.55f, 0.75f, 0.95f, 0.35f));
                    canvas.Line(22, 24 - frame * 3, 20, 4 - frame * 3, new Color(0.55f, 0.75f, 0.95f, 0.4f));
                    canvas.Line(28, 22 - frame * 2, 26, 8 - frame * 2, new Color(0.55f, 0.75f, 0.95f, 0.3f));
                    break;
                case MaterialPaint.Snow:
                    PaintCobble(canvas, tone, rng);
                    canvas.Noise(rng, new Color(1f, 1f, 1f, 0.85f), 20);
                    canvas.Set(rng.Range(6, 26), rng.Range(6, 26), Color.white);
                    break;
                case MaterialPaint.Glacier:
                    PaintCobble(canvas, tone, rng);
                    canvas.ThickLine(2, 8, 30, 20, new Color(0.45f, 0.55f, 0.62f, 0.55f));
                    canvas.Line(4, 22, 28, 10, new Color(1f, 1f, 1f, 0.35f));
                    break;
                case MaterialPaint.Acid:
                    PaintCobble(canvas, tone, rng);
                    canvas.Noise(rng, new Color(0.85f, 1f, 0.25f, 0.7f), 16);
                    canvas.SoftCircle(12, 16, 5, new Color(0.7f, 0.95f, 0.2f, 0.35f));
                    break;
                case MaterialPaint.Water:
                    canvas.Clear(Color.Lerp(tone, Color.black, 0.28f));
                    canvas.FillCircle(16, 16, 13, tone);
                    canvas.SoftCircle(16, 16, 11, new Color(0.12f, 0.28f, 0.48f, 0.35f));
                    canvas.Line(4 + frame, 14 + (frame & 1), 28 - frame, 12 + frame, new Color(0.7f, 0.88f, 1f, 0.45f));
                    canvas.Line(6 - frame, 20, 26 + frame, 18 - (frame & 1), new Color(0.55f, 0.78f, 0.95f, 0.32f));
                    canvas.Line(8, 24 - frame, 22, 22, new Color(0.8f, 0.92f, 1f, 0.2f));
                    canvas.Set(10 + frame * 3, 16, new Color(1f, 1f, 1f, 0.55f));
                    break;
                case MaterialPaint.Plant:
                    PaintCobble(canvas, Color.Lerp(tone, new Color(0.28f, 0.26f, 0.16f), 0.45f), rng);
                    canvas.FillCircle(rng.Range(8, 16), rng.Range(10, 18), 5, tone);
                    canvas.FillCircle(rng.Range(16, 24), rng.Range(16, 24), 4, Color.Lerp(tone, Color.black, 0.15f));
                    canvas.Line(16, 8, 16, 22, new Color(0.18f, 0.32f, 0.1f, 0.7f));
                    break;
                case MaterialPaint.Void:
                    canvas.Clear(tone);
                    canvas.FillCircle(16, 16, 10, Color.black);
                    canvas.SoftCircle(16, 16, 8, new Color(0.12f, 0.04f, 0.16f, 0.45f));
                    break;
                default:
                    PaintCobble(canvas, tone, rng);
                    PaintVein(canvas, new Color(0.55f, 0.38f, 0.22f, 0.45f), rng);
                    break;
            }

            canvas.DitherBand(0, 3, new Color(1f, 1f, 1f, 0.05f), 0.35f);
            canvas.DitherBand(28, 31, new Color(0f, 0f, 0f, 0.12f), 0.4f);
            return canvas.ToSprite(32);
        }

        static Sprite PaintWall(WorldMaterial material, int seed)
        {
            var canvas = new PixelCanvas(32, 40);
            var rng = new HashRng(seed == 0 ? 3 : seed);
            var brick = material.WallTone;
            switch (material.Paint)
            {
                case MaterialPaint.Ice:
                    PaintIceWall(canvas, brick, rng);
                    break;
                case MaterialPaint.Hearth:
                case MaterialPaint.Ember:
                case MaterialPaint.Lava:
                    PaintFlameWall(canvas, brick, rng);
                    break;
                case MaterialPaint.Grove:
                case MaterialPaint.Plant:
                case MaterialPaint.Moss:
                    PaintVineWall(canvas, brick, rng);
                    break;
                default:
                    PaintBrickWall(canvas, brick, rng);
                    break;
            }

            canvas.DitherBand(0, 4, new Color(0f, 0f, 0f, 0.18f), 0.5f);
            canvas.Noise(rng, new Color(0.05f, 0.04f, 0.04f, 0.35f), 8);
            return canvas.ToSprite(32, new Vector2(0.5f, 0.4f));
        }

        static void PaintBrickWall(PixelCanvas canvas, Color brick, HashRng rng)
        {
            var mortar = Color.Lerp(brick, new Color(0.08f, 0.07f, 0.08f), 0.72f);
            canvas.Clear(mortar);
            for (var row = 0; row < 6; row++)
            {
                var stagger = (row % 2) * 5;
                for (var col = -1; col < 5; col++)
                {
                    var x = col * 10 + stagger + rng.Range(-1, 2);
                    var y = 1 + row * 6;
                    var tone = rng.Jitter(Color.Lerp(brick, Color.black, ((row + col) & 1) * 0.1f), 0.06f);
                    canvas.FillRounded(x + 1, y, 9, 5, 1, tone);
                    canvas.Highlight(x + 2, y + 1, 4, 1, 0.12f);
                    canvas.Shade(x + 2, y + 4, 6, 1, 0.12f);
                }
            }

            canvas.Fill(0, 34, 32, 6, Color.Lerp(brick, Color.white, 0.1f));
            canvas.Highlight(0, 36, 32, 2, 0.08f);
        }

        static void PaintIceWall(PixelCanvas canvas, Color ice, HashRng rng)
        {
            canvas.Clear(Color.Lerp(ice, new Color(0.2f, 0.3f, 0.4f), 0.45f));
            for (var i = 0; i < 5; i++)
            {
                var x = 2 + i * 6 + rng.Range(-1, 2);
                var h = rng.Range(16, 34);
                canvas.FillRounded(x, 2, 5, h, 1, Color.Lerp(ice, Color.white, rng.Value() * 0.35f));
                canvas.Highlight(x + 1, h - 4, 2, 6, 0.25f);
            }

            canvas.Fill(0, 0, 32, 5, Color.Lerp(ice, Color.white, 0.2f));
            canvas.SoftCircle(10, 28, 6, new Color(1f, 1f, 1f, 0.2f));
        }

        static void PaintFlameWall(PixelCanvas canvas, Color brick, HashRng rng)
        {
            PaintBrickWall(canvas, Color.Lerp(brick, new Color(0.2f, 0.08f, 0.06f), 0.35f), rng);
            for (var i = 0; i < 7; i++)
            {
                var x = rng.Range(4, 28);
                var y = rng.Range(8, 30);
                canvas.SoftCircle(x, y, rng.Range(2, 4), new Color(1f, 0.45f, 0.1f, 0.45f));
            }

            canvas.SoftCircle(16, 34, 10, new Color(1f, 0.4f, 0.08f, 0.4f));
        }

        static void PaintVineWall(PixelCanvas canvas, Color leaf, HashRng rng)
        {
            canvas.Clear(Color.Lerp(leaf, new Color(0.08f, 0.12f, 0.06f), 0.55f));
            for (var i = 0; i < 8; i++)
            {
                var x = 3 + i * 4 + rng.Range(-1, 2);
                canvas.Fill(x, 2, 3, 34, Color.Lerp(leaf, Color.black, rng.Value() * 0.25f));
                canvas.FillCircle(x + 1, rng.Range(8, 30), 3, leaf);
            }

            canvas.Fill(0, 0, 32, 5, Color.Lerp(leaf, new Color(0.2f, 0.14f, 0.08f), 0.4f));
        }

        static Sprite PaintColumn(WorldMaterial material, int seed)
        {
            var canvas = new PixelCanvas(32, 64);
            var rng = new HashRng(seed == 0 ? 11 : seed);
            var tone = material.WallTone;
            var shaft = Color.Lerp(tone, Color.white, 0.08f);
            var dark = Color.Lerp(tone, Color.black, 0.35f);
            canvas.Clear(Clear);
            canvas.FillRounded(6, 2, 20, 6, 1, dark);
            canvas.FillRounded(8, 6, 16, 44, 2, shaft);
            canvas.Fill(10, 8, 3, 40, Color.Lerp(shaft, Color.white, 0.18f));
            canvas.Fill(19, 8, 3, 40, dark);
            canvas.FillRounded(5, 48, 22, 8, 2, Color.Lerp(tone, Color.white, 0.12f));
            canvas.FillRounded(8, 54, 16, 6, 2, shaft);
            switch (material.Paint)
            {
                case MaterialPaint.Ice:
                    canvas.Highlight(11, 20, 2, 18, 0.35f);
                    canvas.SoftCircle(16, 56, 8, new Color(0.8f, 0.95f, 1f, 0.4f));
                    break;
                case MaterialPaint.Hearth:
                case MaterialPaint.Ember:
                case MaterialPaint.Lava:
                    canvas.Fill(12, 10, 8, 36, new Color(1f, 0.4f, 0.08f, 0.85f));
                    canvas.SoftCircle(16, 52, 8, new Color(1f, 0.45f, 0.1f, 0.55f));
                    break;
                case MaterialPaint.Grove:
                case MaterialPaint.Plant:
                case MaterialPaint.Moss:
                    canvas.Line(10, 10, 14, 48, new Color(0.16f, 0.32f, 0.1f));
                    canvas.Line(20, 12, 16, 50, new Color(0.16f, 0.32f, 0.1f));
                    canvas.FillCircle(11, 28, 3, tone);
                    canvas.FillCircle(21, 36, 3, Color.Lerp(tone, Color.black, 0.2f));
                    break;
                default:
                    canvas.Highlight(11, 14, 2, 28, 0.12f);
                    canvas.Noise(rng, new Color(0.05f, 0.04f, 0.04f, 0.3f), 8);
                    break;
            }

            return canvas.ToSprite(32, new Vector2(0.5f, 0.18f));
        }

        static void PaintCobble(PixelCanvas canvas, Color stone, HashRng rng)
        {
            var grout = Color.Lerp(stone, new Color(0.08f, 0.08f, 0.1f), 0.74f);
            canvas.Clear(grout);
            for (var row = 0; row < 4; row++)
            {
                var stagger = (row % 2) * 4;
                for (var col = -1; col < 4; col++)
                {
                    var x = col * 8 + stagger + rng.Range(-1, 2);
                    var y = row * 8 + rng.Range(0, 2);
                    var shade = rng.Jitter(((row + col) & 1) == 0
                        ? stone
                        : Color.Lerp(stone, Color.black, 0.16f), 0.06f);
                    var w = rng.Range(6, 9);
                    var h = rng.Range(6, 9);
                    canvas.FillRounded(x + 1, y + 1, w, h, 2, shade);
                    canvas.Highlight(x + 2, y + h - 1, 3, 1, 0.16f);
                    canvas.Shade(x + w - 2, y + 2, 2, 3, 0.1f);
                    if (rng.Value() < 0.35f)
                    {
                        canvas.Set(x + 3, y + 3, Color.Lerp(shade, Color.white, 0.14f));
                    }
                }
            }
        }

        static void PaintPlanks(PixelCanvas canvas, Color wood, Color grain, HashRng rng)
        {
            canvas.Clear(grain);
            for (var row = 0; row < 4; row++)
            {
                var y = row * 8;
                var tone = rng.Jitter((row & 1) == 0 ? wood : Color.Lerp(wood, Color.black, 0.14f), 0.04f);
                canvas.FillRounded(1, y + 1, 30, 6, 1, tone);
                canvas.Line(2, y + 3, 28, y + 3, new Color(grain.r, grain.g, grain.b, 0.35f));
                canvas.Set(6 + row * 5 + rng.Range(0, 3), y + 5, Color.Lerp(tone, Color.white, 0.12f));
                canvas.Set(20 + rng.Range(0, 6), y + 2, Color.Lerp(tone, Color.black, 0.2f));
            }
        }

        static void PaintVein(PixelCanvas canvas, Color vein, HashRng rng)
        {
            var x = rng.Range(3, 10);
            var y = rng.Range(12, 22);
            canvas.Blend(x, y, vein);
            canvas.Blend(x + 1, y + 1, vein);
            canvas.Blend(x + 2, y + 2, vein);
            canvas.Blend(rng.Range(18, 26), rng.Range(6, 14), vein);
            canvas.Blend(rng.Range(12, 20), rng.Range(22, 30), vein);
        }

        static int Hash(int x, int y, int salt)
        {
            unchecked
            {
                var h = (uint)(x * 374761393 + y * 668265263 + salt * 1274126177);
                h = (h ^ (h >> 13)) * 1274126177u;
                return (int)(h & 0x7fffffff);
            }
        }

        static Sprite Memo(string key, System.Func<Sprite> build)
        {
            if (Cache.TryGetValue(key, out var sprite) && sprite != null)
            {
                return sprite;
            }

            sprite = build();
            Cache[key] = sprite;
            return sprite;
        }
    }
}
