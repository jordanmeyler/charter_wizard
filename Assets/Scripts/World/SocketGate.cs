using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A staged door. It opens when the adept holds this section's
    /// stones — never the running total of every stone on the floor.
    /// Drag a Portrait to replace the generated lock; leave Look empty
    /// to hide it. Requires is a list of pack item ids, not child objects.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public sealed class SocketGate : MonoBehaviour, ISpellLock, IRuneSource
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; } = System.Array.Empty<SpellId>();
        public bool Resolved { get; private set; }
        public bool FinishesFloor { get; private set; }
        public Vector3 WorldPosition => transform.position;

        public bool IsEmitting => !Resolved;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 3.6f;
        public float VoiceWeight => 1.8f;
        public RuneSourceKind SourceKind => RuneSourceKind.String;

        [Header("Lock")]
        [SerializeField] string authoredName = "Gate";
        [SerializeField] string authoredId = "gate";
        [Tooltip("Pack item ids that open this lock (fire-stone, earth-stone, …). Not objects you attach.")]
        [SerializeField] string[] requires;
        [SerializeField] bool finishes;
        [SerializeField] string note;
        [Tooltip("Door objects this lock opens. Drag WorldDoor objects here.")]
        [SerializeField] WorldDoor[] doors;
        [Tooltip("Legacy tile-door cells. Prefer Door objects.")]
        [SerializeField] Vector2Int[] doorCells;

        [Header("Look")]
        [Tooltip("Your sprite. When set, Play skips the generated socket, glow, and name.")]
        [SerializeField] Sprite portrait;
        [SerializeField] Sprite[] idleFrames;
        [Tooltip("Catalog / sheet id if you are not using Portrait. Empty still uses the generated socket unless Hide Look is on.")]
        [SerializeField] string spriteId = "socket-gate";
        [Tooltip("No picture, glow, or name. The lock still works. Paint tiles on the Tilemap for the look.")]
        [SerializeField] bool hideLook = true;
        [Tooltip("Soft generated glow. Ignored when Portrait is set.")]
        [SerializeField] bool showGlow = true;
        [Tooltip("Floating name. Ignored when Portrait is set.")]
        [SerializeField] bool showLabel = true;
        [Tooltip("Idle scale pulse. Ignored when Portrait is set.")]
        [SerializeField] bool pulse = true;

        string[] _requires;
        string _resolvedNote;
        SanctumDirector _director;
        WorldGrid _grid;
        WorldDoor[] _objectDoors;
        Vector2Int[] _doors;
        float _pulse;
        bool _wired;
        SpriteRenderer _renderer;

        bool HasAuthoredArt =>
            portrait != null || (idleFrames != null && idleFrames.Length > 0);

        public void Bind(
            string displayName,
            string formulaId,
            string[] requires,
            bool finishesFloor,
            string resolvedNote,
            string spriteId,
            WorldGrid grid = null,
            IList<Vector2Int> doors = null,
            IList<WorldDoor> objectDoors = null)
        {
            if (_wired)
            {
                if (grid != null)
                {
                    _grid = grid;
                }

                return;
            }

            _wired = true;
            DisplayName = displayName;
            FormulaId = formulaId;
            _requires = requires ?? System.Array.Empty<string>();
            FinishesFloor = finishesFloor;
            _resolvedNote = resolvedNote;
            _grid = grid;
            _objectDoors = CopyDoors(objectDoors);
            _doors = doors != null ? new Vector2Int[doors.Count] : System.Array.Empty<Vector2Int>();
            if (doors != null)
            {
                for (var i = 0; i < doors.Count; i++)
                {
                    _doors[i] = doors[i];
                }
            }

            ApplyPlayLook(spriteId);
        }

        public void BindFromAuthoring(WorldGrid grid)
        {
            if (_wired)
            {
                return;
            }

            Bind(
                authoredName,
                authoredId,
                requires,
                finishes,
                note,
                spriteId,
                grid,
                doorCells != null && doorCells.Length > 0 ? doorCells : null,
                this.doors);
        }

        public void Collect(List<RuneId> buffer)
        {
        }

        public string FormulaText()
        {
            if (_requires == null || _requires.Length == 0)
            {
                return "empty sockets";
            }

            var parts = new string[_requires.Length];
            for (var i = 0; i < _requires.Length; i++)
            {
                parts[i] = Pretty(_requires[i]);
            }

            return string.Join(" · ", parts);
        }

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            OpenDoors();
            return string.IsNullOrEmpty(_resolvedNote)
                ? $"{DisplayName} takes the stones it asked for. The way opens."
                : _resolvedNote;
        }

        void ApplyPlayLook(string id)
        {
            if (hideLook)
            {
                HideRenderer();
                return;
            }

            if (HasAuthoredArt)
            {
                AuthoringUtil.ApplyLook(gameObject, 8, string.Empty, portrait, idleFrames, 3f);
                return;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = "socket-gate";
            }

            AuthoringUtil.ApplyLook(gameObject, 8, id, null, null, 3f);
            if (showGlow && GetComponentInChildren<FixtureGlow>() == null)
            {
                FixtureGlow.Attach(transform, new Color(0.85f, 0.72f, 0.28f, 0.7f), 2.1f, 0.18f);
            }

            if (showLabel)
            {
                WorldLabel.Attach(transform, DisplayName, new Vector3(0f, 1.05f, 0f),
                    new Color(0.95f, 0.84f, 0.45f));
            }
        }

        void HideRenderer()
        {
            _renderer = AuthoringUtil.GetOrAdd<SpriteRenderer>(gameObject);
            _renderer.sprite = null;
            _renderer.enabled = false;
        }

        void OpenDoors()
        {
            var opened = false;
            if (_objectDoors != null)
            {
                for (var i = 0; i < _objectDoors.Length; i++)
                {
                    if (_objectDoors[i] == null)
                    {
                        continue;
                    }

                    _objectDoors[i].Open();
                    opened = true;
                }
            }

            if (_grid != null && _doors != null)
            {
                for (var i = 0; i < _doors.Length; i++)
                {
                    _grid.Get(_doors[i])?.OpenDoor();
                    opened = true;
                }
            }

            if (opened)
            {
                return;
            }

            var nearby = new List<WorldDoor>();
            WorldDoor.Nearby(transform.position, WorldDoor.AutoLinkRadius, nearby);
            for (var i = 0; i < nearby.Count; i++)
            {
                nearby[i]?.Open();
            }
        }

        static WorldDoor[] CopyDoors(IList<WorldDoor> doors)
        {
            if (doors == null || doors.Count == 0)
            {
                return System.Array.Empty<WorldDoor>();
            }

            var copy = new WorldDoor[doors.Count];
            for (var i = 0; i < doors.Count; i++)
            {
                copy[i] = doors[i];
            }

            return copy;
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (Resolved)
            {
                return;
            }

            if (pulse && !HasAuthoredArt)
            {
                _pulse += Time.deltaTime;
                transform.localScale = Vector3.one * (1f + Mathf.Sin(_pulse * 2.4f) * 0.04f);
            }

            if (_director == null)
            {
                _director = FindFirstObjectByType<SanctumDirector>();
            }

            if (_director == null || _director.Pack == null || _director.Busy)
            {
                return;
            }

            var player = AdeptAvatar.Find();
            if (player == null || Vector2.Distance(player.transform.position, transform.position) > 2.1f)
            {
                return;
            }

            if (!_director.Pack.HasAll(_requires))
            {
                return;
            }

            _director.TurnLock(this);
        }

        static string Pretty(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return "?";
            }

            if (CatalogBook.TryItem(id, out var item) && !string.IsNullOrEmpty(item.name))
            {
                return item.name;
            }

            return id.Replace("-", " ");
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            ApplyEditorLook();
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            UnityEditor.EditorApplication.delayCall += EditorRefresh;
        }

        void EditorRefresh()
        {
            if (this == null || Application.isPlaying)
            {
                return;
            }

            ApplyEditorLook();
        }

        void ApplyEditorLook()
        {
            _renderer = AuthoringUtil.KeepRenderer(gameObject, 8);
            if (hideLook)
            {
                _renderer.enabled = false;
                return;
            }

            if (portrait != null)
            {
                _renderer.enabled = true;
                _renderer.sprite = portrait;
                return;
            }

            if (!string.IsNullOrWhiteSpace(spriteId))
            {
                _renderer.enabled = true;
                _renderer.sprite = SpriteFactory.Named(spriteId);
                return;
            }

            _renderer.enabled = _renderer.sprite != null;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.95f, 0.84f, 0.45f, 0.85f);
            if (doors != null)
            {
                for (var i = 0; i < doors.Length; i++)
                {
                    if (doors[i] == null)
                    {
                        continue;
                    }

                    Gizmos.DrawLine(transform.position, doors[i].transform.position);
                }
            }

            if ((doors == null || doors.Length == 0) && (doorCells == null || doorCells.Length == 0))
            {
                Gizmos.color = new Color(0.95f, 0.84f, 0.45f, 0.25f);
                Gizmos.DrawWireSphere(transform.position, WorldDoor.AutoLinkRadius);
            }
        }
#endif
    }
}
