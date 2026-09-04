using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// An ordered sentence written into the field. Unassigned marks
    /// float on their own — no altar slab.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public sealed class RuneStringSource : MonoBehaviour, IRuneSource, ILookable
    {
        [Header("Authoring")]
        [SerializeField] string[] runes = { "Fire" };
        [SerializeField] string dir = "right";
        [Tooltip("Your picture for this sentence. Leave empty for floating marks.")]
        [SerializeField] Sprite portrait;
        [Tooltip("Catalog / atlas id if you would rather name a sprite than drag one.")]
        [SerializeField] string spriteId;

        public int StringId { get; private set; }
        public RuneId[] Sequence { get; private set; }
        public Vector3 Heading { get; private set; }
        bool _wired;

        public bool IsEmitting => Sequence != null && Sequence.Length > 0;
        public Vector3 WorldOrigin => transform.position;
        public Vector3 WorldPosition => transform.position + Vector3.up * RuneStele.FloorHover;
        public float LookRadius => RuneStele.PickRadius;
        public bool CanLook => IsEmitting;
        public string LookText => Sight.OfRune(Sequence != null && Sequence.Length > 0 ? Sequence[0] : RuneId.None);
        public float VoiceRadius => 3.2f;
        public float VoiceWeight => 1.6f;
        public RuneSourceKind SourceKind => RuneSourceKind.String;

        Transform _picture;
        float _born;

        public static RuneStringSource Spawn(Vector3 origin, IReadOnlyList<RuneId> sequence, Vector3 heading)
        {
            var host = new GameObject("RuneString");
            host.transform.position = origin;
            var source = host.AddComponent<RuneStringSource>();
            source.Bind(sequence, heading);
            return source;
        }

        public void BindFromAuthoring()
        {
            if (_wired)
            {
                return;
            }

            Bind(AuthoringUtil.ParseRunes(runes, RuneId.Fire), MapFile.HeadingOf(dir));
        }

        public void Bind(IReadOnlyList<RuneId> sequence, Vector3 heading)
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            StringId = GetInstanceID();
            Sequence = sequence != null ? new RuneId[sequence.Count] : System.Array.Empty<RuneId>();
            if (sequence != null)
            {
                for (var i = 0; i < sequence.Count; i++)
                {
                    Sequence[i] = sequence[i];
                }
            }

            Heading = heading.sqrMagnitude < 0.0001f ? Vector3.right : heading.normalized;
            _born = Time.time;
            ShowAltar();
            Lookables.Register(this);
        }

        public static bool TryPick(Vector3 world, out RuneId rune, float extra = 0.2f)
        {
            rune = RuneId.None;
            var sources = Object.FindObjectsByType<RuneStringSource>(FindObjectsSortMode.None);
            var best = float.MaxValue;
            for (var i = 0; i < sources.Length; i++)
            {
                if (sources[i] != null && sources[i].TryPickHere(world, extra, out var found, out var distance) &&
                    distance < best)
                {
                    best = distance;
                    rune = found;
                }
            }

            return rune != RuneId.None;
        }

        bool TryPickHere(Vector3 world, float extra, out RuneId rune, out float distance)
        {
            rune = RuneId.None;
            distance = float.MaxValue;
            if (!IsEmitting)
            {
                return false;
            }

            var reach = LookRadius + extra;
            var step = Heading.sqrMagnitude < 0.0001f ? Vector3.right : Heading.normalized;
            for (var i = 0; i < Sequence.Length; i++)
            {
                if (Sequence[i] == RuneId.None)
                {
                    continue;
                }

                var mark = transform.position + step * (i * 0.55f) + new Vector3(0f, RuneStele.FloorHover, 0f);
                var gap = Vector2.Distance(world, mark);
                if (gap <= reach && gap < distance)
                {
                    distance = gap;
                    rune = Sequence[i];
                }
            }

            return rune != RuneId.None;
        }

        void ShowAltar()
        {
            var rune = Sequence.Length > 0 ? Sequence[0] : RuneId.None;
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

            if (Sequence.Length <= 1)
            {
                _picture = RuneSign.MountMark(transform, rune, RuneStele.FloorHover);
                AddPick(_picture);
                return;
            }

            var step = Heading.sqrMagnitude < 0.0001f ? Vector3.right : Heading.normalized;
            for (var i = 0; i < Sequence.Length; i++)
            {
                if (Sequence[i] == RuneId.None)
                {
                    continue;
                }

                var mark = RuneSign.MountMark(transform, Sequence[i], RuneStele.FloorHover, 0.82f);
                mark.localPosition = step * (i * 0.55f) + new Vector3(0f, RuneStele.FloorHover, 0f);
                AddPick(mark);
                if (_picture == null)
                {
                    _picture = mark;
                }
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
            if (!Application.isPlaying)
            {
                Preview();
            }
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
            {
                Preview();
            }
        }

        void Preview()
        {
            var renderer = AuthoringUtil.KeepRenderer(gameObject, 8);
            if (portrait != null)
            {
                renderer.sprite = portrait;
                renderer.enabled = true;
                return;
            }

            if (!string.IsNullOrEmpty(spriteId))
            {
                renderer.sprite = SpriteFactory.Named(spriteId);
                renderer.enabled = true;
                return;
            }

            if (renderer.sprite != null)
            {
                renderer.enabled = true;
                return;
            }

            var parsed = AuthoringUtil.ParseRunes(runes, RuneId.Fire);
            var first = parsed != null && parsed.Length > 0 ? parsed[0] : RuneId.Fire;
            renderer.sprite = RuneMark.AsSprite(first, RunePalette.MarkInk(first));
            renderer.enabled = first != RuneId.None;
            renderer.transform.localPosition = Vector3.zero;
        }

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

            var rune = Sequence != null && Sequence.Length > 0 ? Sequence[0] : RuneId.None;
            RuneSign.Pulse(_picture, rune, Time.time - _born, 0.9f);
        }

        public void Collect(List<RuneId> buffer)
        {
            if (!IsEmitting)
            {
                return;
            }

            for (var i = 0; i < Sequence.Length; i++)
            {
                if (Sequence[i] != RuneId.None)
                {
                    buffer.Add(Sequence[i]);
                }
            }
        }
    }
}
