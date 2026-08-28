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
        RuneId[] _recipe = System.Array.Empty<RuneId>();

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
            shot._recipe = source != null ? source.CastRecipe : System.Array.Empty<RuneId>();
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

            var from = (Vector2)transform.position;
            var step = _velocity * Time.deltaTime;
            var next = from + step;
            if (HitSolid(from, next, out var hit))
            {
                transform.position = hit;
                BreakOnBody();
                return;
            }

            transform.position = next;
            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
            }
        }

        bool HitSolid(Vector2 from, Vector2 to, out Vector2 hit)
        {
            hit = to;
            if (_grid == null)
            {
                return false;
            }

            var travel = to - from;
            var distance = travel.magnitude;
            if (distance < 0.0001f)
            {
                return BlocksAt(to);
            }

            var originTile = _grid.TileAtWorld(from);
            var samples = Mathf.Max(1, Mathf.CeilToInt(distance / 0.12f));
            for (var i = 1; i <= samples; i++)
            {
                var point = from + travel * (i / (float)samples);
                if (!BlocksAt(point))
                {
                    continue;
                }

                var tile = _grid.TileAtWorld(point);
                if (tile == originTile)
                {
                    continue;
                }

                hit = point;
                return true;
            }

            return false;
        }

        bool BlocksAt(Vector2 point)
        {
            var tile = _grid != null ? _grid.TileAtWorld(point) : null;
            return WorldWork.BlocksCell(WorldWork.CoordOf(point), tile);
        }

        void BreakOnBody()
        {
            try
            {
                var color = _kind == ProjectileKind.Fireball
                    ? new Color(1f, 0.45f, 0.12f, 0.85f)
                    : new Color(0.85f, 0.78f, 0.62f, 0.8f);
                var flash = new GameObject("ShotBreak");
                flash.transform.position = transform.position;
                var renderer = flash.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteFactory.Burst(color);
                renderer.sortingOrder = 21;
                flash.transform.localScale = Vector3.one * 0.85f;
                Destroy(flash, 0.22f);
            }
            catch (System.Exception)
            {
            }

            Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            var tile = other.GetComponent<WorldTile>();
            if (tile != null && tile.BlocksTravel)
            {
                BreakOnBody();
                return;
            }

            var door = other.GetComponent<WorldDoor>();
            if (door != null && !door.IsOpen && door.BlocksWhenClosed)
            {
                BreakOnBody();
                return;
            }

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
                    BreakOnBody();
                    return;
                }

                FindFirstObjectByType<SanctumDirector>()?.KillPlayer(
                    DeathCause.OfKind(_kind, _recipe));
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
