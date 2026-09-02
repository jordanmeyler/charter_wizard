using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum SpeechCue
    {
        Approach,
        Interact,
        Both
    }

    /// <summary>
    /// A written window in the world. Approach pops it when the adept
    /// walks in — a gate greeting that can play once. Interact uses
    /// the same E / verb button as prayer: Read a sign, Talk to a
    /// figure. Extra pages are a first step toward conversation trees.
    /// Drop this on a Gate or Sign, or on an empty volume nearby.
    /// </summary>
    public sealed class WorldSpeech : MonoBehaviour, ILookable, IInteractable
    {
        [Header("Words")]
        [SerializeField] string title;
        [SerializeField] string speaker;
        [TextArea(3, 8)]
        [SerializeField] string text = "The way is shut.";
        [Tooltip("More pages after the first. Continue advances. A conversation tree can replace this later.")]
        [SerializeField] string[] pages;

        [Header("Trigger")]
        [SerializeField] SpeechCue cue = SpeechCue.Approach;
        [SerializeField] string verb = "Read";
        [SerializeField] float radius = 1.6f;
        [Tooltip("Approach shows this window only the first time someone walks in.")]
        [SerializeField] bool approachOnce = true;
        [Tooltip("Read / Talk can only be used once. Leave off so a sign can be read again.")]
        [SerializeField] bool interactOnce;
        [SerializeField] string look;

        [Header("Look")]
        [Tooltip("Optional. Leave unset so painted tiles or a parent Gate carry the picture.")]
        [SerializeField] string spriteId;
        [SerializeField] Sprite portrait;
        [Tooltip("No generated picture. The volume still works.")]
        [SerializeField] bool hideLook = true;

        bool _wired;
        bool _inside;
        bool _pendingApproach;
        bool _approachSpent;
        bool _interactSpent;
        SanctumDirector _director;

        public Vector3 WorldPosition => transform.position;
        public float LookRadius => Mathf.Max(0.55f, radius);
        public float InteractRadius => Mathf.Max(0.4f, radius);
        public bool CanLook => true;
        public bool CanInteract =>
            UsesInteract && !_interactSpent && !GameHud.ShowingSpeech;
        public string InteractVerb => string.IsNullOrWhiteSpace(verb) ? "Read" : verb.Trim();
        public string LookText => Sight.OfSpeech(look, title, InteractVerb);
        public SpeechCue Cue => cue;
        public bool ApproachSpent => _approachSpent;
        public bool InteractSpent => _interactSpent;

        bool UsesApproach => cue == SpeechCue.Approach || cue == SpeechCue.Both;
        bool UsesInteract => cue == SpeechCue.Interact || cue == SpeechCue.Both;

        public static WorldSpeech Spawn(
            Vector3 position,
            string text,
            SpeechCue cue = SpeechCue.Approach,
            string verb = "Read",
            bool approachOnce = true,
            string title = "",
            string speaker = "",
            string look = "",
            string spriteId = "")
        {
            var name = cue == SpeechCue.Interact
                ? (string.Equals(verb, "Talk", System.StringComparison.OrdinalIgnoreCase) ? "Talk" : "Sign")
                : "Speech";
            var host = new GameObject(name);
            host.transform.position = position;
            var view = host.AddComponent<WorldSpeech>();
            view.text = text;
            view.cue = cue;
            view.verb = verb;
            view.approachOnce = approachOnce;
            view.title = title;
            view.speaker = speaker;
            view.look = look;
            view.spriteId = spriteId;
            view.hideLook = string.IsNullOrWhiteSpace(spriteId);
            view.BindFromAuthoring();
            return view;
        }

        public void Author(
            SpeechCue cue,
            string verb,
            string text,
            bool approachOnce,
            bool interactOnce,
            bool hideLook,
            string spriteId)
        {
            this.cue = cue;
            this.verb = verb;
            this.text = text;
            this.approachOnce = approachOnce;
            this.interactOnce = interactOnce;
            this.hideLook = hideLook;
            this.spriteId = spriteId;
        }

        public void BindFromAuthoring()
        {
            Bind();
        }

        public void EnsureBound()
        {
            Bind();
        }

        public void Bind()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            if (!hideLook && (!string.IsNullOrWhiteSpace(spriteId) || portrait != null))
            {
                AuthoringUtil.ApplyLook(gameObject, 3, spriteId, portrait, null, 1f);
            }

            Lookables.Register(this);
            if (UsesInteract)
            {
                Interactables.Register(this);
            }
        }

        public void Interact(SanctumDirector director)
        {
            if (!UsesInteract || _interactSpent)
            {
                return;
            }

            _director = director;
            if (!TryOpen())
            {
                return;
            }

            if (interactOnce)
            {
                _interactSpent = true;
                Interactables.Unregister(this);
            }
        }

        public IReadOnlyList<string> CollectPages() => CollectPages(text, pages);

        public static List<string> CollectPages(string text, IReadOnlyList<string> extra)
        {
            var list = new List<string>();
            AddPage(list, text);
            if (extra != null)
            {
                for (var i = 0; i < extra.Count; i++)
                {
                    AddPage(list, extra[i]);
                }
            }

            if (list.Count == 0)
            {
                list.Add("…");
            }

            return list;
        }

        static void AddPage(List<string> list, string page)
        {
            if (!string.IsNullOrWhiteSpace(page))
            {
                list.Add(page.Trim());
            }
        }

        bool TryOpen()
        {
            if (GameHud.ShowingSpeech)
            {
                return false;
            }

            var lines = CollectPages();
            if (lines.Count == 0)
            {
                return false;
            }

            GameHud.ShowSpeech(title, speaker, lines);
            if (_director != null && !string.IsNullOrWhiteSpace(title))
            {
                _director.Log(title.Trim());
            }

            return true;
        }

        void Update()
        {
            if (!Application.isPlaying || !_wired || !UsesApproach)
            {
                return;
            }

            if (_approachSpent)
            {
                return;
            }

            var player = AdeptAvatar.Find();
            var inRange = player != null
                && Vector2.Distance(player.transform.position, transform.position) <= Mathf.Max(0.4f, radius);

            if (!inRange)
            {
                _inside = false;
                _pendingApproach = false;
                return;
            }

            if (!_inside)
            {
                _inside = true;
                _pendingApproach = true;
            }

            if (!_pendingApproach)
            {
                return;
            }

            if (GameHud.HoldsPlay || GameHud.ShowingSpeech)
            {
                return;
            }

            if (_director == null)
            {
                _director = FindFirstObjectByType<SanctumDirector>();
            }

            if (_director != null && (_director.Busy || _director.Mode != PlayMode.Exploring))
            {
                return;
            }

            if (!TryOpen())
            {
                return;
            }

            _pendingApproach = false;
            if (approachOnce)
            {
                _approachSpent = true;
            }
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
            Interactables.Unregister(this);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = UsesInteract
                ? new Color(0.86f, 0.72f, 0.42f, 0.85f)
                : new Color(0.72f, 0.58f, 0.92f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.4f, radius));
        }

        void OnDrawGizmos()
        {
            Gizmos.color = UsesInteract
                ? new Color(0.86f, 0.72f, 0.42f, 0.35f)
                : new Color(0.72f, 0.58f, 0.92f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, 0.18f);
        }
    }
}
