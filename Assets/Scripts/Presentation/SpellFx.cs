using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Visible cast: a bolt, wall, or burst, then the lock resolves.
    /// Never throws — a failed effect still completes so the room can resolve.
    /// </summary>
    public sealed class SpellFx : MonoBehaviour
    {
        Vector3 _from;
        Vector3 _to;
        Vector3 _mid;
        RuneId _material;
        RuneId _aspect;
        float _age;
        float _duration = 0.35f;
        System.Action _done;
        SpriteRenderer _body;
        bool _finished;

        public static void Play(
            Vector3 from,
            Vector3 to,
            RuneId material,
            RuneId aspect,
            string caption,
            System.Action done)
        {
            try
            {
                var host = new GameObject("SpellFx");
                var fx = host.AddComponent<SpellFx>();
                if (!fx.Begin(from, to, material, aspect, done))
                {
                    Object.Destroy(host);
                    done?.Invoke();
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Spell effect failed: " + exception.Message);
                done?.Invoke();
            }
        }

        bool Begin(Vector3 from, Vector3 to, RuneId material, RuneId aspect, System.Action done)
        {
            _done = done;
            _from = from;
            _to = to;
            _mid = Vector3.Lerp(_from, _to, 0.45f) + new Vector3(0f, 0.35f, 0f);
            _material = material;
            _aspect = aspect;
            _duration = DurationFor(aspect, _from, _to);
            transform.position = _from;

            var color = RunePalette.Of(material == RuneId.None ? RuneId.Aether : material);
            CreateSprite("Glow", SpriteFactory.Glow(color), 18, new Vector3(1.4f, 1.4f, 1f));
            _body = CreateSprite("Body", BodySprite(), 19, Vector3.one);
            _body.color = color;
            return true;
        }

        SpriteRenderer CreateSprite(string name, Sprite sprite, int order, Vector3 scale)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            child.transform.localScale = scale;
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        Sprite BodySprite()
        {
            var color = RunePalette.Of(_material == RuneId.None ? RuneId.Aether : _material);
            if (_aspect == RuneId.Salt)
            {
                return SpriteFactory.Pillar(color);
            }

            if (_aspect == RuneId.Sulphur)
            {
                return SpriteFactory.Burst(color);
            }

            return SpriteFactory.Bolt(color);
        }

        void Update()
        {
            if (_finished)
            {
                return;
            }

            _age += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_age / Mathf.Max(0.05f, _duration));
            var ease = t * t * (3f - 2f * t);

            if (_aspect == RuneId.Salt)
            {
                transform.position = _to;
                transform.localScale = new Vector3(1f, Mathf.Lerp(0.2f, 1.35f, ease), 1f);
            }
            else if (_aspect == RuneId.Sulphur)
            {
                transform.position = _to;
                transform.localScale = Vector3.one * Mathf.Lerp(0.35f, 2.2f, ease);
                if (_body != null)
                {
                    var color = _body.color;
                    color.a = 1f - ease * 0.55f;
                    _body.color = color;
                }
            }
            else
            {
                var point = SampleArc(ease);
                if (float.IsNaN(point.x) || float.IsNaN(point.y))
                {
                    point = _to;
                }

                transform.position = point;
                var next = SampleArc(Mathf.Min(1f, ease + 0.08f));
                var delta = next - point;
                if (delta.sqrMagnitude > 0.0001f)
                {
                    var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }

                transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.15f, Mathf.Sin(ease * Mathf.PI));
            }

            if (t < 1f)
            {
                return;
            }

            Finish();
        }

        Vector3 SampleArc(float t)
        {
            var a = Vector3.Lerp(_from, _mid, t);
            var b = Vector3.Lerp(_mid, _to, t);
            return Vector3.Lerp(a, b, t);
        }

        void Finish()
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            try
            {
                var flash = new GameObject("Impact");
                flash.transform.position = _to;
                var renderer = flash.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteFactory.Burst(RunePalette.Of(
                    _material == RuneId.None ? RuneId.Aether : _material));
                renderer.sortingOrder = 21;
                flash.transform.localScale = Vector3.one * 1.4f;
                Destroy(flash, 0.2f);
            }
            catch (System.Exception)
            {
            }

            var callback = _done;
            _done = null;
            callback?.Invoke();
            Destroy(gameObject);
        }

        static float DurationFor(RuneId aspect, Vector3 from, Vector3 to)
        {
            if (aspect == RuneId.Salt || aspect == RuneId.Sulphur)
            {
                return 0.42f;
            }

            var distance = Vector2.Distance(from, to);
            if (float.IsNaN(distance))
            {
                return 0.35f;
            }

            return Mathf.Clamp(0.22f + distance * 0.08f, 0.28f, 0.7f);
        }
    }
}
