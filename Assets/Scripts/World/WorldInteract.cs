using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Use volume. Put this on an empty GameObject and dress the
    /// statue with tiles or child sprites — the component does not
    /// draw unless you assign a look. Prayer shows a written spell
    /// with elemental / catalyst labels, then Cast or Continue.
    /// </summary>
    public sealed class WorldInteract : MonoBehaviour, ILookable, IInteractable
    {
        [Header("Authoring")]
        [SerializeField] string verb = "Pray";
        [Tooltip("Catalog name (Fireball) or a written chain (Fire · Mercury). Empty offers an unkept written spell.")]
        [SerializeField] string spell;
        [SerializeField] string look = "a stone for prayer. The sentence waits.";
        [SerializeField] float radius = 1.15f;
        [Tooltip("Optional. Leave unset so painted tiles or child sprites carry the look.")]
        [SerializeField] string spriteId;
        [SerializeField] Sprite portrait;

        bool _wired;
        string _spell;
        string _look;
        string _verb;

        public Vector3 WorldPosition => transform.position;
        public float LookRadius => Mathf.Max(0.55f, radius);
        public float InteractRadius => Mathf.Max(0.4f, radius);
        public bool CanLook => true;
        public bool CanInteract => true;
        public string InteractVerb => string.IsNullOrWhiteSpace(_verb) ? "Interact" : _verb;
        public string LookText => Sight.OfInteract(_look, InteractVerb);
        public string AuthoredSpell => _spell;

        public static WorldInteract Spawn(Vector3 position, string spell = "", string verb = "Pray")
        {
            var host = new GameObject(string.IsNullOrWhiteSpace(verb) ? "Interact" : verb);
            host.transform.position = position;
            var view = host.AddComponent<WorldInteract>();
            view.spell = spell;
            view.verb = verb;
            view.BindFromAuthoring();
            return view;
        }

        public void BindFromAuthoring()
        {
            Bind(spell, look, verb);
        }

        public void Bind(string spell, string look, string verb)
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            _spell = string.IsNullOrWhiteSpace(spell) ? this.spell : spell;
            _look = string.IsNullOrWhiteSpace(look) ? this.look : look;
            _verb = string.IsNullOrWhiteSpace(verb) ? this.verb : verb;
            if (!string.IsNullOrWhiteSpace(spriteId) || portrait != null)
            {
                AuthoringUtil.ApplyLook(gameObject, 3, spriteId, portrait, null, 1f);
            }

            Lookables.Register(this);
            Interactables.Register(this);
        }

        public void EnsureBound()
        {
            Bind(_spell ?? spell, _look ?? look, _verb ?? verb);
        }

        public void Interact(SanctumDirector director)
        {
            if (director == null)
            {
                return;
            }

            if (!PrayerReveal.TryResolve(_spell, director.Grimoire, out var entry))
            {
                director.Log(GlyphView.Speak(
                    "The altar is silent. No written sentence answers.",
                    "The stone does not answer."));
                return;
            }

            GameHud.RevealWorking(entry);
            director.Log(GlyphView.Speak(
                $"A working is shown. {entry.Name} — {WorkingNames.RunePhrase(entry.RecipeRunes)}.",
                "A working is shown. Cast it, or leave it on the stone."));
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
            Interactables.Unregister(this);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.92f, 0.78f, 0.38f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.4f, radius));
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.92f, 0.78f, 0.38f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, 0.18f);
        }
    }
}
