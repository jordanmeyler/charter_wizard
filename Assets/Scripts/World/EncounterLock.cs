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
        StatusHost _status;

        public void Bind(
            string displayName,
            string formulaId,
            RuneId[] formula,
            SpellId[] keys,
            bool ensouled,
            string spriteId = null,
            bool blocking = false,
            string grantItem = null,
            string attack = null,
            float castSeconds = 0f,
            RuneId[] castRecipe = null)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            Formula = WithLife(formula);
            AcceptedKeys = keys;
            Ensouled = ensouled;
            _grant = grantItem;

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            var art = string.IsNullOrEmpty(spriteId) ? "ash-mite" : spriteId;
            _renderer.sprite = SpriteFactory.Named(art);
            _renderer.sortingOrder = 12;
            var anim = SpriteAnim.On(gameObject, _renderer);
            anim.FreezeWhenWorldHeld = true;
            anim.Play(art, FpsFor(art));
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

            _status = gameObject.AddComponent<StatusHost>();
            _status.Bind(NatureOf(formulaId, ensouled), new Vector3(0f, 1.28f, 0f));
            _status.OnFatal = id =>
            {
                if (id == StatusId.Poisoned && !Resolved)
                {
                    FindFirstObjectByType<SanctumDirector>()?.UnmakeLock(this,
                        $"{DisplayName} cannot hold the foul breath. They fall.");
                }
            };
            var kind = CombatOf(formulaId, attack);
            var combat = gameObject.AddComponent<CombatActor>();
            combat.Bind(kind, castSeconds > 0f ? castSeconds : 2f, FindFirstObjectByType<WorldGrid>(), castRecipe);
        }

        /// <summary>
        /// Living enemies carry Life as a mark. Ice, stone, and fire
        /// recipes stay as written; Life is appended if the author omitted it.
        /// </summary>
        public static RuneId[] WithLife(RuneId[] formula)
        {
            if (formula == null || formula.Length == 0)
            {
                return new[] { RuneId.Vita };
            }

            for (var i = 0; i < formula.Length; i++)
            {
                if (formula[i] == RuneId.Vita)
                {
                    return formula;
                }
            }

            var marked = new RuneId[formula.Length + 1];
            for (var i = 0; i < formula.Length; i++)
            {
                marked[i] = formula[i];
            }

            marked[formula.Length] = RuneId.Vita;
            return marked;
        }

        static CreatureNature NatureOf(string formulaId, bool ensouled)
        {
            switch ((formulaId ?? string.Empty).ToLowerInvariant())
            {
                case "fire-golem":
                case "ash-mite":
                    return CreatureNature.Fire;
                case "ice-thing":
                    return CreatureNature.Ice;
                case "stone-man":
                    return CreatureNature.Earth;
                case "spirit-warden":
                    return ensouled ? CreatureNature.Mind : CreatureNature.Flesh;
                default:
                    return ensouled ? CreatureNature.Mind : CreatureNature.Flesh;
            }
        }

        static float FpsFor(string spriteId)
        {
            switch ((spriteId ?? string.Empty).ToLowerInvariant())
            {
                case "fire-golem": return 5f;
                case "stone-man": return 2.5f;
                case "warden": return 4f;
                case "ice-thing": return 6f;
                default: return 7f;
            }
        }

        static CombatKind CombatOf(string formulaId, string attack)
        {
            switch ((attack ?? string.Empty).ToLowerInvariant())
            {
                case "golem":
                case "melee":
                    return CombatKind.Golem;
                case "wizard":
                    return CombatKind.Wizard;
                case "archer":
                case "ranged":
                    return CombatKind.Archer;
            }

            switch ((formulaId ?? string.Empty).ToLowerInvariant())
            {
                case "fire-golem":
                    return CombatKind.Golem;
                case "spirit-warden":
                    return CombatKind.Wizard;
                default:
                    return CombatKind.None;
            }
        }

        void Update()
        {
            if (Resolved || AdeptAvatar.WorldHeld)
            {
                return;
            }

            if (GetComponent<CombatActor>() != null)
            {
                return;
            }

            if (_status != null && _status.BlocksMove)
            {
                transform.position = _rest;
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

            LockReward.Grant(transform.position, _grant);
            if (_renderer != null)
            {
                _renderer.color = new Color(1f, 1f, 1f, 0.15f);
            }

            Destroy(gameObject, 0.35f);
            if (spell == SpellId.Rage)
            {
                return $"{DisplayName} turns on itself. The mind was the lock.";
            }

            if (spell == SpellId.Terror || spell == SpellId.Jolt)
            {
                return $"{DisplayName} cannot hold a thought. They leave the aisle.";
            }

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
