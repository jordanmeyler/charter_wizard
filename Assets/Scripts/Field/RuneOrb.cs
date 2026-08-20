using UnityEngine;

namespace RuneMagic
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class RuneOrb : MonoBehaviour
    {
        public RuneId Rune { get; private set; }

        RuneField _field;
        float _angle;
        float _radius;
        float _speed;
        SpriteRenderer _body;
        TextMesh _label;

        public void Bind(RuneField field, RuneId rune, float angle, float radius, float speed)
        {
            _field = field;
            Rune = rune;
            _angle = angle;
            _radius = radius;
            _speed = speed;

            _body = gameObject.AddComponent<SpriteRenderer>();
            _body.sprite = SpriteFactory.Circle(RunePalette.Of(rune), 40);
            _body.sortingOrder = 8;

            var labelObject = new GameObject("Glyph");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.zero;
            _label = labelObject.AddComponent<TextMesh>();
            _label.text = RuneCatalog.GlyphOf(rune);
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.fontSize = 32;
            _label.characterSize = 0.12f;
            _label.color = Color.black;
            _label.font = Resources.GetBuiltinResource<Font>("Arial.ttf")
                          ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var labelRenderer = labelObject.GetComponent<MeshRenderer>();
            labelRenderer.sortingOrder = 9;
        }

        void Update()
        {
            if (_field == null)
            {
                return;
            }

            _angle += _speed * Time.deltaTime;
            var offset = new Vector3(Mathf.Cos(_angle), Mathf.Sin(_angle), 0f) * _radius;
            transform.position = _field.transform.position + offset;
        }
    }
}
