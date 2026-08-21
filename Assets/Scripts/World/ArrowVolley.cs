using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Shots down a lane. A stood body — wall or pillar — stops them.
    /// Then the adept walks around the cover to the stone.
    /// </summary>
    public sealed class ArrowVolley : MonoBehaviour, ISpellLock, IRuneSource
    {
        public string DisplayName { get; private set; }
        public string FormulaId { get; private set; }
        public SpellId[] AcceptedKeys { get; private set; }
        public bool Resolved { get; private set; }
        public Vector3 WorldPosition => transform.position;

        public bool IsEmitting => !Resolved && _formula != null && _formula.Length > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 3.4f;
        public float VoiceWeight => 1.8f;
        public RuneSourceKind SourceKind => RuneSourceKind.Creature;

        WorldGrid _grid;
        Vector2Int[] _cover;
        HashSet<Vector2Int> _kill;
        RuneId[] _formula;
        string _resolvedNote;
        float _beat;
        SpriteRenderer _renderer;
        Vector2 _heading = Vector2.down;

        public void Bind(
            string displayName,
            string formulaId,
            SpellId[] keys,
            RuneId[] formula,
            WorldGrid grid,
            IList<Vector2Int> cover,
            IList<Vector2Int> kill,
            string spriteId,
            string resolvedNote,
            Vector3 heading)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            AcceptedKeys = keys ?? System.Array.Empty<SpellId>();
            _formula = formula ?? System.Array.Empty<RuneId>();
            _grid = grid;
            _resolvedNote = resolvedNote;
            _heading = ((Vector2)heading).sqrMagnitude > 0.01f ? (Vector2)heading : Vector2.down;
            _cover = cover != null ? new Vector2Int[cover.Count] : System.Array.Empty<Vector2Int>();
            if (cover != null)
            {
                for (var i = 0; i < cover.Count; i++)
                {
                    _cover[i] = cover[i];
                }
            }

            _kill = new HashSet<Vector2Int>();
            if (kill != null)
            {
                for (var i = 0; i < kill.Count; i++)
                {
                    _kill.Add(kill[i]);
                }
            }

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = SpriteFactory.Named(string.IsNullOrEmpty(spriteId) ? "arrow-rack" : spriteId);
            _renderer.sortingOrder = 8;
            FixtureGlow.Attach(transform, new Color(0.85f, 0.55f, 0.2f, 0.55f), 1.6f, 0.14f);
            WorldLabel.Attach(transform, displayName, new Vector3(0f, 0.95f, 0f),
                new Color(0.95f, 0.7f, 0.35f));
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
            return "shots that will not wait — rest has to stand";
        }

        public string Resolve(SpellId spell)
        {
            Resolved = true;
            if (_renderer != null)
            {
                _renderer.color = new Color(1f, 1f, 1f, 0.25f);
            }

            return string.IsNullOrEmpty(_resolvedNote)
                ? "Rest stands. The shots break on the body you raised."
                : _resolvedNote;
        }

        void Update()
        {
            if (Resolved || AdeptAvatar.WorldHeld)
            {
                return;
            }

            if (CoverStands())
            {
                FindFirstObjectByType<SanctumDirector>()?.TurnLock(this);
                return;
            }

            _beat += Time.deltaTime;
            if (_renderer != null)
            {
                _renderer.color = Color.Lerp(Color.white, new Color(1f, 0.45f, 0.15f),
                    0.5f + Mathf.Sin(_beat * 8f) * 0.5f);
            }

            if (_beat < 0.55f)
            {
                return;
            }

            _beat = 0f;
            var origin = transform.position + (Vector3)(_heading.normalized * 0.4f);
            WorldProjectile.Spawn(origin, _heading, ProjectileKind.Arrow, _grid, 8.2f);
        }

        bool CoverStands()
        {
            if (_grid == null)
            {
                return false;
            }

            for (var i = 0; i < _cover.Length; i++)
            {
                var tile = _grid.Get(_cover[i]);
                if (tile != null && tile.Kind == TileKind.Wall)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
