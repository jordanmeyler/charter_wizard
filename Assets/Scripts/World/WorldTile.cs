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

        public bool IsEmitting => !Def.TearsTapestry && Emission.Count > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 2.4f;
        public float VoiceWeight => Kind == TileKind.Wall ? 0.55f : 1f;
        public RuneSourceKind SourceKind => RuneSourceKind.Tile;

        SpriteRenderer _renderer;
        SpriteRenderer _overlay;
        SpriteRenderer _fx;
        Collider2D _collider;
        int _growth;

        public void Bind(Vector2Int coord, TileDef def)
        {
            Coord = coord;
            Def = def;
            transform.position = new Vector3(coord.x + 0.5f, coord.y + 0.5f, 0f);

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            ApplyVisual();
            ApplyCollider();
        }

        public float Fire { get; private set; }
        public float Wet { get; private set; }
        public float Charge { get; private set; }
        public int Growth => _growth;
        public float Flammability => Def.WorldMaterial.Flammability;
        public float Conductivity => Def.WorldMaterial.Conductivity;
        public bool IsPlantish =>
            Material == MaterialId.Plant || Material == MaterialId.Grove ||
            Material == MaterialId.Moss || Material == MaterialId.Timber;
        public bool CanTakePlant =>
            (Kind == TileKind.Floor || Kind == TileKind.Bridge) &&
            Material != MaterialId.Water && Material != MaterialId.Lava &&
            Material != MaterialId.Void && !IsPlantish;

        public bool CanRaiseBarrier =>
            Kind == TileKind.Floor || Kind == TileKind.Bridge;

        public void BecomeBridge()
        {
            BecomeWalkable(MaterialId.Stone);
        }

        public void BecomeWalkable(MaterialId material)
        {
            Reshape(new TileDef(TileKind.Bridge, material == MaterialId.None ? MaterialId.Stone : material));
        }

        public void BecomeBarrier(MaterialId material)
        {
            if (!CanRaiseBarrier)
            {
                return;
            }

            Reshape(new TileDef(TileKind.Wall, material == MaterialId.None ? MaterialId.Stone : material));
        }

        public void Ignite(float amount)
        {
            if (Kind == TileKind.Pit && Material != MaterialId.Water)
            {
                return;
            }

            Fire = Mathf.Clamp01(Fire + amount);
            if (Fire > 0.05f)
            {
                Wet = Mathf.Max(0f, Wet - amount);
            }

            RefreshFx();
        }

        public void Drench(float amount)
        {
            Wet = Mathf.Clamp01(Wet + amount);
            if (Wet > 0.2f)
            {
                Fire = Mathf.Max(0f, Fire - amount * 1.4f);
            }

            RefreshFx();
        }

        public void Dry(float amount)
        {
            Wet = Mathf.Max(0f, Wet - amount);
            RefreshFx();
        }

        public void ChargeAt(float amount)
        {
            Charge = Mathf.Clamp01(Charge + amount);
            RefreshFx();
        }

        public void Grow(int steps)
        {
            if (!IsPlantish || steps <= 0)
            {
                return;
            }

            _growth = Mathf.Clamp(_growth + steps, 0, 3);
            if (_growth >= 2 && Material != MaterialId.Grove)
            {
                Reshape(new TileDef(Kind == TileKind.Wall ? TileKind.Wall : TileKind.Floor, MaterialId.Grove));
            }

            RefreshFx();
        }

        public void PlantHere()
        {
            if (!CanTakePlant)
            {
                return;
            }

            _growth = 0;
            Reshape(new TileDef(TileKind.Floor, MaterialId.Plant));
            Wet = Mathf.Max(Wet, 0.4f);
            RefreshFx();
        }

        public void BurnDown()
        {
            if (!IsPlantish)
            {
                return;
            }

            Fire = 0.15f;
            _growth = 0;
            Reshape(new TileDef(Kind == TileKind.Wall ? TileKind.Floor : Kind, MaterialId.Ash));
            RefreshFx();
        }

        void Reshape(TileDef def)
        {
            Def = def;
            ApplyVisual();
            RefreshCollider();
            RefreshFx();
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
                        _renderer.sprite = SpriteFactory.Wall(Material, Coord.x, Coord.y);
                        _renderer.sortingOrder = 3;
                        break;
                    case TileKind.Pit:
                        _renderer.sprite = Material == MaterialId.Water
                            ? SpriteFactory.Floor(Material, Coord.x, Coord.y)
                            : SpriteFactory.Pit(Coord.x, Coord.y);
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

        void RefreshFx()
        {
            if (_renderer == null)
            {
                return;
            }

            if (Fire < 0.08f && Wet < 0.18f && Charge < 0.18f && _growth < 1)
            {
                if (_fx != null)
                {
                    _fx.enabled = false;
                }

                return;
            }

            var fx = EnsureFx();
            fx.enabled = true;
            fx.sortingOrder = _renderer.sortingOrder + 2;
            if (Fire > 0.12f)
            {
                fx.sprite = SpriteFactory.Named("tile-fire");
                fx.color = new Color(1f, 0.55f, 0.12f, 0.35f + Fire * 0.5f);
            }
            else if (Charge > 0.18f)
            {
                fx.sprite = SpriteFactory.Named("tile-charge");
                fx.color = new Color(0.75f, 0.9f, 1f, 0.35f + Charge * 0.45f);
            }
            else if (Wet > 0.18f)
            {
                fx.sprite = SpriteFactory.Named("tile-wet");
                fx.color = new Color(0.35f, 0.65f, 1f, 0.22f + Wet * 0.35f);
            }
            else
            {
                fx.sprite = SpriteFactory.Named("tile-grow");
                fx.color = new Color(0.35f, 0.72f, 0.28f, 0.2f + _growth * 0.12f);
            }
        }

        SpriteRenderer EnsureFx()
        {
            if (_fx != null)
            {
                return _fx;
            }

            var child = new GameObject("TileFx");
            child.transform.SetParent(transform, false);
            _fx = child.AddComponent<SpriteRenderer>();
            return _fx;
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
