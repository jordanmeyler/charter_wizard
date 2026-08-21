using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Hand-painted pixel actors and looping clips. SpriteFactory.Named
    /// still returns a still; Clip() is the walk / idle / flicker sheet.
    /// </summary>
    public static class SpriteActors
    {
        static readonly Color Clear = new(0f, 0f, 0f, 0f);
        static readonly Color Ink = new(0.1f, 0.05f, 0.12f);
        static readonly System.Collections.Generic.Dictionary<string, Sprite[]> Clips = new();

        public static Sprite Still(string id)
        {
            var clip = Clip(id);
            return clip != null && clip.Length > 0 ? clip[0] : null;
        }

        public static Sprite[] Clip(string id)
        {
            var key = (id ?? string.Empty).Trim().ToLowerInvariant();
            if (Clips.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            if (CatalogBook.TrySprite(key, out var custom) && custom != null)
            {
                return Cache(key, new[] { custom });
            }

            switch (key)
            {
                case "adept":
                case "adept-idle":
                    return Cache(key, Frames(4, frame => PaintAdept(frame, AdeptPose.Idle)));
                case "adept-walk":
                    return Cache(key, Frames(4, frame => PaintAdept(frame, AdeptPose.Walk)));
                case "adept-cast":
                    return Cache(key, Frames(2, frame => PaintAdept(frame, AdeptPose.Cast)));
                case "adept-hop":
                    return Cache(key, Frames(2, frame => PaintAdept(frame, AdeptPose.Hop)));
                case "ash-mite":
                case "mite":
                    return Cache(key, Frames(4, PaintAshMite));
                case "ice-thing":
                    return Cache(key, Frames(4, PaintIceThing));
                case "fire-golem":
                    return Cache(key, Frames(4, frame => PaintFireGolem(frame, false)));
                case "fire-golem-slam":
                    return Cache(key, Frames(2, frame => PaintFireGolem(frame, true)));
                case "stone-man":
                    return Cache(key, Frames(2, PaintStoneMan));
                case "warden":
                    return Cache(key, Frames(4, frame => PaintWarden(frame, false)));
                case "warden-cast":
                    return Cache(key, Frames(2, frame => PaintWarden(frame, true)));
                case "torch":
                case "torch-unlit":
                    return Cache(key, Frames(1, _ => PaintTorch(0, false)));
                case "torch-lit":
                    return Cache(key, Frames(4, frame => PaintTorch(frame, true)));
                case "rod":
                case "rod-idle":
                    return Cache(key, Frames(1, _ => PaintRod(0, false)));
                case "rod-live":
                    return Cache(key, Frames(4, frame => PaintRod(frame, true)));
                case "charm":
                    return Cache(key, Frames(3, PaintCharm));
                case "flame-curtain":
                    return Cache(key, Frames(4, PaintFlameCurtain));
                case "charge-curtain":
                    return Cache(key, Frames(4, PaintChargeCurtain));
                case "poison-veil":
                case "poison-fog":
                    return Cache(key, Frames(4, PaintPoisonVeil));
                case "spawn-crystal":
                    return Cache(key, Frames(4, PaintSpawnCrystal));
                case "ice-block":
                    return Cache(key, Frames(2, PaintIceBlock));
                case "arrow-rack":
                    return Cache(key, Frames(1, _ => PaintArrowRack()));
                case "rope":
                    return Cache(key, Frames(1, _ => PaintRope()));
                case "socket-gate":
                case "gate":
                    return Cache(key, Frames(2, PaintSocketGate));
                case "fireball-shot":
                    return Cache(key, Frames(3, PaintFireball));
                case "arrow-shot":
                    return Cache(key, Frames(1, _ => PaintArrowShot()));
                case "tile-fire":
                    return Cache(key, Frames(3, PaintTileFire));
                case "tile-wet":
                    return Cache(key, Frames(2, PaintTileWet));
                case "tile-charge":
                    return Cache(key, Frames(2, PaintTileCharge));
                case "tile-grow":
                    return Cache(key, Frames(2, PaintTileGrow));
                case "stone-fire":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.95f, 0.32f, 0.08f), new Color(1f, 0.78f, 0.28f))));
                case "stone-water":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.18f, 0.52f, 0.92f), new Color(0.7f, 0.9f, 1f))));
                case "stone-earth":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.48f, 0.32f, 0.14f), new Color(0.72f, 0.82f, 0.28f))));
                case "stone-air":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.72f, 0.86f, 0.92f), new Color(1f, 1f, 1f))));
                case "stone-body":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.88f, 0.84f, 0.72f), new Color(0.98f, 0.96f, 0.88f))));
                case "stone-spirit":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.55f, 0.42f, 0.82f), new Color(0.82f, 0.88f, 1f))));
                case "stone-mind":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.92f, 0.62f, 0.12f), new Color(1f, 0.92f, 0.45f))));
                case "stone-grove":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.22f, 0.62f, 0.22f), new Color(0.55f, 0.92f, 0.38f))));
                case "stone-flood":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.12f, 0.42f, 0.78f), new Color(0.55f, 0.82f, 1f))));
                case "stone-spark":
                    return Cache(key, Frames(2, frame => PaintStone(frame, new Color(0.95f, 0.78f, 0.18f), new Color(1f, 0.95f, 0.55f))));
                case "key-grove":
                    return Cache(key, Frames(2, frame => PaintKey(frame, new Color(0.28f, 0.72f, 0.28f))));
                case "key-flood":
                    return Cache(key, Frames(2, frame => PaintKey(frame, new Color(0.2f, 0.52f, 0.95f))));
                case "key-spark":
                    return Cache(key, Frames(2, frame => PaintKey(frame, new Color(0.98f, 0.82f, 0.2f))));
                case "key-spare":
                    return Cache(key, Frames(2, frame => PaintKey(frame, new Color(0.82f, 0.68f, 0.28f))));
                case "plaque":
                    return Cache(key, Frames(1, _ => PaintPlaque()));
                default:
                    return null;
            }
        }

        static Sprite[] Cache(string key, Sprite[] frames)
        {
            Clips[key] = frames;
            return frames;
        }

        enum AdeptPose
        {
            Idle,
            Walk,
            Cast,
            Hop
        }

        static Sprite[] Frames(int count, System.Func<int, Sprite> paint)
        {
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
            {
                frames[i] = paint(i);
            }

            return frames;
        }

        static Sprite PaintAdept(int frame, AdeptPose pose)
        {
            return Memo($"actor-adept-v2:{pose}:{frame}", () =>
            {
                var canvas = new PixelCanvas(64);
                canvas.Clear(Clear);
                var night = new Color(0.07f, 0.03f, 0.12f);
                var robe = new Color(0.36f, 0.16f, 0.64f);
                var fold = new Color(0.18f, 0.07f, 0.36f);
                var sheen = Color.Lerp(robe, Color.white, 0.14f);
                var gold = new Color(0.88f, 0.7f, 0.28f);
                var cowl = new Color(0.02f, 0.01f, 0.05f);
                var aether = new Color(0.66f, 0.44f, 1f);

                var lift = pose == AdeptPose.Hop ? 7
                    : pose == AdeptPose.Walk ? (frame & 1)
                    : frame == 1 || frame == 2 ? 1 : 0;
                var lean = pose == AdeptPose.Walk ? (frame < 2 ? -3 : 3)
                    : pose == AdeptPose.Idle ? (frame < 2 ? -1 : 1)
                    : pose == AdeptPose.Cast ? 1
                    : 0;
                var flare = pose == AdeptPose.Hop ? 6
                    : pose == AdeptPose.Walk ? 3
                    : pose == AdeptPose.Cast ? 2
                    : frame == 1 || frame == 2 ? 1 : 0;
                var voidGlow = pose == AdeptPose.Cast ? 0.5f
                    : pose == AdeptPose.Hop ? 0.32f
                    : 0.16f + (frame & 1) * 0.05f;

                canvas.GroundShadow(32, 8, pose == AdeptPose.Hop ? 6 : 11 + flare / 2, 3);

                var cx = 32 + lean;
                var y = 10 + lift;

                canvas.FillEllipse(cx, y + 3, 17 + flare, 7, night);
                canvas.FillEllipse(cx, y + 5, 16 + flare, 6, fold);
                canvas.FillEllipse(cx, y + 7, 15 + flare, 6, robe);
                canvas.FillRounded(cx - 12, y + 8, 24, 22, 8, fold);
                canvas.FillRounded(cx - 10, y + 10, 20, 20, 7, robe);
                canvas.Fill(cx - 6, y + 12, 3, 16, fold);
                canvas.Fill(cx + 3, y + 13, 2, 14, sheen);
                canvas.Fill(cx - 14 - flare / 2, y + 2, 28 + flare, 2, new Color(gold.r, gold.g, gold.b, 0.82f));

                if (pose == AdeptPose.Walk)
                {
                    canvas.FillEllipse(cx - lean * 2, y + 4, 9, 3, fold);
                }

                if (pose == AdeptPose.Cast)
                {
                    canvas.FillEllipse(cx - 15, y + 22, 7, 5, fold);
                    canvas.FillEllipse(cx + 15, y + 24, 7, 5, robe);
                    canvas.SoftCircle(cx, y + 22, 16, new Color(aether.r, aether.g, aether.b, 0.22f));
                }
                else
                {
                    canvas.FillEllipse(cx - 13, y + 18, 6, 4, fold);
                    canvas.FillEllipse(cx + 12, y + 17, 5, 3, Color.Lerp(robe, Color.black, 0.18f));
                }

                canvas.FillEllipse(cx, y + 32, 11, 10, night);
                canvas.FillEllipse(cx, y + 33, 10, 9, fold);
                canvas.FillEllipse(cx, y + 31, pose == AdeptPose.Cast ? 7 : 6, 6, cowl);
                canvas.SoftCircle(cx, y + 31, pose == AdeptPose.Cast ? 8 : 5,
                    new Color(aether.r, aether.g, aether.b, voidGlow));
                canvas.Fill(cx - 6, y + 26, 12, 1, new Color(gold.r, gold.g, gold.b, 0.7f));

                canvas.SoftEllipse(cx, y + 14, 13, 11, new Color(0.55f, 0.3f, 0.9f, 0.16f));
                canvas.Outline(night);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.22f));
            });
        }

        static Sprite PaintAshMite(int frame)
        {
            return Memo($"actor-mite-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var body = new Color(0.78f, 0.2f, 0.08f);
                var ember = new Color(1f, 0.7f, 0.2f);
                var leg = new Color(0.14f, 0.06f, 0.04f);
                var step = (frame % 2) * 2 - 1;
                canvas.GroundShadow(24, 14, 11, 4);
                canvas.SoftCircle(24, 22, 15, new Color(0.95f, 0.32f, 0.06f, 0.32f));
                DrawLeg(canvas, 8, 16 + step, 16, 22, leg);
                DrawLeg(canvas, 6, 24 - step, 16, 24, leg);
                DrawLeg(canvas, 9, 32 + step, 16, 27, leg);
                DrawLeg(canvas, 40, 16 - step, 32, 22, leg);
                DrawLeg(canvas, 42, 24 + step, 32, 24, leg);
                DrawLeg(canvas, 39, 32 - step, 32, 27, leg);
                canvas.FillEllipse(24, 22 + (frame & 1), 12, 10, new Color(0.18f, 0.05f, 0.04f));
                canvas.FillEllipse(24, 23 + (frame & 1), 11, 9, body);
                canvas.FillEllipse(24, 25 + (frame & 1), 7, 6, ember);
                canvas.FillCircle(24, 26 + (frame & 1), frame == 1 || frame == 2 ? 4 : 3, new Color(1f, 0.94f, 0.55f));
                canvas.FillCircle(20, 28 + (frame & 1), 2, Color.white);
                canvas.FillCircle(28, 28 + (frame & 1), 2, Color.white);
                canvas.Set(20, 28 + (frame & 1), Color.black);
                canvas.Set(28, 28 + (frame & 1), Color.black);
                canvas.Line(17, 20, 13, 15 + step, ember);
                canvas.Line(31, 20, 35, 15 - step, ember);
                canvas.Outline(Ink);
                return canvas.ToSprite(34, new Vector2(0.5f, 0.32f));
            });
        }

        static void DrawLeg(PixelCanvas canvas, int x0, int y0, int x1, int y1, Color color)
        {
            canvas.ThickLine(x0, y0, x1, y1, color);
            canvas.FillCircle(x0, y0, 1, color);
        }

        static Sprite PaintIceThing(int frame)
        {
            return Memo($"actor-ice-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var body = new Color(0.55f, 0.84f, 0.96f);
                var core = new Color(0.84f, 0.96f, 1f);
                var spike = new Color(0.78f, 0.93f, 1f);
                var lift = frame == 1 || frame == 2 ? 1 : 0;
                canvas.GroundShadow(24, 14, 10, 4);
                canvas.SoftCircle(24, 22, 16, new Color(0.45f, 0.8f, 1f, 0.28f + frame * 0.04f));
                canvas.FillCircle(24, 21 + lift, 11, new Color(0.2f, 0.38f, 0.52f));
                canvas.FillCircle(24, 22 + lift, 10, body);
                canvas.FillCircle(24, 24 + lift, 6, core);
                canvas.ThickLine(24, 32 + lift, 24, 42 + lift + (frame & 1), spike);
                canvas.ThickLine(14, 30 + lift, 9, 40 + lift, spike);
                canvas.ThickLine(34, 30 + lift, 39, 40 + lift, spike);
                canvas.FillCircle(20, 26 + lift, 2, Color.white);
                canvas.FillCircle(28, 26 + lift, 2, Color.white);
                canvas.Set(20, 26 + lift, new Color(0.08f, 0.2f, 0.35f));
                canvas.Set(28, 26 + lift, new Color(0.08f, 0.2f, 0.35f));
                canvas.Line(8 + frame, 18, 14, 12 + lift, spike);
                canvas.Line(40 - frame, 18, 34, 12 + lift, spike);
                canvas.Set(16, 28 + lift, Color.white);
                canvas.Outline(new Color(0.2f, 0.38f, 0.52f));
                return canvas.ToSprite(34, new Vector2(0.5f, 0.32f));
            });
        }

        static Sprite PaintFireGolem(int frame, bool slam)
        {
            return Memo($"actor-golem-v1:{slam}:{frame}", () =>
            {
                var canvas = new PixelCanvas(52);
                canvas.Clear(Clear);
                var body = new Color(0.78f, 0.2f, 0.05f);
                var ember = new Color(1f, 0.52f, 0.1f);
                var crack = new Color(1f, 0.82f, 0.28f);
                var bob = slam ? 0 : frame & 1;
                var arms = slam ? 10 + frame * 4 : 0;
                canvas.GroundShadow(26, 10, 12, 4);
                canvas.SoftCircle(26, 20, 16, new Color(1f, 0.28f, 0.04f, 0.38f + frame * 0.05f));
                canvas.FillRounded(13, 8 + bob, 26, 20, 4, new Color(0.22f, 0.06f, 0.03f));
                canvas.FillRounded(15, 10 + bob, 22, 16, 3, body);
                canvas.FillRounded(17, 28 + bob, 18, 10, 3, body);
                canvas.FillCircle(26, 36 + bob, 8, body);
                canvas.FillCircle(26, 22 + bob, 6, ember);
                canvas.FillCircle(26, 23 + bob, frame == 1 || slam ? 4 : 3, crack);
                canvas.Fill(22, 38 + bob, 3, 3, new Color(1f, 0.92f, 0.4f));
                canvas.Fill(28, 38 + bob, 3, 3, new Color(1f, 0.92f, 0.4f));
                canvas.Set(23, 39 + bob, Color.black);
                canvas.Set(29, 39 + bob, Color.black);
                canvas.FillRounded(7, 12 + bob + arms / 2, 7, 10, 2, ember);
                canvas.FillRounded(38, 12 + bob + arms / 2, 7, 10, 2, ember);
                canvas.Line(18, 16 + bob, 24, 24 + bob, crack);
                canvas.Line(32, 14 + bob, 28, 22 + bob, crack);
                canvas.FillCircle(26, 42 + bob + (slam ? 2 : 0), 3, crack);
                canvas.Outline(new Color(0.18f, 0.05f, 0.02f));
                return canvas.ToSprite(34, new Vector2(0.5f, 0.28f));
            });
        }

        static Sprite PaintStoneMan(int frame)
        {
            return Memo($"actor-stone-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(48);
                canvas.Clear(Clear);
                var rock = new Color(0.5f, 0.47f, 0.42f);
                var dark = new Color(0.22f, 0.2f, 0.18f);
                var moss = new Color(0.32f, 0.48f, 0.22f);
                var shift = frame == 1 ? 1 : 0;
                canvas.GroundShadow(24, 8, 9, 3);
                canvas.FillRounded(16 + shift, 6, 16, 18, 2, dark);
                canvas.FillRounded(18 + shift, 8, 12, 14, 2, rock);
                canvas.FillRounded(14, 24 + shift, 20, 8, 2, rock);
                canvas.FillRounded(18, 30 + shift, 12, 10, 2, rock);
                canvas.Fill(20, 36 + shift, 3, 3, dark);
                canvas.Fill(26, 36 + shift, 3, 3, dark);
                canvas.Line(20, 14, 28, 12 + shift, new Color(0.7f, 0.68f, 0.62f));
                canvas.Line(18, 20, 30, 22, dark);
                canvas.FillCircle(22, 16, 2, moss);
                canvas.FillCircle(28, 26 + shift, 2, moss);
                canvas.FillRounded(10, 16 + shift, 6, 8, 1, rock);
                canvas.FillRounded(32, 16 + shift, 6, 8, 1, rock);
                canvas.Outline(dark);
                return canvas.ToSprite(34, new Vector2(0.5f, 0.28f));
            });
        }

        static Sprite PaintWarden(int frame, bool casting)
        {
            return Memo($"actor-warden-v1:{casting}:{frame}", () =>
            {
                var canvas = new PixelCanvas(52);
                canvas.Clear(Clear);
                var cloak = new Color(0.26f, 0.14f, 0.48f);
                var fold = new Color(0.14f, 0.06f, 0.24f);
                var metal = new Color(0.74f, 0.72f, 0.62f);
                var gold = new Color(0.92f, 0.74f, 0.28f);
                var bob = casting ? 1 : frame & 1;
                var staffLift = casting ? 6 + frame * 2 : frame & 1;
                canvas.GroundShadow(24, 10, 9, 3);
                canvas.SoftCircle(24, 22, 14, new Color(0.55f, 0.35f, 0.9f, casting ? 0.42f : 0.22f));
                canvas.FillRounded(16, 8 + bob, 16, 22, 4, fold);
                canvas.FillRounded(18, 10 + bob, 12, 18, 3, cloak);
                canvas.FillRounded(20, 26 + bob, 8, 10, 2, cloak);
                canvas.FillCircle(24, 34 + bob, 6, metal);
                canvas.Fill(22, 36 + bob, 2, 2, fold);
                canvas.Fill(26, 36 + bob, 2, 2, fold);
                canvas.FillRounded(14, 18 + bob, 6, 5, 1, gold);
                canvas.FillRounded(28, 18 + bob, 6, 5, 1, gold);
                canvas.Fill(34, 8 + bob + staffLift, 3, 28, new Color(0.42f, 0.24f, 0.12f));
                canvas.Fill(32, 34 + bob + staffLift, 7, 6, metal);
                canvas.FillCircle(35, 40 + bob + staffLift, 3, gold);
                if (casting)
                {
                    canvas.SoftCircle(35, 42 + bob + staffLift, 8, new Color(1f, 0.82f, 0.3f, 0.45f));
                    canvas.FillCircle(24, 22 + bob, 3, new Color(1f, 0.7f, 0.25f, 0.7f));
                }

                canvas.Outline(Ink);
                return canvas.ToSprite(34, new Vector2(0.5f, 0.26f));
            });
        }

        static Sprite PaintTorch(int frame, bool lit)
        {
            return Memo($"actor-torch-v1:{lit}:{frame}", () =>
            {
                var canvas = new PixelCanvas(32, 52);
                canvas.Clear(Clear);
                var pole = new Color(0.44f, 0.26f, 0.12f);
                canvas.FillRounded(14, 4, 4, 26, 1, pole);
                canvas.Highlight(15, 6, 1, 20, 0.18f);
                canvas.FillRounded(10, 26, 12, 7, 2, new Color(0.3f, 0.22f, 0.16f));
                canvas.Rect(10, 26, 12, 7, new Color(0.14f, 0.1f, 0.08f));
                if (lit)
                {
                    var rise = frame % 4;
                    canvas.SoftCircle(16, 38 + rise / 2, 11, new Color(1f, 0.42f, 0.06f, 0.5f));
                    canvas.FillCircle(16, 36 + (rise == 1 ? 1 : 0), 7, new Color(0.95f, 0.4f, 0.06f));
                    canvas.FillCircle(15 + (rise == 2 ? 1 : 0), 38 + (rise & 1), 5, new Color(1f, 0.76f, 0.2f));
                    canvas.FillCircle(16, 41 + (rise == 3 ? 1 : 0), 2, new Color(1f, 0.96f, 0.72f));
                    canvas.Set(13 + rise, 34, new Color(1f, 0.7f, 0.2f, 0.8f));
                }
                else
                {
                    canvas.FillRounded(13, 32, 6, 5, 1, new Color(0.14f, 0.1f, 0.08f));
                    canvas.Line(16, 36, 16, 41, new Color(0.28f, 0.2f, 0.14f));
                }

                canvas.Outline(new Color(0.12f, 0.07f, 0.05f));
                return canvas.ToSprite(32, new Vector2(0.5f, 0.18f));
            });
        }

        static Sprite PaintRod(int frame, bool charged)
        {
            return Memo($"actor-rod-v1:{charged}:{frame}", () =>
            {
                var canvas = new PixelCanvas(32, 52);
                canvas.Clear(Clear);
                var metal = new Color(0.68f, 0.7f, 0.76f);
                var copper = new Color(0.8f, 0.48f, 0.2f);
                canvas.FillRounded(9, 3, 14, 5, 1, new Color(0.24f, 0.2f, 0.16f));
                canvas.Fill(14, 8, 4, 24, metal);
                canvas.Highlight(15, 10, 1, 20, 0.3f);
                canvas.FillRounded(11, 18, 10, 12, 2, copper);
                canvas.Fill(13, 20, 6, 8, new Color(0.92f, 0.6f, 0.24f));
                canvas.FillCircle(16, 36, 5, charged ? new Color(0.98f, 0.92f, 0.4f) : metal);
                if (charged)
                {
                    canvas.SoftCircle(16, 38, 9, new Color(0.75f, 0.9f, 1f, 0.4f + frame * 0.08f));
                    var bolt = new Color(0.88f, 0.96f, 1f);
                    var jag = frame % 4;
                    canvas.ThickLine(16, 42, 10 + jag, 46, bolt);
                    canvas.ThickLine(10 + jag, 46, 20 - jag, 48, bolt);
                    canvas.ThickLine(20 - jag, 48, 14, 50, bolt);
                }

                canvas.Outline(new Color(0.12f, 0.1f, 0.1f));
                return canvas.ToSprite(32, new Vector2(0.5f, 0.18f));
            });
        }

        static Sprite PaintCharm(int frame)
        {
            return Memo($"actor-charm-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var gold = new Color(0.92f, 0.66f, 0.2f);
                var core = new Color(1f, 0.4f, 0.1f);
                canvas.SoftCircle(16, 16, 13, new Color(1f, 0.55f, 0.12f, 0.32f + frame * 0.08f));
                canvas.FillCircle(16, 15, 10, Color.Lerp(gold, Color.black, 0.2f));
                canvas.FillCircle(16, 16, 9, gold);
                canvas.FillCircle(16, 16, 6, core);
                canvas.Fill(15, 24, 2, 6, gold);
                canvas.FillCircle(16, 16, frame == 1 ? 4 : 3, new Color(1f, 0.88f, 0.45f));
                canvas.Set(13, 20, Color.white);
                canvas.Outline(new Color(0.35f, 0.18f, 0.04f));
                return canvas.ToSprite(28, new Vector2(0.5f, 0.4f));
            });
        }

        static Sprite PaintChargeCurtain(int frame)
        {
            return Memo($"actor-charge-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(32, 52);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 28, 16, new Color(0.4f, 0.7f, 1f, 0.38f));
                canvas.FillRounded(6, 4, 20, 10, 2, new Color(0.12f, 0.16f, 0.22f));
                var bolt = new Color(0.65f, 0.86f, 1f);
                var core = new Color(1f, 0.95f, 0.55f);
                var lift = frame % 4;
                canvas.FillCircle(10, 22 + (lift == 0 ? 1 : 0), 6, bolt);
                canvas.FillCircle(16, 28 + (lift == 1 ? 2 : 0), 7, bolt);
                canvas.FillCircle(22, 24 + (lift == 2 ? 1 : 0), 6, bolt);
                canvas.ThickLine(12, 18 + lift, 18, 30, core);
                canvas.ThickLine(18, 30, 14, 38 + (lift & 1), core);
                canvas.ThickLine(20, 20, 16, 34 + lift, core);
                canvas.FillCircle(16, 38 + (lift == 3 ? 1 : 0), 3, core);
                canvas.Outline(new Color(0.12f, 0.22f, 0.4f));
                return canvas.ToSprite(32, new Vector2(0.5f, 0.18f));
            });
        }

        static Sprite PaintFlameCurtain(int frame)
        {
            return Memo($"actor-curtain-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(32, 52);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 28, 16, new Color(0.58f, 0.2f, 0.92f, 0.38f));
                canvas.FillRounded(6, 4, 20, 10, 2, new Color(0.16f, 0.06f, 0.22f));
                var ember = new Color(0.7f, 0.3f, 1f);
                var gold = new Color(0.9f, 0.58f, 1f);
                var lift = frame % 4;
                canvas.FillCircle(10, 22 + (lift == 0 ? 1 : 0), 7, ember);
                canvas.FillCircle(16, 28 + (lift == 1 ? 2 : 0), 8, ember);
                canvas.FillCircle(22, 24 + (lift == 2 ? 1 : 0), 7, ember);
                canvas.FillCircle(16, 34 + lift, 6, gold);
                canvas.FillCircle(12, 30 + (lift & 1), 4, gold);
                canvas.FillCircle(20, 32 + ((lift + 1) & 1), 4, gold);
                canvas.FillCircle(16, 40 + (lift == 3 ? 1 : 0), 3, new Color(0.96f, 0.86f, 1f));
                canvas.Outline(new Color(0.22f, 0.06f, 0.32f));
                return canvas.ToSprite(32, new Vector2(0.5f, 0.18f));
            });
        }

        static Sprite PaintPoisonVeil(int frame)
        {
            return Memo($"actor-poison-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(36);
                canvas.Clear(Clear);
                var drift = frame - 1;
                canvas.SoftCircle(10 + drift, 12, 11, new Color(0.35f, 0.82f, 0.22f, 0.5f));
                canvas.SoftCircle(22 - drift, 16 + (frame & 1), 12, new Color(0.55f, 0.95f, 0.28f, 0.4f));
                canvas.SoftCircle(16, 22 - drift, 10, new Color(0.22f, 0.55f, 0.12f, 0.48f));
                canvas.FillCircle(12 + drift, 18, 3, new Color(0.7f, 1f, 0.35f, 0.7f));
                canvas.FillCircle(20 - drift, 14, 2, new Color(0.85f, 1f, 0.45f, 0.8f));
                return canvas.ToSprite(28);
            });
        }

        static Sprite PaintSpawnCrystal(int frame)
        {
            return Memo($"actor-crystal-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(36);
                canvas.Clear(Clear);
                var gem = new Color(0.72f, 0.5f, 1f);
                var rim = new Color(0.96f, 0.9f, 1f);
                var pulse = frame == 1 || frame == 2;
                canvas.GroundShadow(18, 8, 8, 3);
                canvas.SoftCircle(18, 18, pulse ? 14 : 11, new Color(0.7f, 0.5f, 1f, pulse ? 0.5f : 0.32f));
                canvas.ThickLine(18, 6, 10, 18, gem);
                canvas.ThickLine(18, 6, 26, 18, gem);
                canvas.ThickLine(10, 18, 18, 30, gem);
                canvas.ThickLine(26, 18, 18, 30, gem);
                canvas.FillCircle(18, 18, 7, gem);
                canvas.Fill(16, 8, 4, 20, rim);
                canvas.Fill(8, 16, 20, 4, rim);
                canvas.FillCircle(18, 18, pulse ? 4 : 3, Color.white);
                canvas.Outline(new Color(0.32f, 0.16f, 0.5f));
                return canvas.ToSprite(28, new Vector2(0.5f, 0.32f));
            });
        }

        static Sprite PaintIceBlock(int frame)
        {
            return Memo($"actor-iceblock-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 14, 14, new Color(0.55f, 0.85f, 1f, 0.3f + frame * 0.08f));
                canvas.FillRounded(5, 4, 22, 24, 3, new Color(0.58f, 0.8f, 0.94f));
                canvas.FillRounded(7, 6, 18, 20, 2, new Color(0.78f, 0.92f, 1f));
                canvas.Fill(8, 18, 16, 8, new Color(0.42f, 0.68f, 0.86f, 0.55f));
                canvas.Line(8 + frame, 22, 14 + frame, 8, new Color(1f, 1f, 1f, 0.75f));
                canvas.Line(18, 24, 24, 10, new Color(0.85f, 0.95f, 1f, 0.55f));
                canvas.Set(11, 20, Color.white);
                canvas.Outline(new Color(0.28f, 0.5f, 0.68f));
                return canvas.ToSprite(28);
            });
        }

        static Sprite PaintArrowRack()
        {
            return Memo("actor-rack-v1", () =>
            {
                var canvas = new PixelCanvas(32, 42);
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

                canvas.Outline(dark);
                return canvas.ToSprite(32, new Vector2(0.5f, 0.2f));
            });
        }

        static Sprite PaintRope()
        {
            return Memo("actor-rope-v1", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var coil = new Color(0.74f, 0.5f, 0.22f);
                var dark = new Color(0.42f, 0.26f, 0.1f);
                canvas.Circle(16, 16, 10, coil);
                canvas.Circle(16, 16, 9, dark);
                canvas.Circle(16, 16, 7, coil);
                canvas.Circle(16, 16, 6, dark);
                canvas.Circle(16, 16, 4, coil);
                canvas.FillCircle(16, 16, 2, dark);
                canvas.FillRounded(22, 14, 8, 4, 1, coil);
                canvas.Line(24, 15, 28, 8, dark);
                canvas.Outline(dark);
                return canvas.ToSprite(28);
            });
        }

        static Sprite PaintSocketGate(int frame)
        {
            return Memo($"actor-gate-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(36, 44);
                canvas.Clear(Clear);
                var stone = new Color(0.42f, 0.38f, 0.34f);
                var gold = new Color(0.92f, 0.74f, 0.28f);
                canvas.FillRounded(4, 2, 28, 36, 3, new Color(0.2f, 0.16f, 0.14f));
                canvas.FillRounded(8, 6, 20, 28, 2, stone);
                canvas.Fill(11, 10, 14, 20, new Color(0.06f, 0.04f, 0.06f));
                var glow = frame == 1 ? 4 : 3;
                canvas.FillCircle(18, 30, glow, gold);
                canvas.FillCircle(13, 22, 2, gold);
                canvas.FillCircle(23, 22, 2, gold);
                canvas.FillCircle(18, 15, 2, gold);
                canvas.Highlight(9, 32, 16, 2, 0.16f);
                canvas.Outline(new Color(0.12f, 0.1f, 0.08f));
                return canvas.ToSprite(32, new Vector2(0.5f, 0.26f));
            });
        }

        static Sprite PaintFireball(int frame)
        {
            return Memo($"actor-fireball-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(28);
                canvas.Clear(Clear);
                var grow = frame == 1 ? 1 : 0;
                canvas.SoftCircle(14, 14, 11 + grow, new Color(1f, 0.32f, 0.04f, 0.55f));
                canvas.FillCircle(14, 14, 7 + grow, new Color(1f, 0.45f, 0.08f));
                canvas.FillCircle(14, 14, 4, new Color(1f, 0.85f, 0.3f));
                canvas.FillCircle(15, 13, 2, Color.white);
                canvas.SoftCircle(8 - frame, 14, 4, new Color(1f, 0.5f, 0.1f, 0.4f));
                return canvas.ToSprite(22);
            });
        }

        static Sprite PaintArrowShot()
        {
            return Memo("actor-arrow-v1", () =>
            {
                var canvas = new PixelCanvas(32, 12);
                canvas.Clear(Clear);
                var shaft = new Color(0.55f, 0.32f, 0.12f);
                var head = new Color(0.72f, 0.74f, 0.78f);
                canvas.Fill(2, 5, 22, 2, shaft);
                canvas.Fill(22, 3, 8, 6, head);
                canvas.Fill(24, 4, 6, 4, Color.white);
                canvas.Fill(1, 3, 4, 6, new Color(0.85f, 0.45f, 0.15f));
                canvas.Outline(new Color(0.2f, 0.1f, 0.05f));
                return canvas.ToSprite(24, new Vector2(0.8f, 0.5f));
            });
        }

        static Sprite PaintKey(int frame, Color gem)
        {
            return Memo($"actor-key-v1:{gem}:{frame}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var lift = frame == 1 ? 1 : 0;
                var rim = Color.Lerp(gem, Color.white, 0.4f);
                canvas.GroundShadow(16, 8, 7, 3);
                canvas.FillCircle(11, 19 + lift, 6, Color.Lerp(gem, Color.black, 0.35f));
                canvas.FillCircle(11, 20 + lift, 5, gem);
                canvas.Circle(11, 20 + lift, 3, rim);
                canvas.Fill(15, 18 + lift, 12, 4, gem);
                canvas.Fill(23, 14 + lift, 3, 8, gem);
                canvas.Fill(20, 14 + lift, 3, 3, gem);
                canvas.Circle(11, 20 + lift, 6, rim);
                canvas.Outline(Color.Lerp(gem, Color.black, 0.5f));
                return canvas.ToSprite(28, new Vector2(0.5f, 0.38f));
            });
        }

        static Sprite PaintStone(int frame, Color gem, Color rim)
        {
            return Memo($"actor-stone-v1:{gem}:{rim}:{frame}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                canvas.GroundShadow(16, 8, 8, 3);
                canvas.SoftCircle(16, 16, frame == 1 ? 13 : 11, new Color(gem.r, gem.g, gem.b, 0.38f));
                canvas.FillCircle(16, 14, 9, Color.Lerp(gem, Color.black, 0.4f));
                canvas.FillCircle(16, 16, 8, gem);
                canvas.FillCircle(16, 18, 5, Color.Lerp(gem, rim, 0.55f));
                canvas.FillCircle(16, 19, 2, Color.Lerp(rim, Color.white, 0.55f));
                canvas.Set(13, 21, Color.white);
                canvas.Line(12, 14, 16, 22, Color.Lerp(rim, Color.white, 0.35f));
                canvas.Line(20, 15, 16, 22, Color.Lerp(gem, Color.black, 0.25f));
                canvas.Circle(16, 16, 8, rim);
                canvas.Fill(15, 6, 2, 5, rim);
                canvas.FillCircle(16, 6, 2, rim);
                canvas.Outline(Color.Lerp(gem, Color.black, 0.55f));
                return canvas.ToSprite(28, new Vector2(0.5f, 0.38f));
            });
        }

        static Sprite PaintPlaque()
        {
            return Memo("actor-plaque-v1", () =>
            {
                var canvas = new PixelCanvas(32, 16);
                canvas.Clear(Clear);
                canvas.FillRounded(1, 1, 30, 14, 2, new Color(0.46f, 0.3f, 0.14f));
                canvas.Rect(1, 1, 30, 14, new Color(0.22f, 0.14f, 0.08f));
                canvas.Fill(3, 3, 26, 3, new Color(0.32f, 0.2f, 0.1f));
                canvas.Highlight(4, 10, 8, 1, 0.14f);
                canvas.Line(6, 8, 12, 8, new Color(0.72f, 0.56f, 0.28f));
                canvas.Line(14, 8, 26, 8, new Color(0.62f, 0.44f, 0.2f));
                canvas.Outline(new Color(0.16f, 0.1f, 0.06f));
                return canvas.ToSprite(24);
            });
        }

        static Sprite PaintTileFire(int frame)
        {
            return Memo($"actor-tilefire-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                var lift = frame % 3;
                canvas.SoftCircle(16, 12 + lift, 12, new Color(1f, 0.4f, 0.06f, 0.55f));
                canvas.FillCircle(12, 10 + lift, 5, new Color(1f, 0.5f, 0.1f, 0.7f));
                canvas.FillCircle(20, 11 + (lift == 2 ? 1 : 0), 4, new Color(1f, 0.7f, 0.2f, 0.65f));
                canvas.FillCircle(16, 16 + lift, 3, new Color(1f, 0.92f, 0.55f, 0.8f));
                return canvas.ToSprite(32);
            });
        }

        static Sprite PaintTileWet(int frame)
        {
            return Memo($"actor-tilewet-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 16, 13, new Color(0.25f, 0.55f, 0.95f, 0.42f));
                canvas.Line(4 + frame, 14, 28 - frame, 12, new Color(0.7f, 0.88f, 1f, 0.45f));
                canvas.Line(6, 20 + frame, 24, 18, new Color(0.55f, 0.78f, 0.95f, 0.3f));
                return canvas.ToSprite(32);
            });
        }

        static Sprite PaintTileCharge(int frame)
        {
            return Memo($"actor-tilecharge-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 16, 12, new Color(0.75f, 0.9f, 1f, 0.4f));
                var bolt = new Color(0.9f, 0.96f, 1f, 0.85f);
                canvas.ThickLine(8, 20, 14 + frame, 12, bolt);
                canvas.ThickLine(14 + frame, 12, 22, 22 - frame, bolt);
                canvas.ThickLine(22, 22 - frame, 26, 10, bolt);
                return canvas.ToSprite(32);
            });
        }

        static Sprite PaintTileGrow(int frame)
        {
            return Memo($"actor-tilegrow-v1:{frame}", () =>
            {
                var canvas = new PixelCanvas(32);
                canvas.Clear(Clear);
                canvas.SoftCircle(16, 14, 11, new Color(0.3f, 0.7f, 0.22f, 0.4f));
                canvas.FillCircle(12, 12 + frame, 4, new Color(0.34f, 0.62f, 0.22f, 0.7f));
                canvas.FillCircle(20, 16, 5, new Color(0.28f, 0.52f, 0.18f, 0.65f));
                canvas.Line(16, 8, 16, 20 + frame, new Color(0.18f, 0.36f, 0.12f, 0.7f));
                return canvas.ToSprite(32);
            });
        }

        static Sprite Memo(string key, System.Func<Sprite> build)
        {
            return SpriteFactory.MemoPublic(key, build);
        }
    }
}
