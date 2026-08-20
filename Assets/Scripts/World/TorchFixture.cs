using UnityEngine;

namespace RuneMagic
{
    public sealed class TorchFixture : MonoBehaviour, ISpellLock
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        SpriteRenderer _renderer;
        TextMesh _label;

        public void Bind(string displayName, string formulaId, SpellId[] keys)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            AcceptedKeys = keys;

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = SpriteFactory.Torch(false);
            _renderer.sortingOrder = 5;
            _label = WorldLabel.Attach(transform, "Unlit torch", new Vector3(0f, 1.05f, 0f),
                new Color(0.95f, 0.72f, 0.4f));
        }

        public string FormulaText() => "Plant · Dry wick";

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            _renderer.sprite = SpriteFactory.Torch(true);
            if (_label != null)
            {
                _label.text = "Lit torch";
                _label.color = new Color(1f, 0.82f, 0.35f);
            }

            return spell == SpellId.FlamePillar
                ? "A flame-pillar takes the wick. The chapel wakes."
                : spell == SpellId.Snuff || spell == SpellId.Smother
                    ? "The wick is a lock of fire as much as wood. Ending it still turns the chapel."
                    : "Fire finds the dry wood. The torch burns.";
        }

        void Update()
        {
            if (!Resolved || _renderer == null)
            {
                return;
            }

            var pulse = 0.85f + Mathf.Sin(Time.time * 9f) * 0.15f;
            _renderer.color = new Color(1f, pulse, pulse * 0.85f);
        }
    }
}
