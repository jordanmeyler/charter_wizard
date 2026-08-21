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
            _sprite = GetComponent<SpriteRenderer>();
            if (_sprite != null)
            {
                _baseColor = _sprite.color;
            }

            _chip = WorldLabel.Attach(transform, "", chipOffset, new Color(0.95f, 0.82f, 0.55f), 13);
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

                if (incoming == Essence.Poison && spec.Id == StatusId.Windward)
                {
                    return spec.Name;
                }

                if (spec.IsWard && ElementalLaw.Beats(spec.Element, incoming))
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
                if (_effects[i].Remaining > 0f)
                {
                    parts.Add(_effects[i].Spec.Name);
                }
            }

            return string.Join(" · ", parts);
        }

        public string Apply(StatusId id, float seconds, Component caster = null)
        {
            if (id == StatusId.None)
            {
                return string.Empty;
            }

            var spec = StatusSpec.Of(id);
            if (!spec.NeedsFocus && seconds <= 0f)
            {
                return string.Empty;
            }

            var incoming = spec.Element;
            if (!spec.IsWard && incoming != Essence.None && incoming != Essence.Mind)
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

            var held = spec.NeedsFocus ? float.PositiveInfinity : scale * seconds;
            if (id == StatusId.Burning || id == StatusId.Stunned || id == StatusId.Frozen)
            {
                Drop(StatusId.Sleeping, false);
            }

            if (spec.IsWard)
            {
                DropWhere(effect => effect.Spec.IsWard && effect.Id != id, true);
            }

            if (StatusSpec.IsMindAilment(id))
            {
                DropWhere(effect => StatusSpec.IsMindAilment(effect.Id) && effect.Id != id, true);
            }

            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Id == id)
                {
                    _effects[i].Remaining = spec.NeedsFocus
                        ? float.PositiveInfinity
                        : Mathf.Max(_effects[i].Remaining, held);
                    _effects[i].Caster = caster ?? _effects[i].Caster;
                    RefreshChip();
                    return $"{name} is {spec.Name}.";
                }
            }

            _effects.Add(new StatusInstance(id, held, caster));
            RefreshChip();
            return $"{name} is {spec.Name}.";
        }

        public static int ReleaseAll(Component caster, IReadOnlyList<RuneId> used, StatusId keep)
        {
            var broken = 0;
            for (var i = Live.Count - 1; i >= 0; i--)
            {
                if (Live[i] != null)
                {
                    broken += Live[i].ReleaseFocus(caster, used, keep);
                }
            }

            return broken;
        }

        public int ReleaseFocus(Component caster, IReadOnlyList<RuneId> used, StatusId keep)
        {
            return DropWhere(effect =>
            {
                if (!effect.Held || (keep != StatusId.None && effect.Id == keep))
                {
                    return false;
                }

                if (caster != null && effect.Caster != null && effect.Caster != caster)
                {
                    return false;
                }

                return FocusLaw.Contains(used, effect.Spec.FocusRune);
            }, true);
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

        int Drop(StatusId id, bool fizzle)
        {
            return DropWhere(effect => effect.Id == id, fizzle);
        }

        int DropWhere(System.Predicate<StatusInstance> match, bool fizzle)
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

            if (fizzle)
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
                ? $"The {spec.Name} loses its hold. That rune was asked to do other work."
                : $"{name}'s {spec.Name} lifts. The mind turned to another sentence.");
        }

        float Affinity(StatusId id)
        {
            if (id == StatusId.Poisoned)
            {
                return Nature == CreatureNature.Flesh || Nature == CreatureNature.Mind ? 1f : 0f;
            }

            switch (Nature)
            {
                case CreatureNature.Fire:
                    if (id == StatusId.Burning)
                    {
                        return 0f;
                    }

                    if (id == StatusId.Soaked || id == StatusId.Frozen)
                    {
                        return 1.45f;
                    }

                    break;
                case CreatureNature.Ice:
                    if (id == StatusId.Frozen)
                    {
                        return 0f;
                    }

                    if (id == StatusId.Burning)
                    {
                        return 1.5f;
                    }

                    break;
                case CreatureNature.Earth:
                    if (id == StatusId.Burning || id == StatusId.Frozen || id == StatusId.Soaked)
                    {
                        return 0.25f;
                    }

                    if (id == StatusId.Stunned || StatusSpec.IsMindAilment(id))
                    {
                        return 1.2f;
                    }

                    break;
                case CreatureNature.Mind:
                    if (id == StatusId.Stunned || StatusSpec.IsMindAilment(id))
                    {
                        return 1.35f;
                    }

                    break;
            }

            return 1f;
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

            for (var i = _effects.Count - 1; i >= 0; i--)
            {
                if (_effects[i].Held)
                {
                    continue;
                }

                _effects[i].Remaining -= Time.deltaTime;
                if (_effects[i].Remaining > 0f)
                {
                    continue;
                }

                var id = _effects[i].Id;
                _effects.RemoveAt(i);
                if (id == StatusId.Poisoned)
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

            if (VeilField.Covering(transform.position, out var kind) && kind == VeilKind.Poison)
            {
                Apply(StatusId.Poisoned, StatusSpec.PoisonKillSeconds);
            }
        }

        void RefreshChip()
        {
            var text = Summary();
            if (_chip != null)
            {
                _chip.text = text;
                _chip.color = DominantTint();
            }

            if (_sprite == null)
            {
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
