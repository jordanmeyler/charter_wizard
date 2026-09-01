using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// One lookup for authored looks. Play asks by id:
    /// a <see cref="LookSet"/>, a Sprite Sheet clip, art.json, or
    /// Resources/Sprites/{id}.png. Pack slices and painters stay
    /// downstream in <see cref="SpriteFactory"/>.
    /// </summary>
    public static class LookLibrary
    {
        static bool _loaded;
        static readonly Dictionary<string, Sprite[]> Clips = new(System.StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, float> Fps = new(System.StringComparer.OrdinalIgnoreCase);

        public static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            var looks = Resources.LoadAll<LookSet>(string.Empty);
            for (var i = 0; i < looks.Length; i++)
            {
                Register(looks[i]);
            }
        }

        public static void Register(LookSet look)
        {
            if (look == null)
            {
                return;
            }

            var frames = look.UsableFrames();
            if (frames.Length == 0)
            {
                return;
            }

            Store(look.id, frames, look.fps);
            if (look.aliases == null)
            {
                return;
            }

            for (var i = 0; i < look.aliases.Length; i++)
            {
                Store(look.aliases[i], frames, look.fps);
            }
        }

        public static bool TryAuthored(string id, out Sprite sprite)
        {
            if (TryAuthoredClip(id, out var frames) && frames[0] != null)
            {
                sprite = frames[0];
                return true;
            }

            sprite = null;
            return false;
        }

        public static bool TryAuthored(string[] ids, out Sprite sprite)
        {
            sprite = null;
            if (ids == null)
            {
                return false;
            }

            for (var i = 0; i < ids.Length; i++)
            {
                if (TryAuthored(ids[i], out sprite) && sprite != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryAuthoredClip(string id, out Sprite[] frames)
        {
            EnsureLoaded();
            if (!string.IsNullOrWhiteSpace(id) && Clips.TryGetValue(id.Trim(), out frames)
                && frames != null && frames.Length > 0)
            {
                return true;
            }

            if (SpriteSheetLibrary.TryClip(id, out frames) && frames != null && frames.Length > 0)
            {
                return true;
            }

            if (CatalogBook.TrySprite(id, out var still) && still != null)
            {
                if (SpriteSheetLibrary.TryClip(id, out var baked) && baked != null && baked.Length > 0)
                {
                    frames = baked;
                    return true;
                }

                frames = new[] { still };
                return true;
            }

            frames = null;
            return false;
        }

        public static bool TryAuthoredClip(string[] ids, out Sprite[] frames, out string matched)
        {
            frames = null;
            matched = null;
            if (ids == null)
            {
                return false;
            }

            for (var i = 0; i < ids.Length; i++)
            {
                if (TryAuthoredClip(ids[i], out frames) && frames != null && frames.Length > 0)
                {
                    matched = ids[i];
                    return true;
                }
            }

            return false;
        }

        public static float FpsOf(string id, float fallback = 8f)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(id) && Fps.TryGetValue(id.Trim(), out var fps))
            {
                return fps;
            }

            return SpriteSheetLibrary.FpsOf(id, fallback);
        }

        public static bool HasAuthoredClip(string id)
        {
            return TryAuthoredClip(id, out var frames) && frames != null && frames.Length > 1;
        }

        static void Store(string id, Sprite[] frames, float fps)
        {
            if (string.IsNullOrWhiteSpace(id) || frames == null || frames.Length == 0)
            {
                return;
            }

            var key = id.Trim();
            Clips[key] = frames;
            Fps[key] = fps > 0f ? fps : 8f;
        }
    }
}
