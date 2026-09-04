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

        [Header("Authoring")]
        [SerializeField] string authoredName = "Ash Mite";
        [SerializeField] string authoredId = "ash-mite";
        [Tooltip("Catalog / sheet id if Portrait and Idle Frames are empty. Pack enemies use enemy-001 … enemy-012.")]
        [SerializeField] string spriteId = "ash-mite";
        [Tooltip("Still for the Scene view. Drag a sliced Unity sprite. Idle Frames override this in Play.")]
        [SerializeField] Sprite portrait;
        [Tooltip("Loop while they stand or walk. Drag ElvGames Enemy_*_A slices here.")]
        [SerializeField] Sprite[] idleFrames;
        [Tooltip("Played on a slam or while a wizard writes. Drag Enemy_*_C slices.")]
        [SerializeField] Sprite[] attackFrames;
        [Tooltip("Played once when the lock turns, then the body goes.")]
        [SerializeField] Sprite[] resolveFrames;
        [SerializeField] string resolveClip;
        [Tooltip("Optional clip id if Idle Frames are empty (enemy-011, fire-golem, stone-man, warden).")]
        [SerializeField] string idleClip;
        [Tooltip("Optional clip id if Attack Frames are empty (fire-golem-slam, warden-cast).")]
        [SerializeField] string attackClip;
        [SerializeField] string[] formula = { "Fire", "Salt", "Life" };
        [SerializeField] string[] keys;
        [SerializeField] bool authoredEnsouled;
        [SerializeField] bool authoredBlocking;
        [SerializeField] string grant;
        [Tooltip("Legacy string. The Attack dropdown writes this.")]
        [SerializeField] string attack;
        [Tooltip("Legacy fallback if Attacks is empty. Golem slams. Wizard writes a fireball. Archer looses a shot.")]
        [SerializeField] CombatKind authoredAttack;
        [SerializeField] float authoredCastSeconds = 2f;
        [Tooltip("Marks shown over a caster's head. Empty Wizard writes Fire · Mercury.")]
        [SerializeField] string[] cast;
        [Tooltip("Auto follows Attack: Golem holds ground, Wizard / Archer stand and write.")]
        [SerializeField] CombatMode authoredMode;
        [Tooltip("Slam reach. 0 uses 1.25.")]
        [SerializeField] float closeRange;
        [Tooltip("Mid-band ceiling. 0 uses 4.5.")]
        [SerializeField] float midRange;
        [Tooltip("Long-band / sight. 0 uses 8.2.")]
        [SerializeField] float longRange;
        [Tooltip("Close, mid, and long strikes. Empty falls back to Attack.")]
        [SerializeField] CombatSlot[] attacks;
        [Tooltip("First matching if/then wins. Wall → flame-pillar is the Mixed Court default when this list is empty.")]
        [SerializeField] CombatGambit[] gambits;
        [Tooltip("Auto reads the Id (golem is earth, warden is mind).")]
        [SerializeField] AuthoredNature authoredNature;
        [SerializeField] bool customDefense;
        [SerializeField] int authoredDefense = 2;
        [SerializeField] bool customPush;
        [SerializeField] int authoredPushResist = 1;
        [SerializeField] StrikeAffinity[] strikeAffinities;
        [SerializeField] StatusAffinity[] statusAffinities;

        SpriteRenderer _renderer;
        Vector3 _rest;
        float _phase;
        string _grant;
        Collider2D _hit;
        StatusHost _status;
        bool _wired;

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
            if (_wired)
            {
                return;
            }

            _wired = true;
            DisplayName = displayName;
            FormulaId = formulaId;
            Formula = WithLife(formula);
            AcceptedKeys = keys;
            Ensouled = ensouled;
            _grant = grantItem;

            var art = string.IsNullOrEmpty(spriteId) ? "ash-mite" : spriteId;
            _renderer = AuthoringUtil.ApplyLook(gameObject, DrawDepth.Body, art, portrait, idleFrames, FpsFor(art));
            WorldYSort.On(gameObject);
            var anim = SpriteAnim.On(gameObject, _renderer);
            anim.FreezeWhenWorldHeld = true;

            var body = AuthoringUtil.GetOrAdd<Rigidbody2D>(gameObject, out var addedBody);
            if (addedBody)
            {
                body.gravityScale = 0f;
                body.bodyType = RigidbodyType2D.Kinematic;
            }

            var hit = AuthoringUtil.GetOrAdd<CircleCollider2D>(gameObject, out var addedHit);
            if (addedHit)
            {
                hit.radius = blocking ? 0.48f : 0.42f;
                hit.isTrigger = !blocking;
            }
            _hit = hit;
            _rest = transform.position;

            WorldLabel.Attach(transform, displayName, new Vector3(0f, 0.85f, 0f),
                new Color(1f, 0.7f, 0.35f), DrawDepth.Name);

            _status = AuthoringUtil.GetOrAdd<StatusHost>(gameObject);
            var nature = CombatBook.NatureOf(authoredNature, formulaId, ensouled);
            _status.Bind(nature, new Vector3(0f, 1.28f, 0f), BuildProfile(nature));
            _status.OnFatal = id =>
            {
                if (Resolved || !VitalLaw.IsMeter(id))
                {
                    return;
                }

                FindFirstObjectByType<SanctumDirector>()?.UnmakeLock(this,
                    VitalLaw.FatalNote(id, DisplayName, false));
            };
            var kind = CombatOf(authoredAttack, formulaId, attack);
            var plan = CombatBook.PlanFrom(
                kind,
                authoredMode,
                closeRange,
                midRange,
                longRange,
                attacks,
                gambits,
                castSeconds > 0f ? castSeconds : authoredCastSeconds,
                castRecipe);
            var combat = AuthoringUtil.GetOrAdd<CombatActor>(gameObject);
            combat.Bind(plan, FindFirstObjectByType<WorldGrid>());
            combat.BindLooks(IdleForCombat(), attackFrames, IdleClipName(art, plan.Kind), AttackClipName(plan.Kind));
        }

        AffinityProfile BuildProfile(CreatureNature nature)
        {
            var row = AffinityProfile.Of(nature);
            if (!customDefense && !customPush
                && (strikeAffinities == null || strikeAffinities.Length == 0)
                && (statusAffinities == null || statusAffinities.Length == 0))
            {
                return row;
            }

            return row.WithOverrides(
                customDefense ? authoredDefense : (int?)null,
                customPush ? authoredPushResist : (int?)null,
                strikeAffinities,
                statusAffinities);
        }

        public void AuthorCustom()
        {
            authoredName = "Custom";
            authoredId = "custom";
            spriteId = "enemy-001";
            formula = new[] { "Earth", "Salt", "Life" };
            authoredAttack = CombatKind.Golem;
            attack = "golem";
            authoredMode = CombatMode.Hunt;
            authoredNature = AuthoredNature.Flesh;
            authoredBlocking = true;
            authoredCastSeconds = 0.85f;
            closeRange = CombatBook.DefaultClose;
            midRange = CombatBook.DefaultMid;
            longRange = CombatBook.DefaultLong;
            attacks = new[] { CombatBook.SlamSlot() };
            gambits = System.Array.Empty<CombatGambit>();
        }

        public void SeedAttacksFromKind()
        {
            var kind = CombatOf(authoredAttack, authoredId, attack);
            attacks = CombatBook.SlotsFromKind(kind, AuthoringUtil.ParseRunes(cast), authoredCastSeconds);
            CombatBook.FillEmptyRecipes(attacks);
        }

        public void LoadNatureDefaults()
        {
            var nature = CombatBook.NatureOf(authoredNature, authoredId, authoredEnsouled);
            if (authoredNature == AuthoredNature.Auto)
            {
                authoredNature = CombatBook.AuthoredOf(nature);
            }

            var row = AffinityProfile.Of(nature);
            customDefense = true;
            authoredDefense = row.Defense;
            customPush = true;
            authoredPushResist = row.PushResist;
            strikeAffinities = new StrikeAffinity[CombatBook.TunableStrikes.Length];
            for (var i = 0; i < CombatBook.TunableStrikes.Length; i++)
            {
                var kind = CombatBook.TunableStrikes[i];
                strikeAffinities[i] = new StrikeAffinity
                {
                    Kind = kind,
                    Affinity = row.Strike(kind)
                };
            }

            statusAffinities = new StatusAffinity[CombatBook.TunableStatuses.Length];
            for (var i = 0; i < CombatBook.TunableStatuses.Length; i++)
            {
                var id = CombatBook.TunableStatuses[i];
                statusAffinities[i] = new StatusAffinity
                {
                    Status = id,
                    Affinity = row.Status(id)
                };
            }
        }

        public void ApplyPack(PackEnemies.Spec spec)
        {
            if (spec == null)
            {
                return;
            }

            authoredName = spec.Name;
            authoredId = spec.Id;
            spriteId = spec.SpriteId;
            formula = spec.Formula;
            attack = spec.Attack;
            authoredAttack = PackEnemies.KindOf(spec.Attack);
            authoredBlocking = spec.Blocking;
            authoredEnsouled = spec.Ensouled;
        }

        public void BindFromAuthoring()
        {
            if (_wired)
            {
                return;
            }

            Bind(
                string.IsNullOrEmpty(authoredName) ? "Ash Mite" : authoredName,
                string.IsNullOrEmpty(authoredId) ? "ash-mite" : authoredId,
                AuthoringUtil.ParseRunes(formula, RuneId.Fire, RuneId.Salt, RuneId.Vita),
                AuthoringUtil.ParseKeys(keys, MapBuilder.MiteKeys),
                authoredEnsouled,
                spriteId,
                authoredBlocking,
                grant,
                attack,
                authoredCastSeconds,
                AuthoringUtil.ParseRunes(cast));
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

        Sprite[] IdleForCombat()
        {
            if (idleFrames != null && idleFrames.Length > 0)
            {
                return idleFrames;
            }

            return portrait != null ? new[] { portrait } : null;
        }

        string IdleClipName(string art, CombatKind kind)
        {
            if (!string.IsNullOrWhiteSpace(idleClip))
            {
                return idleClip.Trim();
            }

            if (!string.IsNullOrWhiteSpace(art))
            {
                return art.Trim();
            }

            switch (kind)
            {
                case CombatKind.Golem:
                    return authoredId == "stone-man" || authoredId == "golem" ? "stone-man" : "fire-golem";
                case CombatKind.Wizard:
                case CombatKind.Archer:
                    return "warden";
                default:
                    return authoredId == "stone-man" ? "stone-man" : "ash-mite";
            }
        }

        string AttackClipName(CombatKind kind)
        {
            if (!string.IsNullOrWhiteSpace(attackClip))
            {
                return attackClip.Trim();
            }

            switch (kind)
            {
                case CombatKind.Golem:
                    return "fire-golem-slam";
                case CombatKind.Wizard:
                case CombatKind.Archer:
                    return "warden-cast";
                default:
                    return string.Empty;
            }
        }

        static float FpsFor(string spriteId)
        {
            switch ((spriteId ?? string.Empty).ToLowerInvariant())
            {
                case "fire-golem":
                case "enemy-011":
                    return 5f;
                case "stone-man":
                case "golem":
                    return 2.5f;
                case "warden":
                case "enemy-012":
                    return 4f;
                case "ice-thing":
                    return 6f;
                default:
                    return 7f;
            }
        }

        public static CombatKind CombatOf(CombatKind authored, string formulaId, string attack)
        {
            if (authored != CombatKind.None)
            {
                return authored;
            }

            var named = PackEnemies.KindOf(attack);
            if (named != CombatKind.None)
            {
                return named;
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

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                PreviewLook();
            }
        }

        void OnValidate()
        {
            if (authoredAttack != CombatKind.None)
            {
                attack = PackEnemies.AttackName(authoredAttack);
            }

            CombatBook.FillEmptyRecipes(attacks);
            if (gambits != null)
            {
                for (var i = 0; i < gambits.Length; i++)
                {
                    CombatBook.FillFromSpell(gambits[i]);
                }
            }

            if (Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += EditorRefresh;
#endif
        }

#if UNITY_EDITOR
        void EditorRefresh()
        {
            if (this == null || Application.isPlaying)
            {
                return;
            }

            PreviewLook();
        }
#endif

        void PreviewLook()
        {
            var renderer = AuthoringUtil.KeepRenderer(gameObject, DrawDepth.Body);
            WorldYSort.On(gameObject);
            if (idleFrames != null && idleFrames.Length > 0 && idleFrames[0] != null)
            {
                renderer.sprite = idleFrames[0];
                return;
            }

            if (portrait != null)
            {
                renderer.sprite = portrait;
                return;
            }

            if (renderer.sprite != null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var pack = EditorPackStill(spriteId);
                if (pack != null)
                {
                    renderer.sprite = pack;
                    return;
                }
            }
#endif
        }

#if UNITY_EDITOR
        static Sprite EditorPackStill(string spriteId)
        {
            var path = PackEnemies.SheetPath(spriteId, 'A');
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite first = null;
            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not Sprite sprite)
                {
                    continue;
                }

                first ??= sprite;
                if (PackEnemies.FrameIndex(sprite.name) == 0)
                {
                    return sprite;
                }
            }

            return first;
        }
#endif

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
            var hasChange = (resolveFrames != null && resolveFrames.Length > 0) || !string.IsNullOrEmpty(resolveClip);
            if (hasChange)
            {
                AuthoringUtil.PlayChange(gameObject, _renderer, resolveFrames, resolveClip, 8f, () => Destroy(gameObject));
            }
            else
            {
                if (_renderer != null)
                {
                    _renderer.color = new Color(1f, 1f, 1f, 0.15f);
                }

                Destroy(gameObject, 0.35f);
            }
            if (spell == SpellId.Rage)
            {
                return $"{DisplayName} turns on itself. The mind was the lock.";
            }

            if (spell == SpellId.Charm || spell == SpellId.Command)
            {
                return $"{DisplayName} yields. They will fetch, and they will fight what you mark.";
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
