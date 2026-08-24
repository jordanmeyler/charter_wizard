using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Loads every <see cref="SpriteSheet"/> under Resources and serves
    /// clips by id (<c>adept-walk</c>, <c>fireball-shot</c>, <c>ice-melt</c>).
    /// </summary>
    public static class SpriteSheetLibrary
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
            var sheets = Resources.LoadAll<SpriteSheet>(string.Empty);
            for (var i = 0; i < sheets.Length; i++)
            {
                Register(sheets[i]);
            }
        }

        public static void Register(SpriteSheet sheet)
        {
            if (sheet == null || sheet.clips == null)
            {
                return;
            }

            for (var i = 0; i < sheet.clips.Length; i++)
            {
                var clip = sheet.clips[i];
                if (clip == null)
                {
                    continue;
                }

                var frames = sheet.Slice(clip);
                if (frames == null || frames.Length == 0)
                {
                    continue;
                }

                Store(clip.name, frames, clip.fps);
                if (!string.IsNullOrEmpty(sheet.id))
                {
                    Store(sheet.id + "-" + clip.name, frames, clip.fps);
                    if (i == 0)
                    {
                        Store(sheet.id, frames, clip.fps);
                    }
                }
            }
        }

        public static bool TryClip(string id, out Sprite[] frames)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(id) && Clips.TryGetValue(id.Trim(), out frames) && frames != null && frames.Length > 0)
            {
                return true;
            }

            frames = null;
            return false;
        }

        public static bool TrySprite(string id, out Sprite sprite)
        {
            if (TryClip(id, out var frames) && frames[0] != null)
            {
                sprite = frames[0];
                return true;
            }

            sprite = null;
            return false;
        }

        public static float FpsOf(string id, float fallback = 8f)
        {
            EnsureLoaded();
            return !string.IsNullOrEmpty(id) && Fps.TryGetValue(id.Trim(), out var fps) ? fps : fallback;
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
