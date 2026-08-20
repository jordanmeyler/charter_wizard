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
            renderer.sprite = SpriteFactory.Square(new Color(0.85f, 0.55f, 0.2f), 24);
            renderer.sortingOrder = 4;

            var hit = gameObject.AddComponent<CircleCollider2D>();
            hit.isTrigger = true;
            hit.radius = 0.4f;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Collected || !other.CompareTag("Player"))
            {
                return;
            }

            Collected = true;
            _grimoire.LearnRecipe(RuneId.Fire, RuneId.Mercury);
            _grimoire.LearnInterpretation("cinder-moth");
            _log?.Invoke("The charm unspools. Fire × Mercury → Fireball. The moth is that formula, walking. You borrowed it; later you will compose.");
            Destroy(gameObject);
        }
    }
}
