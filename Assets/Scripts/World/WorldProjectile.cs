using UnityEngine;

namespace RuneMagic
{
    public enum ProjectileKind
    {
        Arrow,
        Fireball
    }

    /// <summary>
    /// A thing that flies, can be blocked by a stood body, hopped over,
    /// and will unmake the adept if it lands.
    /// </summary>
    public sealed class WorldProjectile : MonoBehaviour
    {
        Vector2 _velocity;
        WorldGrid _grid;
        ProjectileKind _kind;
        float _life = 2.8f;
        SpriteRenderer _renderer;
        CombatActor _source;
        ShotAllegiance _allegiance = ShotAllegiance.Hostile;

        public static WorldProjectile Spawn(
            Vector3 from,
            Vector2 direction,
            ProjectileKind kind,
            WorldGrid grid,
            float speed = 7.2f,
            CombatActor source = null,
            ShotAllegiance allegiance = ShotAllegiance.Hostile)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }

            direction.Normalize();
            var host = new GameObject(kind == ProjectileKind.Arrow ? "Arrow" : "Fireball");
            host.transform.position = from;
            var shot = host.AddComponent<WorldProjectile>();
            shot._grid = grid;
            shot._kind = kind;
            shot._source = source;
            shot._allegiance = allegiance;
            shot._velocity = direction * speed;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            host.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            shot._renderer = host.AddComponent<SpriteRenderer>();
            shot._renderer.sprite = kind == ProjectileKind.Arrow
                ? SpriteFactory.Named("arrow-shot")
                : SpriteFactory.Named("fireball-shot");
            shot._renderer.sortingOrder = 18;
            if (kind == ProjectileKind.Fireball)
            {
                SpriteAnim.On(host, shot._renderer).Play("fireball-shot", 10f);
            }
            var body = host.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            var hit = host.AddComponent<CircleCollider2D>();
            hit.radius = 0.22f;
            hit.isTrigger = true;
            return shot;
        }

        void Update()
        {
            if (AdeptAvatar.WorldHeld)
            {
                return;
            }

            var next = (Vector2)transform.position + _velocity * Time.deltaTime;
            if (Blocked(next))
            {
                Destroy(gameObject);
                return;
            }

            transform.position = next;
            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
            }
        }

        bool Blocked(Vector2 point)
        {
            if (_grid == null)
            {
                return false;
            }

            var tile = _grid.TileAtWorld(point);
            return tile != null && tile.Def.BlocksMovement;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (AdeptAvatar.IsAdept(other))
            {
                if (_allegiance == ShotAllegiance.Allied)
                {
                    return;
                }

                var adept = other.GetComponent<AdeptAvatar>();
                if (adept != null && adept.IsAirborne)
                {
                    return;
                }

                var host = StatusHost.On(other);
                var incoming = ElementalLaw.Of(_kind);
                if (host != null && host.Fends(incoming))
                {
                    var ward = host.FendingName(incoming);
                    var note = _kind == ProjectileKind.Fireball
                        ? $"Hunger breaks on the {ward}."
                        : $"The shot breaks on the {ward}.";
                    FindFirstObjectByType<SanctumDirector>()?.Log(note);
                    Destroy(gameObject);
                    return;
                }

                var reason = _kind == ProjectileKind.Arrow
                    ? "An arrow finds you. The crystal calls you back."
                    : "Hunger sent finds you. The crystal calls you back.";
                FindFirstObjectByType<SanctumDirector>()?.KillPlayer(reason);
                Destroy(gameObject);
                return;
            }

            if (_allegiance == ShotAllegiance.Hostile)
            {
                return;
            }

            var encounter = other.GetComponent<EncounterLock>();
            if (encounter == null || encounter.Resolved)
            {
                return;
            }

            if (_source != null && encounter.gameObject == _source.gameObject)
            {
                return;
            }

            FindFirstObjectByType<SanctumDirector>()?.TurnLock(encounter);
            Destroy(gameObject);
        }
    }
}
