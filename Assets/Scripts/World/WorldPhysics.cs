using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// How a finished spell occupies the room: a corridor for a shot,
    /// a disk for a spread or a pillar. Locks, hanging veils, and
    /// fragile matter are tested against this volume — not a pin click.
    /// </summary>
    public readonly struct SpellSweep
    {
        public SpellSweep(
            SpellId spell,
            SpellShape shape,
            Vector3 origin,
            Vector3 from,
            Vector3 to,
            float width,
            List<Vector2Int> cells)
        {
            Spell = spell;
            Shape = shape;
            Origin = origin;
            From = from;
            To = to;
            Width = width;
            Cells = cells ?? new List<Vector2Int>();
        }

        public SpellId Spell { get; }
        public SpellShape Shape { get; }
        public Vector3 Origin { get; }
        public Vector3 From { get; }
        public Vector3 To { get; }
        public float Width { get; }
        public List<Vector2Int> Cells { get; }

        public bool Touches(Vector3 point)
        {
            return CellVolume.SegmentDistance(From, To, point) <= Width + 0.35f;
        }

        public bool Crosses(ISpellVolume volume)
        {
            return volume != null && volume.Crosses(From, To, Width);
        }
    }

    /// <summary>
    /// Game-law physics. Not rigidbodies — a corridor, a disk, and the
    /// four roots deciding what yields. Air sent down a lane clears
    /// every fog cell it crosses. Fire that kisses any ice cell melts
    /// the cage. A wall that is the lock itself does not hide it.
    /// </summary>
    public static class WorldPhysics
    {
        public const float BodyRadius = 0.55f;
        public const float ShotWidth = 0.85f;
        public const float GustWidth = 1.35f;
        public const float GaleWidth = 1.85f;
        public const float StormWidth = 2.6f;

        public static SpellSweep Build(
            WorldGrid grid,
            SpellId spell,
            SpellShape shape,
            Vector3 origin,
            Vector3 from,
            Vector3 to,
            float potency = 1f)
        {
            if (shape == SpellShape.None)
            {
                shape = ShapeOf(spell);
            }

            Vector3 start;
            Vector3 stop;
            if (WorldWork.NeedsSpan(spell))
            {
                start = from;
                stop = to;
            }
            else if (shape == SpellShape.Spread || shape == SpellShape.Self)
            {
                start = origin;
                stop = origin;
            }
            else if (shape == SpellShape.Pillar || shape == SpellShape.Remote)
            {
                start = to;
                stop = to;
            }
            else
            {
                start = origin;
                stop = to;
            }

            var width = WidthOf(spell, shape, potency);
            return new SpellSweep(spell, shape, origin, start, stop, width, CellsAlong(grid, start, stop, width));
        }

        public static SpellShape ShapeOf(SpellId spell)
        {
            if (spell == SpellId.Monsoon || spell == SpellId.Rain || spell == SpellId.StormCall
                || WorldWork.IsOilCover(spell))
            {
                return SpellShape.Remote;
            }

            if (WorldWork.IsSpreadWork(spell) || WorldWork.IsSightVeil(spell) || WorldWork.IsPoisonVeil(spell))
            {
                return SpellShape.Spread;
            }

            if (WorldWork.NeedsSpan(spell) || WorldWork.IsSinglePillar(spell))
            {
                return SpellShape.Pillar;
            }

            if (WorldWork.IsHop(spell) || WorldWork.IsFlight(spell) || SpellVerb.Of(spell).Target == SpellTarget.Self)
            {
                return SpellShape.Self;
            }

            if (SpellVerb.Of(spell).Target == SpellTarget.Area)
            {
                return SpellShape.Spread;
            }

            return SpellShape.Shot;
        }

        public static float WidthOf(SpellId spell, SpellShape shape, float potency = 1f)
        {
            var scale = potency <= 0f ? 1f : potency;
            if (WorldWork.IsAirWork(spell))
            {
                if (spell == SpellId.StormCall)
                {
                    return StormWidth * scale;
                }

                return (spell == SpellId.Gale || spell == SpellId.Push ? GaleWidth : GustWidth) * scale;
            }

            if (shape == SpellShape.Spread || shape == SpellShape.Self)
            {
                return Mathf.Max(SpellVerb.RadiusOf(spell, shape, scale), 1.6f);
            }

            if (shape == SpellShape.Pillar || shape == SpellShape.Remote)
            {
                return Mathf.Max(SpellVerb.RadiusOf(spell, shape, scale), 1.15f);
            }

            if (WorldWork.IsShatterWork(spell))
            {
                return 1.6f * scale;
            }

            if (WorldWork.ClearsVeils(spell) || WorldWork.IsFireWork(spell) || WorldWork.IsWaterWork(spell))
            {
                return 1.15f * scale;
            }

            return ShotWidth * scale;
        }

        public static bool SweepsPath(SpellId spell, SpellShape shape)
        {
            if (shape == SpellShape.Spread || shape == SpellShape.Self || shape == SpellShape.Remote)
            {
                return true;
            }

            return WorldWork.IsAirWork(spell)
                || WorldWork.ClearsVeils(spell)
                || WorldWork.IsFireWork(spell)
                || WorldWork.IsWaterWork(spell)
                || WorldWork.IsShatterWork(spell)
                || WorldWork.IsBoulderWork(spell)
                || WorldWork.IsLightWork(spell)
                || WorldWork.IsVineWork(spell);
        }

        public static ISpellVolume VolumeOf(ISpellLock encounter)
        {
            if (encounter is ISpellVolume volume)
            {
                return volume;
            }

            return encounter != null ? new PointVolume(encounter.WorldPosition, BodyRadius) : null;
        }

        public static float Distance(ISpellLock encounter, Vector3 point)
        {
            var volume = VolumeOf(encounter);
            return volume != null ? volume.DistanceTo(point) : float.MaxValue;
        }

        public static Vector3 ClosestPoint(ISpellLock encounter, Vector3 point)
        {
            var volume = VolumeOf(encounter);
            return volume != null ? volume.ClosestPoint(point) : encounter.WorldPosition;
        }

        public static bool Touches(ISpellLock encounter, Vector3 point, float radius)
        {
            var volume = VolumeOf(encounter);
            return volume != null && volume.Touches(point, radius);
        }

        public static bool Crosses(ISpellLock encounter, Vector3 from, Vector3 to, float width)
        {
            var volume = VolumeOf(encounter);
            return volume != null && volume.Crosses(from, to, width);
        }

        public static bool Occluded(WorldGrid grid, SpellId spell, Vector3 from, ISpellLock encounter)
        {
            if (encounter == null || !WorldWork.StopsOnWalls(spell))
            {
                return false;
            }

            var contact = ClosestPoint(encounter, from);
            var volume = VolumeOf(encounter);
            return WorldWork.HasWallBetween(grid, from, contact, cell => volume != null && volume.OccupiesCell(cell));
        }

        public static void Collect(
            IReadOnlyList<ISpellLock> locks,
            SpellSweep sweep,
            List<ISpellLock> hits,
            SpellVerb verb,
            WorldGrid grid)
        {
            if (locks == null || hits == null)
            {
                return;
            }

            for (var i = 0; i < locks.Count; i++)
            {
                var encounter = locks[i];
                if (!CanTake(encounter, verb) || Occluded(grid, sweep.Spell, sweep.Origin, encounter))
                {
                    continue;
                }

                if (sweep.Crosses(VolumeOf(encounter)) && !hits.Contains(encounter))
                {
                    hits.Add(encounter);
                }
            }
        }

        public static ISpellLock Nearest(
            IReadOnlyList<ISpellLock> locks,
            Vector3 point,
            float radius,
            SpellVerb verb,
            WorldGrid grid,
            SpellId spell,
            Vector3 origin)
        {
            ISpellLock best = null;
            var bestDistance = Mathf.Max(0.2f, radius);
            if (locks == null)
            {
                return null;
            }

            for (var i = 0; i < locks.Count; i++)
            {
                var encounter = locks[i];
                if (!CanTake(encounter, verb) || Occluded(grid, spell, origin, encounter))
                {
                    continue;
                }

                var distance = Distance(encounter, point);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = encounter;
                }
            }

            return best;
        }

        public static ISpellLock FirstAlong(
            IReadOnlyList<ISpellLock> locks,
            Vector3 from,
            Vector3 to,
            float width,
            WorldGrid grid,
            SpellId spell)
        {
            ISpellLock best = null;
            var bestAlong = float.MaxValue;
            if (locks == null)
            {
                return null;
            }

            var a = (Vector2)from;
            var span = (Vector2)to - a;
            var lengthSq = span.sqrMagnitude;
            for (var i = 0; i < locks.Count; i++)
            {
                var encounter = locks[i];
                if (!LockAlive(encounter) || Occluded(grid, spell, from, encounter))
                {
                    continue;
                }

                if (!Crosses(encounter, from, to, width))
                {
                    continue;
                }

                var contact = (Vector2)ClosestPoint(encounter, from);
                var t = lengthSq < 0.0001f ? 0f : Mathf.Clamp01(Vector2.Dot(contact - a, span) / lengthSq);
                if (t < bestAlong)
                {
                    bestAlong = t;
                    best = encounter;
                }
            }

            return best;
        }

        public static Essence MatterOf(SpellId spell)
        {
            if (WorldWork.IsAirWork(spell))
            {
                return Essence.Air;
            }

            if (WorldWork.IsFireWork(spell) || WorldWork.IsLightWork(spell))
            {
                return Essence.Fire;
            }

            if (WorldWork.IsWaterWork(spell) || WorldWork.FreezesWater(spell))
            {
                return Essence.Water;
            }

            if (WorldWork.IsShatterWork(spell) || WorldWork.IsBoulderWork(spell) || WorldWork.IsPillar(spell)
                || spell == SpellId.DirtToss)
            {
                return Essence.Earth;
            }

            return ElementalLaw.Of(SpellVerb.Of(spell).Status);
        }

        public static bool UnmakesMatter(SpellId spell, Essence matter)
        {
            if (spell == SpellId.None || matter == Essence.None)
            {
                return false;
            }

            if (matter == Essence.Poison)
            {
                return WorldWork.IsAirWork(spell) || WorldWork.IsWaterWork(spell);
            }

            if (matter == Essence.Air)
            {
                return WorldWork.ClearsVeils(spell);
            }

            if (matter == Essence.Water)
            {
                return MatterLaw.HeatOf(spell) >= Heat.Fire;
            }

            if (matter == Essence.Fire)
            {
                return WorldWork.IsWaterWork(spell) || spell == SpellId.Smother || spell == SpellId.Snuff;
            }

            if (matter == Essence.Earth || matter == Essence.Physical)
            {
                return WorldWork.IsShatterWork(spell)
                    || WorldWork.IsBoulderWork(spell)
                    || WorldWork.IsWaterWork(spell)
                    || MatterLaw.IsMeltWork(spell);
            }

            return ElementalLaw.Beats(MatterOf(spell), matter);
        }

        public static string Apply(WorldGrid grid, SpellSweep sweep)
        {
            if (grid == null || sweep.Spell == SpellId.None)
            {
                return string.Empty;
            }

            var notes = new List<string>(3);
            var tileNote = ReactAlong(grid, sweep);
            if (!string.IsNullOrEmpty(tileNote))
            {
                notes.Add(tileNote);
            }

            var cleared = VeilField.ClearAlong(grid, sweep);
            if (cleared > 0)
            {
                notes.Add(WorldWork.IsLightWork(sweep.Spell)
                    ? "Light lifts the hanging veil."
                    : WorldWork.IsAirWork(sweep.Spell)
                        ? "Breath tears the hanging veil."
                        : "Hunger eats the hanging veil.");
            }

            var smashed = WorldMatter.Smash(sweep);
            if (!string.IsNullOrEmpty(smashed))
            {
                notes.Add(smashed);
            }

            return FirstFilled(notes);
        }

        public static string ReactAlong(WorldGrid grid, SpellSweep sweep)
        {
            if (grid == null || sweep.Cells == null)
            {
                return string.Empty;
            }

            var fog = 0;
            var poison = 0;
            var washed = 0;
            var scoured = 0;
            var burned = 0;
            var melted = 0;
            var detonated = 0;
            var meltMatter = MaterialId.None;
            var resisted = MaterialId.None;
            for (var i = 0; i < sweep.Cells.Count; i++)
            {
                var tile = grid.Get(sweep.Cells[i]);
                if (tile == null)
                {
                    continue;
                }

                var hadFog = tile.HasFog;
                var hadMiasma = tile.HasMiasma;
                var hadPoison = tile.IsPoisonWater;
                var burning = tile.IsBurning;
                var before = tile.Material;
                if (MatterLaw.ResistsMagic(before)
                    && (MatterLaw.IsMeltWork(sweep.Spell)
                        || WorldWork.IsShatterWork(sweep.Spell)
                        || WorldWork.IsBoulderWork(sweep.Spell)
                        || WorldWork.IsFireWork(sweep.Spell)
                        || WorldWork.IsWaterWork(sweep.Spell)))
                {
                    resisted = before;
                }
                if (WorldWork.IsFireWork(sweep.Spell))
                {
                    // The spell volume lights whatever it hits, including
                    // neutral stone. Neighbor spread still refuses those.
                    var wick = tile.HasOil || tile.Material == MaterialId.Oil || tile.HasVine;
                    tile.Ignite(wick ? 1.4f : 0.85f);
                    if (wick && tile.IsConjured && tile.Material == MaterialId.Oil)
                    {
                        DetonateOil(grid, tile.Coord);
                        detonated++;
                    }
                }

                if (WorldWork.IsWaterWork(sweep.Spell))
                {
                    tile.Drench(1f);
                    if (hadPoison && !tile.IsPoisonWater)
                    {
                        washed++;
                    }
                }

                if (tile.MeltWith(sweep.Spell))
                {
                    melted++;
                    if (meltMatter == MaterialId.None)
                    {
                        meltMatter = before;
                    }
                }

                if (tile.Vent(sweep.Spell))
                {
                    if (hadFog && !tile.HasFog)
                    {
                        fog++;
                    }

                    if (hadMiasma && !tile.HasMiasma)
                    {
                        poison++;
                    }

                    if (hadPoison && tile.Material == MaterialId.Scoured)
                    {
                        scoured++;
                    }
                }

                if (WorldWork.IsFireWork(sweep.Spell) && !burning && tile.Fire > 0.12f)
                {
                    burned++;
                }
            }

            if (poison > 0)
            {
                return "Breath finds the foul tile and takes it.";
            }

            if (washed > 0)
            {
                return "Yield washes the poison from the tile.";
            }

            if (fog > 0)
            {
                return WorldWork.IsLightWork(sweep.Spell)
                    ? "Light lifts the hanging veil from the floor."
                    : "The hanging veil leaves the tiles you sent through.";
            }

            if (scoured > 0)
            {
                return "Yield washes the poison from the tile.";
            }

            if (melted > 0)
            {
                return MatterLaw.MeltNote(meltMatter);
            }

            if (resisted != MaterialId.None)
            {
                return MatterLaw.ResistNote(resisted);
            }

            if (detonated > 0)
            {
                return "The stood wick takes a fire sentence. The column becomes a bomb.";
            }

            if (burned > 0)
            {
                return "Hunger finds the floor.";
            }

            return string.Empty;
        }

        public static bool AuraAt(WorldGrid grid, Vector3 world, out VeilKind kind)
        {
            var field = VeilKind.None;
            var covered = VeilField.Covering(world, out field);
            var tile = grid != null ? grid.TileAtWorld(world) : null;
            kind = DominantAura(field, tile != null && tile.HasMiasma, tile != null && tile.HasFog);
            return kind != VeilKind.None || covered;
        }

        /// <summary>
        /// Miasma is a cloud: the tile, a neighbour, or a hanging veil.
        /// Liquid poison is the cell underfoot — that is not this.
        /// </summary>
        public static bool MiasmaCloudAt(WorldGrid grid, Vector3 world)
        {
            if (VeilField.Covering(world, out var field) && field == VeilKind.Poison)
            {
                return true;
            }

            if (grid == null)
            {
                return false;
            }

            var tiles = grid.TilesInRadius(world, 1f);
            for (var i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] != null && tiles[i].HasMiasma)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A Darkness or poison field wins over leftover cloak on the
        /// tile. Cloak is how fog and darkness both stain the floor;
        /// the field is what the room is actually doing.
        /// </summary>
        public static VeilKind DominantAura(VeilKind field, bool hasMiasma, bool hasFog)
        {
            if (field == VeilKind.Darkness || field == VeilKind.Poison)
            {
                return field;
            }

            if (hasMiasma)
            {
                return VeilKind.Poison;
            }

            if (hasFog)
            {
                return VeilKind.Fog;
            }

            return field;
        }

        public static int BlowFog(IReadOnlyList<ISpellLock> locks, SpellSweep sweep)
        {
            if (locks == null || !WorldWork.IsAirWork(sweep.Spell))
            {
                return 0;
            }

            var cleared = 0;
            for (var i = 0; i < locks.Count; i++)
            {
                if (locks[i] is not RoomFog fog || fog.Resolved)
                {
                    continue;
                }

                if (!fog.Crosses(sweep.From, sweep.To, sweep.Width))
                {
                    continue;
                }

                cleared += fog.BlowAlong(sweep.From, sweep.To, sweep.Width, sweep.Spell);
            }

            return cleared;
        }

        static void DetonateOil(WorldGrid grid, Vector2Int origin)
        {
            if (grid == null)
            {
                return;
            }

            var cells = WorldWork.Disk(origin, 2);
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile == null || MatterLaw.ResistsMagic(tile.Material))
                {
                    continue;
                }

                tile.SlickOil(0.85f);
                tile.Ignite(1.2f);
                if (tile.IsConjured && tile.Material == MaterialId.Oil && tile.Coord != origin)
                {
                    tile.RestoreFoundation();
                }
            }

            var core = grid.Get(origin);
            if (core != null && core.IsConjured && core.Material == MaterialId.Oil)
            {
                core.RestoreFoundation();
            }
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            var mid = new Vector3(4.5f, 2.5f, 0f);
            if (Mathf.Abs(CellVolume.SegmentDistance(new Vector3(0f, 2.5f, 0f), new Vector3(8f, 2.5f, 0f), mid)) > 0.05f)
            {
                broken.Add("A point on a corridor must sit on the corridor");
            }

            var off = new Vector3(4.5f, 4.5f, 0f);
            if (Mathf.Abs(CellVolume.SegmentDistance(new Vector3(0f, 2.5f, 0f), new Vector3(8f, 2.5f, 0f), off) - 2f) > 0.05f)
            {
                broken.Add("A point two tiles off a corridor must read two tiles");
            }

            var fog = new List<Vector2Int>
            {
                new(7, 1), new(7, 2), new(7, 3), new(6, 1), new(8, 1)
            };
            var origin = WorldGrid.Center(12, 12);
            if (!CellVolume.Crosses(new Vector3(7.5f, 12.5f, 0f), new Vector3(7.5f, 1.5f, 0f), GustWidth, origin, fog))
            {
                broken.Add("Gust down a lane must cross a fog bank that occupies the lane");
            }

            if (CellVolume.Crosses(new Vector3(7.5f, 12.5f, 0f), new Vector3(7.5f, 8.5f, 0f), ShotWidth, origin, fog))
            {
                broken.Add("A short shot that stops above the fog must not claim the bank");
            }

            if (WidthOf(SpellId.Gust, SpellShape.Shot) < WidthOf(SpellId.Fireball, SpellShape.Shot))
            {
                broken.Add("Breath sent must be wider than a fire-shot");
            }

            if (!SweepsPath(SpellId.Gust, SpellShape.Shot) || !SweepsPath(SpellId.Gale, SpellShape.Shot))
            {
                broken.Add("Air work must sweep a path");
            }

            if (!UnmakesMatter(SpellId.Gust, Essence.Poison)
                || !UnmakesMatter(SpellId.Douse, Essence.Poison)
                || UnmakesMatter(SpellId.Fireball, Essence.Poison)
                || !UnmakesMatter(SpellId.Fireball, Essence.Water))
            {
                broken.Add("Air must yield miasma, yield must wash poison, and hunger must yield ice");
            }

            if (UnmakesMatter(SpellId.Gust, Essence.Earth))
            {
                broken.Add("Breath must not shatter rest");
            }

            if (SpellVerb.Of(SpellId.Gust).Tiles != TileVerb.Vent
                || SpellVerb.Of(SpellId.Gale).Tiles != TileVerb.Vent)
            {
                broken.Add("Gust and Gale must vent the tiles they cross");
            }

            if (SpellVerb.Of(SpellId.Fog).Tiles != TileVerb.Cloak
                || SpellVerb.Of(SpellId.Blight).Tiles != TileVerb.Foul)
            {
                broken.Add("Fog must cloak tiles and Blight must foul them");
            }

            if (WorldWork.IsOilWork(SpellId.OilPillar)
                || WorldWork.IsFireWork(SpellId.OilPillar)
                || SpellVerb.Of(SpellId.OilPillar).Tiles != TileVerb.None)
            {
                broken.Add("Oil-pillar is a stood wick. A later fire sentence makes it a bomb");
            }

            if (!WorldWork.IsOilWork(SpellId.OilShot) || WorldWork.IsFireWork(SpellId.OilShot))
            {
                broken.Add("Oil shot must slick and grow fire, not detonate a wick");
            }

            if (!WorldWork.IsOilWork(SpellId.OilPuddle)
                || !WorldWork.IsOilWork(SpellId.OilGeyser)
                || !WorldWork.IsOilWork(SpellId.OilSlick)
                || WorldWork.IsFireWork(SpellId.OilPuddle)
                || WorldWork.IsFireWork(SpellId.OilGeyser)
                || WorldWork.IsFireWork(SpellId.OilSlick)
                || ShapeOf(SpellId.OilPuddle) != SpellShape.Remote
                || ShapeOf(SpellId.OilGeyser) != SpellShape.Remote
                || ShapeOf(SpellId.OilSlick) != SpellShape.Remote)
            {
                broken.Add("Oil puddle, geyser, and slick must lay fuel at a point, not as hunger");
            }

            if (SpellVerb.Of(SpellId.OilPuddle).Tiles != TileVerb.Slick
                || SpellVerb.Of(SpellId.OilGeyser).Tiles != TileVerb.Slick
                || SpellVerb.Of(SpellId.OilSlick).Radius < 3.5f)
            {
                broken.Add("Oil puddle and geyser slick the floor; a slick must cover a wide radius");
            }

            if (!FireRunsFaster(WorldSim.OilFireRun, WorldSim.DryFireRun, WorldSim.VineFireRun))
            {
                broken.Add("Oil must run faster than wood, wood faster than plant");
            }

            var oil = MaterialCatalog.Of(MaterialId.Oil);
            var timber = MaterialCatalog.Of(MaterialId.Timber);
            var plant = MaterialCatalog.Of(MaterialId.Plant);
            var grove = MaterialCatalog.Of(MaterialId.Grove);
            var ember = MaterialCatalog.Of(MaterialId.Ember);
            var water = MaterialCatalog.Of(MaterialId.Water);
            if (oil.BurnSeconds <= timber.BurnSeconds
                || timber.BurnSeconds <= plant.BurnSeconds
                || plant.BurnSeconds <= grove.BurnSeconds
                || oil.BurnRate >= timber.BurnRate
                || timber.BurnRate >= plant.BurnRate
                || plant.BurnRate >= grove.BurnRate
                || oil.Flammability <= timber.Flammability
                || timber.Flammability <= plant.Flammability
                || plant.Flammability <= grove.Flammability)
            {
                broken.Add("Oil and wood last longer than plant; grove burns out sooner");
            }

            if (plant.BurnRate <= 0f || timber.BurnRate <= 0f)
            {
                broken.Add("Land plants and timber must carry a burn rate");
            }

            if (CoverCatalog.RestAfterBurn(MaterialId.Timber) != MaterialId.Dirt
                || !VitalLaw.CanBurn(MaterialId.Timber)
                || VitalLaw.ItemBurnSeconds(MaterialId.Timber) != VitalLaw.TimberBurnSeconds)
            {
                broken.Add("A timber wall must burn on the wood clock and fall to leftover dirt");
            }

            if (CoverCatalog.LeftoverFloor(MaterialId.Plant) != MaterialId.Dirt
                || CoverCatalog.LeftoverFloor(MaterialId.Timber) != MaterialId.Dirt)
            {
                broken.Add("A spent plant or timber floor must swap to dirt (look and stamp)");
            }

            if (WorldWork.MaterialFor(RuneId.Fire, SpellId.FirePillar) != MaterialId.Fire
                || WorldWork.MaterialFor(RuneId.Fire, SpellId.FlamePillar) != MaterialId.Hearth
                || WorldWork.MaterialFor(RuneId.Fire, SpellId.LavaPillar) != MaterialId.Lava
                || !WorldWork.IsPillar(SpellId.FirePillar)
                || !WorldWork.IsFlameBody(MaterialId.Fire)
                || VitalLaw.FirePillarSeconds < 2f
                || VitalLaw.FirePillarSeconds > 5f)
            {
                broken.Add("Fire-pillar is temporary hunger; Flame-pillar is hearth; Lava-pillar is lava");
            }

            if (CoverCatalog.LeftoverFloor(MaterialId.Fire) != MaterialId.Fire
                || CoverCatalog.LeftoverFloor(MaterialId.Hearth) != MaterialId.Hearth
                || CoverCatalog.LeftoverFloor(MaterialId.Ember) != MaterialId.Ember
                || VitalLaw.CanBurn(MaterialId.Fire)
                || VitalLaw.CanBurn(MaterialId.Lava)
                || VitalLaw.CanBurn(MaterialId.Ember)
                || VitalLaw.IsRestFire(MaterialId.Ember))
            {
                broken.Add("Rest fire stays; ember is a Fire mark, not fuel, and does not leftover to dirt");
            }

            if (ember.BurnRate != 0f
                || ember.BurnSeconds != 0f
                || ember.Hunger != VitalLaw.HungerNeutral
                || grove.Hunger != VitalLaw.HungerSoft
                || plant.Hunger != VitalLaw.HungerPlant
                || timber.Hunger != VitalLaw.HungerTimber
                || oil.Hunger != VitalLaw.HungerOil
                || VitalLaw.IsStrongSource(plant.Hunger)
                || !VitalLaw.IsStrongSource(timber.Hunger)
                || VitalLaw.CatchReach(timber.Hunger) != 2
                || VitalLaw.CatchReach(oil.Hunger) != 4
                || VitalLaw.CanIgnite(plant.Hunger, plant.Hunger, 1, false)
                || !VitalLaw.CanIgnite(timber.Hunger, timber.Hunger, 1, false)
                || !VitalLaw.CanIgnite(timber.Hunger, plant.Hunger, 2, false)
                || !VitalLaw.CanIgnite(oil.Hunger, timber.Hunger, 1, false)
                || VitalLaw.CanIgnite(timber.Hunger, plant.Hunger, 3, false))
            {
                broken.Add("Hunger 0–10: a strong source (7+) walks fire to equal-or-weaker fuel out to its own reach");
            }

            var mud = MaterialCatalog.Of(MaterialId.Mud);
            if (mud.Hunger != VitalLaw.HungerNeutral
                || mud.Quench != VitalLaw.QuenchMud
                || timber.Quench != VitalLaw.QuenchDry
                || water.Quench != VitalLaw.QuenchWater
                || VitalLaw.SnuffsFire(mud.Quench)
                || !VitalLaw.SnuffsFire(water.Quench)
                || VitalLaw.SuppressesFire(timber.Quench)
                || !VitalLaw.SuppressesFire(mud.Quench)
                || water.Flammability >= 0f)
            {
                broken.Add("Quench 0–10: dry stone leaves fire alone; mud suppresses; water puts it out");
            }

            if (WorldSim.AcceptsFireSpread(null)
                || VitalLaw.IsSpreadFuel(MaterialId.Stone)
                || VitalLaw.IsSpreadFuel(MaterialId.Dirt)
                || VitalLaw.IsSpreadFuel(MaterialId.Ember)
                || VitalLaw.ConductsFire(MaterialId.Stone)
                || !VitalLaw.ConductsFire(MaterialId.Ember)
                || !VitalLaw.IsSpreadFuel(MaterialId.Timber)
                || !VitalLaw.IsSpreadFuel(MaterialId.Oil)
                || !VitalLaw.IsSpreadFuel(MaterialId.Plant))
            {
                broken.Add("Hunger must not run onto empty or neutral ground; ember is a path, not fuel");
            }

            if (water.BurnRate > 0f || water.BurnSeconds > 0f || water.Flammability >= 0f)
            {
                broken.Add("Water itself must quench hunger, not carry it");
            }

            if (!WorldWork.IsVineWork(SpellId.Vine)
                || !SweepsPath(SpellId.Vine, SpellShape.Shot)
                || SpellVerb.Of(SpellId.Vine).Tiles != TileVerb.Vine)
            {
                broken.Add("Vine must send a climbing body that hunger can run as a wick");
            }

            if (DominantAura(VeilKind.Darkness, false, true) != VeilKind.Darkness
                || DominantAura(VeilKind.None, false, true) != VeilKind.Fog
                || SpellVerb.Of(SpellId.Darkness).Tiles != TileVerb.Cloak)
            {
                broken.Add("Darkness must withhold sight even when the floor is cloaked");
            }

            if (!WorldWork.IsChargeWork(SpellId.LightningBolt)
                || !WorldWork.IsChargeWork(SpellId.LightningStrike)
                || !WorldWork.IsChargeWork(SpellId.LiveFloor)
                || WorldWork.IsChargeWork(SpellId.Fireball)
                || WorldWork.IsChargeWork(SpellId.Douse))
            {
                broken.Add("A bolt, strike, and live-floor must be charge work; hunger and yield must not");
            }

            if (SpellVerb.Of(SpellId.Sprout).Radius != PlantLaw.GrowRadius
                || !WorldWork.IsPlantGrowWork(SpellId.Sprout)
                || WorldWork.IsPlantGrowWork(SpellId.Forest))
            {
                broken.Add("Sprout must grow a three-tile plant cover from the feet");
            }

            MatterLaw.Audit(broken);
            ChargeLaw.Audit(broken);
            CoverCatalog.Audit(broken);
            PlantLaw.Audit(broken);
            WorldPaintTile.Audit(broken);
            TilemapLevel.Audit(broken);
            TileAtlas.Audit(broken);
            TileSprite.Audit(broken);
            RoomSentence.Audit(broken);
        }

        static List<Vector2Int> CellsAlong(WorldGrid grid, Vector3 from, Vector3 to, float width)
        {
            var cells = new List<Vector2Int>();
            var reach = Mathf.Max(1, Mathf.CeilToInt(width + 0.5f));
            var a = WorldWork.CoordOf(from);
            var b = WorldWork.CoordOf(to);
            var minX = Mathf.Min(a.x, b.x) - reach;
            var maxX = Mathf.Max(a.x, b.x) + reach;
            var minY = Mathf.Min(a.y, b.y) - reach;
            var maxY = Mathf.Max(a.y, b.y) + reach;
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var coord = new Vector2Int(x, y);
                    if (grid != null && grid.Get(coord) == null)
                    {
                        continue;
                    }

                    if (CellVolume.SegmentDistance(from, to, WorldGrid.Center(x, y)) <= width + CellVolume.TileRadius)
                    {
                        cells.Add(coord);
                    }
                }
            }

            if (cells.Count == 0)
            {
                cells.Add(b);
            }

            return cells;
        }

        static bool FireRunsFaster(float oil, float wood, float plant)
        {
            return oil > wood && wood > plant;
        }

        static bool CanTake(ISpellLock encounter, SpellVerb verb)
        {
            if (!LockAlive(encounter))
            {
                return false;
            }

            if (!StatusSpec.IsMindAilment(verb.Status))
            {
                return true;
            }

            return encounter is MonoBehaviour body && body != null && StatusHost.On(body) != null;
        }

        static bool LockAlive(ISpellLock encounter)
        {
            return encounter is MonoBehaviour body && body != null && !encounter.Resolved;
        }

        static string FirstFilled(List<string> notes)
        {
            for (var i = 0; i < notes.Count; i++)
            {
                if (!string.IsNullOrEmpty(notes[i]))
                {
                    return notes[i];
                }
            }

            return string.Empty;
        }

        sealed class PointVolume : ISpellVolume
        {
            readonly Vector3 _origin;
            readonly float _radius;

            public PointVolume(Vector3 origin, float radius)
            {
                _origin = origin;
                _radius = Mathf.Max(0.2f, radius);
            }

            public float DistanceTo(Vector3 point) =>
                Mathf.Max(0f, Vector2.Distance(point, _origin) - _radius);

            public Vector3 ClosestPoint(Vector3 point) => _origin;

            public bool Touches(Vector3 point, float radius) =>
                Vector2.Distance(point, _origin) <= _radius + Mathf.Max(0.05f, radius);

            public bool Crosses(Vector3 from, Vector3 to, float width) =>
                CellVolume.SegmentDistance(from, to, _origin) <= Mathf.Max(0.2f, width) + _radius;

            public bool OccupiesCell(Vector2Int cell) =>
                WorldWork.CoordOf(_origin) == cell;
        }
    }
}
