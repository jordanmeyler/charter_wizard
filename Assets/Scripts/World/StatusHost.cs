using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Living and unliving bodies keep their conditions here.
    /// The same host holds buffs and debuffs so the world can read them.
    /// </summary>
    public sealed class StatusHost : MonoBehaviour
    {
        public CreatureNature Nature { get; private set; } = CreatureNature.Flesh;
        public AffinityProfile Profile { get; private set; } = AffinityProfile.Of(CreatureNature.Flesh);
        public IReadOnlyList<StatusInstance> Active => _effects;
        public System.Action<StatusId> OnFatal;

        static readonly List<StatusHost> Live = new();

        readonly List<StatusInstance> _effects = new();
        TextMesh _chip;
        SpriteRenderer _sprite;
        Color _baseColor = Color.white;

        void OnEnable()
        {
            if (!Live.Contains(this))
            {
                Live.Add(this);
            }
        }

        void OnDisable()
        {
            Live.Remove(this);
        }

        public void Bind(CreatureNature nature, Vector3 chipOffset)
        {
            Nature = nature;
            Profile = AffinityProfile.Of(nature);
            _sprite = GetComponent<SpriteRenderer>();
            if (_sprite != null)
            {
                _baseColor = _sprite.color;
            }

            _chip = WorldLabel.Attach(transform, "", chipOffset, new Color(0.95f, 0.82f, 0.55f), DrawDepth.Chip);
            if (_chip != null)
            {
                _chip.characterSize = 0.055f;
            }
        }

        public static StatusHost On(Component other)
        {
            return other != null ? other.GetComponent<StatusHost>() : null;
        }

        public bool Has(StatusId id)
        {
            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Id == id && _effects[i].Remaining > 0f)
                {
                    return true;
                }
            }

            return false;
        }

        public bool YieldsPassage
        {
            get
            {
                for (var i = 0; i < _effects.Count; i++)
                {
                    if (_effects[i].Remaining > 0f && StatusSpec.YieldsPassage(_effects[i].Id))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public StatusId MindAilment
        {
            get
            {
                for (var i = 0; i < _effects.Count; i++)
                {
                    if (_effects[i].Remaining > 0f && StatusSpec.IsMindAilment(_effects[i].Id))
                    {
                        return _effects[i].Id;
                    }
                }

                return StatusId.None;
            }
        }

        public bool BlocksAction
        {
            get
            {
                for (var i = 0; i < _effects.Count; i++)
                {
                    if (_effects[i].Remaining > 0f && _effects[i].Spec.BlocksAction)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool BlocksMove
        {
            get
            {
                for (var i = 0; i < _effects.Count; i++)
                {
                    if (_effects[i].Remaining > 0f && _effects[i].Spec.BlocksMove)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool BlocksPhysical
        {
            get
            {
                for (var i = 0; i < _effects.Count; i++)
                {
                    if (_effects[i].Remaining > 0f && _effects[i].Spec.BlocksPhysical)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool WalksOnWater =>
            Has(StatusId.Watershield) || Has(StatusId.TideForm) || Has(StatusId.CloudForm);

        public bool IsHidden =>
            Has(StatusId.GaleForm) || Has(StatusId.Veiled);

        public bool Flies =>
            Has(StatusId.CloudForm) || Has(StatusId.Flying);

        public bool SproutsWhileWalking =>
            Has(StatusId.Plantward) || Has(StatusId.GroveForm);

        public bool ClearsVeilsWhileWalking =>
            Has(StatusId.Windward) || Has(StatusId.GaleForm);

        public bool KindlesWhileWalking =>
            Has(StatusId.FlameForm);

        public bool DousesWhileWalking =>
            Has(StatusId.TideForm);

        public bool Fends(Essence incoming)
        {
            return !string.IsNullOrEmpty(FendingName(incoming));
        }

        public string FendingName(Essence incoming)
        {
            if (incoming == Essence.None)
            {
                return string.Empty;
            }

            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Remaining <= 0f)
                {
                    continue;
                }

                var spec = _effects[i].Spec;
                if (incoming == Essence.Physical && spec.BlocksPhysical)
                {
                    return spec.Name;
                }

                if (incoming == Essence.Poison
                    && (spec.Id == StatusId.Windward || spec.Id == StatusId.GaleForm))
                {
                    return spec.Name;
                }

                if (spec.Id == StatusId.CloudForm && incoming == Essence.Water)
                {
                    return spec.Name;
                }

                if (spec.IsStance && ElementalLaw.WardsAgainst(spec.Element, incoming))
                {
                    return spec.Name;
                }
            }

            return string.Empty;
        }

        public string Summary()
        {
            if (_effects.Count == 0)
            {
                return string.Empty;
            }

            var parts = new List<string>(_effects.Count);
            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Remaining <= 0f)
                {
                    continue;
                }

                var spec = _effects[i].Spec;
                parts.Add(spec.IsMeter
                    ? $"{spec.Name} {_effects[i].Remaining:0.0}"
                    : spec.Name);
            }

            return string.Join(" · ", parts);
        }

        public string Apply(StatusId id, float seconds, Component caster = null, IReadOnlyList<RuneId> heldRunes = null, SpellId source = SpellId.None)
        {
            if (id == StatusId.None)
            {
                return string.Empty;
            }

            var spec = StatusSpec.Of(id);
            if (!spec.NeedsConcentration && !spec.IsMeter && seconds <= 0f)
            {
                return string.Empty;
            }

            var runes = heldRunes != null && heldRunes.Count > 0
                ? heldRunes
                : spec.NeedsConcentration ? FocusLaw.DefaultRunes(id) : System.Array.Empty<RuneId>();

            var incoming = spec.Element;
            if (!spec.IsWard && incoming != Essence.None && incoming != Essence.Mind
                && !StrikeLaw.IgnoresWard(source, incoming))
            {
                var ward = FendingName(incoming);
                if (!string.IsNullOrEmpty(ward))
                {
                    return $"A {ward} turns {spec.Name}.";
                }
            }

            var scale = Affinity(id);
            if (scale <= 0f)
            {
                return $"{name} will not take {spec.Name}.";
            }

            if (id == StatusId.Soaked)
            {
                Drop(StatusId.Burning, false);
            }

            var held = spec.NeedsConcentration
                ? float.PositiveInfinity
                : spec.IsMeter
                    ? MeterCapacity(id)
                    : scale * seconds;
            if (spec.IsMeter && held <= 0f)
            {
                return $"{name} will not take {spec.Name}.";
            }

            if (id == StatusId.Burning || id == StatusId.Stunned || id == StatusId.Frozen)
            {
                Drop(StatusId.Sleeping, false);
            }

            if (id == StatusId.Burning)
            {
                Drop(StatusId.Soaked, false);
            }

            if (spec.IsStance)
            {
                DropWhere(effect => effect.Spec.IsStance && effect.Id != id, true);
            }

            if (StatusSpec.IsMindAilment(id))
            {
                DropWhere(effect => StatusSpec.IsMindAilment(effect.Id) && effect.Id != id, true);
            }

            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Id == id)
                {
                    if (spec.RecastDismisses && SameWorking(_effects[i], source))
                    {
                        Drop(id, false);
                        return $"{name}'s {spec.Name} lifts.";
                    }

                    if (!spec.IsMeter)
                    {
                        _effects[i].Remaining = spec.NeedsConcentration
                            ? float.PositiveInfinity
                            : Mathf.Max(_effects[i].Remaining, held);
                    }

                    _effects[i].Caster = caster ?? _effects[i].Caster;
                    if (runes.Count > 0)
                    {
                        _effects[i].HeldRunes = runes;
                    }

                    if (source != SpellId.None)
                    {
                        _effects[i].SourceSpell = source;
                    }

                    RefreshChip();
                    return spec.IsMeter
                        ? $"{name} is {spec.Name} ({_effects[i].Remaining:0})."
                        : $"{name} is {spec.Name}.";
                }
            }

            _effects.Add(new StatusInstance(id, held, caster, runes, source));
            RefreshChip();
            return spec.IsMeter
                ? $"{name} is {spec.Name} ({held:0})."
                : $"{name} is {spec.Name}.";
        }

        public float MeterLeft(StatusId id)
        {
            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Id == id && _effects[i].Remaining > 0f)
                {
                    return _effects[i].Remaining;
                }
            }

            return 0f;
        }

        public float MeterFraction(StatusId id)
        {
            var capacity = MeterCapacity(id);
            if (capacity <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(MeterLeft(id) / capacity);
        }

        float MeterCapacity(StatusId id)
        {
            return VitalLaw.Seconds(id, Nature, AdeptAvatar.IsAdept(this));
        }

        public static int ReleaseAll(Component caster, IReadOnlyList<RuneId> used, StatusId keep, SpellId keepSpell = SpellId.None)
        {
            var broken = 0;
            for (var i = Live.Count - 1; i >= 0; i--)
            {
                if (Live[i] != null)
                {
                    broken += Live[i].ReleaseFocus(caster, used, keep, keepSpell);
                }
            }

            return broken;
        }

        public int ReleaseFocus(Component caster, IReadOnlyList<RuneId> used, StatusId keep, SpellId keepSpell = SpellId.None)
        {
            return DropWhere(effect =>
            {
                if (!effect.Held)
                {
                    return false;
                }

                if (keepSpell != SpellId.None && effect.SourceSpell == keepSpell)
                {
                    return false;
                }

                if (keepSpell == SpellId.None && keep != StatusId.None && effect.Id == keep)
                {
                    return false;
                }

                if (caster != null && effect.Caster != null && effect.Caster != caster)
                {
                    return false;
                }

                var held = effect.HeldRunes != null && effect.HeldRunes.Count > 0
                    ? effect.HeldRunes
                    : FocusLaw.DefaultRunes(effect.Id);
                return FocusLaw.Overlaps(used, held);
            }, true, keep);
        }

        public static string HeldBy(Component caster)
        {
            if (caster == null)
            {
                return string.Empty;
            }

            var parts = new List<string>(4);
            for (var i = 0; i < Live.Count; i++)
            {
                var host = Live[i];
                if (host == null)
                {
                    continue;
                }

                for (var e = 0; e < host._effects.Count; e++)
                {
                    var effect = host._effects[e];
                    if (!effect.Held || effect.Remaining <= 0f)
                    {
                        continue;
                    }

                    if (effect.Caster != null && effect.Caster != caster)
                    {
                        continue;
                    }

                    if (effect.Caster == null && host.gameObject != caster.gameObject)
                    {
                        continue;
                    }

                    var label = effect.Spec.Name;
                    if (!parts.Contains(label))
                    {
                        parts.Add(label);
                    }
                }
            }

            return string.Join(" · ", parts);
        }

        public void Clear()
        {
            _effects.Clear();
            RefreshChip();
            if (_sprite != null)
            {
                _sprite.color = _baseColor;
            }
        }

        public void Clear(StatusId id)
        {
            Drop(id, false);
        }

        static bool SameWorking(StatusInstance existing, SpellId source)
        {
            if (source == SpellId.None || existing.SourceSpell == SpellId.None)
            {
                return true;
            }

            return existing.SourceSpell == source;
        }

        int Drop(StatusId id, bool fizzle)
        {
            return DropWhere(effect => effect.Id == id, fizzle);
        }

        int DropWhere(System.Predicate<StatusInstance> match, bool fizzle, StatusId quiet = StatusId.None)
        {
            var removed = 0;
            StatusSpec shown = default;
            for (var i = _effects.Count - 1; i >= 0; i--)
            {
                if (!match(_effects[i]))
                {
                    continue;
                }

                shown = _effects[i].Spec;
                _effects.RemoveAt(i);
                removed++;
            }

            if (removed == 0)
            {
                return 0;
            }

            if (fizzle && shown.Id != quiet)
            {
                PlayFizzle(shown);
            }

            RefreshChip();
            return removed;
        }

        void PlayFizzle(StatusSpec spec)
        {
            SpellFx.PlayFizzle(transform.position, null);
            var director = FindFirstObjectByType<SanctumDirector>();
            if (director == null || spec.Id == StatusId.None)
            {
                return;
            }

            director.Log(spec.IsWard
                ? $"The {spec.Name} loses its hold. Another mind sentence asked a mark to do other work."
                : $"{name}'s {spec.Name} lifts. Focus broke — another mind sentence reused a mark.");
        }

        public void Zombify()
        {
            Profile = Profile.AsZombie();
            Nature = CreatureNature.Undead;
            Apply(StatusId.Zombified, float.PositiveInfinity);
        }

        public string Cleanse()
        {
            var removed = DropWhere(effect =>
            {
                if (effect.Id == StatusId.Zombified || effect.Spec.NeedsFocus)
                {
                    return false;
                }

                return effect.Spec.Kind == StatusKind.Debuff;
            }, false);
            return removed > 0
                ? $"{name} is shown clean. The foul lifts."
                : $"{name} is already clean.";
        }

        float Affinity(StatusId id)
        {
            return StrikeLaw.StatusAffinity(Profile, id);
        }

        void Update()
        {
            if (AdeptAvatar.WorldHeld)
            {
                return;
            }

            TickPoisonVeil();
            if (_effects.Count == 0)
            {
                return;
            }

            var grid = FindFirstObjectByType<WorldGrid>();
            var adept = AdeptAvatar.IsAdept(this) ? GetComponent<AdeptAvatar>() : null;
            var airborne = adept != null && adept.IsAirborne;
            for (var i = _effects.Count - 1; i >= 0; i--)
            {
                if (_effects[i].Held)
                {
                    continue;
                }

                if (_effects[i].Spec.IsMeter
                    && VitalLaw.MeterEndsWithoutContact(_effects[i].Id)
                    && !VitalLaw.ContactFeeds(_effects[i].Id, grid, transform.position, airborne)
                    && !(_effects[i].Id == StatusId.Burning && Has(StatusId.Rooted)))
                {
                    _effects.RemoveAt(i);
                    continue;
                }

                var drain = VitalLaw.MeterDrainScale(_effects[i].Id, grid, transform.position, airborne);
                if (_effects[i].Spec.IsMeter
                    && VitalLaw.MeterPausesWithoutContact(_effects[i].Id)
                    && drain <= 0f)
                {
                    continue;
                }

                _effects[i].Remaining -= Time.deltaTime * Mathf.Max(0f, drain);
                if (_effects[i].Remaining > 0f)
                {
                    continue;
                }

                var id = _effects[i].Id;
                var fatal = _effects[i].Spec.IsMeter;
                _effects.RemoveAt(i);
                if (fatal)
                {
                    OnFatal?.Invoke(id);
                }
            }

            RefreshChip();
        }

        void TickPoisonVeil()
        {
            if (AdeptAvatar.IsAdept(this) || Has(StatusId.Poisoned))
            {
                return;
            }

            if (Nature != CreatureNature.Flesh && Nature != CreatureNature.Mind)
            {
                return;
            }

            var grid = FindFirstObjectByType<WorldGrid>();
            if (VitalLaw.IsPoisonLiquidContact(grid != null ? grid.TileAtWorld(transform.position) : null)
                || WorldPhysics.MiasmaCloudAt(grid, transform.position))
            {
                Apply(StatusId.Poisoned, VitalLaw.AdeptPoisonSeconds);
            }
        }

        void RefreshChip()
        {
            var hidden = IsHidden;
            var text = hidden ? string.Empty : Summary();
            if (_chip != null)
            {
                _chip.text = text;
                _chip.color = DominantTint();
            }

            if (_sprite == null)
            {
                return;
            }

            if (hidden)
            {
                var fade = _baseColor;
                fade.a = 0.18f;
                _sprite.color = fade;
                return;
            }

            _sprite.color = text.Length == 0
                ? _baseColor
                : Color.Lerp(_baseColor, DominantTint(), 0.42f);
        }

        Color DominantTint()
        {
            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Remaining > 0f)
                {
                    return _effects[i].Spec.Tint;
                }
            }

            return new Color(0.92f, 0.86f, 0.7f);
        }
    }
}
