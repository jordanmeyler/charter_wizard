using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// An ordered sentence written into the field. The world altar shows
    /// the mark beside a picture of the thing — a flame for Fire, a
    /// standing body for Salt — so Play can read it without a name.
    /// </summary>
    [ExecuteAlways]
    public sealed class RuneStringSource : MonoBehaviour, IRuneSource
    {
        [Header("Authoring")]
        [SerializeField] string[] runes = { "Fire" };
        [SerializeField] string dir = "right";
        [Tooltip("Your picture for this altar. Shown in the Scene view. Leave empty to use the generated mark at Play.")]
        [SerializeField] Sprite portrait;
        [Tooltip("Catalog / atlas id if you would rather name a sprite than drag one.")]
        [SerializeField] string spriteId;

        public int StringId { get; private set; }
        public RuneId[] Sequence { get; private set; }
        public Vector3 Heading { get; private set; }
        bool _wired;

        public bool IsEmitting => Sequence != null && Sequence.Length > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 3.2f;
        public float VoiceWeight => 1.6f;
        public RuneSourceKind SourceKind => RuneSourceKind.String;

        Transform _picture;
        TextMesh _name;
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
        }

        void ShowAltar()
        {
            var rune = Sequence.Length > 0 ? Sequence[0] : RuneId.None;
            if (portrait != null || !string.IsNullOrEmpty(spriteId))
            {
                AuthoringUtil.ApplyLook(gameObject, 5, spriteId, portrait, null, 1f);
                return;
            }

            RuneSign.MountAltar(transform, rune);
            _picture = transform.Find("Nature");
            _name = RuneSign.NamePlate(transform, rune, new Vector3(0f, 0.95f, 0f));
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
            var renderer = AuthoringUtil.GetOrAdd<SpriteRenderer>(gameObject);
            renderer.sortingOrder = 5;
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
            }
        }

        void LateUpdate()
        {
            if (_name != null)
            {
                _name.gameObject.SetActive(GlyphView.IsDevelop);
            }

            var rune = Sequence != null && Sequence.Length > 0 ? Sequence[0] : RuneId.None;
            RuneSign.Pulse(_picture, rune, Time.time - _born);
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
