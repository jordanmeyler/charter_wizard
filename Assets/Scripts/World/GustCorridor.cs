using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A stood wall of air: start to stop. It blows the adept and NPCs
    /// toward the far end while it lasts. Items stay put for now.
    /// </summary>
    public sealed class GustCorridor : MonoBehaviour
    {
        public const float LingerSeconds = 6f;
        public const float NudgeInterval = 0.45f;
        public const int NudgeTiles = 1;

        static readonly Color Breath = new(0.72f, 0.88f, 0.96f, 0.42f);

        WorldGrid _grid;
        Vector3 _start;
        Vector3 _stop;
        float _until;
        float _next;

        public static GustCorridor Lay(WorldGrid grid, Vector3 start, Vector3 stop)
        {
            var host = new GameObject("GustCorridor");
            host.transform.position = Vector3.Lerp(start, stop, 0.5f);
            var wall = host.AddComponent<GustCorridor>();
            wall.Build(grid, start, stop);
            return wall;
        }

        /// <summary>
        /// Shove living bodies on the span toward <paramref name="stop"/>.
        /// <paramref name="tiles"/> of 0 uses each body's Air-wall push.
        /// Carryables are left for a later pass.
        /// </summary>
        public static int Blow(WorldGrid grid, Vector3 start, Vector3 stop, int tiles = 0)
        {
            var blown = 0;
            var adept = AdeptAvatar.Find();
            if (adept != null && TryBlow(grid, start, stop, adept.transform, StatusHost.On(adept), tiles, null))
            {
                blown++;
            }

            var actors = Object.FindObjectsByType<CombatActor>(FindObjectsSortMode.None);
            for (var i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                if (actor == null || AdeptAvatar.IsAdept(actor))
                {
                    continue;
                }

                if (TryBlow(grid, start, stop, actor.transform, StatusHost.On(actor), tiles, actor))
                {
                    blown++;
                }
            }

            return blown;
        }

        void Build(WorldGrid grid, Vector3 start, Vector3 stop)
        {
            _grid = grid;
            _start = start;
            _stop = stop;
            _until = Time.time + LingerSeconds;
            _next = Time.time + NudgeInterval;
            Dress(start, stop);
        }

        void Update()
        {
            if (Time.time >= _until)
            {
                Destroy(gameObject);
                return;
            }

            if (Time.time < _next)
            {
                return;
            }

            _next = Time.time + NudgeInterval;
            Blow(_grid, _start, _stop, NudgeTiles);
        }

        void Dress(Vector3 start, Vector3 stop)
        {
            var delta = (Vector2)(stop - start);
            var length = delta.magnitude;
            if (length < 0.12f)
            {
                length = 0.12f;
                delta = Vector2.right * length;
            }

            var dir = delta / length;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var steps = Mathf.Max(2, Mathf.CeilToInt(length / 0.55f));
            for (var i = 0; i <= steps; i++)
            {
                var t = i / (float)steps;
                var point = (Vector2)start + dir * (length * t);
                var wisp = new GameObject("GustWisp");
                wisp.transform.SetParent(transform, false);
                wisp.transform.position = point;
                wisp.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                var scale = 0.55f + (i % 2) * 0.12f;
                wisp.transform.localScale = new Vector3(scale, scale * 0.7f, 1f);
                var view = wisp.AddComponent<SpriteRenderer>();
                view.sprite = SpriteFactory.Wisp(Breath);
                view.sortingOrder = 12;
                view.color = Color.Lerp(Breath, new Color(0.9f, 0.96f, 1f, 0.22f), (i % 3) * 0.2f);
            }
        }

        static bool TryBlow(
            WorldGrid grid,
            Vector3 start,
            Vector3 stop,
            Transform body,
            StatusHost host,
            int tiles,
            CombatActor actor)
        {
            if (body == null || !WorldWork.OnGustSpan(body.position, start, stop))
            {
                return false;
            }

            if (!StrikeLaw.CanPush(SpellId.AirWall, host))
            {
                return false;
            }

            var reach = tiles > 0 ? tiles : StrikeLaw.PushTiles(SpellId.AirWall, host);
            if (reach <= 0)
            {
                return false;
            }

            var land = WorldWork.GustLanding(grid, start, stop, body.position, reach);
            if (Vector2.Distance(land, body.position) < 0.2f)
            {
                return false;
            }

            if (actor != null)
            {
                actor.PlaceAt(land);
                return true;
            }

            var rb = body.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = land;
            }

            body.position = land;
            return true;
        }
    }
}
