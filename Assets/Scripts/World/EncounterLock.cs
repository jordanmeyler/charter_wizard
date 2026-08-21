using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Every enemy is a lock. The formula is visible (perception is equal).
    /// Interpretation and keys are knowledge-gated.
    /// </summary>
    public sealed class EncounterLock : MonoBehaviour, ISpellLock, IRuneSource
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public RuneId[] Formula { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Ensouled { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        public bool IsEmitting => !Resolved && Formula != null && Formula.Length > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 3.4f;
        public float VoiceWeight => 2.4f;
        public RuneSourceKind SourceKind => RuneSourceKind.Creature;

        SpriteRenderer _renderer;
        Vector3 _rest;
        float _phase;
        string _grant;
        Collider2D _hit;

        public void Bind(
            string displayName,
            string formulaId,
            RuneId[] formula,
            SpellId[] keys,
            bool ensouled,
            string spriteId = null,
            bool blocking = false,
            string grantItem = null)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            Formula = formula;
            AcceptedKeys = keys;
            Ensouled = ensouled;
            _grant = grantItem;

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = SpriteFactory.Named(string.IsNullOrEmpty(spriteId) ? "ash-mite" : spriteId);
            _renderer.sortingOrder = 12;
            FixtureGlow.Attach(transform, new Color(1f, 0.35f, 0.08f, 0.7f), 1.8f, 0.16f);

            var body = gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;

            var hit = gameObject.AddComponent<CircleCollider2D>();
            hit.radius = blocking ? 0.48f : 0.42f;
            hit.isTrigger = !blocking;
            _hit = hit;
            _rest = transform.position;

            WorldLabel.Attach(transform, displayName, new Vector3(0f, 0.85f, 0f),
                new Color(1f, 0.7f, 0.35f));
        }

        void Update()
        {
            if (Resolved || AdeptAvatar.WorldHeld)
            {
                return;
            }

            _phase += Time.deltaTime;
            transform.position = _rest + new Vector3(Mathf.Sin(_phase * 0.7f) * 0.2f, Mathf.Cos(_phase * 0.45f) * 0.12f, 0f);
        }

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            if (_hit != null)
            {
                _hit.enabled = false;
            }

            if (_renderer != null)
            {
                _renderer.color = new Color(1f, 1f, 1f, 0.15f);
            }

            LockReward.Grant(transform.position, _grant);
            Destroy(gameObject, 0.35f);
            return $"{DisplayName} unmakes. A simple lock; many keys would have turned it.";
        }

        public void Collect(System.Collections.Generic.List<RuneId> buffer)
        {
            if (!IsEmitting)
            {
                return;
            }

            for (var i = 0; i < Formula.Length; i++)
            {
                buffer.Add(Formula[i]);
            }
        }

        public string FormulaText()
        {
            var parts = new string[Formula.Length];
            for (var i = 0; i < Formula.Length; i++)
            {
                parts[i] = $"{RuneCatalog.GlyphOf(Formula[i])} {RuneCatalog.NameOf(Formula[i])}";
            }

            return string.Join(" · ", parts);
        }
    }
}
