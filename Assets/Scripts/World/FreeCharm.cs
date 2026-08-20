using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Free-magic items reveal their rune composition. Borrowing teaches the recipe.
    /// </summary>
    public sealed class FreeCharm : MonoBehaviour
    {
        public bool Collected { get; private set; }

        Grimoire _grimoire;
        System.Action<string> _log;

        public void Bind(Grimoire grimoire, System.Action<string> log)
        {
            _grimoire = grimoire;
            _log = log;

            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Charm();
            renderer.sortingOrder = 5;

            var hit = gameObject.AddComponent<CircleCollider2D>();
            hit.isTrigger = true;
            hit.radius = 0.4f;

            WorldLabel.Attach(transform, "Free charm", new Vector3(0f, 0.7f, 0f),
                new Color(1f, 0.72f, 0.3f));
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Collected || !AdeptAvatar.IsAdept(other))
            {
                return;
            }

            Collected = true;
            _grimoire.LearnRecipe(RuneId.Fire, RuneId.Mercury, SpellShape.Shot);
            _grimoire.LearnInterpretation("ash-mite");
            _log?.Invoke("The charm unspools. Fire × Mercury · Shot → Fireball. The mite is burning matter. You borrowed a key; later you will compose others.");
            Destroy(gameObject);
        }
    }
}
