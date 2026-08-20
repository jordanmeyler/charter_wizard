using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Every enemy is a lock. The formula is visible (perception is equal).
    /// Interpretation and keys are knowledge-gated.
    /// </summary>
    public sealed class EncounterLock : MonoBehaviour
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public RuneId[] Formula { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Ensouled { get; private set; }
        public bool Resolved { get; private set; }

        SpriteRenderer _renderer;
        Vector3 _rest;
        float _phase;

        public void Bind(string displayName, string formulaId, RuneId[] formula, SpellId[] keys, Color color, bool ensouled)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            Formula = formula;
            AcceptedKeys = keys;
            Ensouled = ensouled;

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = SpriteFactory.Circle(color, 64);
            _renderer.sortingOrder = 5;

            var body = gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;

            var hit = gameObject.AddComponent<CircleCollider2D>();
            hit.radius = 0.42f;
            hit.isTrigger = true;
            _rest = transform.position;
        }

        void Update()
        {
            if (Resolved)
            {
                return;
            }

            _phase += Time.deltaTime;
            transform.position = _rest + new Vector3(Mathf.Sin(_phase * 0.7f) * 0.35f, Mathf.Cos(_phase * 0.45f) * 0.2f, 0f);
        }

        public void Resolve()
        {
            Resolved = true;
            _renderer.color = new Color(1f, 1f, 1f, 0.15f);
            Destroy(gameObject, 0.35f);
        }

        public string FormulaText()
        {
            var parts = new string[Formula.Length];
            for (var i = 0; i < Formula.Length; i++)
            {
                parts[i] = RuneCatalog.NameOf(Formula[i]);
            }

            return string.Join(" · ", parts);
        }
    }
}
