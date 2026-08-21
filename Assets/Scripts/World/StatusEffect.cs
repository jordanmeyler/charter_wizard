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
        Veiled
    }

    public enum StatusKind
    {
        Debuff,
        Buff
    }

    public enum CreatureNature
    {
        Flesh,
        Fire,
        Ice,
        Earth,
        Mind
    }

    /// <summary>
    /// A named condition the world can see. Buffs and debuffs share this.
    /// </summary>
    public readonly struct StatusSpec
    {
        public StatusSpec(StatusId id, string name, StatusKind kind, Color tint, bool blocksAction, bool blocksMove, bool blocksPhysical)
        {
            Id = id;
            Name = name;
            Kind = kind;
            Tint = tint;
            BlocksAction = blocksAction;
            BlocksMove = blocksMove;
            BlocksPhysical = blocksPhysical;
        }

        public StatusId Id { get; }
        public string Name { get; }
        public StatusKind Kind { get; }
        public Color Tint { get; }
        public bool BlocksAction { get; }
        public bool BlocksMove { get; }
        public bool BlocksPhysical { get; }

        public static StatusSpec Of(StatusId id)
        {
            switch (id)
            {
                case StatusId.Burning:
                    return new StatusSpec(id, "burning", StatusKind.Debuff, new Color(1f, 0.45f, 0.12f), false, false, false);
                case StatusId.Frozen:
                    return new StatusSpec(id, "frozen", StatusKind.Debuff, new Color(0.55f, 0.82f, 1f), true, true, false);
                case StatusId.Soaked:
                    return new StatusSpec(id, "soaked", StatusKind.Debuff, new Color(0.35f, 0.62f, 0.95f), false, false, false);
                case StatusId.Stunned:
                    return new StatusSpec(id, "stunned", StatusKind.Debuff, new Color(0.95f, 0.9f, 0.35f), true, true, false);
                case StatusId.Sleeping:
                    return new StatusSpec(id, "sleeping", StatusKind.Debuff, new Color(0.62f, 0.72f, 1f), true, true, false);
                case StatusId.Rooted:
                    return new StatusSpec(id, "rooted", StatusKind.Debuff, new Color(0.42f, 0.62f, 0.28f), false, true, false);
                case StatusId.Frightened:
                    return new StatusSpec(id, "frightened", StatusKind.Debuff, new Color(0.42f, 0.18f, 0.55f), true, false, false);
                case StatusId.Stoneskin:
                    return new StatusSpec(id, "stoneskin", StatusKind.Buff, new Color(0.62f, 0.58f, 0.5f), false, false, true);
                case StatusId.Veiled:
                    return new StatusSpec(id, "veiled", StatusKind.Buff, new Color(0.28f, 0.22f, 0.4f), false, false, false);
                default:
                    return new StatusSpec(StatusId.None, "—", StatusKind.Debuff, Color.white, false, false, false);
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
    }

    public sealed class StatusInstance
    {
        public StatusInstance(StatusId id, float seconds)
        {
            Id = id;
            Remaining = Mathf.Max(0.05f, seconds);
        }

        public StatusId Id { get; }
        public float Remaining { get; set; }
        public StatusSpec Spec => StatusSpec.Of(Id);
    }
}
