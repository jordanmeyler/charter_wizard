using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// One glyph in the living tapestry. It rides a wandering home, not a tile.
    /// </summary>
    public sealed class TapestryStrand : MonoBehaviour
    {
        public RuneId Rune { get; private set; }
        public Vector3 Home { get; private set; }
        public Vector3 Well { get; set; }
        public bool FromString { get; private set; }
        public int StringId { get; private set; }
        public int StringIndex { get; private set; }
        public bool Dying { get; private set; }
        public float Alpha { get; private set; }

        float _phase;
        float _freqA;
        float _freqB;
        float _amp;
        float _life;
        SpriteRenderer _body;
        TextMesh _glyph;
        MeshRenderer _glyphRenderer;

        public void Bind(RuneId rune, Vector3 well, bool fromString, int stringId, int stringIndex)
        {
            Rune = rune;
            Well = well;
            Home = well + (Vector3)(Random.insideUnitCircle * 0.45f);
            FromString = fromString;
            StringId = stringId;
            StringIndex = stringIndex;
            _phase = Random.Range(0f, Mathf.PI * 2f);
            _freqA = Random.Range(0.28f, 0.72f);
            _freqB = Random.Range(0.22f, 0.64f);
            _amp = fromString ? Random.Range(0.18f, 0.34f) : Random.Range(0.55f, 1.15f);
            _life = 0f;
            Alpha = 0f;

            var color = RunePalette.Of(rune);
            _body = gameObject.GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
            _body.sprite = SpriteFactory.Circle(new Color(color.r, color.g, color.b, 0.9f), 28);
            _body.sortingOrder = 8;

            EnsureGlyph(color);
            transform.position = Home;
        }

        public void Retarget(Vector3 well, bool fromString, int stringId, int stringIndex)
        {
            Well = well;
            FromString = fromString;
            StringId = stringId;
            StringIndex = stringIndex;
            Dying = false;
        }

        public void BeginFade()
        {
            Dying = true;
        }

        public bool Tick(float still, float dt)
        {
            _life += dt;
            var chase = FromString ? 2.4f : 0.85f;
            Home = Vector3.Lerp(Home, Well, 1f - Mathf.Exp(-chase * dt));
            if (!FromString)
            {
                Home += (Vector3)(Random.insideUnitCircle * (0.18f * (1f - still) * dt));
            }

            _phase += (0.35f + (1f - still) * 0.85f) * dt;
            var weave = 1f - still * 0.82f;
            var offset = new Vector3(
                Mathf.Sin(_phase * _freqA + _phase) * _amp,
                Mathf.Cos(_phase * _freqB) * _amp * 0.72f,
                0f) * weave;

            transform.position = Home + offset;

            var target = Dying ? 0f : (FromString ? 0.95f : 0.72f + still * 0.2f);
            Alpha = Mathf.MoveTowards(Alpha, target, dt * (Dying ? 2.4f : 1.6f));
            ApplyAlpha();

            var pulse = 0.72f + Mathf.Sin(_life * 2.4f + _phase) * 0.08f;
            transform.localScale = Vector3.one * (FromString ? 0.92f : pulse);

            return Dying && Alpha <= 0.02f;
        }

        public bool Contains(Vector3 world, float radius)
        {
            return Vector2.Distance(world, transform.position) <= radius;
        }

        void EnsureGlyph(Color color)
        {
            var font = BuiltinFont.Get();
            if (font == null)
            {
                return;
            }

            var labelObject = new GameObject("Glyph");
            labelObject.transform.SetParent(transform, false);
            _glyph = labelObject.AddComponent<TextMesh>();
            _glyph.font = font;
            _glyph.text = RuneCatalog.GlyphOf(Rune);
            _glyph.anchor = TextAnchor.MiddleCenter;
            _glyph.alignment = TextAlignment.Center;
            _glyph.fontSize = 36;
            _glyph.characterSize = 0.085f;
            _glyph.color = Color.Lerp(color, Color.white, 0.55f);
            _glyphRenderer = labelObject.GetComponent<MeshRenderer>();
            if (_glyphRenderer != null)
            {
                if (font.material != null)
                {
                    _glyphRenderer.sharedMaterial = font.material;
                }

                _glyphRenderer.sortingOrder = 9;
            }
        }

        void ApplyAlpha()
        {
            if (_body != null)
            {
                var color = _body.color;
                color.a = Alpha * 0.78f;
                _body.color = color;
            }

            if (_glyph != null)
            {
                var color = _glyph.color;
                color.a = Mathf.Clamp01(Alpha * 1.15f);
                _glyph.color = color;
            }
        }
    }
}
