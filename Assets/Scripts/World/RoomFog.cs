using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A breath of poison standing in a room. It is not a wall.
    /// Air sent pushes it out.
    /// </summary>
    public sealed class RoomFog : MonoBehaviour, ISpellLock, IRuneSource, ISpellVolume
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        public bool IsEmitting => !Resolved && _formula != null && _formula.Length > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 4.8f;
        public float VoiceWeight => 1.6f;
        public RuneSourceKind SourceKind => RuneSourceKind.Creature;

        RuneId[] _formula;
        string _resolvedNote;
        Vector3 _retreat;
        Vector2Int[] _cells;
        WorldGrid _grid;
        readonly List<GameObject> _wisps = new();
        float _pulse;

        public void Bind(
            string displayName,
            string formulaId,
            SpellId[] keys,
            RuneId[] formula,
            IList<Vector2Int> cells,
            string spriteId,
            string resolvedNote,
            WorldGrid grid = null)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            AcceptedKeys = keys ?? System.Array.Empty<SpellId>();
            _formula = formula ?? System.Array.Empty<RuneId>();
            _resolvedNote = resolvedNote;
            _grid = grid;
            _cells = cells != null ? new Vector2Int[cells.Count] : System.Array.Empty<Vector2Int>();
            if (cells != null)
            {
                for (var i = 0; i < cells.Count; i++)
                {
                    _cells[i] = cells[i];
                }
            }

            var sprite = SpriteFactory.Named(string.IsNullOrEmpty(spriteId) ? "poison-fog" : spriteId);
            var north = int.MinValue;
            var midX = 0;
            if (cells != null)
            {
                for (var i = 0; i < cells.Count; i++)
                {
                    if (cells[i].y >= north)
                    {
                        north = cells[i].y;
                        midX = cells[i].x;
                    }

                    var host = new GameObject("FogWisp");
                    host.transform.SetParent(transform, false);
                    host.transform.position = WorldGrid.Center(cells[i].x, cells[i].y);
                    var view = host.AddComponent<SpriteRenderer>();
                    view.sprite = sprite;
                    view.sortingOrder = 7;
                    view.color = new Color(0.45f, 0.95f, 0.28f, 0.72f);
                    var hit = host.AddComponent<BoxCollider2D>();
                    hit.isTrigger = true;
                    hit.size = Vector2.one * 0.92f;
                    host.AddComponent<FogWisp>().Bind(this, cells[i]);
                    _wisps.Add(host);
                    StampCell(cells[i]);
                }
            }

            _retreat = north > int.MinValue
                ? WorldGrid.Center(midX, north + 1)
                : transform.position;

            var mark = gameObject.AddComponent<SpriteRenderer>();
            mark.sprite = sprite;
            mark.sortingOrder = 8;
            FixtureGlow.Attach(transform, new Color(0.35f, 0.9f, 0.2f, 0.55f), 2.2f, 0.2f);
            WorldLabel.Attach(transform, displayName, new Vector3(0f, 0.95f, 0f),
                new Color(0.55f, 0.95f, 0.35f));
        }

        public void Collect(List<RuneId> buffer)
        {
            if (!IsEmitting)
            {
                return;
            }

            for (var i = 0; i < _formula.Length; i++)
            {
                buffer.Add(_formula[i]);
            }
        }

        public string FormulaText()
        {
            if (_formula == null || _formula.Length == 0)
            {
                return "foul breath";
            }

            var parts = new string[_formula.Length];
            for (var i = 0; i < _formula.Length; i++)
            {
                parts[i] = $"{RuneCatalog.GlyphOf(_formula[i])} {RuneCatalog.NameOf(_formula[i])}";
            }

            return string.Join(" · ", parts);
        }

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

        public int BlowAlong(Vector3 from, Vector3 to, float width, SpellId spell = SpellId.Gust)
        {
            if (Resolved)
            {
                return 0;
            }

            var cleared = 0;
            for (var i = _wisps.Count - 1; i >= 0; i--)
            {
                var wisp = _wisps[i];
                if (wisp == null)
                {
                    _wisps.RemoveAt(i);
                    continue;
                }

                if (CellVolume.SegmentDistance(from, to, wisp.transform.position) > width + CellVolume.TileRadius)
                {
                    continue;
                }

                var mark = wisp.GetComponent<FogWisp>();
                if (mark != null)
                {
                    VentCell(mark.Cell, spell);
                }

                Destroy(wisp);
                _wisps.RemoveAt(i);
                cleared++;
            }

            return cleared;
        }

        void StampCell(Vector2Int cell)
        {
            var tile = _grid != null ? _grid.Get(cell) : null;
            tile?.Foul(1f);
        }

        void VentCell(Vector2Int cell, SpellId spell)
        {
            var tile = _grid != null ? _grid.Get(cell) : null;
            tile?.Vent(spell);
        }

        void VentAll(SpellId spell)
        {
            if (_cells == null)
            {
                return;
            }

            for (var i = 0; i < _cells.Length; i++)
            {
                VentCell(_cells[i], spell);
            }
        }

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            VentAll(spell == SpellId.None ? SpellId.Gust : spell);
            for (var i = 0; i < _wisps.Count; i++)
            {
                if (_wisps[i] != null)
                {
                    Destroy(_wisps[i]);
                }
            }

            _wisps.Clear();
            Destroy(gameObject, 0.35f);
            return string.IsNullOrEmpty(_resolvedNote)
                ? "Breath sent. The foul air forgets the room."
                : _resolvedNote;
        }

        public void Choke(Transform player)
        {
            if (Resolved || player == null)
            {
                return;
            }

            var host = StatusHost.On(player);
            var ward = host != null ? host.FendingName(Essence.Poison) : string.Empty;
            if (!string.IsNullOrEmpty(ward))
            {
                FindFirstObjectByType<SanctumDirector>()?.Log(
                    $"A {ward} turns the foul breath. The mist does not take you.");
                return;
            }

            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.position = _retreat;
            }

            player.position = _retreat;
            FindFirstObjectByType<SanctumDirector>()?.Log("The breath is foul. Send air through it.");
        }

        void Update()
        {
            if (Resolved)
            {
                return;
            }

            _pulse += Time.deltaTime;
            var wave = 0.62f + Mathf.Sin(_pulse * 1.7f) * 0.12f;
            for (var i = 0; i < _wisps.Count; i++)
            {
                var wisp = _wisps[i];
                if (wisp == null)
                {
                    continue;
                }

                var view = wisp.GetComponent<SpriteRenderer>();
                if (view != null)
                {
                    view.color = new Color(0.4f, 0.92f, 0.22f, wave);
                }
            }
        }
    }

    sealed class FogWisp : MonoBehaviour
    {
        RoomFog _fog;
        public Vector2Int Cell { get; private set; }

        public void Bind(RoomFog fog, Vector2Int cell)
        {
            _fog = fog;
            Cell = cell;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_fog == null || _fog.Resolved || !AdeptAvatar.IsAdept(other))
            {
                return;
            }

            _fog.Choke(other.transform);
        }
    }
}
