using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Soft light under a lock or charm. Keeps the world readable without a 2D light stack.
    /// </summary>
    public sealed class FixtureGlow : MonoBehaviour
    {
        Color _color;
        float _pulse;
        SpriteRenderer _renderer;

        public static FixtureGlow Attach(Transform parent, Color color, float scale = 1.6f, float pulse = 0.12f)
        {
            var host = new GameObject("Glow");
            host.transform.SetParent(parent, false);
            host.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            host.transform.localScale = Vector3.one * scale;
            var glow = host.AddComponent<FixtureGlow>();
            glow._color = color;
            glow._pulse = pulse;
            glow._renderer = host.AddComponent<SpriteRenderer>();
            glow._renderer.sprite = SpriteFactory.Glow(color);
            glow._renderer.sortingOrder = 2;
            return glow;
        }

        void Update()
        {
            if (_renderer == null)
            {
                return;
            }

            var wave = 0.82f + Mathf.Sin(Time.time * (3.4f + _pulse * 20f)) * _pulse;
            var color = _color;
            color.a = _color.a * wave;
            _renderer.color = color;
        }
    }
}
