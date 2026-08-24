using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A tileset prop: torch, brazier, pillar, bush, fountain, rod art.
    /// Not a lock. Blocking pieces take a small collider.
    /// </summary>
    public sealed class WorldDecor : MonoBehaviour
    {
        public static WorldDecor Spawn(Vector3 world, string spriteId, bool blocking = false, string look = "")
        {
            var id = string.IsNullOrWhiteSpace(spriteId) ? "pillar" : spriteId.Trim();
            var actor = new GameObject("Decor_" + id);
            actor.transform.position = world;
            var decor = actor.AddComponent<WorldDecor>();
            decor.Bind(id, blocking, look);
            return decor;
        }

        public void Bind(string spriteId, bool blocking, string look)
        {
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Named(spriteId);
            renderer.sortingOrder = blocking ? 6 : 4;
            if (blocking)
            {
                var hit = gameObject.AddComponent<CircleCollider2D>();
                hit.radius = 0.32f;
            }

            if (!string.IsNullOrEmpty(look))
            {
                WorldLabel.Attach(transform, look, new Vector3(0f, 0.85f, 0f), new Color(0.82f, 0.78f, 0.7f));
            }
        }
    }
}
