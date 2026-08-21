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

        readonly List<StatusInstance> _effects = new();
        TextMesh _chip;
        SpriteRenderer _sprite;
        Color _baseColor = Color.white;

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

        public string Apply(StatusId id, float seconds)
        {
            if (id == StatusId.None || seconds <= 0f)
            {
                return string.Empty;
            }

            var incoming = StatusSpec.Of(id).Element;
            if (!ElementalLaw.IsWard(id) && incoming != Essence.None && incoming != Essence.Mind)
            {
                var ward = FendingName(incoming);
                if (!string.IsNullOrEmpty(ward))
                {
                    return $"A {ward} turns {StatusSpec.Of(id).Name}.";
                }
            }

            var scale = Affinity(id);
            if (scale <= 0f)
            {
                return $"{name} will not take {StatusSpec.Of(id).Name}.";
            }

            var held = scale * seconds;
            if (ElementalLaw.IsWard(id))
            {
                _effects.RemoveAll(effect => ElementalLaw.IsWard(effect.Id) && effect.Id != id);
            }

            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Id == id)
                {
                    _effects[i].Remaining = Mathf.Max(_effects[i].Remaining, held);
                    RefreshChip();
                    return $"{StatusSpec.Of(id).Name} holds.";
                }
            }

            _effects.Add(new StatusInstance(id, held));
            RefreshChip();
            return $"{StatusSpec.Of(id).Name} takes.";
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
            _effects.RemoveAll(effect => effect.Id == id);
            RefreshChip();
        }

        float Affinity(StatusId id)
        {
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

                    if (id == StatusId.Stunned || id == StatusId.Sleeping || id == StatusId.Frightened)
                    {
                        return 1.2f;
                    }

                    break;
                case CreatureNature.Mind:
                    if (id == StatusId.Stunned || id == StatusId.Sleeping || id == StatusId.Frightened)
                    {
                        return 1.35f;
                    }

                    break;
            }

            return 1f;
        }

        void Update()
        {
            if (AdeptAvatar.WorldHeld || _effects.Count == 0)
            {
                return;
            }

            for (var i = _effects.Count - 1; i >= 0; i--)
            {
                _effects[i].Remaining -= Time.deltaTime;
                if (_effects[i].Remaining <= 0f)
                {
                    _effects.RemoveAt(i);
                }
            }

            RefreshChip();
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
