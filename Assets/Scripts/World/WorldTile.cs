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
        public bool PassageOpen { get; private set; }
        public DoorFace DoorFace { get; private set; }
        string _coverId;
        Sprite _coverLook;
        float _coverAlpha = 1f;

        /// <summary>
        /// Closed masonry and shut doors stop bodies and shots.
        /// An opened door is a hole in the wall.
        /// </summary>
        public bool BlocksTravel =>
            Kind == TileKind.Wall || (Kind == TileKind.Door && !PassageOpen) || _detailBlocks;

        public bool IsEmitting => !Def.TearsTapestry && Emission.Count > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 2.4f;
        public float VoiceWeight => Kind == TileKind.Wall ? 0.55f : 1f;
        public RuneSourceKind SourceKind => RuneSourceKind.Tile;

        SpriteRenderer _renderer;
        SpriteRenderer _underlay;
        SpriteRenderer _overlay;
        SpriteRenderer _cover;
        SpriteRenderer _detail;
        SpriteRenderer _fx;
        Sprite _authoredLook;
        Sprite _underlayLook;
        MaterialId _underlayMaterial;
        Sprite _detailLook;
        MaterialId _detailMaterial;
        bool _detailBlocks;
        Collider2D _collider;
        int _growth;
        GameObject _linger;
        bool _hasFoundation;
        int _animFrame = -1;
        MaterialId _telegraph = MaterialId.None;
        int _telegraphCount;

        public void Bind(Vector2Int coord, TileDef def)
        {
            Coord = coord;
            Def = def;
            transform.position = new Vector3(coord.x + 0.5f, coord.y + 0.5f, 0f);

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            ApplyVisual();
            ApplyCover();
            ApplyCollider();
        }

        public Sprite AuthoredLook => _authoredLook;

        public void AuthorLook(Sprite sprite)
        {
            _authoredLook = sprite;
            if (_renderer != null)
            {
                ApplyVisual();
            }
        }

        /// <summary>
        /// Floor under a wall or door. Pack wall tiles leave the cobble
        /// transparent in the same cell; Play used to show the void there.
        /// </summary>
        public void AuthorUnderlay(Sprite sprite, MaterialId floor = MaterialId.Stone)
        {
            _underlayLook = sprite;
            _underlayMaterial = floor == MaterialId.None ? MaterialId.Stone : floor;
            if (_renderer != null)
            {
                ApplyUnderlay();
            }
        }

        public bool HasWaterCover =>
            Material == MaterialId.Water
            || string.Equals(_coverId, "water", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-water", System.StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// A Decor-layer stamp. Sprite and material sit on top of the
        /// walk cell, so a plant can burn off and leave the stone.
        /// </summary>
        public void AuthorDetail(Sprite sprite, MaterialId material, bool blocks = false)
        {
            _detailLook = sprite;
            _detailMaterial = material;
            _detailBlocks = blocks;
            if (_renderer != null)
            {
                ApplyDetail();
                RefreshCollider();
            }
        }

        public void AuthorBlocks(bool blocks)
        {
            _detailBlocks = blocks;
            if (_renderer != null)
            {
                RefreshCollider();
            }
        }

        public void MarkDoor(DoorFace face)
        {
            DoorFace = face;
            if (Kind == TileKind.Door)
            {
                ApplyDoorSprite(PassageOpen);
            }
        }

        public void PaintCover(TileCover cover)
        {
            PaintCover(cover == TileCover.None ? null : cover.ToString().ToLowerInvariant());
        }

        public void PaintCover(string id)
        {
            _coverId = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
            if (string.Equals(_coverId, "water", System.StringComparison.OrdinalIgnoreCase))
            {
                _coverAlpha = Mathf.Min(_coverAlpha, 0.62f);
            }

            ApplyCover();
        }

        public void AuthorCoverLook(Sprite sprite, float alpha = 1f)
        {
            _coverLook = sprite;
            _coverAlpha = Mathf.Clamp01(alpha <= 0f ? 1f : alpha);
            ApplyCover();
        }

        public float Fire { get; private set; }
        public float Wet { get; private set; }
        public float Charge { get; private set; }
        public float Fog { get; private set; }
        public float Miasma { get; private set; }
        public float Oil { get; private set; }
        public bool Kindled { get; private set; }
        /// <summary>
        /// Fire a spell or NPC working started. Authored torches, kindled
        /// halls, and painted cover stay still until work finds them.
        /// </summary>
        public bool LiveFire { get; private set; }
        public int Growth => _growth;
        public bool IsBurning => Fire > 0.35f;
        public bool HasFog => Fog > 0.2f;
        public bool HasMiasma => Miasma > 0.2f;
        public bool HasOil => Oil > 0.2f || Material == MaterialId.Oil;
        public bool IsPoisonWater =>
            Material == MaterialId.Acid || (Wet > 0.3f && Miasma > 0.15f);
        public float Flammability =>
            HasWaterCover
                ? -1.6f
                : Def.WorldMaterial.Flammability + DetailFlammability + (HasOil ? 1.6f : 0f);
        public float Conductivity => Def.WorldMaterial.Conductivity;
        public bool IsPlantish => IsPlantMaterial(Material);
        public bool HasPlantishDetail => IsPlantMaterial(_detailMaterial);
        public bool HasDetail =>
            _detailLook != null || _detailMaterial != MaterialId.None;
        float DetailFlammability =>
            _detailMaterial == MaterialId.None
                ? 0f
                : MaterialCatalog.Of(_detailMaterial).Flammability;
        public bool CanTakePlant =>
            (Kind == TileKind.Floor || Kind == TileKind.Bridge) &&
            Material != MaterialId.Water && Material != MaterialId.Lava &&
            Material != MaterialId.Void && !IsPlantish;

        /// <summary>
        /// Yield holding a vessel with no floor under it. It drowns.
        /// Ice is how that water is asked to stand.
        /// </summary>
        public bool IsDeepWater =>
            Material == MaterialId.Water &&
            (Kind == TileKind.Pit || Kind == TileKind.Floor);

        public bool IsSafeStand =>
            (Kind == TileKind.Floor || Kind == TileKind.Bridge) &&
            !IsDeepWater &&
            Material != MaterialId.Lava;

        public bool CanRaiseBarrier =>
            Kind == TileKind.Floor || Kind == TileKind.Bridge;

        MaterialId ShownMaterial => _telegraph != MaterialId.None ? _telegraph : Material;

        public void BeginTelegraph(MaterialId material)
        {
            if (material == MaterialId.None)
            {
                return;
            }

            _telegraphCount++;
            _telegraph = material;
            if (material == MaterialId.Hearth || material == MaterialId.Lava || material == MaterialId.Ember)
            {
                Ignite(0.65f);
            }

            ApplyVisual();
            ApplyCover();
        }

        public void EndTelegraph()
        {
            if (_telegraphCount <= 0)
            {
                return;
            }

            _telegraphCount--;
            if (_telegraphCount > 0)
            {
                return;
            }

            _telegraph = MaterialId.None;
            ApplyVisual();
            ApplyCover();
        }

        public void BecomeBridge()
        {
            BecomeWalkable(MaterialId.Stone);
        }

        public void BecomeWater()
        {
            IsConjured = false;
            RaisedAs = RaisedForm.None;
            Reshape(new TileDef(TileKind.Pit, MaterialId.Water));
            Drench(1f);
        }

        /// <summary>
        /// A watery swamp: yield meeting rest on a walkable floor.
        /// Deep water stays water. Pits stay pits until filled.
        /// </summary>
        public bool SlickMud()
        {
            Drench(1f);
            if (Kind != TileKind.Floor && Kind != TileKind.Bridge)
            {
                return false;
            }

            if (IsDeepWater || Material == MaterialId.Lava || Material == MaterialId.Void)
            {
                return false;
            }

            if (Material == MaterialId.Mud)
            {
                return true;
            }

            Reshape(new TileDef(Kind, MaterialId.Mud));
            return true;
        }

        /// <summary>
        /// Loose rest, thrown. Ground-fire goes out. Walkable tiles
        /// become dirt so Earth speaks here.
        /// </summary>
        public bool LayDirt()
        {
            var smothered = SmotherGroundFire();
            if (Kind != TileKind.Floor && Kind != TileKind.Bridge)
            {
                return smothered;
            }

            if (IsDeepWater || Material == MaterialId.Lava || Material == MaterialId.Void)
            {
                return smothered;
            }

            if (Material == MaterialId.Dirt)
            {
                return true;
            }

            Reshape(new TileDef(Kind, MaterialId.Dirt));
            return true;
        }

        public bool SmotherGroundFire()
        {
            if (Fire <= 0.01f && !Kindled)
            {
                return false;
            }

            Fire = 0f;
            Kindled = false;
            LiveFire = false;
            RefreshFx();
            return true;
        }

        public bool FreezeSolid()
        {
            if (!IsDeepWater)
            {
                return false;
            }

            BecomeWalkable(MaterialId.Ice, conjured: true);
            Wet = Mathf.Min(Wet, 0.15f);
            RefreshFx();
            return true;
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

            var restored = _hasFoundation
                ? Foundation
                : new TileDef(TileKind.Floor, MaterialId.Stone);

            IsConjured = false;
            RaisedAs = RaisedForm.None;
            Foundation = default;
            _hasFoundation = false;
            ClearLinger();
            Reshape(restored);
            return true;
        }

        public bool Transmute(MaterialId material)
        {
            if (!IsConjured)
            {
                return false;
            }

            Reshape(new TileDef(Kind, material == MaterialId.None ? MaterialId.Stone : material));
            return true;
        }

        void RememberFoundation()
        {
            if (_hasFoundation)
            {
                return;
            }

            Foundation = Def;
            _hasFoundation = true;
        }

        public void Ignite(float amount, bool live = true)
        {
            if (Kind == TileKind.Pit && Material != MaterialId.Water)
            {
                return;
            }

            if (amount > 0f && (HasWaterCover || Wet > 0.35f))
            {
                Fire = 0f;
                Kindled = false;
                LiveFire = false;
                RefreshFx();
                return;
            }

            var boost = HasOil ? 1.55f : 1f;
            Fire = Mathf.Clamp01(Fire + amount * boost);
            if (amount > 0f && live && Fire > 0.05f)
            {
                LiveFire = true;
            }

            if (Fire <= 0.01f)
            {
                LiveFire = false;
            }

            if (Fire > 0.05f)
            {
                Wet = Mathf.Max(0f, Wet - amount);
            }

            if (HasOil && amount > 0f)
            {
                Fire = Mathf.Clamp01(Fire + 0.35f);
            }

            RefreshFx();
        }

        public bool SlickOil(float amount = 1f)
        {
            if (Kind == TileKind.Wall || Kind == TileKind.Door || Material == MaterialId.Void)
            {
                return false;
            }

            Oil = Mathf.Clamp01(Oil + amount);
            if (Kind == TileKind.Floor || Kind == TileKind.Bridge)
            {
                if (IsBurning)
                {
                    Ignite(0.7f);
                }
            }

            RefreshFx();
            return true;
        }

        /// <summary>
        /// A painted hall-fire. It stays hungry until yield is thrown.
        /// </summary>
        public void Kindle(float amount = 0.95f)
        {
            Kindled = true;
            Ignite(amount, live: false);
        }

        public void KeepKindled()
        {
            if (!Kindled || Wet > 0.15f)
            {
                return;
            }

            if (Fire < 0.85f)
            {
                Ignite(0.85f - Fire, live: false);
            }
        }

        public void Drench(float amount)
        {
            Wet = Mathf.Clamp01(Wet + amount);
            if (Wet > 0.2f)
            {
                Fire = Mathf.Max(0f, Fire - amount * 1.4f);
                if (Fire <= 0.05f)
                {
                    Kindled = false;
                    LiveFire = false;
                }
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

        public void Cloak(float amount)
        {
            if (Kind == TileKind.Wall || Kind == TileKind.Door)
            {
                return;
            }

            Fog = Mathf.Clamp01(Fog + amount);
            RefreshFx();
        }

        public void Foul(float amount)
        {
            if (Kind == TileKind.Wall || Kind == TileKind.Door)
            {
                return;
            }

            Miasma = Mathf.Clamp01(Miasma + amount);
            RefreshFx();
        }

        /// <summary>
        /// Breath, hunger, or light takes what hangs on this cell.
        /// Air also dries yield and scours an acid slick.
        /// </summary>
        public bool Vent(SpellId spell)
        {
            var changed = false;
            if (Fog > 0.05f && WorldWork.ClearsVeil(spell, VeilKind.Fog))
            {
                Fog = 0f;
                changed = true;
            }

            if (Miasma > 0.05f && WorldWork.ClearsVeil(spell, VeilKind.Poison))
            {
                Miasma = 0f;
                changed = true;
            }

            if (WorldWork.IsAirWork(spell) && Wet > 0.05f)
            {
                Dry(0.55f);
                changed = true;
            }

            if ((WorldWork.IsAirWork(spell) || WorldWork.IsFireWork(spell))
                && Material == MaterialId.Acid
                && (Kind == TileKind.Floor || Kind == TileKind.Bridge))
            {
                Reshape(new TileDef(Kind, MaterialId.Scoured));
                changed = true;
            }

            if (changed)
            {
                RefreshFx();
            }

            return changed;
        }

        /// <summary>
        /// Heat finds this cell. Ice always yields to fire. Witchfire
        /// takes glacier and glass. Melt bores stone and steel, even a
        /// room wall at the edge of the map — the cell opens. A stood
        /// body falls back to what was under it. Obsidian will not hear it.
        /// </summary>
        public bool MeltWith(SpellId spell)
        {
            if (Kind == TileKind.Door || MatterLaw.ResistsMagic(Material))
            {
                return false;
            }

            if (MatterLaw.IsPlasmaWork(spell) && MatterLaw.IsAnnihilable(Material))
            {
                return Annihilate();
            }

            if (!MatterLaw.Melts(spell, Material))
            {
                return false;
            }

            if (MatterLaw.IsMeltWork(spell)
                && MatterLaw.IsBoreable(Material)
                && Kind != TileKind.Wall
                && !IsConjured
                && !MatterLaw.CanMelt(Material, MatterLaw.HeatOf(spell)))
            {
                return false;
            }

            if (IsConjured)
            {
                var ice = WorldWork.IsIceBody(Material);
                var restored = RestoreFoundation();
                if (restored && ice)
                {
                    LeaveMeltWater();
                }

                return restored;
            }

            if (WorldWork.IsIceBody(Material))
            {
                RevealFloorUnderIce();
                LeaveMeltWater();
                return true;
            }

            var leftover = MatterLaw.MeltsTo(Material);
            if (leftover == MaterialId.None)
            {
                leftover = MaterialId.Damp;
            }

            Wet = Mathf.Max(Wet, 0.55f);
            if (Fire > 0.05f)
            {
                Fire = Mathf.Max(0f, Fire - 0.15f);
            }

            var kind = Kind == TileKind.Wall ? TileKind.Floor : Kind;
            Reshape(new TileDef(kind, leftover));
            return true;
        }

        void RevealFloorUnderIce()
        {
            var floor = _underlayMaterial != MaterialId.None ? _underlayMaterial : MaterialId.Stone;
            if (WorldWork.IsIceBody(floor))
            {
                floor = MaterialId.Stone;
            }

            var look = _underlayLook;
            var kind = Kind == TileKind.Wall || Kind == TileKind.Door ? TileKind.Floor : Kind;
            if (kind != TileKind.Floor && kind != TileKind.Bridge)
            {
                kind = TileKind.Floor;
            }

            Reshape(new TileDef(kind, floor));
            if (look != null)
            {
                AuthorLook(look);
            }
        }

        void LeaveMeltWater()
        {
            PaintCover(TileCover.Water);
            Drench(1f);
            SmotherGroundFire();
        }

        public bool Annihilate()
        {
            if (MatterLaw.ResistsMagic(Material) || Material == MaterialId.Void)
            {
                return false;
            }

            Oil = 0f;
            Fire = 0f;
            if (IsConjured)
            {
                return RestoreFoundation();
            }

            if (Kind == TileKind.Wall || Kind == TileKind.Door)
            {
                Reshape(new TileDef(TileKind.Floor, MaterialId.Scoured));
                return true;
            }

            if (Kind == TileKind.Floor || Kind == TileKind.Bridge)
            {
                Reshape(new TileDef(Kind, MaterialId.Scoured));
                return true;
            }

            return false;
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
            var detailBurned = HasPlantishDetail;
            if (detailBurned)
            {
                LeaveAshPile();
            }

            if (IsPlantish)
            {
                Fire = 0.15f;
                _growth = 0;
                Reshape(new TileDef(Kind == TileKind.Wall ? TileKind.Floor : Kind, MaterialId.Ash));
                RefreshFx();
                return;
            }

            if (detailBurned)
            {
                // Coals on the pile so hunger can still run onto a
                // plant or timber floor beside (or under) the furniture.
                Fire = Mathf.Max(Fire, 0.65f);
                RefreshCollider();
                RefreshFx();
            }
        }

        void LeaveAshPile()
        {
            _detailMaterial = MaterialId.Ash;
            _detailBlocks = false;
            _detailLook = SpriteFactory.Floor(MaterialId.Ash, Coord.x, Coord.y);
            ApplyDetail();
        }

        void Reshape(TileDef def)
        {
            Def = def;
            _authoredLook = null;
            _underlayLook = null;
            if (_underlay != null)
            {
                _underlay.enabled = false;
                _underlay.sprite = null;
            }
            if (_detailMaterial != MaterialId.Ash)
            {
                ClearDetail();
            }
            if (def.Kind != TileKind.Door)
            {
                PassageOpen = false;
            }

            ApplyVisual();
            ApplyCover();
            RefreshCollider();
            RefreshFx();
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

            PassageOpen = true;
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
                if (_authoredLook != null && _telegraph == MaterialId.None && !IsConjured)
                {
                    _renderer.sprite = _authoredLook;
                    _renderer.sortingOrder = Kind == TileKind.Wall ? 3 : Kind == TileKind.Door ? 4 : 0;
                    ApplyDetail();
                    ApplyUnderlay();
                    return;
                }

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
                        _renderer.sprite = Material == MaterialId.Water
                            ? SpriteFactory.Floor(Material, Coord.x, Coord.y, _animFrame < 0 ? 0 : _animFrame)
                            : SpriteFactory.Pit(Coord.x, Coord.y);
                        _renderer.sortingOrder = 1;
                        break;
                    case TileKind.Bridge:
                        _renderer.sprite = _telegraph != MaterialId.None || (IsConjured && Material != MaterialId.Stone)
                            ? SpriteFactory.Floor(ShownMaterial, Coord.x, Coord.y, _animFrame < 0 ? 0 : _animFrame)
                            : SpriteFactory.Bridge();
                        _renderer.sortingOrder = 1;
                        break;
                    case TileKind.Door:
                        ApplyDoorSprite(open: false);
                        break;
                    default:
                        _renderer.sprite = SpriteFactory.Floor(ShownMaterial, Coord.x, Coord.y, _animFrame < 0 ? 0 : _animFrame);
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

            ApplyDetail();
            ApplyUnderlay();
        }

        void ApplyUnderlay()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_underlayLook == null || (Kind != TileKind.Wall && Kind != TileKind.Door))
            {
                if (_underlay != null)
                {
                    _underlay.enabled = false;
                }

                return;
            }

            var view = EnsureUnderlay();
            view.sprite = _underlayLook;
            view.sortingOrder = 0;
            view.enabled = true;
        }

        SpriteRenderer EnsureUnderlay()
        {
            if (_underlay != null)
            {
                return _underlay;
            }

            var child = new GameObject("TileUnderlay");
            child.transform.SetParent(transform, false);
            _underlay = child.AddComponent<SpriteRenderer>();
            return _underlay;
        }

        void ApplyDoorSprite(bool open)
        {
            _renderer.sprite = SpriteFactory.Door(open, DoorFace != DoorFace.Jamb);
            _renderer.sortingOrder = 4;
        }

        void ClearDetail()
        {
            _detailLook = null;
            _detailMaterial = MaterialId.None;
            _detailBlocks = false;
            if (_detail != null)
            {
                _detail.enabled = false;
                _detail.sprite = null;
            }
        }

        void ApplyDetail()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_detailLook == null)
            {
                if (_detail != null)
                {
                    _detail.enabled = false;
                }

                return;
            }

            var view = EnsureDetail();
            view.sprite = _detailLook;
            view.sortingOrder = _renderer.sortingOrder + 2;
            view.transform.localScale = _detailMaterial == MaterialId.Ash
                ? new Vector3(0.7f, 0.7f, 1f)
                : Vector3.one;
            view.enabled = true;
        }

        SpriteRenderer EnsureDetail()
        {
            if (_detail != null)
            {
                return _detail;
            }

            var child = new GameObject("TileDetail");
            child.transform.SetParent(transform, false);
            _detail = child.AddComponent<SpriteRenderer>();
            return _detail;
        }

        static bool IsPlantMaterial(MaterialId material)
        {
            return material == MaterialId.Plant || material == MaterialId.Grove ||
                   material == MaterialId.Moss || material == MaterialId.Timber;
        }

        void ApplyCover()
        {
            if (_renderer == null)
            {
                return;
            }

            if (Kind == TileKind.Pit && Material != MaterialId.Water)
            {
                if (_cover != null)
                {
                    _cover.enabled = false;
                }

                return;
            }

            var cover = ResolveCoverSprite();
            if (cover == null)
            {
                if (_cover != null)
                {
                    _cover.enabled = false;
                }

                return;
            }

            var view = EnsureCover();
            view.sprite = cover;
            view.color = new Color(1f, 1f, 1f, _coverAlpha > 0.01f ? _coverAlpha : 1f);
            view.sortingOrder = _renderer.sortingOrder + 1;
            view.enabled = true;
        }

        Sprite ResolveCoverSprite()
        {
            if (_coverLook != null)
            {
                return _coverLook;
            }

            if (!string.IsNullOrEmpty(_coverId))
            {
                var named = _coverId.StartsWith("cover-", System.StringComparison.OrdinalIgnoreCase) ||
                            _coverId.StartsWith("fx-", System.StringComparison.OrdinalIgnoreCase)
                    ? _coverId
                    : "cover-" + _coverId;
                if (TileAtlas.TryGet(named, out var painted) && painted != null)
                {
                    return painted;
                }

                if (TileAtlas.TryGet(_coverId, out painted) && painted != null)
                {
                    return painted;
                }
            }

            return TileAtlas.Cover(ShownMaterial, Coord.x, Coord.y);
        }

        SpriteRenderer EnsureCover()
        {
            if (_cover != null)
            {
                return _cover;
            }

            var child = new GameObject("TileCover");
            child.transform.SetParent(transform, false);
            _cover = child.AddComponent<SpriteRenderer>();
            return _cover;
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
                    _overlay.sortingOrder = _renderer.sortingOrder + 2;
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
                    _overlay.sortingOrder = _renderer.sortingOrder + 2;
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

            if (Fire < 0.08f && Miasma < 0.18f && Fog < 0.18f && Wet < 0.18f && Charge < 0.18f && Oil < 0.18f && _growth < 1)
            {
                if (_fx != null)
                {
                    _fx.enabled = false;
                }

                return;
            }

            var fx = EnsureFx();
            fx.enabled = true;
            fx.sortingOrder = _renderer.sortingOrder + 3;
            if (Fire > 0.12f)
            {
                fx.sprite = SpriteFactory.Named("tile-fire");
                fx.color = new Color(1f, 0.55f, 0.12f, 0.35f + Fire * 0.5f);
            }
            else if (Miasma > 0.18f)
            {
                fx.sprite = SpriteFactory.Named("tile-poison");
                fx.color = new Color(0.42f, 0.88f, 0.2f, 0.28f + Miasma * 0.45f);
            }
            else if (Fog > 0.18f)
            {
                fx.sprite = SpriteFactory.Named("tile-fog");
                fx.color = new Color(0.62f, 0.66f, 0.7f, 0.24f + Fog * 0.4f);
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
            else if (Oil > 0.18f)
            {
                fx.sprite = SpriteFactory.Named("tile-wet");
                fx.color = new Color(0.18f, 0.12f, 0.05f, 0.28f + Oil * 0.4f);
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
            var solid = BlocksTravel;
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

        void Update()
        {
            if (_renderer == null)
            {
                return;
            }

            var live = SpriteFactory.Animates(ShownMaterial) || Fire > 0.08f || Miasma > 0.18f || Fog > 0.18f || Wet > 0.18f || Charge > 0.18f || _growth >= 1 || _telegraph != MaterialId.None;
            if (!live)
            {
                return;
            }

            var frame = Mathf.FloorToInt(Time.time * 3.6f) & 3;
            if (frame == _animFrame)
            {
                return;
            }

            _animFrame = frame;
            if (SpriteFactory.Animates(ShownMaterial) &&
                (Kind == TileKind.Floor || (Kind == TileKind.Bridge && (_telegraph != MaterialId.None || IsConjured)) || (Kind == TileKind.Pit && Material == MaterialId.Water)))
            {
                _renderer.sprite = SpriteFactory.Floor(ShownMaterial, Coord.x, Coord.y, frame);
            }

            if (_fx != null && _fx.enabled)
            {
                if (Fire > 0.12f)
                {
                    _fx.sprite = SpriteFactory.Clip("tile-fire")[frame % 3];
                }
                else if (Miasma > 0.18f)
                {
                    _fx.sprite = SpriteFactory.Clip("tile-poison")[frame % 2];
                }
                else if (Fog > 0.18f)
                {
                    _fx.sprite = SpriteFactory.Clip("tile-fog")[frame % 2];
                }
                else if (Charge > 0.18f)
                {
                    _fx.sprite = SpriteFactory.Clip("tile-charge")[frame % 2];
                }
                else if (Wet > 0.18f)
                {
                    _fx.sprite = SpriteFactory.Clip("tile-wet")[frame % 2];
                }
                else if (_growth >= 1)
                {
                    _fx.sprite = SpriteFactory.Clip("tile-grow")[frame % 2];
                }
            }
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
