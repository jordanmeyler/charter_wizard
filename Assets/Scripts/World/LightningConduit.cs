using UnityEngine;

namespace RuneMagic
{
    public sealed class LightningConduit : MonoBehaviour, ISpellLock
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
            _renderer.sprite = SpriteFactory.LightningRod(false);
            _renderer.sortingOrder = 5;
            _label = WorldLabel.Attach(transform, "Storm rod", new Vector3(0f, 1.15f, 0f),
                new Color(0.75f, 0.88f, 1f));
        }

        public string FormulaText() => "Spark · waiting";

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            _renderer.sprite = SpriteFactory.LightningRod(true);
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
