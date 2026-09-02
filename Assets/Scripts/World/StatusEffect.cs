using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum StatusId
    {
        None = 0,
        Burning,
        Frozen,
        Soaked,
        Stunned,
        Sleeping,
        Rooted,
        Frightened,
        Stoneskin,
        Veiled,
        Watershield,
        Flameward,
        Windward,
        Plantward,
        FlameForm,
        TideForm,
        StoneForm,
        GaleForm,
        GroveForm,
        CloudForm,
        Raging,
        Charmed,
        Confused,
        Poisoned,
        Zombified
    }

    public enum StatusKind
    {
        Debuff,
        Buff,
        Ward,
        Form
    }

    public enum CreatureNature
    {
        Flesh,
        Fire,
        Ice,
        Earth,
        Mind,
        Plant,
        Undead
    }

    /// <summary>
    /// A named condition the world can see. Buffs, wards, and debuffs share this.
    /// </summary>
    public readonly struct StatusSpec
    {
        public StatusSpec(
            StatusId id,
            string name,
            StatusKind kind,
            Color tint,
            Essence element,
            bool blocksAction,
            bool blocksMove,
            bool blocksPhysical)
        {
            Id = id;
            Name = name;
            Kind = kind;
            Tint = tint;
            Element = element;
            BlocksAction = blocksAction;
            BlocksMove = blocksMove;
            BlocksPhysical = blocksPhysical;
        }

        public StatusId Id { get; }
        public string Name { get; }
        public StatusKind Kind { get; }
        public Color Tint { get; }
        public Essence Element { get; }
        public bool BlocksAction { get; }
        public bool BlocksMove { get; }
        public bool BlocksPhysical { get; }
        public bool IsWard => Kind == StatusKind.Ward;
        public bool IsForm => Kind == StatusKind.Form;
        public bool IsStance => IsWard || IsForm;
        /// <summary>
        /// Focus holds mind work — ailments and wards. Wards use
        /// Sulphur; they are mind spells. Another focus sentence
        /// that reuses a mark other than Sulphur lets the held
        /// working go. A fireball or a wall does not.
        /// Burning and poison are meters: they run while the body
        /// still stands in that fire or foul, then empty is ash or death.
        /// Leave the cover and the condition resets.
        /// </summary>
        public bool NeedsConcentration => IsStance || IsMindAilment(Id);
        public bool NeedsFocus => NeedsConcentration;
        public bool IsMeter => VitalLaw.IsMeter(Id);
        public StatusClock Clock => VitalLaw.ClockOf(Id);
        public RuneId FocusRune
        {
            get
            {
                if (IsStance)
                {
                    switch (Id)
                    {
                        case StatusId.Stoneskin:
                        case StatusId.StoneForm: return RuneId.Earth;
                        case StatusId.Watershield:
                        case StatusId.TideForm: return RuneId.Water;
                        case StatusId.Flameward:
                        case StatusId.FlameForm: return RuneId.Fire;
                        case StatusId.Windward:
                        case StatusId.GaleForm:
                        case StatusId.CloudForm: return RuneId.Air;
                        case StatusId.Plantward:
                        case StatusId.GroveForm: return RuneId.Plant;
                    }
                }

                return IsMindAilment(Id) ? RuneId.Sulphur : RuneId.None;
            }
        }

        public const float PoisonKillSeconds = VitalLaw.AdeptPoisonSeconds;

        public static StatusSpec Of(StatusId id)
        {
            switch (id)
            {
                case StatusId.Burning:
                    return new StatusSpec(id, "burning", StatusKind.Debuff, new Color(1f, 0.45f, 0.12f), Essence.Fire, false, false, false);
                case StatusId.Frozen:
                    return new StatusSpec(id, "frozen", StatusKind.Debuff, new Color(0.55f, 0.82f, 1f), Essence.Water, true, true, false);
                case StatusId.Soaked:
                    return new StatusSpec(id, "soaked", StatusKind.Debuff, new Color(0.35f, 0.62f, 0.95f), Essence.Water, false, false, false);
                case StatusId.Stunned:
                    return new StatusSpec(id, "stunned", StatusKind.Debuff, new Color(0.95f, 0.9f, 0.35f), Essence.Air, true, true, false);
                case StatusId.Sleeping:
                    return new StatusSpec(id, "sleeping", StatusKind.Debuff, new Color(0.62f, 0.72f, 1f), Essence.Mind, true, true, false);
                case StatusId.Rooted:
                    return new StatusSpec(id, "rooted", StatusKind.Debuff, new Color(0.42f, 0.62f, 0.28f), Essence.Plant, false, true, false);
                case StatusId.Frightened:
                    return new StatusSpec(id, "frightened", StatusKind.Debuff, new Color(0.42f, 0.18f, 0.55f), Essence.Mind, false, false, false);
                case StatusId.Raging:
                    return new StatusSpec(id, "raging", StatusKind.Debuff, new Color(1f, 0.28f, 0.1f), Essence.Mind, false, false, false);
                case StatusId.Charmed:
                    return new StatusSpec(id, "charmed", StatusKind.Debuff, new Color(0.95f, 0.42f, 0.72f), Essence.Mind, false, false, false);
                case StatusId.Confused:
                    return new StatusSpec(id, "confused", StatusKind.Debuff, new Color(0.78f, 0.86f, 0.28f), Essence.Mind, false, false, false);
                case StatusId.Poisoned:
                    return new StatusSpec(id, "poisoned", StatusKind.Debuff, new Color(0.42f, 0.82f, 0.22f), Essence.Poison, false, false, false);
                case StatusId.Zombified:
                    return new StatusSpec(id, "zombified", StatusKind.Debuff, new Color(0.32f, 0.42f, 0.28f), Essence.None, false, false, false);
                case StatusId.Stoneskin:
                    return new StatusSpec(id, "stoneskin", StatusKind.Ward, new Color(0.62f, 0.58f, 0.5f), Essence.Earth, false, false, true);
                case StatusId.Veiled:
                    return new StatusSpec(id, "veiled", StatusKind.Buff, new Color(0.28f, 0.22f, 0.4f), Essence.None, false, false, false);
                case StatusId.Watershield:
                    return new StatusSpec(id, "water ward", StatusKind.Ward, new Color(0.28f, 0.58f, 0.95f), Essence.Water, false, false, false);
                case StatusId.Flameward:
                    return new StatusSpec(id, "flame ward", StatusKind.Ward, new Color(1f, 0.42f, 0.12f), Essence.Fire, false, false, false);
                case StatusId.Windward:
                    return new StatusSpec(id, "wind ward", StatusKind.Ward, new Color(0.72f, 0.86f, 0.95f), Essence.Air, false, false, false);
                case StatusId.Plantward:
                    return new StatusSpec(id, "plant ward", StatusKind.Ward, new Color(0.28f, 0.62f, 0.24f), Essence.Plant, false, false, false);
                case StatusId.FlameForm:
                    return new StatusSpec(id, "flame-form", StatusKind.Form, new Color(1f, 0.32f, 0.06f), Essence.Fire, false, false, false);
                case StatusId.TideForm:
                    return new StatusSpec(id, "tide-form", StatusKind.Form, new Color(0.18f, 0.52f, 0.95f), Essence.Water, false, false, false);
                case StatusId.StoneForm:
                    return new StatusSpec(id, "stone-form", StatusKind.Form, new Color(0.52f, 0.48f, 0.42f), Essence.Earth, false, false, true);
                case StatusId.GaleForm:
                    return new StatusSpec(id, "gale-form", StatusKind.Form, new Color(0.82f, 0.9f, 1f), Essence.Air, false, false, false);
                case StatusId.GroveForm:
                    return new StatusSpec(id, "grove-form", StatusKind.Form, new Color(0.16f, 0.48f, 0.2f), Essence.Plant, false, false, false);
                case StatusId.CloudForm:
                    return new StatusSpec(id, "cloud-form", StatusKind.Form, new Color(0.78f, 0.86f, 0.95f), Essence.Air, false, false, false);
                default:
                    return new StatusSpec(StatusId.None, "—", StatusKind.Debuff, Color.white, Essence.None, false, false, false);
            }
        }

        public static bool TryParse(string name, out StatusId id)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                id = StatusId.None;
                return false;
            }

            return System.Enum.TryParse(name, true, out id) && id != StatusId.None;
        }

        public static bool IsMindAilment(StatusId id)
        {
            return id == StatusId.Sleeping
                || id == StatusId.Frightened
                || id == StatusId.Raging
                || id == StatusId.Charmed
                || id == StatusId.Confused;
        }

        public static bool YieldsPassage(StatusId id)
        {
            return id == StatusId.Sleeping
                || id == StatusId.Stunned
                || id == StatusId.Frozen
                || id == StatusId.Charmed;
        }
    }

    public sealed class StatusInstance
    {
        public StatusInstance(StatusId id, float seconds, Component caster = null, IReadOnlyList<RuneId> heldRunes = null, SpellId source = SpellId.None)
        {
            Id = id;
            Remaining = StatusSpec.Of(id).NeedsConcentration
                ? float.PositiveInfinity
                : Mathf.Max(StatusSpec.Of(id).IsMeter ? 0f : 0.05f, seconds);
            Caster = caster;
            HeldRunes = heldRunes ?? System.Array.Empty<RuneId>();
            SourceSpell = source;
        }

        public StatusId Id { get; }
        public float Remaining { get; set; }
        public Component Caster { get; set; }
        public IReadOnlyList<RuneId> HeldRunes { get; set; }
        public SpellId SourceSpell { get; set; }
        public StatusSpec Spec => StatusSpec.Of(Id);
        public bool Held => Spec.NeedsConcentration;
    }
}
