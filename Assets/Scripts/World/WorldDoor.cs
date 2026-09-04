using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum DoorState
    {
        Closed = 0,
        Open = 1
    }

    /// <summary>
    /// A placeable door with an open and a closed picture. Shut, it
    /// stops walking and shots. A Gate opens the ones you assign, or
    /// any door standing next to it.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public sealed class WorldDoor : MonoBehaviour, ILookable
    {
        public const float AutoLinkRadius = 3.6f;

        static readonly List<WorldDoor> Live = new();
        static readonly List<Vector2Int> Scratch = new();

        [Header("Authoring")]
        [SerializeField] string authoredName = "Door";
        [SerializeField] DoorState startState = DoorState.Closed;
        [SerializeField] bool blocksWhenClosed = true;
        [Tooltip("How many cells the leaf covers, centred on this object.")]
        [SerializeField] int blockWidth = 1;
        [SerializeField] int blockHeight = 1;
        [Tooltip("Extra cells if the leaf is not a simple rectangle.")]
        [SerializeField] Vector2Int[] extraCells;
        [SerializeField] string look;

        [Header("Closed")]
        [SerializeField] Sprite closedPortrait;
        [SerializeField] string closedSpriteId = "door";
        [SerializeField] Sprite[] closedFrames;

        [Header("Open")]
        [SerializeField] Sprite openPortrait;
        [SerializeField] string openSpriteId = "door-open";
        [SerializeField] Sprite[] openFrames;

        WorldGrid _grid;
        SpriteRenderer _renderer;
        BoxCollider2D _hit;
        Vector2Int[] _cells = System.Array.Empty<Vector2Int>();
        bool _wired;

        public string DisplayName =>
            string.IsNullOrEmpty(authoredName) ? "Door" : authoredName;
        public bool IsOpen { get; private set; }
        public DoorState State => IsOpen ? DoorState.Open : DoorState.Closed;
        public bool BlocksWhenClosed => blocksWhenClosed;
        public Vector3 WorldPosition => transform.position;
        public float LookRadius => 0.7f;
        public bool CanLook => true;
        public string LookText =>
            !string.IsNullOrEmpty(look) ? look : Sight.OfDoor(this);

        public static WorldDoor Spawn(Vector3 world, DoorState start = DoorState.Closed)
        {
            var host = new GameObject("Door");
            host.transform.position = AuthoringUtil.Snap(world);
            var door = host.AddComponent<WorldDoor>();
            door.startState = start;
            door.BindFromAuthoring();
            return door;
        }

        public static bool BlocksCell(Vector2Int cell)
        {
            for (var i = Live.Count - 1; i >= 0; i--)
            {
                var door = Live[i];
                if (door == null)
                {
                    Live.RemoveAt(i);
                    continue;
                }

                if (door.Blocks(cell))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool BlocksWorld(Vector3 world) =>
            BlocksCell(AuthoringUtil.CellOf(world));

        public static void Nearby(Vector3 world, float radius, List<WorldDoor> buffer)
        {
            if (buffer == null)
            {
                return;
            }

            for (var i = Live.Count - 1; i >= 0; i--)
            {
                var door = Live[i];
                if (door == null)
                {
                    Live.RemoveAt(i);
                    continue;
                }

                if (Vector2.Distance(world, door.transform.position) <= radius)
                {
                    buffer.Add(door);
                }
            }
        }

        /// <summary>
        /// Cells this leaf covers. A Gate uses these as the lock
        /// even when the lock object sits a few tiles away.
        /// </summary>
        public Vector2Int[] OccupiedCells()
        {
            if (_cells == null || _cells.Length == 0)
            {
                RefreshCells();
            }

            return _cells ?? System.Array.Empty<Vector2Int>();
        }

        /// <summary>
        /// The lock's own cell, any authored extra cells, and every
        /// cell a linked door occupies.
        /// </summary>
        public static void GatherLockCells(
            Vector3 origin,
            IList<WorldDoor> doors,
            IList<Vector2Int> extra,
            List<Vector2Int> buffer)
        {
            if (buffer == null)
            {
                return;
            }

            AddCell(buffer, AuthoringUtil.CellOf(origin));
            if (extra != null)
            {
                for (var i = 0; i < extra.Count; i++)
                {
                    AddCell(buffer, extra[i]);
                }
            }

            if (doors == null)
            {
                return;
            }

            for (var i = 0; i < doors.Count; i++)
            {
                if (doors[i] == null)
                {
                    continue;
                }

                var cells = doors[i].OccupiedCells();
                if (cells == null)
                {
                    continue;
                }

                for (var j = 0; j < cells.Length; j++)
                {
                    AddCell(buffer, cells[j]);
                }
            }
        }

        static void AddCell(List<Vector2Int> buffer, Vector2Int cell)
        {
            if (!buffer.Contains(cell))
            {
                buffer.Add(cell);
            }
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            var origin = new Vector3(0.5f, 32.5f, 0f);
            var door = new Vector2Int(0, 38);
            var atDoor = WorldGrid.Center(door.x, door.y);
            var reach = new List<Vector2Int>();
            GatherLockCells(origin, null, new[] { door }, reach);
            if (CellVolume.DistanceTo(atDoor, origin, reach) > SocketGate.ApproachRadius)
            {
                broken.Add("A gate must turn when you stand at a linked door, even if the lock object sits a few tiles away");
            }

            var onlyLock = new List<Vector2Int> { AuthoringUtil.CellOf(origin) };
            if (CellVolume.DistanceTo(atDoor, origin, onlyLock) <= SocketGate.ApproachRadius)
            {
                broken.Add("A lock six tiles from a door must not feel a body at that door unless the door is linked");
            }

            if (!reach.Contains(AuthoringUtil.CellOf(origin)) || !reach.Contains(door))
            {
                broken.Add("A gate's reach must include its own cell and every linked door cell");
            }
        }

        public void BindFromAuthoring(WorldGrid grid = null)
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
            _grid = grid;
            IsOpen = startState == DoorState.Open;
            RefreshCells();
            ApplyState(Application.isPlaying);
            if (!Live.Contains(this))
            {
                Live.Add(this);
            }

            Lookables.Register(this);
        }

        public bool Occupies(Vector2Int cell)
        {
            if (_cells == null || _cells.Length == 0)
            {
                RefreshCells();
            }

            for (var i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Blocks(Vector2Int cell) =>
            !IsOpen && blocksWhenClosed && Occupies(cell);

        public void Open() => SetOpen(true);

        public void Close() => SetOpen(false);

        public void SetOpen(bool open)
        {
            if (IsOpen == open && _wired)
            {
                ApplyCollision();
                return;
            }

            IsOpen = open;
            ApplyState(Application.isPlaying);
            if (!open)
            {
                return;
            }

            OpenTileDoors();
        }

        void OpenTileDoors()
        {
            if (_grid == null)
            {
                _grid = FindFirstObjectByType<WorldGrid>();
            }

            if (_grid == null || _cells == null)
            {
                return;
            }

            for (var i = 0; i < _cells.Length; i++)
            {
                _grid.Get(_cells[i])?.OpenDoor();
            }
        }

        void RefreshCells()
        {
            Scratch.Clear();
            var origin = AuthoringUtil.CellOf(transform.position);
            var width = Mathf.Max(1, blockWidth);
            var height = Mathf.Max(1, blockHeight);
            var ox = origin.x - (width - 1) / 2;
            var oy = origin.y - (height - 1) / 2;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    Scratch.Add(new Vector2Int(ox + x, oy + y));
                }
            }

            if (extraCells != null)
            {
                for (var i = 0; i < extraCells.Length; i++)
                {
                    if (!Scratch.Contains(extraCells[i]))
                    {
                        Scratch.Add(extraCells[i]);
                    }
                }
            }

            _cells = Scratch.ToArray();
        }

        void ApplyState(bool playing)
        {
            RefreshCells();
            _renderer = AuthoringUtil.KeepRenderer(gameObject, 7);
            var frames = IsOpen ? openFrames : closedFrames;
            var portrait = IsOpen ? openPortrait : closedPortrait;
            var id = CurrentSpriteId();
            if (playing)
            {
                AuthoringUtil.ApplyLook(gameObject, 7, id, portrait, frames, 4f);
            }
            else if (portrait != null)
            {
                _renderer.sprite = portrait;
            }
            else if (_renderer.sprite == null)
            {
                _renderer.sprite = SpriteFactory.Named(id);
            }

            ApplyCollision();
        }

        string CurrentSpriteId()
        {
            if (IsOpen)
            {
                return string.IsNullOrEmpty(openSpriteId) ? "door-open" : openSpriteId;
            }

            return string.IsNullOrEmpty(closedSpriteId) ? "door" : closedSpriteId;
        }

        void ApplyCollision()
        {
            _hit = AuthoringUtil.GetOrAdd<BoxCollider2D>(gameObject);
            ColliderBounds(out var size, out var offset);
            _hit.size = size;
            _hit.offset = offset;
            _hit.isTrigger = false;
            _hit.enabled = Application.isPlaying && !IsOpen && blocksWhenClosed;
        }

        void ColliderBounds(out Vector2 size, out Vector2 offset)
        {
            if (_cells == null || _cells.Length == 0)
            {
                size = Vector2.one;
                offset = Vector2.zero;
                return;
            }

            var minX = _cells[0].x;
            var maxX = _cells[0].x;
            var minY = _cells[0].y;
            var maxY = _cells[0].y;
            for (var i = 1; i < _cells.Length; i++)
            {
                minX = Mathf.Min(minX, _cells[i].x);
                maxX = Mathf.Max(maxX, _cells[i].x);
                minY = Mathf.Min(minY, _cells[i].y);
                maxY = Mathf.Max(maxY, _cells[i].y);
            }

            size = new Vector2(maxX - minX + 1, maxY - minY + 1);
            var center = new Vector3((minX + maxX) * 0.5f + 0.5f, (minY + maxY) * 0.5f + 0.5f, 0f);
            offset = center - transform.position;
        }

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                IsOpen = startState == DoorState.Open;
                ApplyState(false);
                return;
            }

            BindFromAuthoring();
        }

        void OnDisable()
        {
            Live.Remove(this);
            Lookables.Unregister(this);
        }

        void OnValidate()
        {
            blockWidth = Mathf.Max(1, blockWidth);
            blockHeight = Mathf.Max(1, blockHeight);
            if (Application.isPlaying)
            {
                return;
            }

            IsOpen = startState == DoorState.Open;
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

            IsOpen = startState == DoorState.Open;
            ApplyState(false);
        }

        void OnDrawGizmos()
        {
            RefreshCells();
            Gizmos.color = IsOpen
                ? new Color(0.35f, 0.82f, 0.45f, 0.35f)
                : new Color(0.82f, 0.42f, 0.22f, 0.4f);
            if (_cells == null)
            {
                return;
            }

            for (var i = 0; i < _cells.Length; i++)
            {
                Gizmos.DrawCube(WorldGrid.Center(_cells[i].x, _cells[i].y), new Vector3(0.92f, 0.92f, 0.08f));
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = IsOpen
                ? new Color(0.45f, 0.95f, 0.55f, 0.85f)
                : new Color(0.95f, 0.55f, 0.28f, 0.9f);
            if (_cells == null)
            {
                return;
            }

            for (var i = 0; i < _cells.Length; i++)
            {
                Gizmos.DrawWireCube(WorldGrid.Center(_cells[i].x, _cells[i].y), Vector3.one);
            }
        }
#endif
    }
}
