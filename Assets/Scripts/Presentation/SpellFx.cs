using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Visible cast: a bolt, wall, or burst, then the lock resolves.
    /// </summary>
    public sealed class SpellFx : MonoBehaviour
    {
        Vector3 _from;
        Vector3 _to;
        Vector3 _mid;
        RuneId _material;
        RuneId _aspect;
        string _caption;
        float _age;
        float _duration;
        System.Action _done;
        SpriteRenderer _body;
        SpriteRenderer _glow;
        TextMesh _label;
        bool _finished;

        public static void Play(
            Vector3 from,
            Vector3 to,
            RuneId material,
            RuneId aspect,
            string caption,
            System.Action done)
        {
            var host = new GameObject("SpellFx");
            var fx = host.AddComponent<SpellFx>();
            fx.Begin(from, to, material, aspect, caption, done);
        }

        void Begin(Vector3 from, Vector3 to, RuneId material, RuneId aspect, string caption, System.Action done)
        {
            _from = from;
            _to = to;
            _mid = Vector3.Lerp(from, to, 0.45f) + new Vector3(0f, 0.35f, 0f);
            _material = material;
            _aspect = aspect;
            _caption = string.IsNullOrEmpty(caption) ? "surge" : caption;
            _done = done;
            _duration = DurationFor(aspect, from, to);

            var color = RunePalette.Of(material == RuneId.None ? RuneId.Aether : material);

            _glow = CreateSprite("Glow", SpriteFactory.Glow(color), 18, new Vector3(1.4f, 1.4f, 1f));
            _body = CreateSprite("Body", BodySprite(), 19, Vector3.one);
            _body.color = color;

            var labelObject = new GameObject("Caption");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            _label = labelObject.AddComponent<TextMesh>();
            _label.text = _caption;
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.fontSize = 36;
            _label.characterSize = 0.08f;
            _label.color = Color.white;
            _label.font = Resources.GetBuiltinResource<Font>("Arial.ttf")
                          ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var mesh = labelObject.GetComponent<MeshRenderer>();
            mesh.sortingOrder = 22;

            transform.position = from;
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
            if (_aspect == RuneId.Salt)
            {
                return SpriteFactory.Pillar(RunePalette.Of(_material));
            }

            if (_aspect == RuneId.Sulphur)
            {
                return SpriteFactory.Burst(RunePalette.Of(_material));
            }

            return SpriteFactory.Bolt(RunePalette.Of(_material));
        }

        void Update()
        {
            _age += Time.deltaTime;
            var t = Mathf.Clamp01(_age / _duration);
            var ease = t * t * (3f - 2f * t);

            if (_aspect == RuneId.Salt)
            {
                transform.position = _to;
                var grow = Mathf.Lerp(0.2f, 1.35f, ease);
                transform.localScale = new Vector3(1f, grow, 1f);
            }
            else if (_aspect == RuneId.Sulphur)
            {
                transform.position = _to;
                var grow = Mathf.Lerp(0.35f, 2.2f, ease);
                transform.localScale = Vector3.one * grow;
                var fade = 1f - ease * 0.55f;
                if (_body != null)
                {
                    var color = _body.color;
                    color.a = fade;
                    _body.color = color;
                }
            }
            else
            {
                var point = SampleArc(ease);
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

            if (_label != null)
            {
                _label.transform.localPosition = new Vector3(0f, 0.55f + ease * 0.25f, 0f);
            }

            if (t < 1f || _finished)
            {
                return;
            }

            _finished = true;
            Impact();
            _done?.Invoke();
            Destroy(gameObject, 0.12f);
        }

        Vector3 SampleArc(float t)
        {
            var a = Vector3.Lerp(_from, _mid, t);
            var b = Vector3.Lerp(_mid, _to, t);
            return Vector3.Lerp(a, b, t);
        }

        void Impact()
        {
            var flash = new GameObject("Impact");
            flash.transform.position = _to;
            var renderer = flash.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Burst(RunePalette.Of(_material));
            renderer.sortingOrder = 21;
            flash.transform.localScale = Vector3.one * 1.4f;
            Destroy(flash, 0.2f);
        }

        static float DurationFor(RuneId aspect, Vector3 from, Vector3 to)
        {
            if (aspect == RuneId.Salt || aspect == RuneId.Sulphur)
            {
                return 0.42f;
            }

            var distance = Vector2.Distance(from, to);
            return Mathf.Clamp(0.22f + distance * 0.08f, 0.28f, 0.7f);
        }
    }
}
