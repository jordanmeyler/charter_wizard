using UnityEngine;

namespace RuneMagic
{
    [System.Serializable]
    public sealed class SpriteSheetClip
    {
        public string name = "idle";
        [Tooltip("Unity sprites, in order. When set, Play uses these and ignores Start / Count.")]
        public Sprite[] sprites;
        public int start;
        public int count = 4;
        public float fps = 8f;
    }

    /// <summary>
    /// A sliced sheet you drop in the project. Each clip is a run of
    /// cells. Register it under Resources so play can find it by id.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Magic/Sprite Sheet", fileName = "SpriteSheet")]
    public sealed class SpriteSheet : ScriptableObject
    {
        public string id = "adept";
        public Texture2D texture;
        public int cellWidth = 16;
        public int cellHeight = 16;
        public float pixelsPerUnit = 16f;
        public Vector2 pivot = new(0.5f, 0.5f);
        public SpriteSheetClip[] clips;

        public Sprite[] FramesOf(string clipName)
        {
            var clip = FindClip(clipName);
            return clip != null ? Slice(clip) : System.Array.Empty<Sprite>();
        }

        public SpriteSheetClip FindClip(string clipName)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(clipName))
            {
                return clips[0];
            }

            var key = clipName.Trim().ToLowerInvariant();
            for (var i = 0; i < clips.Length; i++)
            {
                var name = clips[i] != null ? clips[i].name : null;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (name.Trim().ToLowerInvariant() == key)
                {
                    return clips[i];
                }

                var prefixed = (id + "-" + name).Trim().ToLowerInvariant();
                if (prefixed == key)
                {
                    return clips[i];
                }
            }

            if (key == (id ?? string.Empty).Trim().ToLowerInvariant())
            {
                return clips[0];
            }

            return null;
        }

        public Sprite[] Slice(SpriteSheetClip clip)
        {
            if (clip == null)
            {
                return System.Array.Empty<Sprite>();
            }

            if (clip.sprites != null && clip.sprites.Length > 0)
            {
                var count = 0;
                for (var i = 0; i < clip.sprites.Length; i++)
                {
                    if (clip.sprites[i] != null)
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    return System.Array.Empty<Sprite>();
                }

                if (count == clip.sprites.Length)
                {
                    return clip.sprites;
                }

                var kept = new Sprite[count];
                var write = 0;
                for (var i = 0; i < clip.sprites.Length; i++)
                {
                    if (clip.sprites[i] != null)
                    {
                        kept[write++] = clip.sprites[i];
                    }
                }

                return kept;
            }

            if (texture == null || clip.count <= 0 || cellWidth <= 0 || cellHeight <= 0)
            {
                return System.Array.Empty<Sprite>();
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            var cols = Mathf.Max(1, texture.width / cellWidth);
            var frames = new Sprite[clip.count];
            var ppu = pixelsPerUnit > 0f ? pixelsPerUnit : 16f;
            for (var i = 0; i < clip.count; i++)
            {
                var index = clip.start + i;
                var col = index % cols;
                var row = index / cols;
                var x = col * cellWidth;
                var y = texture.height - (row + 1) * cellHeight;
                if (x < 0 || y < 0 || x + cellWidth > texture.width || y + cellHeight > texture.height)
                {
                    frames[i] = frames[0];
                    continue;
                }

                frames[i] = Sprite.Create(
                    texture,
                    new Rect(x, y, cellWidth, cellHeight),
                    pivot,
                    ppu);
            }

            return frames;
        }
    }
}
