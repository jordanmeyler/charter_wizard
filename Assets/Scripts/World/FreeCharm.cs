using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Free-magic items reveal their rune composition. Borrowing teaches the recipe.
    /// Charmed bodies can carry the charm to the adept.
    /// </summary>
    public sealed class FreeCharm : MonoBehaviour, ILookable, ICarryable
    {
        public bool Collected { get; private set; }
        public bool CanFetch => !Collected && _carrier == null;
        public string CarryName => "the charm";
        public Vector3 WorldPosition => transform.position;
        public float LookRadius => 0.55f;
        public bool CanLook => !Collected;
        public string LookText =>
            CatalogBook.TryItem("free-charm", out var item) ? Sight.OfItem(item) : "hunger given a path.";

        [Header("Authoring")]
        [SerializeField] string spriteId = "charm";
        [SerializeField] Sprite portrait;
        [SerializeField] Sprite[] idleFrames;

        Grimoire _grimoire;
        System.Action<string> _log;
        Transform _carrier;
        CircleCollider2D _hit;
        bool _wired;

        public void Bind(Grimoire grimoire, System.Action<string> log)
        {
            _grimoire = grimoire;
            _log = log;
            if (_wired)
            {
                return;
            }

            _wired = true;
            var art = spriteId;
            if (string.IsNullOrEmpty(art) && CatalogBook.TryItem("free-charm", out var item) && !string.IsNullOrEmpty(item.sprite))
            {
                art = item.sprite;
            }

            AuthoringUtil.ApplyLook(gameObject, 5, string.IsNullOrEmpty(art) ? "charm" : art, portrait, idleFrames, 5f);
            PropBob.Attach(transform, 0.08f, 1.6f);
            if (GetComponentInChildren<FixtureGlow>() == null)
            {
                FixtureGlow.Attach(transform, new Color(1f, 0.5f, 0.12f, 0.65f), 1.5f, 0.14f);
            }

            _hit = AuthoringUtil.GetOrAdd<CircleCollider2D>(gameObject);
            _hit.isTrigger = true;
            _hit.radius = 0.4f;

            WorldLabel.Attach(transform, "Free charm", new Vector3(0f, 0.7f, 0f),
                new Color(1f, 0.72f, 0.3f));
            Lookables.Register(this);
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
        }

        public bool TryCarry(Transform carrier)
        {
            if (!CanFetch || carrier == null)
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

        public void Drop()
        {
            if (Collected)
            {
                return;
            }

            if (_carrier != null)
            {
                transform.position = _carrier.position;
            }

            _carrier = null;
            if (_hit != null)
            {
                _hit.enabled = true;
            }
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
            if (!CanFetch || !AdeptAvatar.IsAdept(other))
            {
                return;
            }

            CollectInto();
        }

        void CollectInto()
        {
            if (Collected)
            {
                return;
            }

            Collected = true;
            _carrier = null;
            _grimoire?.LearnRecipe(RuneId.Fire, RuneId.Mercury, SpellShape.Shot);
            _grimoire?.LearnInterpretation("ash-mite");
            _log?.Invoke("The charm unspools. Hunger given a path — Fire · Mercury.");
            Destroy(gameObject);
        }
    }
}
