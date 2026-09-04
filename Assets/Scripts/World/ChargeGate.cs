using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A staged door that turns when electricity finds it — a bolt,
    /// a spark sentence, or charge walking onto its cells. Same
    /// Doors list as a socket gate; the key is the spark, not a stone.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public sealed class ChargeGate : MonoBehaviour, ISpellLock, IRuneSource, ISpellVolume
    {
        public const float ChargeThreshold = ChargeLaw.LiveMin;

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
        [SerializeField] string authoredName = "Electric Gate";
        [SerializeField] string authoredId = "electric-gate";
        [SerializeField] bool finishes;
        [SerializeField] string note;
        [Tooltip("Lightning / spark sentences that open this lock. Empty uses the rod keys.")]
        [SerializeField] string[] keys;
        [Tooltip("Door objects this lock opens. Drag WorldDoor objects here.")]
        [SerializeField] WorldDoor[] doors;
        [Tooltip("Legacy tile-door cells. Prefer Door objects.")]
        [SerializeField] Vector2Int[] doorCells;
        [Tooltip("Cells that take the spark. Empty uses this object's cell.")]
        [SerializeField] Vector2Int[] sensorCells;

        [Header("Look")]
        [Tooltip("Your sprite. When set, Play skips the generated socket, glow, and name.")]
        [SerializeField] Sprite portrait;
        [SerializeField] Sprite[] idleFrames;
        [SerializeField] Sprite[] liveFrames;
        [Tooltip("Catalog / sheet id if you are not using Portrait.")]
        [SerializeField] string spriteId = "rod";
        [SerializeField] string spriteLit = "rod-live";
        [Tooltip("No picture, glow, or name. The lock still works. Paint tiles on the Tilemap for the look.")]
        [SerializeField] bool hideLook = true;
        [Tooltip("Soft generated glow. Ignored when Portrait is set.")]
        [SerializeField] bool showGlow = true;
        [Tooltip("Floating name. Ignored when Portrait is set.")]
        [SerializeField] bool showLabel = true;
        [Tooltip("Idle scale pulse. Ignored when Portrait is set.")]
        [SerializeField] bool pulse = true;

        string _resolvedNote;
        SanctumDirector _director;
        WorldGrid _grid;
        WorldDoor[] _objectDoors;
        Vector2Int[] _doors;
        Vector2Int[] _cells;
        float _pulse;
        bool _wired;
        SpriteRenderer _renderer;

        bool HasAuthoredArt =>
            portrait != null || (idleFrames != null && idleFrames.Length > 0);

        public void Bind(
            string displayName,
            string formulaId,
            SpellId[] accepted,
            bool finishesFloor,
            string resolvedNote,
            string spriteId,
            WorldGrid grid = null,
            IList<Vector2Int> doors = null,
            IList<WorldDoor> objectDoors = null,
            IList<Vector2Int> sensors = null)
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
            AcceptedKeys = accepted != null && accepted.Length > 0
                ? accepted
                : MapBuilder.RodKeys;
            FinishesFloor = finishesFloor;
            _resolvedNote = resolvedNote;
            _grid = grid;
            _objectDoors = CopyDoors(objectDoors);
            _doors = CopyCells(doors);
            _cells = sensors != null && sensors.Count > 0
                ? CopyCells(sensors)
                : AuthoringUtil.CellsOrHere(null, transform.position);
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
                AuthoringUtil.ParseKeys(keys, MapBuilder.RodKeys),
                finishes,
                note,
                this.spriteId,
                grid,
                doorCells != null && doorCells.Length > 0 ? doorCells : null,
                doors,
                sensorCells);
        }

        public void Collect(List<RuneId> buffer)
        {
            if (buffer == null)
            {
                return;
            }

            buffer.Add(RuneId.Spark);
            buffer.Add(RuneId.Fire);
            buffer.Add(RuneId.Air);
            if (Resolved)
            {
                buffer.Add(RuneId.Lightning);
            }
        }

        public string FormulaText() =>
            Resolved ? "Sp Spark · live" : "Sp Spark · waiting";

        /// <summary>
        /// Any charge-bearing sentence turns the lock — a bolt, a
        /// strike, live-floor, jolt — not only the named keys.
        /// </summary>
        public bool YieldsTo(SpellId spell) =>
            WorldWork.IsChargeWork(spell);

        public float DistanceTo(Vector3 point) =>
            CellVolume.DistanceTo(point, transform.position, _cells);

        public Vector3 ClosestPoint(Vector3 point) =>
            CellVolume.ClosestPoint(point, transform.position, _cells);

        public bool Touches(Vector3 point, float radius) =>
            CellVolume.Touches(point, radius, transform.position, _cells);

        public bool Crosses(Vector3 from, Vector3 to, float width) =>
            CellVolume.Crosses(from, to, width, transform.position, _cells);

        public bool OccupiesCell(Vector2Int cell) =>
            CellVolume.Occupies(_cells, cell, transform.position);

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            OpenDoors();
            ShowLive();
            if (!string.IsNullOrEmpty(_resolvedNote))
            {
                return _resolvedNote;
            }

            return WorldWork.IsChargeWork(spell)
                ? $"{DisplayName} drinks the spark. The way opens."
                : $"{DisplayName} takes the charge. The way opens.";
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
                id = "rod";
            }

            AuthoringUtil.ApplyLook(gameObject, 8, id, null, null, 3f);
            if (showGlow && GetComponentInChildren<FixtureGlow>() == null)
            {
                FixtureGlow.Attach(transform, new Color(0.55f, 0.75f, 1f, 0.7f), 2.1f, 0.18f);
            }

            if (showLabel)
            {
                WorldLabel.Attach(transform, DisplayName, new Vector3(0f, 1.05f, 0f),
                    new Color(0.75f, 0.88f, 1f));
            }
        }

        void HideRenderer()
        {
            _renderer = AuthoringUtil.GetOrAdd<SpriteRenderer>(gameObject);
            _renderer.sprite = null;
            _renderer.enabled = false;
        }

        void ShowLive()
        {
            if (hideLook)
            {
                return;
            }

            _renderer = AuthoringUtil.GetOrAdd<SpriteRenderer>(gameObject);
            if (liveFrames != null && liveFrames.Length > 0)
            {
                SpriteAnim.On(gameObject, _renderer).Play(liveFrames, 12f, true, spriteLit);
                return;
            }

            if (!string.IsNullOrWhiteSpace(spriteLit))
            {
                _renderer.sprite = SpriteFactory.Named(spriteLit);
                SpriteAnim.On(gameObject, _renderer).Play(spriteLit, 12f);
            }
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

        static Vector2Int[] CopyCells(IList<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return System.Array.Empty<Vector2Int>();
            }

            var copy = new Vector2Int[cells.Count];
            for (var i = 0; i < cells.Count; i++)
            {
                copy[i] = cells[i];
            }

            return copy;
        }

        bool ChargedHere()
        {
            if (_grid == null)
            {
                _grid = FindFirstObjectByType<WorldGrid>();
            }

            if (_grid == null)
            {
                return false;
            }

            var tile = _grid.TileAtWorld(transform.position);
            if (tile != null && tile.Charge > ChargeThreshold)
            {
                return true;
            }

            if (_cells == null)
            {
                return false;
            }

            for (var i = 0; i < _cells.Length; i++)
            {
                var other = _grid.Get(_cells[i]);
                if (other != null && other.Charge > ChargeThreshold)
                {
                    return true;
                }
            }

            return false;
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

            if (pulse && !HasAuthoredArt && !hideLook)
            {
                _pulse += Time.deltaTime;
                transform.localScale = Vector3.one * (1f + Mathf.Sin(_pulse * 2.4f) * 0.04f);
            }

            if (_director == null)
            {
                _director = FindFirstObjectByType<SanctumDirector>();
            }

            if (_director == null || _director.Busy)
            {
                return;
            }

            if (ChargedHere())
            {
                _director.TurnLock(this);
            }
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
            Gizmos.color = new Color(0.55f, 0.82f, 1f, 0.85f);
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
                Gizmos.color = new Color(0.55f, 0.82f, 1f, 0.25f);
                Gizmos.DrawWireSphere(transform.position, WorldDoor.AutoLinkRadius);
            }

            var sensors = sensorCells != null && sensorCells.Length > 0
                ? sensorCells
                : new[] { AuthoringUtil.CellOf(transform.position) };
            Gizmos.color = new Color(0.75f, 0.92f, 1f, 0.35f);
            for (var i = 0; i < sensors.Length; i++)
            {
                Gizmos.DrawCube(WorldGrid.Center(sensors[i].x, sensors[i].y), new Vector3(0.88f, 0.88f, 0.08f));
            }
        }
#endif
    }
}
