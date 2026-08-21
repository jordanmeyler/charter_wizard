using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Plays a short clip of generated sprites. One component per
    /// renderer — idle, walk, flicker, or a one-shot pose.
    /// </summary>
    public sealed class SpriteAnim : MonoBehaviour
    {
        SpriteRenderer _renderer;
        Sprite[] _frames;
        string _clip = string.Empty;
        float _fps = 8f;
        float _age;
        bool _loop = true;
        int _index = -1;

        public bool FreezeWhenWorldHeld { get; set; }
        public string Clip => _clip;
        public int Frame => _index;

        public static SpriteAnim On(GameObject host, SpriteRenderer renderer = null)
        {
            if (host == null)
            {
                return null;
            }

            var anim = host.GetComponent<SpriteAnim>();
            if (anim == null)
            {
                anim = host.AddComponent<SpriteAnim>();
            }

            anim._renderer = renderer != null ? renderer : host.GetComponent<SpriteRenderer>();
            return anim;
        }

        public void Play(string clip, float fps = 8f, bool loop = true)
        {
            if (string.IsNullOrWhiteSpace(clip))
            {
                return;
            }

            if (_clip == clip && _frames != null && _frames.Length > 0)
            {
                _fps = fps;
                _loop = loop;
                return;
            }

            Play(SpriteFactory.Clip(clip), fps, loop, clip);
        }

        public void Play(Sprite[] frames, float fps, bool loop, string clip)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            _frames = frames;
            _fps = Mathf.Max(1f, fps);
            _loop = loop;
            _clip = clip ?? string.Empty;
            _age = 0f;
            _index = -1;
            Apply(0);
        }

        void LateUpdate()
        {
            if (FreezeWhenWorldHeld && AdeptAvatar.WorldHeld)
            {
                return;
            }

            if (_frames == null || _frames.Length <= 1)
            {
                return;
            }

            _age += Time.unscaledDeltaTime;
            var next = Mathf.FloorToInt(_age * _fps);
            if (!_loop)
            {
                next = Mathf.Min(next, _frames.Length - 1);
            }
            else
            {
                next %= _frames.Length;
            }

            Apply(next);
        }

        void Apply(int index)
        {
            if (_renderer == null || _frames == null || _frames.Length == 0)
            {
                return;
            }

            index = Mathf.Clamp(index, 0, _frames.Length - 1);
            if (index == _index)
            {
                return;
            }

            _index = index;
            if (_frames[index] != null)
            {
                _renderer.sprite = _frames[index];
            }
        }
    }
}
