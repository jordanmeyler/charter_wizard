using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class WorldTile : MonoBehaviour, IRuneSource
    {
        public Vector2Int Coord { get; private set; }
        public TileDef Def { get; private set; }
        public TileKind Kind => Def.Kind;
        public TileSubstance Substance => Def.Substance;
        public RuneId Element => Def.Element;
        public IReadOnlyList<RuneId> Emission => Def.Emission;

        public bool IsEmitting => !Def.TearsTapestry && Emission.Count > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 2.4f;
        public float VoiceWeight => Kind == TileKind.Wall ? 0.55f : 1f;
        public RuneSourceKind SourceKind => RuneSourceKind.Tile;

        SpriteRenderer _renderer;
        Collider2D _collider;

        public void Bind(Vector2Int coord, TileDef def)
        {
            Coord = coord;
            Def = def;
            transform.position = new Vector3(coord.x + 0.5f, coord.y + 0.5f, 0f);

            _renderer = gameObject.GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
            ApplyVisual();
            ApplyCollider();
        }

        public void BecomeBridge()
        {
            Def = new TileDef(TileKind.Bridge, TileSubstance.Stone);
            ApplyVisual();
            if (_collider != null)
            {
                Destroy(_collider);
                _collider = null;
            }
        }

        public void OpenDoor()
        {
            if (Kind != TileKind.Door)
            {
                return;
            }

            ApplyDoorSprite(open: true);
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        public void Collect(List<RuneId> buffer)
        {
            if (!IsEmitting)
            {
                return;
            }

            var emission = Emission;
            for (var i = 0; i < emission.Count; i++)
            {
                buffer.Add(emission[i]);
            }
        }

        void ApplyVisual()
        {
            switch (Kind)
            {
                case TileKind.Wall:
                    _renderer.sprite = SpriteFactory.Wall(Substance);
                    _renderer.sortingOrder = 3;
                    break;
                case TileKind.Pit:
                    _renderer.sprite = SpriteFactory.Pit();
                    _renderer.sortingOrder = 1;
                    break;
                case TileKind.Bridge:
                    _renderer.sprite = SpriteFactory.Bridge();
                    _renderer.sortingOrder = 1;
                    break;
                case TileKind.Door:
                    ApplyDoorSprite(open: false);
                    break;
                default:
                    _renderer.sprite = SpriteFactory.Floor(Substance);
                    _renderer.sortingOrder = 0;
                    break;
            }
        }

        void ApplyDoorSprite(bool open)
        {
            _renderer.sprite = SpriteFactory.Door(open);
            _renderer.sortingOrder = 4;
        }

        void ApplyCollider()
        {
            if (_collider != null)
            {
                return;
            }

            if (Kind == TileKind.Floor || Kind == TileKind.Bridge)
            {
                return;
            }

            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
            box.isTrigger = Kind == TileKind.Pit;
            _collider = box;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Kind != TileKind.Pit || !AdeptAvatar.IsAdept(other))
            {
                return;
            }

            var director = FindFirstObjectByType<SanctumDirector>();
            director?.FallInPit(other.transform);
        }
    }
}
