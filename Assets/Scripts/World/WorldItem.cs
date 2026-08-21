using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A pickup from the item catalog. Walking into it teaches a recipe.
    /// </summary>
    public sealed class WorldItem : MonoBehaviour
    {
        public bool Collected { get; private set; }

        CatalogItem _item;
        Grimoire _grimoire;
        AdeptPack _pack;
        System.Action<string> _log;

        public static GameObject Spawn(Vector3 position, CatalogItem item)
        {
            var host = new GameObject(item != null ? item.name : "Item");
            host.transform.position = position;
            var view = host.AddComponent<WorldItem>();
            view._item = item;
            return host;
        }

        public void Bind(Grimoire grimoire, System.Action<string> log, AdeptPack pack = null)
        {
            _grimoire = grimoire;
            _log = log;
            _pack = pack;
            var spriteId = _item != null && !string.IsNullOrEmpty(_item.sprite) ? _item.sprite : "charm";
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Named(spriteId);
            renderer.sortingOrder = 5;
            FixtureGlow.Attach(transform, new Color(1f, 0.5f, 0.12f, 0.65f), 1.5f, 0.14f);
            var hit = gameObject.AddComponent<CircleCollider2D>();
            hit.isTrigger = true;
            hit.radius = 0.4f;
            WorldLabel.Attach(transform, _item != null ? _item.name : "Item", new Vector3(0f, 0.7f, 0f),
                new Color(1f, 0.72f, 0.3f));
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Collected || !AdeptAvatar.IsAdept(other) || _item == null)
            {
                return;
            }

            Collected = true;
            var carried = AdeptPack.CanCarry(_item) && _pack != null && _pack.Take(_item);

            if (!string.IsNullOrEmpty(_item.teachesSpell) && SpellCodex.TryGet(SpellRegistry.Parse(_item.teachesSpell), out var entry))
            {
                _grimoire?.LearnRecipe(entry.RecipeRunes.Count > 0 ? entry.RecipeRunes[0] : RuneId.Fire, RuneId.Mercury, entry.Shape);
            }

            if (!string.IsNullOrEmpty(_item.teachesFormula))
            {
                _grimoire?.LearnInterpretation(_item.teachesFormula);
            }

            if (!string.IsNullOrEmpty(_item.note))
            {
                _log?.Invoke(_item.note);
            }
            else if (carried)
            {
                _log?.Invoke($"{_item.name} goes into the pack. I to look.");
            }
            else
            {
                _log?.Invoke($"{_item.name} unspools. A recipe is borrowed.");
            }
            Destroy(gameObject);
        }
    }
}
