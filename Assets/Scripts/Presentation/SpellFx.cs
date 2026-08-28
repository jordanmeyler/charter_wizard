using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Visible cast: a particle body that flies, rises, wells, or lands,
    /// with a light that matches the element. The lock may then resolve.
    /// Never throws — a failed effect still completes so the room can resolve.
    /// </summary>
    public sealed class SpellFx : MonoBehaviour
    {
        Vector3 _from;
        Vector3 _to;
        Vector3 _mid;
        SpellShape _shape;
        ElementLook _look;
        SpellId _spell;
        float _age;
        float _duration = 0.35f;
        float _potency = 1f;
        System.Action _done;
        SpriteRenderer _body;
        SpriteRenderer _glow;
        bool _finished;
        bool _impacted;
        bool _authored;

        public static void Play(
            Vector3 from,
            Vector3 to,
            RuneId material,
            SpellShape shape,
            string caption,
            System.Action done,
            float potency = 1f,
            SpellId spell = SpellId.None)
        {
            try
            {
                var host = new GameObject(string.IsNullOrEmpty(caption) ? "SpellFx" : caption);
                var fx = host.AddComponent<SpellFx>();
                if (!fx.Begin(from, to, material, shape, done, potency, spell))
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
                var look = ElementLook.Of(ElementFamily.Aether);
                ElementFx.Burst(origin, look, SpellShape.Spread, 0.7f);
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

        bool Begin(
            Vector3 from,
            Vector3 to,
            RuneId material,
            SpellShape shape,
            System.Action done,
            float potency,
            SpellId spell)
        {
            _done = done;
            _potency = potency <= 0f ? 1f : potency;
            _from = from;
            _to = to;
            if (WorldWork.IsSkyStrike(spell))
            {
                _from = to + new Vector3(0f, 7.2f, 0f);
                _to = to;
                shape = SpellShape.Shot;
            }

            _mid = Vector3.Lerp(_from, _to, 0.45f) + new Vector3(0f, 0.35f, 0f);
            if (shape == SpellShape.Self)
            {
                shape = SpellShape.Spread;
            }

            _shape = shape == SpellShape.None ? SpellShape.Shot : shape;
            _spell = spell;
            _look = ElementLook.For(material == RuneId.None ? RuneId.Aether : material, spell);
            _duration = DurationFor(_shape, _from, _to, _look.Family);
            transform.position = _from;

            if (TryAuthoredBody(out var frames))
            {
                _authored = true;
                _body = CreateSprite("Body", frames[0], 19, Vector3.one);
                _body.color = Color.white;
                if (frames.Length > 1)
                {
                    SpriteAnim.On(_body.gameObject, _body).Play(frames, 10f, true, AuthoredId() ?? "fx");
                }
            }
            else
            {
                var color = _look.Core;
                _glow = CreateSprite("Glow", SpriteFactory.Glow(_look.Glow), 18, new Vector3(1.8f, 1.8f, 1f) * _potency);
                _glow.color = _look.Glow;
                _body = CreateSprite("Body", BodySprite(), 19, BodyScale());
                _body.color = color;
            }

            ElementFx.Stream(transform, _look, _shape, _potency);
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

        bool TryAuthoredBody(out Sprite[] frames)
        {
            var ids = AuthoredIds();
            for (var i = 0; i < ids.Length; i++)
            {
                if (SpriteFactory.TryAuthoredClip(ids[i], out frames))
                {
                    return true;
                }
            }

            frames = null;
            return false;
        }

        string AuthoredId()
        {
            var ids = AuthoredIds();
            return ids.Length > 0 ? ids[0] : null;
        }

        string[] AuthoredIds()
        {
            var family = _look.Family.ToString().ToLowerInvariant();
            var spell = _spell.ToString().ToLowerInvariant();
            return new[]
            {
                spell + "-shot",
                spell,
                family + "-shot",
                "fx-" + family
            };
        }

        Sprite BodySprite()
        {
            var color = _look.Core;
            switch (_look.Family)
            {
                case ElementFamily.Fire:
                case ElementFamily.Lava:
                    return SpriteFactory.Ember(color);
                case ElementFamily.Water:
                    return SpriteFactory.Droplet(color);
                case ElementFamily.Ice:
                    return SpriteFactory.Shard(color);
                case ElementFamily.Earth:
                    return SpriteFactory.Pebble(color);
                case ElementFamily.Lightning:
                case ElementFamily.Spark:
                    return SpriteFactory.Arc(color);
                case ElementFamily.Fog:
                case ElementFamily.Poison:
                case ElementFamily.Steam:
                    return SpriteFactory.Wisp(color);
                case ElementFamily.Plant:
                    return SpriteFactory.Leaf(color);
                default:
                    break;
            }

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

        Vector3 BodyScale()
        {
            switch (_look.Family)
            {
                case ElementFamily.Fog:
                case ElementFamily.Poison:
                    return new Vector3(1.6f, 1.1f, 1f);
                case ElementFamily.Earth:
                    return new Vector3(0.85f, 0.85f, 1f);
                case ElementFamily.Lightning:
                    return new Vector3(1.3f, 0.45f, 1f);
                default:
                    return Vector3.one;
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
            PulseGlow(ease);

            switch (_shape)
            {
                case SpellShape.Pillar:
                    transform.position = _to;
                    transform.localScale = new Vector3(_potency, Mathf.Lerp(0.2f, 1.55f, ease) * _potency, 1f);
                    break;
                case SpellShape.Spread:
                    transform.position = _from;
                    transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 2.8f, ease) * _potency;
                    FadeBody(1f - ease * 0.7f);
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

                    transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.2f, Mathf.Sin(ease * Mathf.PI)) * _potency;
                    break;
            }

            if (t < 1f)
            {
                return;
            }

            Finish();
        }

        void PulseGlow(float ease)
        {
            if (_glow == null)
            {
                return;
            }

            var color = _look.Glow;
            color.a = _look.Glow.a * (0.55f + Mathf.Sin((_age * 14f) + ease) * 0.25f);
            _glow.color = color;
            _glow.transform.localScale = Vector3.one * (1.6f + Mathf.Sin(_age * 9f) * 0.15f) * _potency;
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
            if (_look.Family == ElementFamily.Lightning)
            {
                var straight = Vector3.Lerp(_from, _to, t);
                var jag = Mathf.Sin(t * 27f) * 0.18f;
                var side = Vector2.Perpendicular((Vector2)(_to - _from));
                if (side.sqrMagnitude > 0.0001f)
                {
                    side.Normalize();
                    straight += (Vector3)(side * jag);
                }

                return straight;
            }

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
                if (!_impacted)
                {
                    _impacted = true;
                    var impact = _shape == SpellShape.Spread ? _from : _to;
                    ElementFx.Burst(impact, _look, _shape, _potency);
                    if (!_authored)
                    {
                        var flash = new GameObject("Impact");
                        flash.transform.position = impact;
                        var renderer = flash.AddComponent<SpriteRenderer>();
                        renderer.sprite = SpriteFactory.Burst(_look.Core);
                        renderer.sortingOrder = 21;
                        flash.transform.localScale = Vector3.one * 1.4f * _potency;
                        Destroy(flash, 0.28f);
                    }
                }
            }
            catch (System.Exception)
            {
            }

            var callback = _done;
            _done = null;
            callback?.Invoke();
            Destroy(gameObject);
        }

        static float DurationFor(SpellShape shape, Vector3 from, Vector3 to, ElementFamily family)
        {
            if (family == ElementFamily.Lightning)
            {
                return 0.22f;
            }

            if (shape == SpellShape.Pillar || shape == SpellShape.Spread)
            {
                return family == ElementFamily.Fog || family == ElementFamily.Poison ? 0.62f : 0.48f;
            }

            if (shape == SpellShape.Remote)
            {
                return 0.52f;
            }

            var distance = Vector2.Distance(from, to);
            if (float.IsNaN(distance))
            {
                return 0.35f;
            }

            return Mathf.Clamp(0.24f + distance * 0.08f, 0.3f, 0.75f);
        }
    }
}
