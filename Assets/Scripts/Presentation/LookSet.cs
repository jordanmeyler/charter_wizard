using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A named look you assign in the Inspector — one still or a
    /// looping clip. Play finds it by <see cref="id"/> (and aliases).
    /// This is the Unity path: slice a texture, drag the sprites here.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Magic/Look", fileName = "Look")]
    public sealed class LookSet : ScriptableObject
    {
        [Tooltip("What Play asks for: wall, wall-ice, bridge, tile-fire, fireball-shot…")]
        public string id = "wall";

        [Tooltip("Extra ids that use this same clip.")]
        public string[] aliases;

        [Tooltip("Unity sprites, in order. One sprite is a still; more than one loops.")]
        public Sprite[] frames;

        [Tooltip("Frames per second when Frames has more than one sprite.")]
        public float fps = 8f;

        public Sprite[] UsableFrames()
        {
            if (frames == null || frames.Length == 0)
            {
                return System.Array.Empty<Sprite>();
            }

            var count = 0;
            for (var i = 0; i < frames.Length; i++)
            {
                if (frames[i] != null)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return System.Array.Empty<Sprite>();
            }

            if (count == frames.Length)
            {
                return frames;
            }

            var kept = new Sprite[count];
            var write = 0;
            for (var i = 0; i < frames.Length; i++)
            {
                if (frames[i] != null)
                {
                    kept[write++] = frames[i];
                }
            }

            return kept;
        }
    }
}
