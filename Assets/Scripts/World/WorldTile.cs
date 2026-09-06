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
        bool LeafOpen;
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
            LeafOpen
                ? (_detailBlocks || _detail2Blocks)
                : Kind == TileKind.Wall
                    || (Kind == TileKind.Door && !PassageOpen)
                    || _detailBlocks
                    || _detail2Blocks;

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
        SpriteRenderer _detail2;
        SpriteRenderer _fx;
        Sprite _authoredLook;
        Sprite _underlayLook;
        MaterialId _underlayMaterial;
        Sprite _detailLook;
        MaterialId _detailMaterial;
        bool _detailBlocks;
        Sprite _detail2Look;
        MaterialId _detail2Material;
        bool _detail2Blocks;
        Collider2D _collider;
        int _growth;
        GameObject _linger;
        bool _hasFoundation;
        int _animFrame = -1;
        MaterialId _telegraph = MaterialId.None;
        int _telegraphCount;
        float _overlayBurn;
        float _hungerLife;

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
            _authoredLook = TileSprite.Solid(sprite);
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
            _underlayLook = TileSprite.Solid(sprite);
            _underlayMaterial = floor == MaterialId.None ? MaterialId.Stone : floor;
            if (_renderer != null)
            {
                ApplyUnderlay();
            }
        }

        public bool HasWaterCover =>
            !HasAshCover
            && !HasIceCover
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

        public bool HasWitherCover =>
            Cover == TileCover.Wither
            || string.Equals(_coverId, "wither", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(_coverId, "cover-wither", System.StringComparison.OrdinalIgnoreCase);

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
        /// A second stamp on the same cell (Environment Details lvl 2)
        /// stacks on top of the first instead of replacing it.
        /// </summary>
        public void AuthorDetail(Sprite sprite, MaterialId material, bool blocks = false)
        {
            if (_detailLook != null || _detailMaterial != MaterialId.None)
            {
                _detail2Look = TileSprite.Solid(sprite);
                _detail2Material = material;
                _detail2Blocks = blocks;
            }
            else
            {
                _detailLook = TileSprite.Solid(sprite);
                _detailMaterial = material;
                _detailBlocks = blocks;
            }

            if (_renderer != null)
            {
                ApplyDetail();
                RefreshCollider();
            }
        }

        public void AuthorBlocks(bool blocks)
        {
            if (_detail2Look != null || _detail2Material != MaterialId.None)
            {
                _detail2Blocks = blocks;
            }
            else
            {
                _detailBlocks = blocks;
            }

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
            || CoverMaterial == MaterialId.Hearth
            || CoverMaterial == MaterialId.Lava
            || CoverMaterial == MaterialId.Fire;

        /// <summary>
        /// Coals on this cell — the walk, a cover, or a detail. They
        /// provide fire and let hunger cross. The tile underneath
        /// stays; ember does not leftover to dirt.
        /// </summary>
        public bool HasEmber =>
            !HasAshCover
            && (Material == MaterialId.Ember
                || Cover == TileCover.Ember
                || CoverMaterial == MaterialId.Ember
                || _detailMaterial == MaterialId.Ember
                || _detail2Material == MaterialId.Ember);

        /// <summary>
        /// Hunger seated in the walk itself — Floor-Fire, a hearth, lava.
        /// Rest matter. It does not burn out. Coverings and spells
        /// are what react. Ember is a Fire mark that hosts fire, not this.
        /// </summary>
        public bool IsFireFloor => VitalLaw.IsRestFire(Material);

        /// <summary>
        /// Fuel sitting on the walk — vine, oil, a plant or timber
        /// detail. The floor underneath is not this. Fire cover is
        /// the flame, not the fuel it lights.
        /// </summary>
        public bool HasOverlayFuel =>
            HasVine
            || (HasOil && !IsGeyser)
            || HasPlantishDetail
            || HasPoisonCover;

        /// <summary>
        /// Walk, a timber / plant detail, or oil this cell can leftover.
        /// A covering on stone is not this.
        /// </summary>
        public bool HasWalkFuel =>
            !HasAshCover
            && !IsFireFloor
            && (VitalLaw.CanBurn(Material)
                || HasPlantishDetail
                || (HasOil && !IsGeyser));

        /// <summary>
        /// Fuel a rest flame lights on a neighbor at rest: a plant /
        /// vine covering. Floors, walls, oil, and details stay at rest
        /// until that covering wicks into them.
        /// </summary>
        public bool HasRestCatchFuel =>
            !HasAshCover && HasPlantCover;

        /// <summary>
        /// Plant, oil, timber, or other fuel this cell can burn.
        /// Rest fire on a neighbor uses <see cref="HasRestCatchFuel"/>
        /// — only a covering catches; the walk and wall stay. A spell
        /// still takes the floor. Fire cover and rest-fire walks are
        /// the source, not catchable fuel.
        /// </summary>
        public bool HasCatchableFuel
        {
            get
            {
                if (HasAshCover)
                {
                    return false;
                }

                if (HasRestCatchFuel
                    || (VitalLaw.CanBurn(Material) && !IsFireFloor))
                {
                    return true;
                }

                if (IsFireFloor || HasFireCover)
                {
                    return false;
                }

                return Hunger > VitalLaw.HungerNeutral;
            }
        }

        /// <summary>
        /// A painted flame that stays without a spell: fire cover,
        /// rest fire in the walk, ember, or a kindled hall.
        /// </summary>
        public bool ProvidesRestFlame =>
            !HasAshCover && (HasFireCover || IsFireFloor || HasEmber || Kindled);

        /// <summary>
        /// A stood Fire · Salt column. Hunger without rest. It
        /// falls unless a source still feeds it.
        /// </summary>
        public bool IsHungerPillar =>
            IsConjured && Material == MaterialId.Fire;

        /// <summary>
        /// Kindled halls, geysers, oil, overlay fuel, ember, fire
        /// cover, or rest fire already in the walk. Those keep a
        /// fire-pillar standing.
        /// </summary>
        public bool FeedsHunger
        {
            get
            {
                if (Kindled || IsGeyser || HasOil || HasOverlayFuel || HasEmber || HasFireCover)
                {
                    return true;
                }

                return _hasFoundation && VitalLaw.IsRestFire(Foundation.Material);
            }
        }

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
            _coverLook = TileSprite.Solid(sprite);
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
        public bool IsPoisonWell { get; private set; }
        /// <summary>
        /// A vegetable body the grave has taken. Water remembers
        /// the green. More poison walks to a neighbour, the way
        /// yield walks a living plant.
        /// </summary>
        public bool IsPoisonPlant { get; private set; }
        public bool IsLightWell { get; private set; }
        public int LightWellRadius { get; private set; }
        /// <summary>
        /// Fire a spell or NPC working started. Authored torches, kindled
        /// halls, and painted cover stay still until work finds them.
        /// </summary>
        public bool LiveFire { get; private set; }
        /// <summary>
        /// Rest fire lit a covering. Spend the cover; leave the walk
        /// and wall underneath. A later spell Ignite clears this.
        /// </summary>
        public bool CoverOnlyBurn { get; private set; }
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
        /// Ice is sitting where yield was. The sheet should read as
        /// ice-wall, not a wash over the pool.
        /// </summary>
        bool IceSeatsOnWater =>
            Material == MaterialId.Water
            || (_hasFoundation && Foundation.Material == MaterialId.Water);

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

                if (_detail2Material != MaterialId.None)
                {
                    var top = MaterialCatalog.Of(_detail2Material).BurnSeconds;
                    if (top > 0f)
                    {
                        seconds = seconds > 0f ? Mathf.Min(seconds, top) : top;
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

                if (HasPoisonCover)
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
                    value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOf(MaterialId.Water));
                }

                if (HasPlantCover || HasVine)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOf(MaterialId.Plant));
                }

                if (HasIceCover)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOf(MaterialId.Ice));
                }

                value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOfCover(Cover));
                if (Cover == TileCover.None && CoverMaterial != MaterialId.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOf(CoverMaterial));
                }

                value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOfWetness(Wet));
                if (HasOil)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOf(MaterialId.Oil));
                }

                if (_detailMaterial != MaterialId.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOf(_detailMaterial));
                }

                if (_detail2Material != MaterialId.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.LeftoverOf(_detail2Material));
                }

                if (WorldMatter.TryOverlayConductivity(WorldOrigin, out var item))
                {
                    value = ChargeLaw.Combine(value, item);
                }

                return value;
            }
        }

        /// <summary>
        /// 0–10 conduct on this cell. Walk, cover, wet, a detail,
        /// and a stood item combine. An insulator wins — wood on
        /// metal breaks the path. Water on stone runs the spark.
        /// </summary>
        public int Conduct
        {
            get
            {
                var value = ChargeLaw.Of(Material);
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

                if (Cover != TileCover.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.OfCover(Cover));
                }

                if (Cover == TileCover.None && CoverMaterial != MaterialId.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(CoverMaterial));
                }

                if (Wet > 0.2f)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.OfWetness(Wet));
                }

                if (HasOil)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(MaterialId.Oil));
                }

                if (_detailMaterial != MaterialId.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(_detailMaterial));
                }

                if (_detail2Material != MaterialId.None)
                {
                    value = ChargeLaw.Combine(value, ChargeLaw.Of(_detail2Material));
                }

                if (WorldMatter.TryOverlayConduct(WorldOrigin, out var item))
                {
                    value = ChargeLaw.Combine(value, item);
                }

                return value;
            }
        }

        public bool Conducts => ChargeLaw.Conducts(Conduct);
        public bool Insulates => ChargeLaw.Insulates(Conduct);
        public bool IsCharged => Charge > ChargeLaw.LiveMin;
        public bool IsPlantish => IsPlantMaterial(Material) && !HasAshCover;
        public bool HoldsPlant =>
            !HasAshCover
            && (IsPlantish || HasPlantCover || HasPlantishDetail || HasVine || IsPoisonPlant);
        public bool IsPoisonedPlant =>
            HoldsPlant && (IsPoisonPlant || IsPoisonWell);
        public bool HasPlantishDetail =>
            !HasAshCover
            && (IsPlantMaterial(_detailMaterial) || IsPlantMaterial(_detail2Material));
        /// <summary>
        /// Fuel hunger can finish. Kindled halls, rest fire, ember,
        /// and fire cover stay. Timber walls and plant / timber
        /// floors catch once, then leftover dirt. An embered tile
        /// keeps whatever walk was already there.
        /// </summary>
        public bool HoldsBurnFuel =>
            !HasAshCover
            && !IsFireFloor
            && !HasEmber
            && !HasFireCover
            && (IsPlantish
                || HasPlantishDetail
                || HasVine
                || HasPoisonCover
                || (HasOil && !IsGeyser)
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
        /// Ember is a Fire path, not fuel. Hunger can walk across it
        /// toward the source and sit on it. The mark itself does not
        /// leftover.
        /// </summary>
        public bool ConductsFire =>
            !HasAshCover
            && (HasEmber
                || VitalLaw.ConductsFire(Material)
                || VitalLaw.ConductsFire(_detailMaterial)
                || VitalLaw.ConductsFire(_detail2Material)
                || VitalLaw.ConductsFire(CoverMaterial));

        /// <summary>
        /// 0–10 hunger on this cell. Walk, a timber / plant detail, vine,
        /// oil, and fire cover raise the grade. Rest fire in the floor
        /// stays 0 for the 7+ walk — at rest it still lights adjacent
        /// covers, not floors or walls.
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

                if (_detail2Material != MaterialId.None)
                {
                    hunger = Mathf.Max(hunger, VitalLaw.HungerOf(_detail2Material));
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

                if (HasPoisonCover)
                {
                    hunger = Mathf.Max(hunger, VitalLaw.HungerTinder);
                }

                return hunger;
            }
        }

        /// <summary>
        /// How hard this live flame may light other cells. A kindled
        /// hall, a geyser, or a lit rest-fire walk is an oil-grade
        /// source (10). A covering a spell left on that walk is the
        /// fuel — the floor stays rest. Only a strong source (7+)
        /// walks fire, onto equal-or-weaker fuel, out to its own reach.
        /// A burning plant covering still wicks adjacent wood and oil.
        /// </summary>
        public int FirePotency
        {
            get
            {
                if (Kindled || IsGeyser)
                {
                    return VitalLaw.HungerOil;
                }

                if ((IsFireFloor && LiveFire) || (HasEmber && LiveFire))
                {
                    return HasOverlayFuel || HasFireCover ? Hunger : VitalLaw.HungerOil;
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

                if (_detail2Material != MaterialId.None)
                {
                    quench = Mathf.Max(quench, VitalLaw.QuenchOf(_detail2Material));
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
            _detailLook != null
            || _detailMaterial != MaterialId.None
            || _detail2Look != null
            || _detail2Material != MaterialId.None;
        float DetailFlammability =>
            Mathf.Max(
                _detailMaterial == MaterialId.None
                    ? 0f
                    : MaterialCatalog.Of(_detailMaterial).Flammability,
                _detail2Material == MaterialId.None
                    ? 0f
                    : MaterialCatalog.Of(_detail2Material).Flammability);

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
            Material != MaterialId.Void && !IsFireFloor && !IsPlantish;

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
            if (material == MaterialId.Hearth || material == MaterialId.Lava)
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
                SealWaterForIce();
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
        /// Yield keeps its picture underneath. Ice on water uses the
        /// ice-wall face so an ice-column freeze matches ice-wall.
        /// A pit must become a walk so the drop trigger does not fire.
        /// </summary>
        void SealWaterForIce()
        {
            if (Kind != TileKind.Pit)
            {
                return;
            }

            AuthorKind(TileKind.Floor, Material == MaterialId.None ? MaterialId.Water : Material);
        }

        /// <summary>
        /// Hard water on this cell. Water becomes the same ice sheet
        /// ice-wall uses — the ice-wall face, not a UI cover tile.
        /// A dry walk takes that same cover sheen.
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
            var onWater = IceSeatsOnWater || IsDeepWater || HasWaterCover;
            _coverLook = null;
            PaintCover(TileCover.Ice);
            var face = TileAtlas.Get("wall-ice") ?? CoverCatalog.Sheen(TileCover.Ice);
            if (face != null)
            {
                _coverLook = TileSprite.Solid(face);
                if (onWater)
                {
                    _coverAlpha = 1f;
                }

                ApplyCover();
            }
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
            LightNewPlant();
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
                LightNewPlant();
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
            if (conjured && material == MaterialId.Fire)
            {
                BeginHungerLife();
            }
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
            if (material == MaterialId.Fire)
            {
                BeginHungerLife();
            }
        }

        public void BeginHungerLife()
        {
            _hungerLife = VitalLaw.FirePillarSeconds;
        }

        /// <summary>
        /// Tick a Fire · Salt column. Fed hunger stays. Unfed
        /// hunger falls when the clock is spent.
        /// </summary>
        public bool TickHungerLife(float dt)
        {
            if (!IsHungerPillar)
            {
                return false;
            }

            if (FeedsHunger)
            {
                return false;
            }

            _hungerLife -= dt;
            return _hungerLife <= 0f && RestoreFoundation();
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
            IsPoisonWell = false;
            IsPoisonPlant = false;
            IsLightWell = false;
            LightWellRadius = 0;
            _hungerLife = 0f;
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
            CoverOnlyBurn = false;
            RefreshFx();
        }

        public void Ignite(float amount, bool live = true, bool coverOnly = false)
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
                CoverOnlyBurn = false;
                RefreshFx();
                return;
            }

            var spoke = Fire > 0.05f;
            var boost = HasOil ? 1.55f : 1f;
            Fire = Mathf.Clamp01(Fire + amount * boost);
            if (amount > 0f && live && Fire > 0.05f)
            {
                LiveFire = true;
                CoverOnlyBurn = coverOnly;
            }

            if (Fire <= 0.01f)
            {
                LiveFire = false;
                CoverOnlyBurn = false;
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

            if (amount > 0f && Fire > 0.05f)
            {
                BurnPoisonToMiasma();
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

        public bool MarkPoisonWell()
        {
            if (Kind == TileKind.Door || Material == MaterialId.Void)
            {
                return false;
            }

            IsPoisonWell = true;
            IsPoisonPlant = true;
            PaintCover(TileCover.Poison);
            RefreshFx();
            return true;
        }

        public bool MarkLightWell(int radius = 2)
        {
            if (Kind == TileKind.Door || Material == MaterialId.Void)
            {
                return false;
            }

            IsLightWell = true;
            LightWellRadius = radius < 1 ? 1 : radius;
            return true;
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
        /// Floor and wall stamps stay at rest; a spell that lays this
        /// covering on hunger lights the plant, not the masonry.
        /// </summary>
        public bool LayVine()
        {
            if (Kind == TileKind.Door || Material == MaterialId.Void)
            {
                return false;
            }

            if (Kind == TileKind.Wall && !AcceptsVineOnWall)
            {
                return false;
            }

            if (IsDeepWater)
            {
                return false;
            }

            if (HasVine)
            {
                LightNewPlant(0.45f);
                return true;
            }

            PaintCover(TileCover.Vine);
            LightNewPlant(0.55f);
            RefreshFx();
            return true;
        }

        /// <summary>
        /// Stone walls stay bare. A fire wall, a kindled hall, or
        /// live hunger takes the climbing body so the covering can
        /// catch.
        /// </summary>
        bool AcceptsVineOnWall =>
            IsFireFloor || HasFireCover || Kindled || LiveFire || IsBurning;

        /// <summary>
        /// A spell laid plant on hunger — rest fire, a hall, live
        /// flame, or fire cover. The covering lights. The walk does
        /// not become a source by itself.
        /// </summary>
        bool ShouldLightNewPlant =>
            IsBurning || LiveFire || Kindled || IsFireFloor || HasFireCover;

        void LightNewPlant(float amount = 0.55f)
        {
            if (!ShouldLightNewPlant)
            {
                return;
            }

            Ignite(amount);
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
        /// Liquid poison on the walk. A living plant becomes its
        /// poison variant. Yield washes a slick; a poisoned plant
        /// needs yield on the body, or shown work, to remember itself.
        /// </summary>
        public void SlickPoison(float amount = 1f)
        {
            if (Kind == TileKind.Wall || Kind == TileKind.Door)
            {
                return;
            }

            if (HoldsPlant && !IsPoisonedPlant)
            {
                PoisonPlant();
                if (IsBurning || LiveFire || Fire > 0.05f)
                {
                    BurnPoisonToMiasma();
                }

                return;
            }

            PaintCover(TileCover.Poison);
            if (amount > 0f)
            {
                Wet = Mathf.Max(Wet, Mathf.Clamp01(amount * 0.35f));
            }

            if (IsBurning || LiveFire || Fire > 0.05f)
            {
                BurnPoisonToMiasma();
                return;
            }

            RefreshFx();
        }

        /// <summary>
        /// Hunger takes a poison slick and lifts it as foul breath.
        /// The recipe is the same join: Poison · Fire → Miasma.
        /// The cloud itself will not catch.
        /// </summary>
        public bool BurnPoisonToMiasma()
        {
            if (!HasPoisonCover && Material != MaterialId.Acid)
            {
                return false;
            }

            if (HasPoisonCover)
            {
                PaintCover(TileCover.None);
            }

            if (Material == MaterialId.Acid
                && (Kind == TileKind.Floor || Kind == TileKind.Bridge))
            {
                Reshape(new TileDef(Kind, MaterialId.Scoured));
            }

            Foul(1f);
            RefreshFx();
            return true;
        }

        /// <summary>
        /// The grave takes a standing plant. It stays a plant, but
        /// it speaks poison until yield or shown work wakes it.
        /// </summary>
        public bool PoisonPlant()
        {
            if (!HoldsPlant || IsPoisonedPlant)
            {
                return false;
            }

            IsPoisonPlant = true;
            PaintCover(TileCover.Poison);
            RefreshFx();
            return true;
        }

        /// <summary>
        /// Withhold a vegetable body. Living green dies. Leftover
        /// dirt (or the old walk) keeps a wither covering that
        /// speaks Death, so the grave can be drawn.
        /// </summary>
        public bool WitherPlant()
        {
            var changed = false;
            if (IsPoisonWell)
            {
                IsPoisonWell = false;
                changed = true;
            }

            if (IsPoisonPlant)
            {
                IsPoisonPlant = false;
                changed = true;
            }

            if (HasVine || HasPlantCover)
            {
                PaintCover(TileCover.Wither);
                changed = true;
            }

            if (HasPlantishDetail)
            {
                if (IsPlantMaterial(_detailMaterial))
                {
                    _detailMaterial = MaterialId.None;
                }

                if (IsPlantMaterial(_detail2Material))
                {
                    _detail2Material = MaterialId.None;
                }

                changed = true;
            }

            if (IsConjured && WorldWork.IsPlantBody(Material))
            {
                RestoreFoundation();
                PaintCover(TileCover.Wither);
                return true;
            }

            if (IsPlantish)
            {
                var leftover = CoverCatalog.LeftoverFloor(Material);
                if (leftover == MaterialId.None)
                {
                    leftover = MaterialId.Dirt;
                }

                if (Kind == TileKind.Wall)
                {
                    Reshape(new TileDef(TileKind.Floor, leftover));
                }
                else
                {
                    Reshape(new TileDef(Kind == TileKind.None ? TileKind.Floor : Kind, leftover));
                }

                PaintCover(TileCover.Wither);
                _growth = 0;
                return true;
            }

            if (changed && !HasWitherCover && Cover != TileCover.Poison)
            {
                PaintCover(TileCover.Wither);
            }

            if (changed)
            {
                _growth = 0;
                RefreshFx();
            }

            return changed;
        }

        /// <summary>
        /// Shown or living plant-work remembers a blighted body.
        /// Wither, poison slick, and foul breath lift. A weeping
        /// poison tree wakes as a living plant again.
        /// </summary>
        public bool RestoreNature()
        {
            var wasPlant = HoldsPlant || IsPoisonWell || IsPoisonPlant;
            var changed = false;
            if (IsPoisonWell)
            {
                IsPoisonWell = false;
                changed = true;
            }

            if (IsPoisonPlant)
            {
                IsPoisonPlant = false;
                changed = true;
            }

            if (Miasma > 0.05f)
            {
                Miasma = 0f;
                if (Cover == TileCover.Miasma)
                {
                    PaintCover(TileCover.None);
                }

                changed = true;
            }

            if (HasPoisonCover)
            {
                PaintCover(TileCover.None);
                Wet = Mathf.Max(0f, Wet - 0.35f);
                changed = true;
            }

            if (HasWitherCover)
            {
                PaintCover(TileCover.None);
                PlacePlantCover();
                RefreshFx();
                return true;
            }

            if (changed && wasPlant && !HasPlantCover && !HasVine)
            {
                PlacePlantCover();
            }

            if (changed && IsPlantish)
            {
                Grow(1);
            }

            if (changed)
            {
                RefreshFx();
            }

            return changed;
        }

        public void Dry(float amount)
        {
            Wet = Mathf.Max(0f, Wet - amount);
            RefreshFx();
        }

        public void ChargeAt(float amount)
        {
            if (amount > 0f && Insulates)
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

        public bool Annihilate(bool breakWards = false)
        {
            if (Material == MaterialId.Void)
            {
                return false;
            }

            if (!breakWards && MatterLaw.ResistsMagic(Material))
            {
                return false;
            }

            Oil = 0f;
            IsGeyser = false;
            IsPoisonWell = false;
            IsPoisonPlant = false;
            IsLightWell = false;
            LightWellRadius = 0;
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
            if (!HasOverlayFuel && !HasCatchableFuel)
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
            CoverOnlyBurn = false;
            if (Kindled)
            {
                KeepKindled();
                return;
            }

            if (IsFireFloor || HasEmber || HasFireCover)
            {
                Fire = 0f;
                RefreshFx();
            }
        }

        /// <summary>
        /// Ember is coals, not leftover dirt. Restore the ember mark
        /// if overlay fuel or a passing flame cleared the sheen.
        /// The walk underneath stays whatever it already was.
        /// </summary>
        void KeepEmber()
        {
            if (Material == MaterialId.Ember || _detailMaterial == MaterialId.Ember || _detail2Material == MaterialId.Ember)
            {
                return;
            }

            if (Cover == TileCover.Ember || CoverMaterial == MaterialId.Ember)
            {
                return;
            }

            PaintCover(TileCover.Ember);
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
        /// Plant or timber under a staying fire cover leftovers to
        /// dirt. The covering remains.
        /// </summary>
        void LeftoverFuelWalk()
        {
            if (IsFireFloor || HasEmber)
            {
                return;
            }

            if (!VitalLaw.CanBurn(Material) && !IsPlantish)
            {
                return;
            }

            var leftover = Kind == TileKind.Wall
                ? CoverCatalog.RestAfterBurn(Material)
                : CoverCatalog.LeftoverFloor(Material);
            if (leftover == MaterialId.None || leftover == Material)
            {
                return;
            }

            Reshape(Kind == TileKind.Wall
                ? new TileDef(TileKind.Floor, leftover)
                : new TileDef(Kind, leftover));
        }

        /// <summary>
        /// Hunger finishes the fuel. A plant or timber floor swaps
        /// stamp and look to leftover dirt — it does not draw ash
        /// over the tile you placed. A timber or plant wall burns
        /// for its clock, then falls to that leftover dirt so a key
        /// behind it can be reached. Floor-Fire, ember, and fire
        /// cover stay. Ember keeps the walk that was already there.
        /// It only spends what sat on it. Covers and spells may still
        /// sit on the leftover.
        /// </summary>
        public void BurnOut()
        {
            if (CoverOnlyBurn)
            {
                CoverOnlyBurn = false;
                _overlayBurn = 0f;
                if (HasPlantCover)
                {
                    PaintCover(TileCover.None);
                }

                Fire = 0f;
                LiveFire = false;
                RefreshFx();
                return;
            }

            if (IsFireFloor || HasEmber || HasFireCover)
            {
                SpendOverlayFuel();
                if (HasFireCover && !IsFireFloor && !HasEmber)
                {
                    LeftoverFuelWalk();
                }

                if (HasEmber)
                {
                    KeepEmber();
                }

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

        /// <summary>
        /// A WorldDoor leaf covers this cell. Open a painted door, or
        /// drop the wall collider so the object door is the passage.
        /// </summary>
        public void YieldToLeaf()
        {
            if (Kind == TileKind.Door)
            {
                OpenDoor();
                return;
            }

            if (!BlocksTravel)
            {
                return;
            }

            LeafOpen = true;
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
                    StopLook(_renderer);
                    _renderer.sprite = TileSprite.Solid(_authoredLook);
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
                        _renderer.sprite = _telegraph != MaterialId.None
                            ? SpriteFactory.Floor(ShownMaterial, Coord.x, Coord.y, _animFrame < 0 ? 0 : _animFrame)
                            : SpriteFactory.Bridge(Material, Coord.x, Coord.y);
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

                PlayConjuredLook();
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

        void PlayConjuredLook()
        {
            if (_renderer == null)
            {
                return;
            }

            string[] ids;
            switch (Kind)
            {
                case TileKind.Wall:
                    ids = RaisedAs == RaisedForm.Pillar
                        ? LookIds.Column(Material)
                        : LookIds.Wall(Material);
                    break;
                case TileKind.Bridge:
                    ids = LookIds.Bridge(Material);
                    break;
                case TileKind.Door:
                    ids = LookIds.Door(false, true);
                    break;
                case TileKind.Pit:
                    ids = Material == MaterialId.Water ? LookIds.Floor(MaterialId.Water) : LookIds.Pit();
                    break;
                default:
                    ids = LookIds.Floor(ShownMaterial);
                    break;
            }

            if (LookLibrary.TryAuthoredClip(ids, out var frames, out var id) && frames != null && frames.Length > 1)
            {
                SpriteAnim.On(gameObject, _renderer).Play(frames, LookLibrary.FpsOf(id), true, id);
                return;
            }

            StopLook(_renderer);
        }

        static void StopLook(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            var anim = renderer.GetComponent<SpriteAnim>();
            anim?.Stop();
        }

        void PlayFxLook(SpriteRenderer fx, string id, Color color)
        {
            fx.sprite = SpriteFactory.Named(id);
            fx.color = color;
            SpriteAnim.On(fx.gameObject, fx).Play(id, LookLibrary.FpsOf(id));
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
            _detail2Look = null;
            _detail2Material = MaterialId.None;
            _detail2Blocks = false;
            if (_detail != null)
            {
                _detail.enabled = false;
                _detail.sprite = null;
            }

            if (_detail2 != null)
            {
                _detail2.enabled = false;
                _detail2.sprite = null;
            }
        }

        void ApplyDetail()
        {
            if (_renderer == null)
            {
                return;
            }

            ApplyDetailSlot(_detailLook, _detailMaterial, ref _detail, 2, "TileDetail");
            ApplyDetailSlot(_detail2Look, _detail2Material, ref _detail2, 3, "TileDetail2");
        }

        void ApplyDetailSlot(Sprite look, MaterialId material, ref SpriteRenderer view, int orderAdd, string name)
        {
            if (look == null)
            {
                if (view != null)
                {
                    view.enabled = false;
                }

                return;
            }

            if (view == null)
            {
                var child = new GameObject(name);
                child.transform.SetParent(transform, false);
                view = child.AddComponent<SpriteRenderer>();
            }

            view.sprite = look;
            view.sortingOrder = _renderer.sortingOrder + orderAdd;
            view.transform.localScale = material == MaterialId.Ash
                ? new Vector3(0.7f, 0.7f, 1f)
                : Vector3.one;
            view.enabled = true;
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
                var sheenId = CoverCatalog.SheenId(Cover);
                if (LookLibrary.HasAuthoredClip(sheenId))
                {
                    SpriteAnim.On(view.gameObject, view).Play(sheenId, LookLibrary.FpsOf(sheenId));
                }
                else
                {
                    StopLook(view);
                }
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
                StopLook(_cover);
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
            if (Cover == TileCover.Ice && IceSeatsOnWater)
            {
                return alpha;
            }

            if (_authoredLook == null)
            {
                return alpha;
            }

            // A painted walk tile must stay visible. Opaque pack covers
            // (hell lava, ice sheets) used to hide that sprite in Play.
            // Ice that replaced water is the ice-wall face — it should
            // hide the pool, the way ice-wall on water does.
            if (Cover == TileCover.Ice || Cover == TileCover.Ash || Cover == TileCover.Mud || Cover == TileCover.Wither)
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

                if (string.Equals(_coverId, "ember", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(_coverId, "cover-ember", System.StringComparison.OrdinalIgnoreCase))
                {
                    return SpriteFactory.Named("fx-ember");
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
                if (LookLibrary.TryAuthored(named, out var authored) && authored != null)
                {
                    return authored;
                }

                if (LookLibrary.TryAuthored(_coverId, out authored) && authored != null)
                {
                    return authored;
                }

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

                // Stamped / painted tilesets are the picture. The old
                // brown pit bars sat on dirt and read as random lines.
                if (_authoredLook != null)
                {
                    if (_overlay != null)
                    {
                        _overlay.enabled = false;
                    }

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

            if (Fire < 0.08f && Miasma < 0.18f && Fog < 0.18f && Wet < 0.18f && Charge < 0.18f && Oil < 0.18f && !IsGeyser && !HasPoisonCover && !HasEmber && _growth < 1)
            {
                if (_fx != null)
                {
                    StopLook(_fx);
                    _fx.enabled = false;
                }

                return;
            }

            var fx = EnsureFx();
            fx.enabled = true;
            fx.sortingOrder = _renderer.sortingOrder + 3;
            if (Fire > 0.12f)
            {
                PlayFxLook(fx, "tile-fire", new Color(1f, 0.55f, 0.12f, 0.35f + Fire * 0.5f));
            }
            else if (HasEmber)
            {
                PlayFxLook(fx, "fx-ember", new Color(0.95f, 0.42f, 0.1f, 0.28f));
            }
            else if (Miasma > 0.18f)
            {
                PlayFxLook(fx, "tile-poison", new Color(0.42f, 0.88f, 0.2f, 0.28f + Miasma * 0.45f));
            }
            else if (HasPoisonCover)
            {
                PlayFxLook(fx, "tile-wet", new Color(0.28f, 0.62f, 0.12f, 0.42f));
            }
            else if (Fog > 0.18f)
            {
                PlayFxLook(fx, "tile-fog", new Color(0.62f, 0.66f, 0.7f, 0.24f + Fog * 0.4f));
            }
            else if (Charge > 0.18f)
            {
                PlayFxLook(fx, "tile-charge", new Color(0.75f, 0.9f, 1f, 0.35f + Charge * 0.45f));
            }
            else if (Wet > 0.18f)
            {
                PlayFxLook(fx, "tile-wet", new Color(0.35f, 0.65f, 1f, 0.22f + Wet * 0.35f));
            }
            else if (Oil > 0.18f || IsGeyser)
            {
                PlayFxLook(fx, "tile-wet", IsGeyser
                    ? new Color(0.32f, 0.2f, 0.06f, 0.4f + Oil * 0.35f)
                    : new Color(0.18f, 0.12f, 0.05f, 0.28f + Oil * 0.4f));
            }
            else
            {
                PlayFxLook(fx, "tile-grow", new Color(0.35f, 0.72f, 0.28f, 0.2f + _growth * 0.12f));
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

        public Collider2D TravelCollider => _collider;

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

                AdeptAvatar.Find()?.NoteUserBuiltCollider(this);
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
            AdeptAvatar.Find()?.NoteUserBuiltCollider(this);
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
                case TileCover.Ember:
                    return ElementLook.Of(ElementFamily.Fire);
                case TileCover.Lightning:
                    return ElementLook.Of(ElementFamily.Lightning);
                case TileCover.Vine:
                    return ElementLook.Of(ElementFamily.Plant);
                case TileCover.Wither:
                    return ElementLook.Of(ElementFamily.Poison);
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
            var lookAnim = GetComponent<SpriteAnim>();
            var authoredClip = lookAnim != null && LookLibrary.HasAuthoredClip(lookAnim.Clip);
            if (!keepAuthored &&
                !authoredClip &&
                SpriteFactory.Animates(ShownMaterial) &&
                (Kind == TileKind.Floor || (Kind == TileKind.Bridge && (_telegraph != MaterialId.None || IsConjured)) || (Kind == TileKind.Pit && Material == MaterialId.Water)))
            {
                _renderer.sprite = SpriteFactory.Floor(ShownMaterial, Coord.x, Coord.y, frame);
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
