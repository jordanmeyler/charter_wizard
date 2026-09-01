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
        MaterialId _coverMaterial;
        bool _openVoid;

        /// <summary>
        /// Closed masonry and shut doors stop bodies and shots.
        /// An opened door is a hole in the wall.
        /// </summary>
        public bool BlocksTravel =>
            Kind == TileKind.Wall || (Kind == TileKind.Door && !PassageOpen) || _detailBlocks;

        public bool IsEmitting =>
            !Def.TearsTapestry
            && (Emission.Count > 0
                || CoverCatalog.RuneOf(Cover) != RuneId.None
                || CoverMaterial != MaterialId.None
                || Fire > 0.05f);
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 2.4f;
        public float VoiceWeight => Kind == TileKind.Wall ? 0.55f : 1f;
        public RuneSourceKind SourceKind => RuneSourceKind.Tile;

        SpriteRenderer _renderer;
        SpriteRenderer _underlay;
        SpriteRenderer _overlay;
        SpriteRenderer _cover;
        SpriteRenderer _coverMark;
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
        float _overlayBurn;

        public void Bind(Vector2Int coord, TileDef def)
        {
            Coord = coord;
            Def = def;
            transform.position = new Vector3(coord.x + 0.5f, coord.y + 0.5f, 0f);

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.spriteSortPoint = SpriteSortPoint.Pivot;
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
                ApplyCover();
            }
        }

        /// <summary>
        /// Change walk family / material without throwing away the
        /// tileset already stamped on this cell.
        /// </summary>
        public void AuthorKind(TileKind kind, MaterialId material)
        {
            if (kind == TileKind.None)
            {
                return;
            }

            Def = new TileDef(kind, material);
            if (_renderer != null)
            {
                ApplyVisual();
                ApplyCover();
                RefreshCollider();
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
            !HasAshCover
            && (Material == MaterialId.Water
            || string.Equals(_coverId, "water", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-water", System.StringComparison.OrdinalIgnoreCase));

        public bool HasPlantCover =>
            !HasAshCover
            && (Cover == TileCover.Vine
            || string.Equals(_coverId, "vine", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-vine", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-plant", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-grove", System.StringComparison.OrdinalIgnoreCase));

        public bool HasAshCover =>
            Cover == TileCover.Ash
            || string.Equals(_coverId, "ash", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-ash", System.StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Water on this cell: a water floor, a water covering, or
        /// yield a spell left behind. Neighbor water is not enough.
        /// </summary>
        public bool HasWaterSource =>
            Wet > 0.2f || HasWaterCover || IsDeepWater || IsOverWater;

        public bool HasIceCover =>
            Cover == TileCover.Ice
            || string.Equals(_coverId, "ice", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-ice", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-ice-b", System.StringComparison.OrdinalIgnoreCase);

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

        public TileCover Cover { get; private set; }

        /// <summary>
        /// Overlay matter a stamp left. Inert until a spell or a live
        /// reaction finds it — then ice melts, oil fuels, metal conducts.
        /// </summary>
        public MaterialId CoverMaterial =>
            _coverMaterial != MaterialId.None
                ? _coverMaterial
                : CoverCatalog.MaterialOf(Cover);

        public bool HasFireCover =>
            Cover == TileCover.Fire
            || CoverMaterial == MaterialId.Ember
            || CoverMaterial == MaterialId.Hearth
            || CoverMaterial == MaterialId.Lava
            || CoverMaterial == MaterialId.Fire;

        /// <summary>
        /// Hunger seated in the walk itself — Floor-Fire, a hearth, lava.
        /// Rest matter. It does not burn out. Coverings and spells
        /// are what react. Leftover ember tiles count as this too.
        /// </summary>
        public bool IsFireFloor => VitalLaw.IsRestFire(Material);

        /// <summary>
        /// Fuel sitting on the walk — vine, oil, a plant or timber
        /// detail. The floor underneath is not this.
        /// </summary>
        public bool HasOverlayFuel =>
            HasVine
            || (HasOil && !IsGeyser)
            || HasPlantishDetail
            || (HasFireCover && !Kindled);

        public bool HasPoisonCover =>
            !HasAshCover
            && (Cover == TileCover.Poison
            || CoverMaterial == MaterialId.Acid
            || string.Equals(_coverId, "poison", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-poison", System.StringComparison.OrdinalIgnoreCase));

        public void PaintCover(TileCover cover)
        {
            PaintCover(cover == TileCover.None ? null : cover.ToString().ToLowerInvariant());
        }

        public void PaintCover(string id)
        {
            var before = Cover;
            _coverId = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
            Cover = ParseCover(_coverId);
            _coverMaterial = CoverCatalog.MaterialOf(Cover);
            if (before != Cover)
            {
                _coverLook = null;
            }

            if (string.Equals(_coverId, "water", System.StringComparison.OrdinalIgnoreCase))
            {
                _coverAlpha = Mathf.Min(_coverAlpha, 0.62f);
            }

            ApplyCover();
            if (before != Cover)
            {
                NoteSpokenChange();
            }
        }

        /// <summary>
        /// A material on the Cover layer that is not a spoken TileCover
        /// (oil, metal, timber). Look stays; live fire / charge / wet
        /// do not start until work finds the cell.
        /// </summary>
        public void AuthorCoverMaterial(MaterialId material)
        {
            if (!CoverCatalog.IsOverlayMaterial(material))
            {
                return;
            }

            _coverMaterial = material;
        }

        static TileCover ParseCover(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return TileCover.None;
            }

            var name = id.Trim();
            if (name.StartsWith("cover-", System.StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(6);
            }

            return System.Enum.TryParse(name, true, out TileCover cover) ? cover : TileCover.None;
        }

        public void AuthorCoverLook(Sprite sprite, float alpha = 1f)
        {
            _coverLook = sprite;
            _coverAlpha = Mathf.Clamp01(alpha <= 0f ? 1f : alpha);
            ApplyCover();
        }

        /// <summary>
        /// Blank space baked as a drop — dark void, not a carved hole.
        /// Stamp Kind=Pit if you want the pit sprite on a painted cell.
        /// </summary>
        public void MarkOpenVoid()
        {
            _openVoid = true;
            if (_renderer != null)
            {
                ApplyVisual();
            }
        }

        public float Fire { get; private set; }
        public float Wet { get; private set; }
        public float Charge { get; private set; }
        public float Fog { get; private set; }
        public float Miasma { get; private set; }
        public float Oil { get; private set; }
        public bool Kindled { get; private set; }
        /// <summary>
        /// A stood oil fountain. Hunger that finds it kindles
        /// and will not leave until yield is thrown.
        /// </summary>
        public bool IsGeyser { get; private set; }
        /// <summary>
        /// Fire a spell or NPC working started. Authored torches, kindled
        /// halls, and painted cover stay still until work finds them.
        /// </summary>
        public bool LiveFire { get; private set; }
        public int Growth => _growth;
        public bool IsBurning => Fire > 0.35f;
        public bool HasFog => Fog > 0.2f;
        public bool HasMiasma => Miasma > 0.2f;
        public bool HasOil =>
            Oil > 0.2f
            || Material == MaterialId.Oil
            || IsGeyser
            || CoverMaterial == MaterialId.Oil;
        public bool HasVine =>
            Cover == TileCover.Vine
            || string.Equals(_coverId, "vine", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-vine", System.StringComparison.OrdinalIgnoreCase);
        public bool IsPoisonWater =>
            Material == MaterialId.Acid || HasPoisonCover;
        /// <summary>
        /// Standing yield under this cell — a water floor, a water
        /// covering, or a vegetable body grown over a water foundation.
        /// </summary>
        public bool IsOverWater =>
            HasWaterCover
            || IsDeepWater
            || (_hasFoundation && Foundation.Material == MaterialId.Water && !HasIceCover);

        /// <summary>
        /// A plant standing on water. It can light, but it does not
        /// carry the flame. Oil on the same cell still runs.
        /// </summary>
        public bool IsPlantOnWater =>
            !HasOil
            && IsOverWater
            && (IsPlantish || HasPlantCover || HasPlantishDetail || HasVine);

        /// <summary>
        /// Oil floats. A plant on water can still catch. Standing
        /// yield does not put those fires out by itself.
        /// </summary>
        public bool FireIgnoresWater => HasOil || IsPlantOnWater;

        public float Flammability
        {
            get
            {
                if (HasAshCover)
                {
                    return 0f;
                }

                if (HasOil)
                {
                    return Mathf.Max(0.5f, MaterialCatalog.Of(MaterialId.Oil).Flammability);
                }

                var body = Def.WorldMaterial.Flammability + DetailFlammability
                    + (HasVine ? 1.4f : 0f);
                if (!HasVine && !HasWaterCover)
                {
                    body += CoverFlammability;
                }

                if (IsPlantOnWater)
                {
                    return Mathf.Max(0.4f, body);
                }

                if (HasWaterCover || IsDeepWater)
                {
                    return -1.6f;
                }

                return body;
            }
        }

        /// <summary>
        /// How long a full fire lasts here. Oil is five seconds.
        /// Wood is four. Plant is three. Grove is two. Oil on a
        /// cell extends the clock; it does not cut a longer fuel
        /// short. Fire cover is tinder — a short clock.
        /// </summary>
        public float BurnSeconds
        {
            get
            {
                var seconds = 0f;
                if (Def.WorldMaterial.BurnSeconds > 0f)
                {
                    seconds = Def.WorldMaterial.BurnSeconds;
                }

                if (_detailMaterial != MaterialId.None)
                {
                    var detail = MaterialCatalog.Of(_detailMaterial).BurnSeconds;
                    if (detail > 0f)
                    {
                        seconds = seconds > 0f ? Mathf.Min(seconds, detail) : detail;
                    }
                }

                if (HasAshCover)
                {
                    return 0f;
                }

                if (HasVine)
                {
                    seconds = seconds > 0f
                        ? Mathf.Min(seconds, VitalLaw.PlantBurnSeconds)
                        : VitalLaw.PlantBurnSeconds;
                }

                if (HasOil)
                {
                    seconds = seconds > 0f
                        ? Mathf.Max(seconds, VitalLaw.OilBurnSeconds)
                        : VitalLaw.OilBurnSeconds;
                }

                if (HasFireCover && !Kindled)
                {
                    seconds = seconds > 0f
                        ? Mathf.Min(seconds, VitalLaw.TinderBurnSeconds)
                        : VitalLaw.TinderBurnSeconds;
                }

                return seconds;
            }
        }

        /// <summary>
        /// Clock leftover on this cell. Spread uses Hunger.
        /// A plant on water lights and stays put.
        /// </summary>
        public float BurnRate
        {
            get
            {
                if (IsPlantOnWater)
                {
                    return 0f;
                }

                return VitalLaw.FireRun(BurnSeconds);
            }
        }

        public float Conductivity
        {
            get
            {
                var value = Def.WorldMaterial.Conductivity;
                if (HasWaterCover)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(MaterialId.Water));
                }

                if (HasPlantCover || HasVine)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(MaterialId.Plant));
                }

                if (HasIceCover)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(MaterialId.Ice));
                }

                value = ChargeLaw.Combine(value, ChargeLaw.OfCover(Cover));
                if (Cover == TileCover.None && CoverMaterial != MaterialId.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(CoverMaterial));
                }

                value = ChargeLaw.Combine(value, ChargeLaw.OfWetness(Wet));
                if (HasOil)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(MaterialId.Oil));
                }

                if (_detailMaterial != MaterialId.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(_detailMaterial));
                }

                if (WorldMatter.TryOverlayConductivity(WorldOrigin, out var item))
                {
                    value = ChargeLaw.Combine(value, item);
                }

                return value;
            }
        }

        public bool Conducts => ChargeLaw.Conducts(Conductivity);
        public bool Insulates => ChargeLaw.Insulates(Conductivity);
        public bool IsPlantish => IsPlantMaterial(Material) && !HasAshCover;
        public bool HasPlantishDetail => IsPlantMaterial(_detailMaterial) && !HasAshCover;
        /// <summary>
        /// Fuel hunger can finish. Kindled halls and rest fire floors
        /// stay. Fire cover, timber walls, and plant / timber floors
        /// catch once, then leftover dirt.
        /// </summary>
        public bool HoldsBurnFuel =>
            !HasAshCover
            && !IsFireFloor
            && (IsPlantish
                || HasPlantishDetail
                || HasVine
                || (HasOil && !IsGeyser)
                || (HasFireCover && !Kindled)
                || (Kind == TileKind.Wall && VitalLaw.CanBurn(Material))
                || ((Kind == TileKind.Floor || Kind == TileKind.Bridge)
                    && VitalLaw.CanBurn(Material)));

        /// <summary>
        /// Neighbor fire may take this cell. Neutral walk (stone, dirt)
        /// only lights when a spell's volume hits it. Weaker fuel still
        /// needs a strong source (7+) within that source's reach, and
        /// must touch fuel toward it so fire does not leap a gap.
        /// </summary>
        public bool IsSpreadFuel => !HasAshCover && Hunger > VitalLaw.HungerNeutral;

        /// <summary>
        /// 0–10 hunger on this cell. Walk, a timber / plant detail, vine,
        /// oil, and fire cover raise the grade. Rest fire in the floor
        /// stays 0 — a spell starts that source.
        /// </summary>
        public int Hunger
        {
            get
            {
                if (HasAshCover)
                {
                    return VitalLaw.HungerNeutral;
                }

                var hunger = IsFireFloor
                    ? VitalLaw.HungerNeutral
                    : VitalLaw.HungerOf(Material);
                if (_detailMaterial != MaterialId.None)
                {
                    hunger = Mathf.Max(hunger, VitalLaw.HungerOf(_detailMaterial));
                }

                if (HasVine)
                {
                    hunger = Mathf.Max(hunger, VitalLaw.HungerPlant);
                }

                if (HasOil && !IsGeyser)
                {
                    hunger = Mathf.Max(hunger, VitalLaw.HungerOil);
                }

                if (HasFireCover && !Kindled)
                {
                    hunger = Mathf.Max(hunger, VitalLaw.HungerTinder);
                }

                return hunger;
            }
        }

        /// <summary>
        /// How hard this live flame may light other cells. A kindled
        /// hall, a geyser, or a lit rest-fire walk is an oil-grade
        /// source (10). Only a strong source (7+) walks fire, onto
        /// equal-or-weaker fuel, out to its own reach.
        /// </summary>
        public int FirePotency
        {
            get
            {
                if (Kindled || IsGeyser || (IsFireFloor && LiveFire))
                {
                    return VitalLaw.HungerOil;
                }

                return Hunger;
            }
        }

        /// <summary>
        /// 0–10 quench on this cell. Walk, a wet detail or cover, standing
        /// water, ice, and spell wet raise the grade. Oil floats — it
        /// stays dry even on a water foundation. Dry stone is 0.
        /// </summary>
        public int Quench
        {
            get
            {
                if (HasOil)
                {
                    return VitalLaw.QuenchDry;
                }

                var quench = VitalLaw.QuenchOf(Material);
                if (_detailMaterial != MaterialId.None)
                {
                    quench = Mathf.Max(quench, VitalLaw.QuenchOf(_detailMaterial));
                }

                var cover = CoverMaterial;
                if (cover != MaterialId.None && cover != MaterialId.Oil)
                {
                    quench = Mathf.Max(quench, VitalLaw.QuenchOf(cover));
                }

                if (HasWaterCover || IsDeepWater)
                {
                    quench = Mathf.Max(quench, VitalLaw.QuenchWater);
                }

                if (HasIceCover)
                {
                    quench = Mathf.Max(quench, VitalLaw.QuenchIce);
                }

                if (Wet >= 0.7f)
                {
                    quench = Mathf.Max(quench, VitalLaw.QuenchWater);
                }
                else if (Wet >= 0.35f)
                {
                    quench = Mathf.Max(quench, VitalLaw.QuenchRain);
                }
                else if (Wet > 0.15f)
                {
                    quench = Mathf.Max(quench, VitalLaw.QuenchDamp);
                }

                return quench;
            }
        }

        public bool HasDetail =>
            _detailLook != null || _detailMaterial != MaterialId.None;
        float DetailFlammability =>
            _detailMaterial == MaterialId.None
                ? 0f
                : MaterialCatalog.Of(_detailMaterial).Flammability;

        float CoverFlammability
        {
            get
            {
                var material = CoverMaterial;
                if (material == MaterialId.None || material == Material || material == MaterialId.Oil)
                {
                    return 0f;
                }

                if (Cover == TileCover.Vine || Cover == TileCover.Water)
                {
                    return 0f;
                }

                return MaterialCatalog.Of(material).Flammability;
            }
        }
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
            (Kind == TileKind.Pit || Kind == TileKind.Floor) &&
            !HasIceCover &&
            !HasPlantCover;

        public bool IsSafeStand =>
            ((Kind == TileKind.Floor || Kind == TileKind.Bridge) &&
            !IsDeepWater &&
            Material != MaterialId.Lava)
            || HasIceCover
            || HasPlantCover;

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
            if (HasIceCover && (Kind == TileKind.Floor || Kind == TileKind.Bridge) && !IsDeepWater)
            {
                return true;
            }

            if (IsDeepWater)
            {
                BecomeWalkable(MaterialId.Ice, conjured: true);
                PaintIceCover();
                Wet = Mathf.Min(Wet, 0.15f);
                RefreshFx();
                return true;
            }

            if (HasWaterCover)
            {
                PaintIceCover();
                Wet = Mathf.Min(Wet, 0.15f);
                RefreshFx();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Hard water on this cell. Water becomes the same ice sheet
        /// ice-wall uses. A dry walk takes that same cover sheen.
        /// </summary>
        public bool LayIce()
        {
            if (FreezeSolid())
            {
                return true;
            }

            if (Kind != TileKind.Floor && Kind != TileKind.Bridge)
            {
                return false;
            }

            if (Material == MaterialId.Lava || Material == MaterialId.Void || HasAshCover)
            {
                return false;
            }

            if (HasIceCover)
            {
                return true;
            }

            PaintIceCover();
            Wet = Mathf.Min(Wet, 0.15f);
            RefreshFx();
            return true;
        }

        void PaintIceCover()
        {
            _coverLook = null;
            PaintCover(TileCover.Ice);
        }

        /// <summary>
        /// A vegetable body over yield. Green covers the water
        /// and holds you, the way ice does. The walk tile stays.
        /// Hunger can light that cover; it will not run from it.
        /// </summary>
        public bool GrowOverWater(MaterialId material = MaterialId.Plant)
        {
            if (!IsDeepWater && !HasWaterCover)
            {
                return false;
            }

            if (!WorldWork.IsPlantBody(material))
            {
                material = MaterialId.Plant;
            }

            PaintCover(TileCover.Vine);
            RefreshFx();
            return true;
        }

        /// <summary>
        /// Plant cover on this cell only — ice's law, not a walk
        /// across the pool. Water takes a walkable vine; a hollow
        /// takes the same cover; dry walk takes a climbing body.
        /// </summary>
        public bool PlacePlantCover(MaterialId material = MaterialId.Plant)
        {
            if (IsDeepWater || HasWaterCover)
            {
                return GrowOverWater(material);
            }

            if (Kind == TileKind.Pit || Material == MaterialId.Void)
            {
                if (HasAshCover)
                {
                    return false;
                }

                PaintCover(TileCover.Vine);
                RefreshFx();
                return true;
            }

            return LayVine();
        }

        /// <summary>
        /// Standard earth over water does not span. Yield meeting
        /// rest leaves a mud covering. It will not hold you.
        /// </summary>
        public bool LayMudCover()
        {
            Drench(1f);
            if (!IsDeepWater)
            {
                return SlickMud();
            }

            PaintCover(TileCover.Mud);
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
            IsGeyser = false;
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

        /// <summary>
        /// Put the fire out now. A kindled hall forgets the flame.
        /// Rest fire in the walk stays as walk — only the live work dies.
        /// </summary>
        public void Snuff()
        {
            if (Fire <= 0f && !Kindled && !LiveFire)
            {
                return;
            }

            Fire = 0f;
            Kindled = false;
            LiveFire = false;
            RefreshFx();
        }

        public void Ignite(float amount, bool live = true)
        {
            if (Kind == TileKind.Pit && Material != MaterialId.Water)
            {
                return;
            }

            if (amount > 0f && HasIceCover)
            {
                MeltIceCover();
                return;
            }

            if (amount > 0f && !FireIgnoresWater && (HasWaterCover || IsDeepWater || Wet > 0.35f))
            {
                Fire = 0f;
                Kindled = false;
                LiveFire = false;
                RefreshFx();
                return;
            }

            var spoke = Fire > 0.05f;
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

            if ((Fire > 0.05f) != spoke)
            {
                NoteSpokenChange();
            }

            if (amount > 0f && Fire > 0.05f)
            {
                Wet = Mathf.Max(0f, Wet - amount);
            }

            if (HasOil && amount > 0f)
            {
                Fire = Mathf.Clamp01(Fire + 0.35f);
            }

            if (IsGeyser && amount > 0f && Fire > 0.05f)
            {
                Kindled = true;
            }

            RefreshFx();
        }

        public bool MarkGeyser()
        {
            if (Kind == TileKind.Wall || Kind == TileKind.Door || Material == MaterialId.Void)
            {
                return false;
            }

            IsGeyser = true;
            return SlickOil(1f);
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
        /// A climbing body on the walk. Hunger runs it like a wick.
        /// </summary>
        public bool LayVine()
        {
            if (Kind == TileKind.Wall || Kind == TileKind.Door || Material == MaterialId.Void)
            {
                return false;
            }

            if (IsDeepWater || Material == MaterialId.Lava)
            {
                return false;
            }

            if (HasVine)
            {
                if (IsBurning)
                {
                    Ignite(0.45f);
                }

                return true;
            }

            PaintCover(TileCover.Vine);
            if (IsBurning)
            {
                Ignite(0.55f);
            }

            RefreshFx();
            return true;
        }

        public void BurnVine()
        {
            BurnOut();
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

            WashPoison();
            RefreshFx();
        }

        /// <summary>
        /// Yield takes liquid poison. It does not lift a miasma cloud.
        /// </summary>
        public bool WashPoison()
        {
            var washed = false;
            if (HasPoisonCover)
            {
                PaintCover(TileCover.None);
                washed = true;
            }

            if (Material == MaterialId.Acid
                && (Kind == TileKind.Floor || Kind == TileKind.Bridge))
            {
                Reshape(new TileDef(Kind, MaterialId.Scoured));
                washed = true;
            }

            return washed;
        }

        /// <summary>
        /// Liquid poison on the walk. Contact only; yield washes it.
        /// </summary>
        public void SlickPoison(float amount = 1f)
        {
            if (Kind == TileKind.Wall || Kind == TileKind.Door)
            {
                return;
            }

            PaintCover(TileCover.Poison);
            if (amount > 0f)
            {
                Wet = Mathf.Max(Wet, Mathf.Clamp01(amount * 0.35f));
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
            if (amount > 0f && ChargeLaw.Insulates(Conductivity))
            {
                return;
            }

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
            if (Miasma > 0.2f && Cover == TileCover.None)
            {
                PaintCover(TileCover.Miasma);
            }

            RefreshFx();
        }

        /// <summary>
        /// Breath lifts a hanging veil. Wind takes miasma. Yield
        /// washes a poison slick — that is not this verb.
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
                if (Cover == TileCover.Miasma)
                {
                    PaintCover(TileCover.None);
                }

                changed = true;
            }

            if (WorldWork.IsAirWork(spell) && Wet > 0.05f)
            {
                Dry(0.55f);
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

            if (HasIceCover && MatterLaw.Melts(spell, MaterialId.Ice))
            {
                MeltIceCover();
                return true;
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

        bool MeltIceCover()
        {
            if (!HasIceCover)
            {
                return false;
            }

            LeaveMeltWater();
            return true;
        }

        public bool Annihilate()
        {
            if (MatterLaw.ResistsMagic(Material) || Material == MaterialId.Void)
            {
                return false;
            }

            Oil = 0f;
            IsGeyser = false;
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
            RefreshFx();
        }

        public void BurnDown()
        {
            BurnOut();
        }

        /// <summary>
        /// The film is spent. A geyser keeps its fountain until yield
        /// is thrown.
        /// </summary>
        public void SpendFuel()
        {
            if (IsGeyser)
            {
                return;
            }

            BurnOut();
        }

        /// <summary>
        /// Overlay fuel on a rest fire floor. Vine, oil, and plant
        /// details burn on their clock. The fire walk does not.
        /// </summary>
        public void TickOverlayFuel(float seconds)
        {
            if (!HasOverlayFuel)
            {
                _overlayBurn = 0f;
                return;
            }

            _overlayBurn += Mathf.Max(0f, seconds);
            var clock = BurnSeconds > 0.05f ? BurnSeconds : VitalLaw.PlantBurnSeconds;
            if (_overlayBurn < clock)
            {
                return;
            }

            _overlayBurn = 0f;
            BurnOut();
        }

        public void EndSpellFire()
        {
            LiveFire = false;
            if (Kindled)
            {
                KeepKindled();
                return;
            }

            if (IsFireFloor)
            {
                Fire = 0f;
                RefreshFx();
            }
        }

        void SpendOverlayFuel()
        {
            _overlayBurn = 0f;
            if (HasPlantishDetail)
            {
                ClearDetail();
            }

            if (!IsGeyser)
            {
                Oil = 0f;
            }

            if (HasVine)
            {
                PaintCover(TileCover.None);
            }
        }

        /// <summary>
        /// Hunger finishes the fuel. Fire cover wears off. A plant or
        /// timber floor swaps stamp and look to leftover dirt — it
        /// does not draw ash over the tile you placed. A timber or
        /// plant wall burns for its clock, then falls to that leftover
        /// dirt so a key behind it can be reached. Floor-Fire stays.
        /// It only spends what sat on it. Covers and spells may still
        /// sit on the leftover.
        /// </summary>
        public void BurnOut()
        {
            if (IsFireFloor)
            {
                SpendOverlayFuel();
                if (Kindled)
                {
                    KeepKindled();
                }
                else
                {
                    LiveFire = false;
                    Fire = 0f;
                }

                RefreshCollider();
                RefreshFx();
                return;
            }

            if (Kindled && !LiveFire)
            {
                return;
            }

            var waterWalk = Material == MaterialId.Water
                || (_hasFoundation && Foundation.Material == MaterialId.Water);
            if (!waterWalk)
            {
                Wet = 0f;
            }

            var fuelWall = Kind == TileKind.Wall && VitalLaw.CanBurn(Material);
            var fuelFloor = !IsFireFloor
                && (Kind == TileKind.Floor || Kind == TileKind.Bridge)
                && (VitalLaw.CanBurn(Material) || IsPlantish || HasPlantishDetail || HasVine || HasOil);

            Fire = 0f;
            LiveFire = false;
            _growth = 0;
            if (!IsGeyser)
            {
                Oil = 0f;
            }

            if (HasPlantishDetail)
            {
                ClearDetail();
            }

            _coverLook = null;
            var underLook = _underlayLook;
            if (fuelWall)
            {
                if (IsConjured)
                {
                    RestoreFoundation();
                }
                else
                {
                    var walk = CoverCatalog.RestAfterBurn(Material);
                    if (walk == MaterialId.None)
                    {
                        walk = MaterialId.Dirt;
                    }

                    Reshape(new TileDef(TileKind.Floor, walk));
                    if (underLook != null)
                    {
                        AuthorLook(underLook);
                    }
                }

                PaintCover(TileCover.None);
                RefreshCollider();
                RefreshFx();
                return;
            }

            if (fuelFloor && !waterWalk
                && Material != MaterialId.Lava && Material != MaterialId.Void)
            {
                if (IsConjured)
                {
                    RestoreFoundation();
                }
                else
                {
                    var leftover = CoverCatalog.LeftoverFloor(Material);
                    if (leftover != MaterialId.None && leftover != Material)
                    {
                        Reshape(new TileDef(Kind, leftover));
                    }
                }

                PaintCover(TileCover.None);
                RefreshCollider();
                RefreshFx();
                return;
            }

            var leftoverWalk = CoverCatalog.RestAfterBurn(Material);
            var changeWalk = leftoverWalk != MaterialId.None
                && leftoverWalk != Material
                && (Kind == TileKind.Floor || Kind == TileKind.Bridge)
                && !waterWalk
                && Material != MaterialId.Lava
                && Material != MaterialId.Void;

            if (changeWalk)
            {
                Reshape(new TileDef(Kind, leftoverWalk));
            }

            PaintCover(TileCover.None);
            RefreshCollider();
            RefreshFx();
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

            if (!HasAshCover)
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
            grid?.NoteSpokenChange();
        }

        void NoteSpokenChange()
        {
            GetComponentInParent<WorldGrid>()?.NoteSpokenChange();
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

            if (Fire > 0.05f)
            {
                buffer.Add(RuneId.Fire);
            }

            CoverCatalog.Speak(Cover, buffer);
            if (Cover == TileCover.None && CoverMaterial != MaterialId.None)
            {
                CoverCatalog.SpeakMaterial(CoverMaterial, buffer);
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
                            : _openVoid
                                ? SpriteFactory.OpenVoid()
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

        static bool IsPlantPackCover(string id)
        {
            return string.Equals(id, "vine", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "cover-vine", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "cover-plant", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "cover-grove", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "cover-moss", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "cover-moss-b", System.StringComparison.OrdinalIgnoreCase);
        }

        void ApplyCover()
        {
            if (_renderer == null)
            {
                return;
            }

            if (Kind == TileKind.Pit && Material != MaterialId.Water)
            {
                HideCover();
                HideCoverMark();
                return;
            }

            var sheen = ResolveCoverSprite();
            if (sheen != null)
            {
                var view = EnsureCover();
                view.sprite = sheen;
                view.color = new Color(1f, 1f, 1f, CoverDrawAlpha());
                view.sortingOrder = _renderer.sortingOrder + 1;
                view.enabled = true;
            }
            else
            {
                HideCover();
            }

            ApplyCoverMark();
            RefreshLinger();
        }

        void ApplyCoverMark()
        {
            var rune = CoverCatalog.RuneOf(Cover);
            if (rune == RuneId.None)
            {
                HideCoverMark();
                return;
            }

            var mark = EnsureCoverMark();
            mark.sprite = RuneMark.AsSprite(rune, RunePalette.MarkInk(rune));
            mark.color = Color.white;
            mark.sortingOrder = _renderer.sortingOrder + 2;
            mark.enabled = true;
        }

        void HideCover()
        {
            if (_cover != null)
            {
                _cover.enabled = false;
            }
        }

        void HideCoverMark()
        {
            if (_coverMark != null)
            {
                _coverMark.enabled = false;
            }
        }

        float CoverDrawAlpha()
        {
            var alpha = _coverAlpha > 0.01f ? _coverAlpha : 1f;
            if (_authoredLook == null)
            {
                return alpha;
            }

            // A painted walk tile must stay visible. Opaque pack covers
            // (hell lava, ice sheets) used to hide that sprite in Play.
            if (Cover == TileCover.Ice || Cover == TileCover.Ash || Cover == TileCover.Mud)
            {
                return Mathf.Min(alpha, 0.72f);
            }

            return Mathf.Min(alpha, 0.48f);
        }

        Sprite ResolveCoverSprite()
        {
            if (_coverLook != null)
            {
                return _coverLook;
            }

            if (!string.IsNullOrEmpty(_coverId))
            {
                // Water cover without a painted sheen generates the Water
                // mark. cover-water is the same opaque pool as floor-water
                // and would hide the tile the author already stamped.
                if (string.Equals(_coverId, "water", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-water", System.StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // cover-fire / cover-lightning are full hell tiles. A
                // live wash sits on the authored floor instead.
                if (string.Equals(_coverId, "fire", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-fire", System.StringComparison.OrdinalIgnoreCase))
                {
                    return SpriteFactory.Named("tile-fire");
                }

                if (string.Equals(_coverId, "lightning", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-lightning", System.StringComparison.OrdinalIgnoreCase))
                {
                    return SpriteFactory.Named("tile-charge");
                }

                // A Plant / Timber material stamp used to invent Vine
                // cover. Keep the tileset; spells still PaintCover on
                // stone or water and show a sheen.
                if (_authoredLook != null &&
                    IsPlantMaterial(Material) &&
                    IsPlantPackCover(_coverId))
                {
                    return null;
                }
            }

            var sheen = CoverCatalog.Sheen(Cover);
            if (sheen != null)
            {
                return sheen;
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

                if (string.Equals(_coverId, "miasma", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-miasma", System.StringComparison.OrdinalIgnoreCase))
                {
                    return SpriteFactory.Named("tile-poison");
                }

                if (string.Equals(_coverId, "poison", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-poison", System.StringComparison.OrdinalIgnoreCase))
                {
                    return SpriteFactory.Named("tile-wet");
                }

                if (string.Equals(_coverId, "fog", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-fog", System.StringComparison.OrdinalIgnoreCase))
                {
                    return SpriteFactory.Named("tile-fog");
                }

                if (string.Equals(_coverId, "mud", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-mud", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (TileAtlas.TryGet("floor-mud", out var mud) && mud != null)
                    {
                        return mud;
                    }
                }

                if (string.Equals(_coverId, "ash", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-ash", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (TileAtlas.TryGet("floor-ash", out var ash) && ash != null)
                    {
                        return ash;
                    }
                }
            }

            // Plant / timber material stamps resolve to cover-plant /
            // cover-vine. Those pack tiles hide the authored tileset.
            // Spell-grown vine still arrives through _coverId above.
            // A Floor / Wall stamp is the tile the author placed — do
            // not invent a material sheen on top of it. Spells and
            // Cover stamps set _coverId / _coverLook when they may draw.
            return null;
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

        SpriteRenderer EnsureCoverMark()
        {
            if (_coverMark != null)
            {
                return _coverMark;
            }

            var child = new GameObject("CoverMark");
            child.transform.SetParent(transform, false);
            child.transform.localScale = Vector3.one * 0.62f;
            _coverMark = child.AddComponent<SpriteRenderer>();
            return _coverMark;
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

            if (Fire < 0.08f && Miasma < 0.18f && Fog < 0.18f && Wet < 0.18f && Charge < 0.18f && Oil < 0.18f && !IsGeyser && !HasPoisonCover && _growth < 1)
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
            else if (HasPoisonCover)
            {
                fx.sprite = SpriteFactory.Named("tile-wet");
                fx.color = new Color(0.28f, 0.62f, 0.12f, 0.42f);
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
            else if (Oil > 0.18f || IsGeyser)
            {
                fx.sprite = SpriteFactory.Named("tile-wet");
                fx.color = IsGeyser
                    ? new Color(0.32f, 0.2f, 0.06f, 0.4f + Oil * 0.35f)
                    : new Color(0.18f, 0.12f, 0.05f, 0.28f + Oil * 0.4f);
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
            var look = LingerLook();
            if (!NeedsLinger(look.Family))
            {
                return;
            }

            var offset = RaisedAs == RaisedForm.Pillar
                ? new Vector3(0f, 0.55f, 0f)
                : new Vector3(0f, 0.2f, 0f);
            _linger = ElementFx.Linger(transform, look, 0.85f, offset);
        }

        ElementLook LingerLook()
        {
            if (IsConjured)
            {
                return ElementLook.For(Element);
            }

            switch (Cover)
            {
                case TileCover.Ice:
                    return ElementLook.Of(ElementFamily.Ice);
                case TileCover.Fire:
                    return ElementLook.Of(ElementFamily.Fire);
                case TileCover.Lightning:
                    return ElementLook.Of(ElementFamily.Lightning);
                case TileCover.Vine:
                    return ElementLook.Of(ElementFamily.Plant);
                default:
                    return default;
            }
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

            var live = SpriteFactory.Animates(ShownMaterial) || Fire > 0.08f || Miasma > 0.18f || Fog > 0.18f || Wet > 0.18f || Charge > 0.18f || Oil > 0.18f || IsGeyser || HasPoisonCover || _growth >= 1 || _telegraph != MaterialId.None;
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
            var keepAuthored = _authoredLook != null && _telegraph == MaterialId.None && !IsConjured;
            if (!keepAuthored &&
                SpriteFactory.Animates(ShownMaterial) &&
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
                else if (HasPoisonCover)
                {
                    _fx.sprite = SpriteFactory.Clip("tile-wet")[frame % 2];
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
