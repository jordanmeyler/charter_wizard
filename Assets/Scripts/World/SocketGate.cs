using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A staged door. It opens when the adept holds this section's
    /// stones — never the running total of every stone on the floor.
    /// </summary>
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

        [Header("Authoring")]
        [SerializeField] string authoredName = "Gate";
        [SerializeField] string authoredId = "gate";
        [SerializeField] string[] requires;
        [SerializeField] bool finishes;
        [SerializeField] string note;
        [SerializeField] string spriteId = "socket-gate";
        [SerializeField] Sprite portrait;
        [SerializeField] Sprite[] idleFrames;
        [SerializeField] Vector2Int[] doorCells;

        string[] _requires;
        string _resolvedNote;
        SanctumDirector _director;
        WorldGrid _grid;
        Vector2Int[] _doors;
        float _pulse;
        bool _wired;

        public void Bind(
            string displayName,
            string formulaId,
            string[] requires,
            bool finishesFloor,
            string resolvedNote,
            string spriteId,
            WorldGrid grid = null,
            IList<Vector2Int> doors = null)
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
            _doors = doors != null ? new Vector2Int[doors.Count] : System.Array.Empty<Vector2Int>();
            if (doors != null)
            {
                for (var i = 0; i < doors.Count; i++)
                {
                    _doors[i] = doors[i];
                }
            }

            var art = string.IsNullOrEmpty(spriteId) ? "socket-gate" : spriteId;
            AuthoringUtil.ApplyLook(gameObject, 8, art, portrait, idleFrames, 3f);
            if (GetComponentInChildren<FixtureGlow>() == null)
            {
                FixtureGlow.Attach(transform, new Color(0.85f, 0.72f, 0.28f, 0.7f), 2.1f, 0.18f);
            }

            WorldLabel.Attach(transform, displayName, new Vector3(0f, 1.05f, 0f),
                new Color(0.95f, 0.84f, 0.45f));
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
                doorCells != null && doorCells.Length > 0 ? doorCells : null);
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

        void OpenDoors()
        {
            if (_grid == null || _doors == null)
            {
                return;
            }

            for (var i = 0; i < _doors.Length; i++)
            {
                _grid.Get(_doors[i])?.OpenDoor();
            }
        }

        void Update()
        {
            if (Resolved)
            {
                return;
            }

            _pulse += Time.deltaTime;
            transform.localScale = Vector3.one * (1f + Mathf.Sin(_pulse * 2.4f) * 0.04f);
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
    }
}
