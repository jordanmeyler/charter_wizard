using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// One teaching slab. An empty use volume — place it over
    /// tiles or under a prefab group. Transform / parent scale
    /// sizes the pray range (the gold circle). Check Teach Recipe
    /// to pray a written sentence. Check Show Birth to pray the
    /// join (sources = the born mark) on the same screen. Neither
    /// draws marks in the world.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    [AddComponentMenu("Rune Magic/Altar")]
    public sealed class WorldAltar : MonoBehaviour, ILookable, IInteractable, IRuneSource
    {
        const string DisplayChild = "Display";

        [Header("What this slab teaches")]
        [Tooltip("Pray shows a written recipe and Cast can aim it.")]
        [SerializeField] bool teachRecipe = true;
        [Tooltip("Pray shows the join: sources = the born mark (Fire · Air = Spark).")]
        [SerializeField] bool showBirth;

        [Header("Recipe")]
        [Tooltip("The sentence prayer shows and Cast aims. Click marks and Add.")]
        [RuneChain]
        [SerializeField] RuneId[] recipe = System.Array.Empty<RuneId>();
        [Tooltip("Optional second writing of the same working.")]
        [RuneChain]
        [SerializeField] RuneId[] via = System.Array.Empty<RuneId>();
        [Tooltip("When on, prayer also shows Via, or the catalog's other writing when Via is empty. Turn off to teach only the Recipe.")]
        [SerializeField] bool showOtherWriting = true;

        [Header("Birth")]
        [Tooltip("The wrought mark. Spark fills Fire · Air.")]
        [SerializeField] RuneId result = RuneId.Spark;
        [Tooltip("Leave empty to use the birth recipe.")]
        [RuneChain]
        [SerializeField] RuneId[] sources = System.Array.Empty<RuneId>();

        [Header("Use")]
        [SerializeField] string verb = "Pray";
        [SerializeField] string look = "a stone for prayer. The sentence waits.";
        [Tooltip("Pray / look reach. Transform or parent scale multiplies this. The gold circle in the Scene is that range.")]
        [SerializeField] float radius = 1.15f;
        [Tooltip("Optional. Leave unset so tiles or grouped prefabs carry the look.")]
        [SerializeField] string spriteId;
        [SerializeField] Sprite portrait;
        [HideInInspector]
        [SerializeField] string spell;

        bool _wired;
        RuneId[] _recipe;
        RuneId[] _via;
        bool _showOtherWriting = true;
        string _spell;
        string _look;
        string _verb;
        public RuneId Result { get; private set; } = RuneId.Spark;
        public RuneId[] Sources { get; private set; } = System.Array.Empty<RuneId>();
        public bool TeachesRecipe => teachRecipe;
        public bool ShowsBirth => showBirth;

        public Vector3 WorldOrigin => transform.position;
        public Vector3 WorldPosition => transform.position;
        public float LookRadius => Mathf.Max(0.55f, radius) * SizeScale;
        public float InteractRadius => Mathf.Max(0.4f, radius) * SizeScale;
        public float SizeScale
        {
            get
            {
                var scale = transform.lossyScale;
                return Mathf.Max(0.01f, Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
            }
        }

        public bool CanLook => true;
        public bool CanInteract => teachRecipe || showBirth;
        public string InteractVerb => string.IsNullOrWhiteSpace(_verb) ? "Pray" : _verb;
        public string LookText => showBirth && !teachRecipe
            ? Sight.OfBirth(Sources, Result)
            : Sight.OfInteract(_look, InteractVerb);
        public IReadOnlyList<RuneId> AuthoredRecipe => _recipe;
        public IReadOnlyList<RuneId> AuthoredVia => _via;
        public bool ShowsOtherWriting => _showOtherWriting;
        public bool IsEmitting => false;
        public float VoiceRadius => 3.2f;
        public float VoiceWeight => 1.8f;
        public RuneSourceKind SourceKind => RuneSourceKind.String;

        public static WorldAltar Spawn(
            Vector3 position,
            IReadOnlyList<RuneId> recipe,
            IReadOnlyList<RuneId> via = null,
            string verb = "Pray",
            string leftoverName = null,
            bool showOtherWriting = true)
        {
            var host = new GameObject("Altar");
            host.SetActive(false);
            host.transform.position = position;
            var view = host.AddComponent<WorldAltar>();
            view.teachRecipe = true;
            view.showBirth = false;
            view.recipe = PrayerWorking.Copy(recipe);
            view.via = PrayerWorking.Copy(via);
            view.showOtherWriting = showOtherWriting;
            view.verb = verb;
            view.spell = leftoverName ?? string.Empty;
            host.SetActive(true);
            view.BindFromAuthoring();
            return view;
        }

        public static WorldAltar SpawnBirth(
            Vector3 position,
            RuneId result,
            IReadOnlyList<RuneId> sources = null)
        {
            var host = new GameObject("Altar");
            host.SetActive(false);
            host.transform.position = position;
            var view = host.AddComponent<WorldAltar>();
            view.teachRecipe = false;
            view.showBirth = true;
            view.result = result;
            view.sources = PrayerWorking.HasMarks(sources)
                ? PrayerWorking.Copy(sources)
                : System.Array.Empty<RuneId>();
            host.SetActive(true);
            view.BindFromAuthoring();
            return view;
        }

        public static bool TryBirth(
            RuneId result,
            IReadOnlyList<RuneId> authoredSources,
            out IReadOnlyList<RuneId> sources,
            out RuneId born)
        {
            born = result;
            var copied = PrayerWorking.Copy(authoredSources);
            if (copied.Length > 0)
            {
                sources = copied;
                if (born == RuneId.None)
                {
                    born = copied[copied.Length - 1];
                }

                return born != RuneId.None;
            }

            if (born != RuneId.None && ChainBook.TryBirth(born, out var birth) && birth.Count > 0)
            {
                sources = PrayerWorking.Copy(birth);
                return true;
            }

            sources = System.Array.Empty<RuneId>();
            return born != RuneId.None;
        }

        public static bool TryPick(Vector3 world, out RuneId rune, float extra = 0.2f)
        {
            rune = RuneId.None;
            var altars = Object.FindObjectsByType<WorldAltar>(FindObjectsSortMode.None);
            var best = float.MaxValue;
            for (var i = 0; i < altars.Length; i++)
            {
                if (altars[i] != null && altars[i].TryPickHere(world, extra, out var found, out var distance) &&
                    distance < best)
                {
                    best = distance;
                    rune = found;
                }
            }

            return rune != RuneId.None;
        }

        public void BindFromAuthoring()
        {
            Bind();
        }

        public void EnsureWorking()
        {
            Bind();
        }

        public void Bind(
            string spell,
            string look,
            string verb,
            IReadOnlyList<RuneId> recipe,
            IReadOnlyList<RuneId> via,
            bool showOtherWriting = true)
        {
            this.spell = spell;
            this.look = look;
            this.verb = verb;
            this.recipe = PrayerWorking.Copy(recipe);
            this.via = PrayerWorking.Copy(via);
            this.showOtherWriting = showOtherWriting;
            teachRecipe = true;
            Bind();
        }

        void Bind()
        {
            _spell = spell;
            _look = look;
            _verb = verb;
            _recipe = PrayerWorking.Copy(recipe);
            _via = PrayerWorking.Copy(via);
            _showOtherWriting = showOtherWriting;
            ResolveBirth();
            ClearDisplay();
            if (!string.IsNullOrWhiteSpace(spriteId) || portrait != null)
            {
                AuthoringUtil.ApplyLook(gameObject, 3, spriteId, portrait, null, 1f);
            }

            if (_wired)
            {
                return;
            }

            _wired = true;
            Lookables.Register(this);
            if (teachRecipe || showBirth)
            {
                Interactables.Register(this);
            }
        }

        public void EnsureBound()
        {
            Bind();
        }

        public void AuthorBirth(RuneId born, IReadOnlyList<RuneId> authoredSources)
        {
            result = born;
            sources = authoredSources != null && PrayerWorking.HasMarks(authoredSources)
                ? PrayerWorking.Copy(authoredSources)
                : System.Array.Empty<RuneId>();
            ResolveBirth();
            ClearDisplay();
        }

        public void Interact(SanctumDirector director)
        {
            if (director == null || !CanInteract)
            {
                return;
            }

            var working = default(PrayerWorking);
            var taught = false;
            if (teachRecipe
                && PrayerReveal.TryResolve(_recipe, _via, _spell, director.Grimoire, out working, _showOtherWriting)
                && working.HasRecipe)
            {
                taught = true;
            }

            if (showBirth && (Result != RuneId.None || PrayerWorking.HasMarks(Sources)))
            {
                working = taught
                    ? PrayerReveal.WithBirth(working, Sources, Result)
                    : PrayerReveal.BirthOnly(Sources, Result);
                taught = true;
            }

            if (!taught || !working.HasContent)
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
            if (working.HasBirth && !working.HasRecipe)
            {
                var join = BirthPhrase(working);
                return GlyphView.Speak(
                    "A join is shown. " + join + ".",
                    "A join is shown. The marks become another mark.");
            }

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

            if (working.HasBirth)
            {
                phrase += " — " + BirthPhrase(working);
            }

            var develop = working.Entry.Spell != SpellId.None
                ? $"A working is shown. {phrase}. ({working.Entry.Name})"
                : $"A working is shown. {phrase}.";
            return GlyphView.Speak(
                develop,
                "A working is shown. Cast it, or leave it on the stone.");
        }

        static string BirthPhrase(PrayerWorking working)
        {
            var sources = WorkingNames.RunePhrase(working.BirthSources);
            var born = working.BirthResult == RuneId.None
                ? "a mark"
                : RuneCatalog.NameOf(working.BirthResult);
            return string.IsNullOrEmpty(sources) ? born : sources + " become " + born;
        }

        void ResolveBirth()
        {
            TryBirth(result, sources, out var resolved, out var shown);
            Result = shown;
            Sources = PrayerWorking.Copy(resolved);
        }

        bool TryPickHere(Vector3 world, float extra, out RuneId rune, out float distance)
        {
            rune = RuneId.None;
            distance = float.MaxValue;
            return false;
        }

        void ClearDisplay()
        {
            var child = transform.Find(DisplayChild);
            if (child == null)
            {
                return;
            }

            child.name = DisplayChild + ".old";
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                Bind();
                return;
            }

            ResolveBirth();
            ClearDisplay();
        }

        void OnValidate()
        {
            if (!teachRecipe && !showBirth)
            {
                teachRecipe = true;
            }

            if (Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += EditorRefresh;
#endif
        }

#if UNITY_EDITOR
        void EditorRefresh()
        {
            if (this == null || Application.isPlaying)
            {
                return;
            }

            ResolveBirth();
            ClearDisplay();
        }
#endif

        void OnDisable()
        {
            Lookables.Unregister(this);
            Interactables.Unregister(this);
        }

        public void Collect(List<RuneId> buffer)
        {
            if (buffer == null || !IsEmitting)
            {
                return;
            }

            if (Sources != null)
            {
                for (var i = 0; i < Sources.Length; i++)
                {
                    if (Sources[i] != RuneId.None)
                    {
                        buffer.Add(Sources[i]);
                    }
                }
            }

            if (Result != RuneId.None)
            {
                buffer.Add(Result);
            }
        }

        void OnDrawGizmosSelected()
        {
            DrawRange(new Color(0.92f, 0.78f, 0.38f, 0.9f));
        }

        void OnDrawGizmos()
        {
            DrawRange(new Color(0.92f, 0.78f, 0.38f, 0.45f));
        }

        void DrawRange(Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(WorldPosition, UseRadius);
            Gizmos.color = new Color(color.r, color.g, color.b, color.a * 0.7f);
            Gizmos.DrawWireSphere(transform.position, 0.12f);
        }

        float UseRadius => InteractRadius;
    }
}
