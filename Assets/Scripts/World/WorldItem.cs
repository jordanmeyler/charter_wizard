using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A pickup from the item catalog. Walking into it teaches a recipe.
    /// Charmed bodies can carry a nearby prize to the adept.
    /// </summary>
    public sealed class WorldItem : MonoBehaviour, ILookable
    {
        public bool Collected { get; private set; }
        public bool Available => !Collected && _carrier == null;
        public CatalogItem Item => _item;
        public Vector3 WorldPosition => transform.position;
        public float LookRadius => 0.55f;
        public bool CanLook => !Collected && _item != null;
        public string LookText => Sight.OfItem(_item);

        CatalogItem _item;
        Grimoire _grimoire;
        AdeptPack _pack;
        System.Action<string> _log;
        Transform _carrier;
        Collider2D _hit;

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
            SpriteAnim.On(gameObject, renderer).Play(spriteId, 4f);
            PropBob.Attach(transform, 0.08f, 1.4f);
            FixtureGlow.Attach(transform, new Color(1f, 0.5f, 0.12f, 0.65f), 1.5f, 0.14f);
            _hit = gameObject.AddComponent<CircleCollider2D>();
            _hit.isTrigger = true;
            _hit.radius = 0.4f;
            WorldLabel.Attach(transform, _item != null ? _item.name : "Item", new Vector3(0f, 0.7f, 0f),
                new Color(1f, 0.72f, 0.3f));
            Lookables.Register(this);
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
        }

        public bool TryCarry(Transform carrier)
        {
            if (!Available || carrier == null)
            {
                return false;
            }

            _carrier = carrier;
            if (_hit != null)
            {
                _hit.enabled = false;
            }

            return true;
        }

        public bool DeliverTo(AdeptAvatar adept)
        {
            if (Collected || adept == null)
            {
                return false;
            }

            CollectInto();
            return true;
        }

        void Update()
        {
            if (_carrier == null || Collected)
            {
                return;
            }

            transform.position = _carrier.position + Vector3.up * 0.55f;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!Available || !AdeptAvatar.IsAdept(other) || _item == null)
            {
                return;
            }

            CollectInto();
        }

        void CollectInto()
        {
            if (Collected || _item == null)
            {
                return;
            }

            Collected = true;
            _carrier = null;
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
