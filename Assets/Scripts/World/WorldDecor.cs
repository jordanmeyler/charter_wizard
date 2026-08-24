using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A tileset prop: torch art, brazier, pillar, bush, fountain, rod.
    /// Not a lock. Drop one in the scene and set the sprite id.
    /// </summary>
    public sealed class WorldDecor : MonoBehaviour
    {
        [Header("Authoring")]
        [SerializeField] string spriteId = "pillar";
        [SerializeField] Sprite portrait;
        [SerializeField] bool blocking;
        [SerializeField] string look;

        bool _wired;

        public static WorldDecor Spawn(Vector3 world, string spriteId, bool blocking = false, string look = "")
        {
            var id = string.IsNullOrWhiteSpace(spriteId) ? "pillar" : spriteId.Trim();
            var actor = new GameObject("Decor_" + id);
            actor.transform.position = world;
            var decor = actor.AddComponent<WorldDecor>();
            decor.spriteId = id;
            decor.blocking = blocking;
            decor.look = look;
            decor.BindFromAuthoring();
            return decor;
        }

        public void BindFromAuthoring()
        {
            Bind(spriteId, blocking, look);
        }

        public void Bind(string spriteId, bool blocking, string look)
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            var id = string.IsNullOrWhiteSpace(spriteId) ? "pillar" : spriteId.Trim();
            AuthoringUtil.ApplyLook(gameObject, blocking ? 6 : 4, id, portrait, null, 1f);
            if (blocking && GetComponent<CircleCollider2D>() == null)
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
