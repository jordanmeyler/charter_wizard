using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A teaching slab that shows how a wrought element is made.
    /// Fire and Air stand on the left, an equals, then Spark.
    /// Pick the born rune and the birth recipe fills in; override
    /// Sources when you want a different writing.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public sealed class ElementalAltar : MonoBehaviour, IRuneSource, ILookable
    {
        const string DisplayChild = "Display";
        public const float MarkHover = 0.48f;
        public const float PickRadius = 0.85f;
        const float MarkStep = 0.55f;

        [Header("Authoring")]
        [Tooltip("The wrought mark this slab teaches. Spark fills Fire · Air.")]
        [SerializeField] RuneId result = RuneId.Spark;
        [Tooltip("Leave empty to use the birth recipe. Set marks to show another writing.")]
        [RuneChain]
        [SerializeField] RuneId[] sources = System.Array.Empty<RuneId>();
        [Tooltip("Optional. Leave unset so the generated slab and marks carry the look.")]
        [SerializeField] Sprite portrait;
        [SerializeField] string spriteId;

        public RuneId Result { get; private set; } = RuneId.Spark;
        public RuneId[] Sources { get; private set; } = System.Array.Empty<RuneId>();
        bool _wired;

        public bool IsEmitting => Result != RuneId.None || (Sources != null && Sources.Length > 0);
        public Vector3 WorldOrigin => transform.position;
        public Vector3 WorldPosition => transform.position + Vector3.up * MarkHover;
        public float LookRadius => PickRadius + 0.35f * (Sources != null ? Sources.Length : 0);
        public bool CanLook => IsEmitting;
        public string LookText => Sight.OfBirth(Sources, Result);
        public float VoiceRadius => 3.2f;
        public float VoiceWeight => 1.8f;
        public RuneSourceKind SourceKind => RuneSourceKind.String;

        Transform _picture;
        float _born;

        public static ElementalAltar Spawn(
            Vector3 origin,
            RuneId result,
            IReadOnlyList<RuneId> sources = null)
        {
            var host = new GameObject(NameOf(result));
            host.transform.position = origin;
            var altar = host.AddComponent<ElementalAltar>();
            altar.result = result;
            altar.sources = PrayerWorking.HasMarks(sources)
                ? PrayerWorking.Copy(sources)
                : System.Array.Empty<RuneId>();
            altar.BindFromAuthoring();
            return altar;
        }

        public static string NameOf(RuneId result)
        {
            return result == RuneId.None
                ? "Elemental Altar"
                : RuneCatalog.NameOf(result) + " Altar";
        }

        public static bool TryResolve(
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
            var altars = Object.FindObjectsByType<ElementalAltar>(FindObjectsSortMode.None);
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
            if (_wired)
            {
                return;
            }

            Bind(result, sources);
        }

        public void Bind(RuneId born, IReadOnlyList<RuneId> authoredSources)
        {
            _wired = true;
            Author(born, authoredSources);
            _born = Time.time;
            Lookables.Register(this);
        }

        public void Author(RuneId born, IReadOnlyList<RuneId> authoredSources)
        {
            TryResolve(born, authoredSources, out var resolved, out var shown);
            result = born == RuneId.None ? shown : born;
            sources = authoredSources != null && PrayerWorking.HasMarks(authoredSources)
                ? PrayerWorking.Copy(authoredSources)
                : System.Array.Empty<RuneId>();
            Result = shown;
            Sources = PrayerWorking.Copy(resolved);
            gameObject.name = NameOf(Result);
            RefreshLook();
        }

        bool TryPickHere(Vector3 world, float extra, out RuneId rune, out float distance)
        {
            rune = RuneId.None;
            distance = float.MaxValue;
            if (!IsEmitting)
            {
                return false;
            }

            var reach = 0.48f + extra;
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
            if (Result == RuneId.None && (Sources == null || Sources.Length == 0))
            {
                TryResolve(result, sources, out var resolved, out var shown);
                Result = shown;
                Sources = PrayerWorking.Copy(resolved);
            }

            gameObject.name = NameOf(Result);
            ClearDisplay();
            if (HasAuthoredLook())
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
            var slab = new GameObject("Altar");
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

        bool HasAuthoredLook()
        {
            return portrait != null || !string.IsNullOrEmpty(spriteId);
        }

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                TryResolve(result, sources, out var resolved, out var shown);
                Result = shown;
                Sources = PrayerWorking.Copy(resolved);
                RefreshLook();
            }
        }

        void OnValidate()
        {
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

            TryResolve(result, sources, out var resolved, out var shown);
            Result = shown;
            Sources = PrayerWorking.Copy(resolved);
            RefreshLook();
        }
#endif

        void OnDisable()
        {
            Lookables.Unregister(this);
        }

        void LateUpdate()
        {
            if (!Application.isPlaying)
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

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.92f, 0.78f, 0.35f, 0.7f);
            Gizmos.DrawWireSphere(WorldPosition, LookRadius);
        }
#endif
    }
}
