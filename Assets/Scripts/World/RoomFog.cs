using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A breath of poison standing in a room. It is not a wall.
    /// Air sent pushes it out.
    /// </summary>
    public sealed class RoomFog : MonoBehaviour, ISpellLock, IRuneSource
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
        readonly List<GameObject> _wisps = new();
        float _pulse;

        public void Bind(
            string displayName,
            string formulaId,
            SpellId[] keys,
            RuneId[] formula,
            IList<Vector2Int> cells,
            string spriteId,
            string resolvedNote)
        {
            DisplayName = displayName;
            FormulaId = formulaId;
            AcceptedKeys = keys ?? System.Array.Empty<SpellId>();
            _formula = formula ?? System.Array.Empty<RuneId>();
            _resolvedNote = resolvedNote;

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
                    host.AddComponent<FogWisp>().Bind(this);
                    _wisps.Add(host);
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

        public string Resolve(SpellId spell)
        {
            Resolved = true;
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

        public void Bind(RoomFog fog)
        {
            _fog = fog;
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
