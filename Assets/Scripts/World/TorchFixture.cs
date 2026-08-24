using UnityEngine;

namespace RuneMagic
{
    public sealed class TorchFixture : MonoBehaviour, ISpellLock, IRuneSource
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        public bool IsEmitting => true;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 3.2f;
        public float VoiceWeight => 2.2f;
        public RuneSourceKind SourceKind => RuneSourceKind.Creature;

        [Header("Authoring")]
        [SerializeField] string authoredName = "Cold Torch";
        [SerializeField] string authoredId = "cold-torch";
        [SerializeField] string authoredSprite = "torch";
        [SerializeField] string authoredSpriteLit = "torch-lit";
        [SerializeField] Sprite portrait;
        [SerializeField] Sprite[] idleFrames;
        [SerializeField] Sprite[] litFrames;
        [SerializeField] string[] keys;

        SpriteRenderer _renderer;
        TextMesh _label;
        string _spriteId = "torch";
        string _spriteLit = "torch-lit";
        bool _wired;

        public void Bind(string displayName, string formulaId, SpellId[] keys, string spriteId = null, string spriteLit = null)
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
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

            _renderer = AuthoringUtil.ApplyLook(gameObject, 5, _spriteId, portrait, idleFrames, 4f);
            if (GetComponentInChildren<FixtureGlow>() == null)
            {
                FixtureGlow.Attach(transform, new Color(0.95f, 0.45f, 0.12f, 0.35f), 1.3f, 0.08f);
            }

            _label = WorldLabel.Attach(transform, "Unlit torch", new Vector3(0f, 1.05f, 0f),
                new Color(0.95f, 0.72f, 0.4f));
        }

        public void BindFromAuthoring()
        {
            if (_wired)
            {
                return;
            }

            Bind(
                authoredName,
                authoredId,
                AuthoringUtil.ParseKeys(keys, MapBuilder.TorchKeys),
                authoredSprite,
                authoredSpriteLit);
        }

        public void Collect(System.Collections.Generic.List<RuneId> buffer)
        {
            buffer.Add(RuneId.Plant);
            if (Resolved)
            {
                buffer.Add(RuneId.Fire);
            }
        }

        public string FormulaText() => "Pl Plant · dry wick";

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            if (litFrames != null && litFrames.Length > 0)
            {
                SpriteAnim.On(gameObject, _renderer).Play(litFrames, 10f, true, _spriteLit);
            }
            else
            {
                _renderer.sprite = SpriteFactory.Named(_spriteLit);
                SpriteAnim.On(gameObject, _renderer).Play(_spriteLit, 10f);
            }
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
