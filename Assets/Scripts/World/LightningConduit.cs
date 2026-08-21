using UnityEngine;

namespace RuneMagic
{
    public sealed class LightningConduit : MonoBehaviour, ISpellLock, IRuneSource
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        public bool IsEmitting => true;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 3.4f;
        public float VoiceWeight => 2.6f;
        public RuneSourceKind SourceKind => RuneSourceKind.Creature;

        SpriteRenderer _renderer;
        TextMesh _label;
        string _spriteId = "rod";
        string _spriteLit = "rod-live";

        public void Bind(string displayName, string formulaId, SpellId[] keys, string spriteId = null, string spriteLit = null)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            AcceptedKeys = keys;
            if (!string.IsNullOrEmpty(spriteId))
            {
                _spriteId = spriteId;
            }

            if (!string.IsNullOrEmpty(spriteLit))
            {
                _spriteLit = spriteLit;
            }

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = SpriteFactory.Named(_spriteId);
            _renderer.sortingOrder = 5;
            SpriteAnim.On(gameObject, _renderer).Play(_spriteId, 4f);
            FixtureGlow.Attach(transform, new Color(0.55f, 0.75f, 1f, 0.4f), 1.4f, 0.1f);
            _label = WorldLabel.Attach(transform, "Storm rod", new Vector3(0f, 1.15f, 0f),
                new Color(0.75f, 0.88f, 1f));
        }

        public void Collect(System.Collections.Generic.List<RuneId> buffer)
        {
            buffer.Add(RuneId.Spark);
            if (Resolved)
            {
                buffer.Add(RuneId.Lightning);
            }
        }

        public string FormulaText() => "Sp Spark · waiting";

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            _renderer.sprite = SpriteFactory.Named(_spriteLit);
            SpriteAnim.On(gameObject, _renderer).Play(_spriteLit, 12f);
            if (_label != null)
            {
                _label.text = "Live bolt";
                _label.color = new Color(1f, 0.95f, 0.45f);
            }

            return spell == SpellId.LightningBolt
                ? "The rod drinks the bolt. Lightning stands in the cell."
                : "Spark takes the rod. The cell wakes.";
        }

        void Update()
        {
            if (!Resolved || _renderer == null)
            {
                return;
            }

            var pulse = 0.75f + Mathf.Sin(Time.time * 14f) * 0.25f;
            _renderer.color = new Color(pulse, pulse, 1f);
        }
    }
}
