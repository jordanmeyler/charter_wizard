using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Use volume. Put this on an empty GameObject and dress the
    /// statue with tiles or child sprites — the component does not
    /// draw unless you assign a look. Prayer shows the authored
    /// recipe (and a second writing when the spell has one), then
    /// Cast or Continue.
    /// </summary>
    public sealed class WorldInteract : MonoBehaviour, ILookable, IInteractable
    {
        [Header("Authoring")]
        [SerializeField] string verb = "Pray";
        [Tooltip("The sentence this altar teaches. Set the runes — names are not locked.")]
        [SerializeField] RuneId[] recipe;
        [Tooltip("Optional other writing of the same working (Spark · Mercury beside Fire · Air · Mercury). Leave empty to show the catalog's other chain when there is one.")]
        [SerializeField] RuneId[] via;
        [Tooltip("Leftover. A catalog name or written chain used only when Recipe is empty.")]
        [SerializeField] string spell;
        [SerializeField] string look = "a stone for prayer. The sentence waits.";
        [SerializeField] float radius = 1.15f;
        [Tooltip("Optional. Leave unset so painted tiles or child sprites carry the look.")]
        [SerializeField] string spriteId;
        [SerializeField] Sprite portrait;

        bool _wired;
        RuneId[] _recipe;
        RuneId[] _via;
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
        public IReadOnlyList<RuneId> AuthoredRecipe => _recipe;
        public IReadOnlyList<RuneId> AuthoredVia => _via;

        public static WorldInteract Spawn(Vector3 position, string spell = "", string verb = "Pray")
        {
            return Spawn(position, null, null, verb, spell);
        }

        public static WorldInteract Spawn(
            Vector3 position,
            IReadOnlyList<RuneId> recipe,
            IReadOnlyList<RuneId> via = null,
            string verb = "Pray",
            string spell = "")
        {
            var host = new GameObject(string.IsNullOrWhiteSpace(verb) ? "Interact" : verb);
            host.transform.position = position;
            var view = host.AddComponent<WorldInteract>();
            view.recipe = PrayerWorking.Copy(recipe);
            view.via = PrayerWorking.Copy(via);
            view.spell = spell ?? string.Empty;
            view.verb = verb;
            view.BindFromAuthoring();
            return view;
        }

        public void BindFromAuthoring()
        {
            Bind(spell, look, verb, recipe, via);
        }

        public void Bind(string spell, string look, string verb)
        {
            Bind(spell, look, verb, recipe, via);
        }

        public void Bind(
            string spell,
            string look,
            string verb,
            IReadOnlyList<RuneId> recipe,
            IReadOnlyList<RuneId> via)
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            _spell = string.IsNullOrWhiteSpace(spell) ? this.spell : spell;
            _look = string.IsNullOrWhiteSpace(look) ? this.look : look;
            _verb = string.IsNullOrWhiteSpace(verb) ? this.verb : verb;
            _recipe = PrayerWorking.Copy(recipe ?? this.recipe);
            _via = PrayerWorking.Copy(via ?? this.via);
            if (!string.IsNullOrWhiteSpace(spriteId) || portrait != null)
            {
                AuthoringUtil.ApplyLook(gameObject, 3, spriteId, portrait, null, 1f);
            }

            Lookables.Register(this);
            Interactables.Register(this);
        }

        public void EnsureBound()
        {
            Bind(_spell ?? spell, _look ?? look, _verb ?? verb, _recipe ?? recipe, _via ?? via);
        }

        public void Interact(SanctumDirector director)
        {
            if (director == null)
            {
                return;
            }

            if (!PrayerReveal.TryResolve(_recipe, _via, _spell, director.Grimoire, out var working)
                || !working.HasRecipe)
            {
                director.Log(GlyphView.Speak(
                    "The altar is silent. No written sentence answers.",
                    "The stone does not answer."));
                return;
            }

            GameHud.RevealWorking(working);
            director.Log(RevealLine(working));
        }

        public static string RevealLine(PrayerWorking working)
        {
            if (!working.HasRecipe)
            {
                return GlyphView.Speak(
                    "A working is shown.",
                    "A working is shown. Cast it, or leave it on the stone.");
            }

            var phrase = WorkingNames.RunePhrase(working.Recipe);
            if (working.HasVia)
            {
                phrase += " — or " + WorkingNames.RunePhrase(working.Via);
            }

            var develop = working.Entry.Spell != SpellId.None
                ? $"A working is shown. {phrase}. ({working.Entry.Name})"
                : $"A working is shown. {phrase}.";
            return GlyphView.Speak(
                develop,
                "A working is shown. Cast it, or leave it on the stone.");
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
