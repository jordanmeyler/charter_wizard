using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A terrain lock: ice across a door, a rope on a portcullis,
    /// poison in a chamber. The right spell clears the cells.
    /// </summary>
    public sealed class BarrierLock : MonoBehaviour, ISpellLock, IRuneSource, ISpellVolume
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        public bool IsEmitting => !Resolved && _formula != null && _formula.Length > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 3.2f;
        public float VoiceWeight => 2f;
        public RuneSourceKind SourceKind => RuneSourceKind.Creature;

        WorldGrid _grid;
        Vector2Int[] _cells;
        RuneId[] _formula;
        MaterialId _matter;
        string _grant;
        string _clearMaterial;
        string _resolvedNote;
        SpriteRenderer _renderer;

        public void Bind(
            string displayName,
            string formulaId,
            SpellId[] keys,
            RuneId[] formula,
            WorldGrid grid,
            IList<Vector2Int> cells,
            string grant,
            string clearMaterial,
            string spriteId,
            string resolvedNote)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            AcceptedKeys = keys ?? System.Array.Empty<SpellId>();
            _formula = formula ?? System.Array.Empty<RuneId>();
            _grid = grid;
            _grant = grant;
            _clearMaterial = clearMaterial;
            _resolvedNote = resolvedNote;
            _matter = MatterLaw.MatterOf(formula);
            _cells = cells != null ? new Vector2Int[cells.Count] : System.Array.Empty<Vector2Int>();
            if (cells != null)
            {
                for (var i = 0; i < cells.Count; i++)
                {
                    _cells[i] = cells[i];
                }
            }

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            var art = string.IsNullOrEmpty(spriteId) ? "torch" : spriteId;
            _renderer.sprite = SpriteFactory.Named(art);
            _renderer.sortingOrder = 6;
            SpriteAnim.On(gameObject, _renderer).Play(art, art.Contains("flame") || art.Contains("poison") || art.Contains("charge") ? 8f : 4f);
            var tint = _formula.Length > 0 ? RunePalette.Of(_formula[0]) : new Color(0.85f, 0.7f, 0.4f);
            FixtureGlow.Attach(transform, new Color(tint.r, tint.g, tint.b, 0.55f), 1.5f, 0.12f);
            WorldLabel.Attach(transform, displayName, new Vector3(0f, 0.9f, 0f), tint);
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

        /// <summary>
        /// An ice cage yields to any heat that can melt ice, not only
        /// the spells named on the lock.
        /// </summary>
        public bool YieldsTo(SpellId spell) =>
            MatterLaw.Melts(spell, _matter);

        public string FormulaText()
        {
            if (_formula == null || _formula.Length == 0)
            {
                return "a lock of the room";
            }

            var parts = new string[_formula.Length];
            for (var i = 0; i < _formula.Length; i++)
            {
                parts[i] = $"{RuneCatalog.GlyphOf(_formula[i])} {RuneCatalog.NameOf(_formula[i])}";
            }

            return string.Join(" · ", parts);
        }

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            ClearCells();
            LockReward.Grant(transform.position + new Vector3(0f, 0.2f, 0f), _grant);
            if (_renderer != null)
            {
                _renderer.color = new Color(1f, 1f, 1f, 0.2f);
            }

            Destroy(gameObject, 0.4f);
            if (!string.IsNullOrEmpty(_resolvedNote))
            {
                return _resolvedNote;
            }

            return $"{DisplayName} yields.";
        }

        void ClearCells()
        {
            if (_grid == null || _cells == null)
            {
                return;
            }

            var material = MapFile.ParseMaterial(_clearMaterial, MaterialId.Stone);
            for (var i = 0; i < _cells.Length; i++)
            {
                var tile = _grid.Get(_cells[i]);
                if (tile == null)
                {
                    continue;
                }

                if (tile.Kind == TileKind.Pit)
                {
                    tile.BecomeWalkable(material);
                    continue;
                }

                if (tile.Kind == TileKind.Wall || tile.Kind == TileKind.Door)
                {
                    tile.BecomeWalkable(material);
                }
            }
        }
    }
}
