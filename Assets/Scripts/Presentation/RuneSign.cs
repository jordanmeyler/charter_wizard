using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A mark beside a picture of what it does. Fire sits next to a flame,
    /// Salt next to a standing body. The pairing is how Play learns.
    /// </summary>
    public static class RuneSign
    {
        public static void MountAltar(Transform parent, RuneId rune)
        {
            var baseView = Child(parent, "Altar", Vector3.zero, 5);
            baseView.sprite = SpriteFactory.AltarBase();
            baseView.color = Color.white;

            if (SpriteFactory.HasNature(rune))
            {
                var picture = Child(parent, "Nature", new Vector3(-0.28f, 0.42f, 0f), 7);
                picture.sprite = SpriteFactory.NatureOf(rune);
                picture.color = Color.white;
            }

            var mark = Child(parent, "Mark", new Vector3(0.3f, 0.48f, 0f), 8);
            mark.sprite = RuneMark.AsSprite(rune, RunePalette.MarkInk(rune));
            mark.color = Color.white;
            mark.transform.localScale = Vector3.one * 0.55f;

            var glow = SpriteFactory.HasNature(rune)
                ? Color.Lerp(RunePalette.Of(rune), Color.white, 0.15f)
                : new Color(0.9f, 0.82f, 0.55f);
            FixtureGlow.Attach(parent, new Color(glow.r, glow.g, glow.b, 0.5f), 1.45f, 0.12f);
        }

        public static void MountFloor(Transform parent, RuneId rune)
        {
            var slab = Child(parent, "Slab", Vector3.zero, 3);
            slab.sprite = SpriteFactory.FloorCarve();
            slab.color = Color.white;

            var mark = Child(parent, "Mark", new Vector3(0.12f, 0.04f, 0f), 4);
            mark.sprite = RuneMark.AsSprite(rune, RunePalette.MarkInk(rune));
            mark.color = Color.white;
            mark.transform.localScale = Vector3.one * 0.62f;

            if (SpriteFactory.HasNature(rune))
            {
                var picture = Child(parent, "Nature", new Vector3(-0.22f, 0.02f, 0f), 4);
                picture.sprite = SpriteFactory.NatureOf(rune);
                picture.color = Color.white;
                picture.transform.localScale = Vector3.one * 0.55f;
            }
        }

        public static void MountPillar(Transform parent, RuneId rune)
        {
            var shaft = Child(parent, "Shaft", Vector3.zero, 5);
            shaft.sprite = SpriteFactory.AspectColumn();
            shaft.color = Color.white;

            var mark = Child(parent, "Mark", new Vector3(0f, 0.72f, 0f), 7);
            mark.sprite = RuneMark.AsSprite(rune, RunePalette.MarkInk(rune));
            mark.color = Color.white;
            mark.transform.localScale = Vector3.one * 0.48f;

            if (SpriteFactory.HasNature(rune))
            {
                var picture = Child(parent, "Nature", new Vector3(0f, 1.18f, 0f), 8);
                picture.sprite = SpriteFactory.NatureOf(rune);
                picture.color = Color.white;
                picture.transform.localScale = Vector3.one * 0.62f;
            }

            var glow = SpriteFactory.HasNature(rune)
                ? RunePalette.Of(rune)
                : new Color(0.9f, 0.82f, 0.55f);
            FixtureGlow.Attach(parent, new Color(glow.r, glow.g, glow.b, 0.45f), 1.6f, 0.1f);
        }

        public static TextMesh NamePlate(Transform parent, RuneId rune, Vector3 offset)
        {
            var color = SpriteFactory.HasNature(rune)
                ? Color.Lerp(RunePalette.Of(rune), Color.white, 0.35f)
                : new Color(0.92f, 0.86f, 0.72f);
            var mesh = WorldLabel.Attach(parent, Title(rune), offset, color, 12);
            if (mesh != null)
            {
                mesh.gameObject.SetActive(GlyphView.IsDevelop);
            }

            return mesh;
        }

        public static string Title(RuneId rune)
        {
            switch (rune)
            {
                case RuneId.Salt: return "BODY";
                case RuneId.Mercury: return "SPIRIT";
                case RuneId.Sulphur: return "MIND";
                default: return RuneCatalog.NameOf(rune).ToUpperInvariant();
            }
        }

        public static void Pulse(Transform picture, RuneId rune, float time, float baseScale = 1f)
        {
            if (picture == null)
            {
                return;
            }

            var scale = 1f;
            switch (rune)
            {
                case RuneId.Fire:
                    scale = 0.96f + Mathf.Sin(time * 9f) * 0.06f;
                    break;
                case RuneId.Water:
                    scale = 0.97f + Mathf.Sin(time * 3.2f) * 0.04f;
                    break;
                case RuneId.Air:
                    scale = 0.95f + Mathf.Sin(time * 5.5f) * 0.07f;
                    break;
                default:
                    scale = 0.98f + Mathf.Sin(time * 1.6f) * 0.02f;
                    break;
            }

            picture.localScale = Vector3.one * (baseScale * scale);
        }

        static SpriteRenderer Child(Transform parent, string name, Vector3 local, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = local;
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
