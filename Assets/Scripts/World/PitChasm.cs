using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class PitChasm : MonoBehaviour, ISpellLock, IRuneSource, ISpellVolume
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        public bool IsEmitting => !Resolved;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 2.2f;
        public float VoiceWeight => 0.4f;
        public RuneSourceKind SourceKind => RuneSourceKind.Creature;

        [Header("Authoring")]
        [SerializeField] string authoredName = "Chasm";
        [SerializeField] string authoredId = "chasm";
        [SerializeField] string[] keys;
        [Tooltip("Pit cells this lock covers. Leave empty to take nearby pits.")]
        [SerializeField] Vector2Int[] pitCells;

        WorldGrid _grid;
        Vector2Int[] _pits;
        bool _wired;

        public void BindFromAuthoring(WorldGrid grid)
        {
            if (_wired)
            {
                return;
            }

            var pits = pitCells != null && pitCells.Length > 0
                ? pitCells
                : NearbyPits(grid, transform.position, 12);
            Bind(
                authoredName,
                authoredId,
                AuthoringUtil.ParseKeys(keys, MapBuilder.PitKeys),
                grid,
                pits);
        }

        public void Bind(string displayName, string formulaId, SpellId[] keys, WorldGrid grid, IList<Vector2Int> pits)
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            DisplayName = displayName;
            FormulaId = formulaId;
            AcceptedKeys = keys;
            _grid = grid;
            _pits = new Vector2Int[pits.Count];
            for (var i = 0; i < pits.Count; i++)
            {
                _pits[i] = pits[i];
            }

            WorldLabel.Attach(transform, "PIT", new Vector3(0f, 0.15f, 0f), new Color(0.95f, 0.55f, 0.35f));
        }

        public void Collect(System.Collections.Generic.List<RuneId> buffer)
        {
        }

        public float DistanceTo(Vector3 point) =>
            CellVolume.DistanceTo(point, transform.position, _pits);

        public Vector3 ClosestPoint(Vector3 point) =>
            CellVolume.ClosestPoint(point, transform.position, _pits);

        public bool Touches(Vector3 point, float radius) =>
            CellVolume.Touches(point, radius, transform.position, _pits);

        public bool Crosses(Vector3 from, Vector3 to, float width) =>
            CellVolume.Crosses(from, to, width, transform.position, _pits);

        public bool OccupiesCell(Vector2Int cell) =>
            CellVolume.Occupies(_pits, cell, transform.position);

        public string FormulaText() => "Earth is missing — the weave tears";

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            if (!WorldWork.LeavesGapsWhenCrossing(spell) && _grid != null && _pits != null)
            {
                foreach (var coord in _pits)
                {
                    _grid.Get(coord)?.BecomeBridge();
                }
            }

            return spell == SpellId.Hop
                ? "Breath given a body carries you. The drop is crossed."
                : spell == SpellId.Flight
                    ? "A body of breath stays on you. The drop cannot take you."
                    : spell == SpellId.Float
                        ? "Breath going stands on you. You hang. The drop cannot take you."
                    : spell == SpellId.Blink
                        ? "The seed jumps you. The drop is crossed."
                    : spell == SpellId.Teleport
                        ? "The seed is withheld and shown. The drop cannot take you."
                    : spell == SpellId.Wall || spell == SpellId.IceWall || spell == SpellId.ObsidianWall || spell == SpellId.StonePillar || spell == SpellId.EarthPillar
                        ? "A standing body fills the gap, or bars the floor."
                        : WorldWork.IsPillar(spell)
                            ? "A column settles into the hollow and holds."
                            : spell == SpellId.Pit || spell == SpellId.RaisedEarth
                                ? "Earth answers away from you and leaves a hollow filled."
                                : spell == SpellId.Bridge
                                    ? "A body of rest given breath spans the drop."
                                    : "Earth takes spirit and flies. Hurled stone piles into a bridge.";
        }

        static Vector2Int[] NearbyPits(WorldGrid grid, Vector3 world, int reach)
        {
            if (grid == null)
            {
                return new[] { AuthoringUtil.CellOf(world) };
            }

            var origin = AuthoringUtil.CellOf(world);
            var found = new List<Vector2Int>();
            for (var y = origin.y - reach; y <= origin.y + reach; y++)
            {
                for (var x = origin.x - reach; x <= origin.x + reach; x++)
                {
                    var tile = grid.Get(x, y);
                    if (tile != null && tile.Kind == TileKind.Pit)
                    {
                        found.Add(new Vector2Int(x, y));
                    }
                }
            }

            return found.Count > 0 ? found.ToArray() : new[] { origin };
        }
    }
}
