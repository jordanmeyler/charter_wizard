using UnityEngine;

namespace RuneMagic
{
    public enum CombatKind
    {
        None,
        Golem,
        Wizard
    }

    /// <summary>
    /// A lock that can strike back. Golems slam. Wizards write a sentence
    /// and send it. A beginner fireball takes two seconds.
    /// </summary>
    public sealed class CombatActor : MonoBehaviour
    {
        public CombatKind Kind { get; private set; }
        public float CastSeconds { get; private set; } = 2f;
        public Vector2 Facing { get; private set; } = Vector2.left;

        ISpellLock _lock;
        StatusHost _status;
        WorldGrid _grid;
        float _windup;
        bool _casting;
        Vector2 _committed;
        SpriteRenderer _sprite;
        TextMesh _castChip;
        float _reach = 1.2f;
        float _sight = 8.2f;

        public void Bind(CombatKind kind, float castSeconds, WorldGrid grid)
        {
            Kind = kind;
            CastSeconds = Mathf.Max(0.35f, castSeconds);
            _grid = grid;
            _lock = GetComponent<ISpellLock>();
            _status = GetComponent<StatusHost>();
            _sprite = GetComponent<SpriteRenderer>();
            _castChip = WorldLabel.Attach(transform, "", new Vector3(0f, 1.62f, 0f),
                new Color(1f, 0.72f, 0.28f), 14);
            if (_castChip != null)
            {
                _castChip.characterSize = 0.05f;
            }

            if (kind == CombatKind.Golem)
            {
                _reach = 1.25f;
                CastSeconds = Mathf.Max(0.7f, castSeconds <= 2.01f ? 0.85f : castSeconds);
            }
        }

        void Update()
        {
            if (Kind == CombatKind.None || _lock == null || _lock.Resolved || AdeptAvatar.WorldHeld)
            {
                ClearCastChip();
                return;
            }

            if (_status != null && _status.BlocksAction)
            {
                _casting = false;
                _windup = 0f;
                ClearCastChip();
                return;
            }

            var player = AdeptAvatar.Find();
            if (player == null)
            {
                return;
            }

            var toPlayer = (Vector2)(player.transform.position - transform.position);
            var distance = toPlayer.magnitude;
            if (distance > 0.05f)
            {
                Facing = toPlayer.normalized;
                if (_sprite != null && Mathf.Abs(Facing.x) > 0.12f)
                {
                    _sprite.flipX = Facing.x > 0f;
                }
            }

            if (Kind == CombatKind.Golem)
            {
                TickGolem(player, distance);
                return;
            }

            TickWizard(player, toPlayer, distance);
        }

        void TickGolem(AdeptAvatar player, float distance)
        {
            if (distance > _reach + 0.15f)
            {
                _windup = 0f;
                ClearCastChip();
                return;
            }

            _windup += Time.deltaTime;
            ShowCast("slam…");
            if (_windup < CastSeconds)
            {
                return;
            }

            _windup = 0f;
            ClearCastChip();
            if (player.IsAirborne)
            {
                FindFirstObjectByType<SanctumDirector>()?.Log("The slam passes under you.");
                return;
            }

            var host = StatusHost.On(player);
            if (host != null && host.Fends(Essence.Physical))
            {
                FindFirstObjectByType<SanctumDirector>()?.Log($"{host.FendingName(Essence.Physical)} takes the blow.");
                return;
            }

            FindFirstObjectByType<SanctumDirector>()?.KillPlayer("The golem's rest finds you. The crystal calls you back.");
        }

        void TickWizard(AdeptAvatar player, Vector2 toPlayer, float distance)
        {
            if (!_casting)
            {
                if (distance > _sight)
                {
                    return;
                }

                _casting = true;
                _windup = 0f;
                _committed = toPlayer.sqrMagnitude > 0.01f ? toPlayer.normalized : Facing;
            }

            _windup += Time.deltaTime;
            var left = Mathf.Max(0f, CastSeconds - _windup);
            ShowCast($"casting… {left:0.0}");
            if (_windup < CastSeconds)
            {
                return;
            }

            _casting = false;
            _windup = 0f;
            ClearCastChip();
            var origin = transform.position + (Vector3)(_committed * 0.45f);
            WorldProjectile.Spawn(origin, _committed, ProjectileKind.Fireball, _grid, 6.4f);
        }

        void ShowCast(string text)
        {
            if (_castChip != null)
            {
                _castChip.text = text;
            }
        }

        void ClearCastChip()
        {
            if (_castChip != null)
            {
                _castChip.text = string.Empty;
            }
        }
    }
}
