using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// One teaching slab. An empty use volume — dress the statue
    /// with a Portrait, child sprites, or nearby tiles. Transform
    /// scale sizes this volume, an authored look, and Show Birth
    /// marks; painted tilemap tiles stay tile-sized. Check Teach
    /// Recipe to pray a written sentence. Uncheck Show Other
    /// Writing to teach only that recipe. Check Show Birth to
    /// stand sources = the born mark (Fire · Air = Spark). Recipe
    /// and birth can be on at once.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    [AddComponentMenu("Rune Magic/Altar")]
    public sealed class WorldAltar : MonoBehaviour, ILookable, IInteractable, IRuneSource
    {
        const string DisplayChild = "Display";
        public const float MarkHover = 0.48f;
        public const float PickRadius = 0.85f;
        const float MarkStep = 0.55f;

        [Header("What this slab teaches")]
        [Tooltip("Pray shows a written recipe and Cast can aim it.")]
        [SerializeField] bool teachRecipe = true;
        [Tooltip("Stand the birth equation in the world: sources, an equals, then the born mark.")]
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
        [SerializeField] float radius = 1.15f;
        [Tooltip("Optional. Leave unset so tiles or the generated birth marks carry the look.")]
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
        Transform _picture;
        float _born;

        public RuneId Result { get; private set; } = RuneId.Spark;
        public RuneId[] Sources { get; private set; } = System.Array.Empty<RuneId>();
        public bool TeachesRecipe => teachRecipe;
        public bool ShowsBirth => showBirth;

        public Vector3 WorldOrigin => transform.position;
        public Vector3 WorldPosition => showBirth
            ? transform.TransformPoint(Vector3.up * MarkHover)
            : transform.position;
        public float LookRadius => LocalLookRadius * SizeScale;
        public float InteractRadius => Mathf.Max(0.4f, radius) * SizeScale;
        public float SizeScale
        {
            get
            {
                var scale = transform.lossyScale;
                return Mathf.Max(0.01f, Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
            }
        }

        float LocalLookRadius => showBirth
            ? PickRadius + 0.35f * (Sources != null ? Sources.Length : 0)
            : Mathf.Max(0.55f, radius);
        public bool CanLook => true;
        public bool CanInteract => teachRecipe;
        public string InteractVerb => string.IsNullOrWhiteSpace(_verb) ? "Pray" : _verb;
        public string LookText => showBirth
            ? Sight.OfBirth(Sources, Result)
            : Sight.OfInteract(_look, InteractVerb);
        public IReadOnlyList<RuneId> AuthoredRecipe => _recipe;
        public IReadOnlyList<RuneId> AuthoredVia => _via;
        public bool ShowsOtherWriting => _showOtherWriting;
        public bool IsEmitting => showBirth && (Result != RuneId.None || PrayerWorking.HasMarks(Sources));
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
            if (!string.IsNullOrWhiteSpace(spriteId) || portrait != null)
            {
                AuthoringUtil.ApplyLook(gameObject, 3, spriteId, portrait, null, 1f);
            }
            else if (showBirth)
            {
                RefreshLook();
            }

            if (_wired)
            {
                return;
            }

            _wired = true;
            _born = Time.time;
            Lookables.Register(this);
            if (teachRecipe)
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
            if (showBirth && portrait == null && string.IsNullOrEmpty(spriteId))
            {
                RefreshLook();
            }
        }

        public void Interact(SanctumDirector director)
        {
            if (director == null || !teachRecipe)
            {
                return;
            }

            if (!PrayerReveal.TryResolve(_recipe, _via, _spell, director.Grimoire, out var working, _showOtherWriting)
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
            if (!showBirth || !IsEmitting)
            {
                return false;
            }

            var reach = (0.48f + extra) * SizeScale;
            var layout = Layout();
            for (var i = 0; i < layout.Marks.Length; i++)
            {
                var gap = Vector2.Distance(world, transform.TransformPoint(layout.Marks[i].Local));
                if (gap <= reach && gap < distance)
                {
                    distance = gap;
                    rune = layout.Marks[i].Rune;
                }
            }

            return rune != RuneId.None;
        }

        void RefreshLook()
        {
            ResolveBirth();
            ClearDisplay();
            if (!showBirth)
            {
                return;
            }

            if (portrait != null || !string.IsNullOrEmpty(spriteId))
            {
                AuthoringUtil.ApplyLook(gameObject, 5, spriteId, portrait, null, 1f);
                return;
            }

            var host = GetComponent<SpriteRenderer>();
            if (host != null)
            {
                host.enabled = false;
                host.sprite = null;
            }

            var display = new GameObject(DisplayChild);
            display.transform.SetParent(transform, false);
            var slab = new GameObject("Slab");
            slab.transform.SetParent(display.transform, false);
            var baseView = slab.AddComponent<SpriteRenderer>();
            baseView.sprite = SpriteFactory.AltarBase();
            baseView.color = Color.white;
            baseView.sortingOrder = 5;
            var layout = Layout();
            slab.transform.localScale = new Vector3(Mathf.Max(1f, layout.Width / 1.15f), 1f, 1f);

            for (var i = 0; i < layout.Marks.Length; i++)
            {
                var mark = layout.Marks[i];
                var view = RuneSign.MountMark(display.transform, mark.Rune, MarkHover, mark.Rune == Result ? 0.95f : 0.82f);
                view.localPosition = mark.Local;
                AddPick(view);
                RuneSign.NamePlate(view, mark.Rune, new Vector3(0f, -0.42f, 0f));
                if (mark.Rune == Result)
                {
                    _picture = view;
                }
            }

            if (layout.HasEquals)
            {
                RuneSign.MountEquals(display.transform, layout.EqualsLocal, 0.72f);
            }

            var glow = SpriteFactory.HasNature(Result)
                ? Color.Lerp(RunePalette.Of(Result), Color.white, 0.15f)
                : new Color(0.9f, 0.82f, 0.55f);
            FixtureGlow.Attach(display.transform, new Color(glow.r, glow.g, glow.b, 0.5f), 1.55f, 0.12f);
        }

        readonly struct MarkAt
        {
            public MarkAt(RuneId rune, Vector3 local)
            {
                Rune = rune;
                Local = local;
            }

            public RuneId Rune { get; }
            public Vector3 Local { get; }
        }

        readonly struct AltarLayout
        {
            public AltarLayout(MarkAt[] marks, Vector3 equalsLocal, bool hasEquals, float width)
            {
                Marks = marks;
                EqualsLocal = equalsLocal;
                HasEquals = hasEquals;
                Width = width;
            }

            public MarkAt[] Marks { get; }
            public Vector3 EqualsLocal { get; }
            public bool HasEquals { get; }
            public float Width { get; }
        }

        AltarLayout Layout()
        {
            var count = 0;
            if (Sources != null)
            {
                for (var i = 0; i < Sources.Length; i++)
                {
                    if (Sources[i] != RuneId.None)
                    {
                        count++;
                    }
                }
            }

            var hasResult = Result != RuneId.None;
            var hasEquals = count > 0 && hasResult;
            var slots = count + (hasEquals ? 1 : 0) + (hasResult ? 1 : 0);
            if (slots == 0)
            {
                return new AltarLayout(System.Array.Empty<MarkAt>(), Vector3.zero, false, MarkStep);
            }

            var marks = new MarkAt[count + (hasResult ? 1 : 0)];
            var width = (slots - 1) * MarkStep;
            var x = -width * 0.5f;
            var written = 0;
            if (Sources != null)
            {
                for (var i = 0; i < Sources.Length; i++)
                {
                    if (Sources[i] == RuneId.None)
                    {
                        continue;
                    }

                    marks[written++] = new MarkAt(Sources[i], new Vector3(x, MarkHover, 0f));
                    x += MarkStep;
                }
            }

            var equalsLocal = Vector3.zero;
            if (hasEquals)
            {
                equalsLocal = new Vector3(x, MarkHover, 0f);
                x += MarkStep;
            }

            if (hasResult)
            {
                marks[written] = new MarkAt(Result, new Vector3(x, MarkHover, 0f));
            }

            return new AltarLayout(marks, equalsLocal, hasEquals, width + MarkStep);
        }

        void ClearDisplay()
        {
            _picture = null;
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

        static void AddPick(Transform mark)
        {
            if (mark == null)
            {
                return;
            }

            var collider = mark.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = mark.gameObject.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger = true;
            collider.radius = 0.48f;
        }

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                Bind();
                return;
            }

            PreviewLook();
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

            PreviewLook();
        }
#endif

        void PreviewLook()
        {
            ResolveBirth();
            if (portrait != null || !string.IsNullOrEmpty(spriteId))
            {
                if (showBirth)
                {
                    RefreshLook();
                    return;
                }

                AuthoringUtil.ApplyLook(gameObject, 3, spriteId, portrait, null, 1f);
                ClearDisplay();
                return;
            }

            if (showBirth)
            {
                RefreshLook();
                return;
            }

            ClearDisplay();
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
            Interactables.Unregister(this);
        }

        void LateUpdate()
        {
            if (!Application.isPlaying || !showBirth)
            {
                return;
            }

            RuneSign.Pulse(_picture, Result, Time.time - _born, 0.95f);
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
            var prior = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.92f, 0.78f, 0.38f, 0.85f);
            var local = showBirth ? Vector3.up * MarkHover : Vector3.zero;
            Gizmos.DrawWireSphere(local, Mathf.Max(0.4f, LocalLookRadius));
            Gizmos.matrix = prior;
        }

        void OnDrawGizmos()
        {
            var prior = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.92f, 0.78f, 0.38f, 0.35f);
            Gizmos.DrawWireSphere(Vector3.zero, 0.18f);
            Gizmos.matrix = prior;
        }
    }
}
