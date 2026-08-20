using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class PitChasm : MonoBehaviour, ISpellLock
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        WorldGrid _grid;
        Vector2Int[] _pits;

        public void Bind(string displayName, string formulaId, SpellId[] keys, WorldGrid grid, IList<Vector2Int> pits)
        {
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

        public string FormulaText() => "missing Earth";

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            if (_grid != null && _pits != null)
            {
                foreach (var coord in _pits)
                {
                    _grid.Get(coord)?.BecomeBridge();
                }
            }

            return spell == SpellId.StoneWall
                ? "Stone wall settles into the gap and holds."
                : "Earth takes spirit and flies. Hurled stone piles into a bridge.";
        }
    }
}
