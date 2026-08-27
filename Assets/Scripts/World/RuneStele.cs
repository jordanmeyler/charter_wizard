using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A teaching mark. Every catalog rune can stand as an inscription.
    /// With no portrait it is only a floating sign — no slab or shaft.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public sealed class RuneStele : MonoBehaviour, IRuneSource, ILookable
    {
        public enum Kind
        {
            Floor,
            Pillar
        }

        const string MarkChild = "Mark";
        public const float FloorHover = 0.42f;
        public const float PillarHover = 0.62f;
        public const float PickRadius = 0.85f;

        [Header("Authoring")]
        [SerializeField] RuneId authoredRune = RuneId.Fire;
        [SerializeField] Kind authoredForm = Kind.Floor;
        [Tooltip("Your picture for this rune. Leave empty for a floating mark.")]
        [SerializeField] Sprite portrait;
        [Tooltip("Catalog / atlas id if you would rather name a sprite than drag one.")]
        [SerializeField] string spriteId;

        public RuneId Rune { get; private set; }
        public Kind Form { get; private set; }
        bool _wired;

        public bool IsEmitting => Rune != RuneId.None;
        public Vector3 WorldOrigin => transform.position;
        public Vector3 WorldPosition => transform.position + Vector3.up * Hover;
        public float LookRadius => PickRadius;
        public bool CanLook => Rune != RuneId.None;
        public string LookText => Sight.OfRune(Rune);
        public float VoiceRadius => Form == Kind.Pillar ? 3.4f : 2.6f;
        public float VoiceWeight => 1.8f;
        public RuneSourceKind SourceKind => RuneSourceKind.String;
        public float Hover => Form == Kind.Pillar ? PillarHover : FloorHover;

        Transform _picture;

        public static RuneStele Inscribe(Vector3 origin, RuneId rune)
        {
            return Spawn(origin, rune, Kind.Floor);
        }

        public static RuneStele Raise(Vector3 origin, RuneId rune)
        {
            return Spawn(origin, rune, Kind.Pillar);
        }

        static RuneStele Spawn(Vector3 origin, RuneId rune, Kind form)
        {
            var host = new GameObject(NameOf(rune, form));
            host.transform.position = origin;
            var stele = host.AddComponent<RuneStele>();
            stele.Bind(rune, form);
            return stele;
        }

        public static string NameOf(RuneId rune, Kind form)
        {
            var mark = rune == RuneId.None ? "Rune" : RuneCatalog.NameOf(rune);
            return form == Kind.Pillar ? mark + " Pillar" : mark + " Inscription";
        }

        public static bool TryPick(Vector3 world, out RuneId rune, float extra = 0.2f)
        {
            rune = RuneId.None;
            var steles = Object.FindObjectsByType<RuneStele>(FindObjectsSortMode.None);
            var best = float.MaxValue;
            for (var i = 0; i < steles.Length; i++)
            {
                var stele = steles[i];
                if (stele == null || !stele.IsEmitting)
                {
                    continue;
                }

                var distance = Vector2.Distance(world, stele.WorldPosition);
                if (distance <= stele.LookRadius + extra && distance < best)
                {
                    best = distance;
                    rune = stele.Rune;
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

            Bind(authoredRune, authoredForm);
        }

        public void Bind(RuneId rune, Kind form)
        {
            _wired = true;
            Author(rune, form);
            Lookables.Register(this);
        }

        public void Author(RuneId rune, Kind form)
        {
            authoredRune = rune;
            authoredForm = form;
            Rune = rune;
            Form = form;
            gameObject.name = NameOf(rune, form);
            RefreshLook();
        }

        void RefreshLook()
        {
            if (Rune == RuneId.None)
            {
                Rune = authoredRune;
                Form = authoredForm;
            }

            gameObject.name = NameOf(Rune, Form);
            if (HasAuthoredLook())
            {
                ClearMark();
                var order = Form == Kind.Pillar ? 5 : 3;
                AuthoringUtil.ApplyLook(gameObject, order, spriteId, portrait, null, 1f);
                EnsurePickCollider(transform, 0.4f);
                return;
            }

            var host = GetComponent<SpriteRenderer>();
            if (host != null)
            {
                host.enabled = false;
                host.sprite = null;
            }

            _picture = EnsureMark();
            var mark = _picture.GetComponent<SpriteRenderer>();
            if (mark != null)
            {
                mark.sprite = RuneMark.AsSprite(Rune, RunePalette.MarkInk(Rune));
                mark.color = Color.white;
                mark.enabled = Rune != RuneId.None;
                mark.sortingOrder = 8;
            }

            _picture.localPosition = new Vector3(0f, Hover, 0f);
            _picture.localScale = Vector3.one;
            EnsurePickCollider(_picture, 0.48f);
        }

        Transform EnsureMark()
        {
            var child = transform.Find(MarkChild);
            if (child != null)
            {
                return child;
            }

            var mark = new GameObject(MarkChild);
            mark.transform.SetParent(transform, false);
            mark.AddComponent<SpriteRenderer>();
            return mark.transform;
        }

        void ClearMark()
        {
            _picture = null;
            var child = transform.Find(MarkChild);
            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        static void EnsurePickCollider(Transform host, float radius)
        {
            if (host == null)
            {
                return;
            }

            var collider = host.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = host.gameObject.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger = true;
            collider.radius = radius;
        }

        bool HasAuthoredLook()
        {
            return portrait != null || !string.IsNullOrEmpty(spriteId);
        }

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                Rune = authoredRune;
                Form = authoredForm;
                RefreshLook();
            }
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            Rune = authoredRune;
            Form = authoredForm;
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

            Rune = authoredRune;
            Form = authoredForm;
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

            RuneSign.Pulse(_picture, Rune, Time.time, Form == Kind.Pillar ? 0.95f : 0.9f);
        }

        public void Collect(List<RuneId> buffer)
        {
            if (IsEmitting)
            {
                buffer.Add(Rune);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.92f, 0.78f, 0.35f, 0.7f);
            Gizmos.DrawWireSphere(WorldPosition, PickRadius);
        }
#endif
    }
}
