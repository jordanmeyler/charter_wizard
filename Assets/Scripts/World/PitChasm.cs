using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class PitChasm : MonoBehaviour, ISpellLock, IRuneSource
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

        public void Collect(System.Collections.Generic.List<RuneId> buffer)
        {
        }

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
                    : spell == SpellId.Wall || spell == SpellId.IceWall || spell == SpellId.StonePillar || spell == SpellId.EarthPillar
                        ? "A standing body fills the gap, or bars the floor."
                        : WorldWork.IsPillar(spell)
                            ? "A column settles into the hollow and holds."
                            : spell == SpellId.Pit || spell == SpellId.RaisedEarth
                                ? "Earth answers away from you and leaves a hollow filled."
                                : spell == SpellId.Bridge
                                    ? "A body of rest given breath spans the drop."
                                    : "Earth takes spirit and flies. Hurled stone piles into a bridge.";
        }
    }
}
