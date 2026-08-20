using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Visible cast: shot, pillar, spread, or remote, then the lock may resolve.
    /// Never throws — a failed effect still completes so the room can resolve.
    /// </summary>
    public sealed class SpellFx : MonoBehaviour
    {
        Vector3 _from;
        Vector3 _to;
        Vector3 _mid;
        RuneId _material;
        SpellShape _shape;
        float _age;
        float _duration = 0.35f;
        float _potency = 1f;
        System.Action _done;
        SpriteRenderer _body;
        bool _finished;

        public static void Play(
            Vector3 from,
            Vector3 to,
            RuneId material,
            SpellShape shape,
            string caption,
            System.Action done,
            float potency = 1f)
        {
            try
            {
                var host = new GameObject("SpellFx");
                var fx = host.AddComponent<SpellFx>();
                if (!fx.Begin(from, to, material, shape, done, potency))
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

        public static void PlayFizzle(Vector3 origin, System.Action done)
        {
            try
            {
                var flash = new GameObject("Fizzle");
                flash.transform.position = origin;
                var renderer = flash.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteFactory.Burst(new Color(0.55f, 0.52f, 0.58f, 0.7f));
                renderer.sortingOrder = 21;
                flash.transform.localScale = Vector3.one * 0.7f;
                Object.Destroy(flash, 0.28f);
            }
            catch (System.Exception)
            {
            }

            done?.Invoke();
        }

        bool Begin(Vector3 from, Vector3 to, RuneId material, SpellShape shape, System.Action done, float potency)
        {
            _done = done;
            _potency = potency <= 0f ? 1f : potency;
            _from = from;
            _to = to;
            _mid = Vector3.Lerp(_from, _to, 0.45f) + new Vector3(0f, 0.35f, 0f);
            _material = material;
            if (shape == SpellShape.Self)
            {
                shape = SpellShape.Spread;
            }

            _shape = shape == SpellShape.None ? SpellShape.Shot : shape;
            _duration = DurationFor(_shape, _from, _to);
            transform.position = _shape == SpellShape.Spread ? _from : _from;

            var color = RunePalette.Of(material == RuneId.None ? RuneId.Aether : material);
            CreateSprite("Glow", SpriteFactory.Glow(color), 18, new Vector3(1.4f, 1.4f, 1f) * _potency);
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
            switch (_shape)
            {
                case SpellShape.Pillar:
                    return SpriteFactory.Pillar(color);
                case SpellShape.Spread:
                    return SpriteFactory.Burst(color);
                case SpellShape.Remote:
                    return SpriteFactory.Circle(color, 40);
                default:
                    return SpriteFactory.Bolt(color);
            }
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

            switch (_shape)
            {
                case SpellShape.Pillar:
                    transform.position = _to;
                    transform.localScale = new Vector3(_potency, Mathf.Lerp(0.2f, 1.35f, ease) * _potency, 1f);
                    break;
                case SpellShape.Spread:
                    transform.position = _from;
                    transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 2.6f, ease) * _potency;
                    FadeBody(1f - ease * 0.65f);
                    break;
                case SpellShape.Remote:
                    if (ease < 0.28f)
                    {
                        transform.position = _from;
                        transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 0.9f, ease / 0.28f) * _potency;
                    }
                    else
                    {
                        var remote = (ease - 0.28f) / 0.72f;
                        transform.position = _to;
                        transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.45f, remote) * _potency;
                    }

                    break;
                default:
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

                    transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.15f, Mathf.Sin(ease * Mathf.PI)) * _potency;
                    break;
            }

            if (t < 1f)
            {
                return;
            }

            Finish();
        }

        void FadeBody(float alpha)
        {
            if (_body == null)
            {
                return;
            }

            var color = _body.color;
            color.a = alpha;
            _body.color = color;
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
                flash.transform.position = _shape == SpellShape.Spread ? _from : _to;
                var renderer = flash.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteFactory.Burst(RunePalette.Of(
                    _material == RuneId.None ? RuneId.Aether : _material));
                renderer.sortingOrder = 21;
                flash.transform.localScale = Vector3.one * 1.4f * _potency;
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

        static float DurationFor(SpellShape shape, Vector3 from, Vector3 to)
        {
            if (shape == SpellShape.Pillar || shape == SpellShape.Spread)
            {
                return 0.42f;
            }

            if (shape == SpellShape.Remote)
            {
                return 0.5f;
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
