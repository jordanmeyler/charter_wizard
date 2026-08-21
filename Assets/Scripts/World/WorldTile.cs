using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class WorldTile : MonoBehaviour, IRuneSource
    {
        public Vector2Int Coord { get; private set; }
        public TileDef Def { get; private set; }
        public TileKind Kind => Def.Kind;
        public MaterialId Material => Def.Material;
        public TileSubstance Substance => Def.Substance;
        public RuneId Element => Def.Element;
        public IReadOnlyList<RuneId> Emission => Def.Emission;
        public bool IsConjured { get; private set; }
        public RaisedForm RaisedAs { get; private set; }
        public TileDef Foundation { get; private set; }

        public bool IsEmitting => !Def.TearsTapestry && Emission.Count > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 2.4f;
        public float VoiceWeight => Kind == TileKind.Wall ? 0.55f : 1f;
        public RuneSourceKind SourceKind => RuneSourceKind.Tile;

        SpriteRenderer _renderer;
        SpriteRenderer _overlay;
        Collider2D _collider;
        GameObject _linger;

        public void Bind(Vector2Int coord, TileDef def)
        {
            Coord = coord;
            Def = def;
            transform.position = new Vector3(coord.x + 0.5f, coord.y + 0.5f, 0f);

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            ApplyVisual();
            ApplyCollider();
        }

        public bool CanRaiseBarrier =>
            Kind == TileKind.Floor || Kind == TileKind.Bridge;

        public void BecomeBridge()
        {
            BecomeWalkable(MaterialId.Stone);
        }

        public void BecomeWalkable(MaterialId material, bool conjured = false)
        {
            if (conjured)
            {
                RememberFoundation();
            }

            IsConjured = conjured;
            RaisedAs = conjured ? RaisedForm.Span : RaisedForm.None;
            Reshape(new TileDef(TileKind.Bridge, material == MaterialId.None ? MaterialId.Stone : material));
        }

        public void BecomeBarrier(MaterialId material, RaisedForm form = RaisedForm.Wall)
        {
            if (!CanRaiseBarrier)
            {
                return;
            }

            RememberFoundation();
            IsConjured = true;
            RaisedAs = form == RaisedForm.None ? RaisedForm.Wall : form;
            Reshape(new TileDef(TileKind.Wall, material == MaterialId.None ? MaterialId.Stone : material));
        }

        public bool RestoreFoundation()
        {
            if (!IsConjured)
            {
                return false;
            }

            var restored = Foundation;
            if (restored.Kind == 0 && restored.Material == MaterialId.None)
            {
                restored = new TileDef(TileKind.Floor, MaterialId.Stone);
            }

            IsConjured = false;
            RaisedAs = RaisedForm.None;
            Foundation = default;
            ClearLinger();
            Reshape(restored);
            return true;
        }

        void RememberFoundation()
        {
            if (!IsConjured)
            {
                Foundation = Def;
            }
        }

        void Reshape(TileDef def)
        {
            Def = def;
            ApplyVisual();
            RefreshCollider();
            RefreshLinger();
            var grid = GetComponentInParent<WorldGrid>();
            grid?.DressLooks();
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
            try
            {
                switch (Kind)
                {
                    case TileKind.Wall:
                        if (RaisedAs == RaisedForm.Pillar)
                        {
                            _renderer.sprite = SpriteFactory.Column(Material, Coord.x, Coord.y);
                            _renderer.sortingOrder = 5;
                        }
                        else
                        {
                            _renderer.sprite = SpriteFactory.Wall(Material, Coord.x, Coord.y);
                            _renderer.sortingOrder = 3;
                        }

                        break;
                    case TileKind.Pit:
                        _renderer.sprite = SpriteFactory.Pit(Coord.x, Coord.y);
                        _renderer.sortingOrder = 1;
                        break;
                    case TileKind.Bridge:
                        _renderer.sprite = IsConjured && Material != MaterialId.Stone
                            ? SpriteFactory.Floor(Material, Coord.x, Coord.y)
                            : SpriteFactory.Bridge();
                        _renderer.sortingOrder = 1;
                        break;
                    case TileKind.Door:
                        ApplyDoorSprite(open: false);
                        break;
                    default:
                        _renderer.sprite = SpriteFactory.Floor(Material, Coord.x, Coord.y);
                        _renderer.sortingOrder = 0;
                        break;
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Tile sprite failed (" + Kind + " " + Material + "): " + exception);
                _renderer.sprite = SpriteFactory.Square(new Color(0.32f, 0.3f, 0.34f));
                _renderer.sortingOrder = Kind == TileKind.Wall ? 3 : 0;
            }
        }

        void ApplyDoorSprite(bool open)
        {
            _renderer.sprite = SpriteFactory.Door(open);
            _renderer.sortingOrder = 4;
        }

        public void DressNeighborhood(WorldGrid grid)
        {
            if (_renderer == null || grid == null)
            {
                return;
            }

            if (Kind == TileKind.Floor || Kind == TileKind.Bridge)
            {
                var north = grid.Get(Coord.x, Coord.y + 1);
                if (north != null && (north.Kind == TileKind.Wall || north.Kind == TileKind.Door))
                {
                    EnsureOverlay().sprite = SpriteFactory.WallShadow();
                    _overlay.sortingOrder = _renderer.sortingOrder + 1;
                    _overlay.enabled = true;
                    return;
                }

                var mask = 0;
                if (IsPit(grid.Get(Coord.x, Coord.y - 1))) mask |= 1;
                if (IsPit(grid.Get(Coord.x, Coord.y + 1))) mask |= 2;
                if (IsPit(grid.Get(Coord.x - 1, Coord.y))) mask |= 4;
                if (IsPit(grid.Get(Coord.x + 1, Coord.y))) mask |= 8;
                if (mask != 0)
                {
                    EnsureOverlay().sprite = SpriteFactory.PitRim(mask);
                    _overlay.sortingOrder = _renderer.sortingOrder + 1;
                    _overlay.enabled = true;
                    return;
                }
            }

            if (_overlay != null)
            {
                _overlay.enabled = false;
            }
        }

        static bool IsPit(WorldTile tile) => tile != null && tile.Kind == TileKind.Pit;

        SpriteRenderer EnsureOverlay()
        {
            if (_overlay != null)
            {
                return _overlay;
            }

            var child = new GameObject("TileOverlay");
            child.transform.SetParent(transform, false);
            _overlay = child.AddComponent<SpriteRenderer>();
            return _overlay;
        }

        void ApplyCollider()
        {
            RefreshCollider();
        }

        void RefreshCollider()
        {
            var solid = Kind == TileKind.Wall || Kind == TileKind.Door;
            var pit = Kind == TileKind.Pit;
            if (!solid && !pit)
            {
                if (_collider != null)
                {
                    Destroy(_collider);
                    _collider = null;
                }

                return;
            }

            if (_collider == null)
            {
                var box = gameObject.AddComponent<BoxCollider2D>();
                box.size = Vector2.one;
                _collider = box;
            }

            _collider.isTrigger = pit;
            _collider.enabled = true;
        }

        void RefreshLinger()
        {
            ClearLinger();
            if (!IsConjured)
            {
                return;
            }

            var look = ElementLook.For(Element);
            if (!NeedsLinger(look.Family))
            {
                return;
            }

            var offset = RaisedAs == RaisedForm.Pillar
                ? new Vector3(0f, 0.55f, 0f)
                : new Vector3(0f, 0.2f, 0f);
            _linger = ElementFx.Linger(transform, look, 0.85f, offset);
        }

        static bool NeedsLinger(ElementFamily family)
        {
            switch (family)
            {
                case ElementFamily.Fire:
                case ElementFamily.Ice:
                case ElementFamily.Plant:
                case ElementFamily.Lava:
                case ElementFamily.Lightning:
                case ElementFamily.Spark:
                    return true;
                default:
                    return false;
            }
        }

        void ClearLinger()
        {
            if (_linger == null)
            {
                return;
            }

            Destroy(_linger);
            _linger = null;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Kind != TileKind.Pit || !AdeptAvatar.IsAdept(other))
            {
                return;
            }

            var adept = other.GetComponent<AdeptAvatar>();
            if (adept != null && adept.IsAirborne)
            {
                return;
            }

            var director = FindFirstObjectByType<SanctumDirector>();
            director?.FallInPit(other.transform);
        }
    }
}
