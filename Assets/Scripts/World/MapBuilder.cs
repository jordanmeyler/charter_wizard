using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Stamps a <see cref="MapFile"/> onto a live grid. The sanctum JSON is
    /// the first map; later chambers are the same format.
    /// </summary>
    public static class MapBuilder
    {
        public static readonly SpellId[] MiteKeys =
        {
            SpellId.Fireball, SpellId.FlamePillar, SpellId.FirePillar, SpellId.WaterJet, SpellId.IcePillar, SpellId.IceWall,
            SpellId.LightningBolt, SpellId.LightningStrike, SpellId.LiveFloor, SpellId.Jolt,
            SpellId.HurledStone, SpellId.StonePillar, SpellId.Dread,
            SpellId.Gale, SpellId.Scald, SpellId.ScatterDust,
            SpellId.SunLance, SpellId.Drive, SpellId.BrilliantArc,
            SpellId.ChainLightning, SpellId.Thunderclap, SpellId.StormCall, SpellId.LavaFlood,
            SpellId.IceSpear, SpellId.Rage, SpellId.Lull, SpellId.Command, SpellId.Charm, SpellId.Confuse, SpellId.Terror,
            SpellId.Swamp, SpellId.Blight, SpellId.Unmake, SpellId.LastBreath,
            SpellId.TimeStop
        };

        public static readonly SpellId[] TorchKeys =
        {
            SpellId.Fireball, SpellId.FlamePillar, SpellId.FirePillar, SpellId.Frenzy, SpellId.Snuff, SpellId.SunLance, SpellId.Smother,
            SpellId.Ignite, SpellId.Melt, SpellId.Witchfire
        };

        public static readonly SpellId[] PitKeys =
        {
            SpellId.HurledStone, SpellId.StonePillar, SpellId.EarthPillar, SpellId.RaisedEarth,
            SpellId.Pit, SpellId.Bridge, SpellId.Wall, SpellId.IceWall, SpellId.MetalWall, SpellId.ObsidianWall,
            SpellId.FlamePillar, SpellId.FirePillar, SpellId.IcePillar, SpellId.MetalPillar, SpellId.TaintedTree,
            SpellId.Tree, SpellId.WoodWall,
            SpellId.Hop, SpellId.Flight
        };

        public static readonly SpellId[] RodKeys =
        {
            SpellId.LightningBolt, SpellId.LightningStrike, SpellId.LiveFloor, SpellId.Jolt, SpellId.BrilliantArc, SpellId.Blackout,
            SpellId.ChainLightning, SpellId.StormCall, SpellId.Thunderclap
        };

        public static readonly SpellId[] FogKeys =
        {
            SpellId.Gust, SpellId.Gale, SpellId.Push, SpellId.StormCall, SpellId.Flight,
            SpellId.Fireball, SpellId.FlamePillar, SpellId.FirePillar, SpellId.Ignite, SpellId.Melt, SpellId.Witchfire, SpellId.SunLance,
            SpellId.DayWake, SpellId.BrilliantArc
        };

        public static readonly SpellId[] ArrowKeys =
        {
            SpellId.Wall, SpellId.IceWall, SpellId.MetalWall, SpellId.ObsidianWall, SpellId.StonePillar, SpellId.EarthPillar, SpellId.FlamePillar, SpellId.FirePillar,
            SpellId.IcePillar, SpellId.MetalPillar, SpellId.TaintedTree, SpellId.Tree, SpellId.WoodWall,
            SpellId.Menhir, SpellId.Bridge
        };

        public static SanctumBuild Build(MapFile map, bool includeProps = true)
        {
            if (map == null || map.rooms == null || map.rooms.Length == 0)
            {
                throw new System.ArgumentException("Map has no rooms.");
            }

            var root = new GameObject(string.IsNullOrEmpty(map.name) ? "MapGrid" : map.name);
            var grid = root.AddComponent<WorldGrid>();
            var rooms = new RoomInfo[map.rooms.Length];
            var locks = new List<ISpellLock>();
            GameObject charm = null;

            for (var i = 0; i < map.rooms.Length; i++)
            {
                rooms[i] = BuildRoom(grid, map.rooms[i], locks, ref charm, includeProps);
            }

            if (map.halls != null)
            {
                for (var i = 0; i < map.halls.Length; i++)
                {
                    Connect(grid, FindRoom(map.rooms, map.halls[i].from), FindRoom(map.rooms, map.halls[i].to),
                        map.halls[i]);
                }
            }

            grid.DressLooks();
            WorldSim.Ensure(grid);
            var spawn = map.spawn != null
                ? WorldGrid.Center(map.spawn.x, map.spawn.y)
                : rooms[0].Entrance;

            return new SanctumBuild
            {
                Grid = grid,
                Spawn = spawn,
                Locks = locks.ToArray(),
                Rooms = rooms,
                Charm = charm
            };
        }

        static RoomInfo BuildRoom(WorldGrid grid, MapRoom spec, List<ISpellLock> locks, ref GameObject charm, bool includeProps)
        {
            var origin = spec.origin != null ? spec.origin.Cell : Vector2Int.zero;
            var width = Mathf.Max(3, spec.width);
            var height = Mathf.Max(3, spec.height);
            var wall = MapFile.ParseMaterial(spec.wall);
            var floor = MapFile.ParseMaterial(spec.floor);
            grid.RoomShell(origin.x, origin.y, origin.x + width - 1, origin.y + height - 1, wall, floor);
            ApplyStamps(grid, origin, spec.stamps);

            var room = new RoomInfo(
                string.IsNullOrEmpty(spec.id) ? spec.name : spec.id,
                string.IsNullOrEmpty(spec.name) ? spec.id : spec.name,
                new RectInt(origin.x, origin.y, width, height),
                WorldGrid.Center(origin.x + 2, origin.y + height / 2));

            if (!string.IsNullOrEmpty(spec.exit) && spec.exit != "none")
            {
                room.ExitDoors = PlaceExit(grid, origin, width, height, spec.exit, wall);
            }

            if (!includeProps || spec.props == null)
            {
                return room;
            }

            for (var i = 0; i < spec.props.Length; i++)
            {
                PlaceProp(grid, origin, room, spec.props[i], locks, ref charm);
            }

            return room;
        }

        static void ApplyStamps(WorldGrid grid, Vector2Int origin, MapStamp[] stamps)
        {
            if (stamps == null)
            {
                return;
            }

            for (var i = 0; i < stamps.Length; i++)
            {
                var stamp = stamps[i];
                if (stamp == null || stamp.cells == null)
                {
                    continue;
                }

                var kind = MapFile.ParseKind(stamp.kind);
                var material = MapFile.ParseMaterial(stamp.material);
                for (var c = 0; c + 1 < stamp.cells.Length; c += 2)
                {
                    var tile = grid.Set(origin.x + stamp.cells[c], origin.y + stamp.cells[c + 1], kind, material);
                    ApplyAura(tile, stamp.aura);
                    ApplyCover(tile, stamp.cover);
                }
            }
        }

        static void ApplyAura(WorldTile tile, string aura)
        {
            if (tile == null || string.IsNullOrEmpty(aura))
            {
                return;
            }

            switch (aura.Trim().ToLowerInvariant())
            {
                case "miasma":
                    tile.Foul(1f);
                    break;
                case "poison":
                    tile.SlickPoison();
                    break;
                case "fog":
                    tile.Cloak(1f);
                    break;
                case "fire":
                    tile.Kindle();
                    break;
            }
        }

        static void ApplyCover(WorldTile tile, string cover)
        {
            tile?.PaintCover(cover);
        }

        static void PlaceProp(
            WorldGrid grid,
            Vector2Int origin,
            RoomInfo room,
            MapProp prop,
            List<ISpellLock> locks,
            ref GameObject charm)
        {
            if (prop == null || string.IsNullOrEmpty(prop.type))
            {
                return;
            }

            var world = WorldGrid.Center(origin.x + prop.x, origin.y + prop.y);
            if (!string.IsNullOrEmpty(prop.item) && CatalogBook.TryItem(prop.item, out var catalogItem))
            {
                ApplyItem(prop, catalogItem);
            }

            var type = prop.type.ToLowerInvariant();
            switch (type)
            {
                case "item":
                    if (CatalogBook.TryItem(prop.item, out var spawned))
                    {
                        WorldItem.Spawn(world, spawned);
                    }

                    break;
                case "plaque":
                    if (!string.IsNullOrEmpty(prop.text))
                    {
                        HintPlaque.Spawn(world, prop.text);
                    }

                    break;
                case "altar":
                case "pray":
                case "interact":
                    WorldInteract.Spawn(world,
                        string.IsNullOrEmpty(prop.spell) ? prop.note : prop.spell,
                        string.IsNullOrEmpty(prop.text) ? "Pray" : prop.text);
                    break;
                case "runes":
                    RuneStringSource.Spawn(world, ParseRunes(prop.runes), MapFile.HeadingOf(prop.dir));
                    break;
                case "inscription":
                    {
                        var carved = FirstRune(prop.runes);
                        if (carved != RuneId.None)
                        {
                            RuneStele.Inscribe(world, carved);
                        }
                    }

                    break;
                case "pillar":
                case "stele":
                    {
                        var raised = FirstRune(prop.runes);
                        if (raised != RuneId.None)
                        {
                            RuneStele.Raise(world, raised);
                        }
                    }

                    break;
                case "lesson":
                    PlaceLesson(world, ParseRunes(prop.runes), MapFile.HeadingOf(prop.dir));
                    break;
                case "charm":
                    charm = new GameObject("FreeCharm");
                    charm.transform.position = world;
                    break;
                case "mite":
                case "lock":
                    BindLock(room, locks, SpawnMite(prop, world));
                    break;
                case "torch":
                    BindLock(room, locks, SpawnTorch(prop, world));
                    break;
                case "rod":
                    BindLock(room, locks, SpawnRod(prop, world));
                    break;
                case "chasm":
                    BindLock(room, locks, SpawnChasm(prop, world, grid, room));
                    break;
                case "barrier":
                    BindLock(room, locks, SpawnBarrier(prop, world, grid, origin));
                    break;
                case "gate":
                    BindLock(room, locks, SpawnGate(prop, world, grid, origin));
                    break;
                case "charge-gate":
                case "electric-gate":
                    BindLock(room, locks, SpawnChargeGate(prop, world, grid, origin));
                    break;
                case "fog":
                    BindLock(room, locks, SpawnFog(prop, world, origin, grid));
                    break;
                case "arrows":
                    BindLock(room, locks, SpawnArrows(prop, world, grid, origin));
                    break;
                case "crystal":
                case "anchor":
                    SpawnCrystal.Spawn(world);
                    break;
                case "decor":
                case "prop":
                    WorldDecor.Spawn(world, prop.sprite, prop.blocking, prop.note);
                    break;
            }
        }

        static EncounterLock SpawnMite(MapProp prop, Vector3 world)
        {
            var actor = new GameObject(NameOf(prop, "Ash Mite"));
            actor.transform.position = world;
            var encounter = actor.AddComponent<EncounterLock>();
            encounter.Bind(
                NameOf(prop, "Ash Mite"),
                IdOf(prop, "ash-mite"),
                ParseRunes(prop.formula, RuneId.Fire, RuneId.Salt, RuneId.Vita),
                ParseKeys(prop.keys, MiteKeys),
                ensouled: prop.ensouled,
                spriteId: prop.sprite,
                blocking: prop.blocking,
                grantItem: prop.grant,
                attack: prop.attack,
                castSeconds: prop.castSeconds,
                castRecipe: ParseRunes(prop.cast));
            return encounter;
        }

        static TorchFixture SpawnTorch(MapProp prop, Vector3 world)
        {
            var actor = new GameObject(NameOf(prop, "Cold Torch"));
            actor.transform.position = world;
            var torch = actor.AddComponent<TorchFixture>();
            torch.Bind(NameOf(prop, "Cold Torch"), IdOf(prop, "cold-torch"), ParseKeys(prop.keys, TorchKeys), prop.sprite);
            return torch;
        }

        static LightningConduit SpawnRod(MapProp prop, Vector3 world)
        {
            var actor = new GameObject(NameOf(prop, "Storm Rod"));
            actor.transform.position = world;
            var rod = actor.AddComponent<LightningConduit>();
            rod.Bind(NameOf(prop, "Storm Rod"), IdOf(prop, "storm-rod"), ParseKeys(prop.keys, RodKeys), prop.sprite);
            return rod;
        }

        static PitChasm SpawnChasm(MapProp prop, Vector3 world, WorldGrid grid, RoomInfo room)
        {
            var actor = new GameObject(NameOf(prop, "Chasm"));
            actor.transform.position = world;
            var pits = CollectPits(grid, room.Bounds);
            var chasm = actor.AddComponent<PitChasm>();
            chasm.Bind(NameOf(prop, "Chasm"), IdOf(prop, "chasm"), ParseKeys(prop.keys, PitKeys), grid, pits);
            return chasm;
        }

        static BarrierLock SpawnBarrier(MapProp prop, Vector3 world, WorldGrid grid, Vector2Int origin)
        {
            var actor = new GameObject(NameOf(prop, "Barrier"));
            actor.transform.position = world;
            var barrier = actor.AddComponent<BarrierLock>();
            barrier.Bind(
                NameOf(prop, "Barrier"),
                IdOf(prop, "barrier"),
                ParseKeys(prop.keys, TorchKeys),
                ParseRunes(prop.formula),
                grid,
                LocalCells(origin, prop.cells),
                prop.grant,
                prop.clearMaterial,
                prop.sprite,
                prop.note);
            return barrier;
        }

        static SocketGate SpawnGate(MapProp prop, Vector3 world, WorldGrid grid, Vector2Int origin)
        {
            var actor = new GameObject(NameOf(prop, "Gate"));
            actor.transform.position = world;
            var gate = actor.AddComponent<SocketGate>();
            gate.Bind(
                NameOf(prop, "Gate"),
                IdOf(prop, "gate"),
                prop.requires,
                prop.finishes,
                prop.note,
                prop.sprite,
                grid,
                LocalCells(origin, prop.cells));
            return gate;
        }

        static ChargeGate SpawnChargeGate(MapProp prop, Vector3 world, WorldGrid grid, Vector2Int origin)
        {
            var actor = new GameObject(NameOf(prop, "Electric Gate"));
            actor.transform.position = world;
            var gate = actor.AddComponent<ChargeGate>();
            gate.Bind(
                NameOf(prop, "Electric Gate"),
                IdOf(prop, "electric-gate"),
                ParseKeys(prop.keys, RodKeys),
                prop.finishes,
                prop.note,
                string.IsNullOrEmpty(prop.sprite) ? "rod" : prop.sprite,
                grid,
                LocalCells(origin, prop.cells));
            return gate;
        }

        static RoomFog SpawnFog(MapProp prop, Vector3 world, Vector2Int origin, WorldGrid grid)
        {
            var actor = new GameObject(NameOf(prop, "Fog"));
            actor.transform.position = world;
            var fog = actor.AddComponent<RoomFog>();
            fog.Bind(
                NameOf(prop, "Poison fog"),
                IdOf(prop, "poison-fog"),
                ParseKeys(prop.keys, FogKeys),
                ParseRunes(prop.formula, RuneId.Air),
                LocalCells(origin, prop.cells),
                prop.sprite,
                prop.note,
                grid);
            return fog;
        }

        static ArrowVolley SpawnArrows(MapProp prop, Vector3 world, WorldGrid grid, Vector2Int origin)
        {
            var actor = new GameObject(NameOf(prop, "Arrows"));
            actor.transform.position = world;
            var volley = actor.AddComponent<ArrowVolley>();
            volley.Bind(
                NameOf(prop, "Arrow volley"),
                IdOf(prop, "arrow-volley"),
                ParseKeys(prop.keys, ArrowKeys),
                ParseRunes(prop.formula, RuneId.Earth),
                grid,
                LocalCells(origin, prop.cover),
                LocalCells(origin, prop.cells),
                prop.sprite,
                prop.note,
                MapFile.HeadingOf(string.IsNullOrEmpty(prop.dir) ? "south" : prop.dir));
            return volley;
        }

        static List<Vector2Int> LocalCells(Vector2Int origin, int[] cells)
        {
            var list = new List<Vector2Int>();
            if (cells == null)
            {
                return list;
            }

            for (var i = 0; i + 1 < cells.Length; i += 2)
            {
                list.Add(new Vector2Int(origin.x + cells[i], origin.y + cells[i + 1]));
            }

            return list;
        }

        static List<Vector2Int> CollectPits(WorldGrid grid, RectInt bounds)
        {
            var pits = new List<Vector2Int>();
            for (var y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (var x = bounds.xMin; x < bounds.xMax; x++)
                {
                    var tile = grid.Get(x, y);
                    if (tile != null && tile.Kind == TileKind.Pit)
                    {
                        pits.Add(new Vector2Int(x, y));
                    }
                }
            }

            return pits;
        }

        static void BindLock(RoomInfo room, List<ISpellLock> locks, ISpellLock encounter)
        {
            room.Lock = encounter;
            locks.Add(encounter);
        }

        static void Connect(WorldGrid grid, MapRoom from, MapRoom to, MapHall hall)
        {
            if (from == null || to == null || from.origin == null || to.origin == null || hall == null)
            {
                return;
            }

            var material = MapFile.ParseMaterial(hall.material);
            var kindle = IsFireHall(hall);
            const int half = 1;
            if (to.origin.x > from.origin.x + from.width - 1)
            {
                var y0 = Mathf.Max(from.origin.y + 1, to.origin.y + 1);
                var y1 = Mathf.Min(from.origin.y + from.height - 2, to.origin.y + to.height - 2);
                var mid = y0 <= y1 ? (y0 + y1) / 2 : to.origin.y + to.height / 2;
                StampHall(grid, from.origin.x + from.width, to.origin.x - 1, mid, half, true, material, kindle);
                for (var dy = -half; dy <= half; dy++)
                {
                    OpenPassage(grid, from.origin.x + from.width - 1, mid + dy, material, false);
                    OpenPassage(grid, to.origin.x, mid + dy, material, kindle);
                }

                if (kindle)
                {
                    MarkFlameHall(from.origin.x + from.width - 1, mid);
                }

                return;
            }

            if (to.origin.y > from.origin.y + from.height - 1)
            {
                var x0 = Mathf.Max(from.origin.x + 1, to.origin.x + 1);
                var x1 = Mathf.Min(from.origin.x + from.width - 2, to.origin.x + to.width - 2);
                var mid = x0 <= x1 ? (x0 + x1) / 2 : to.origin.x + to.width / 2;
                StampHall(grid, from.origin.y + from.height, to.origin.y - 1, mid, half, false, material, kindle);
                for (var dx = -half; dx <= half; dx++)
                {
                    OpenPassage(grid, mid + dx, from.origin.y + from.height - 1, material, false);
                    OpenPassage(grid, mid + dx, to.origin.y, material, kindle);
                }

                if (kindle)
                {
                    MarkFlameHall(mid, from.origin.y + from.height - 1);
                }
            }
        }

        static bool IsFireHall(MapHall hall)
        {
            var hazard = hall != null ? hall.hazard : string.Empty;
            return !string.IsNullOrEmpty(hazard)
                && hazard.Equals("fire", System.StringComparison.OrdinalIgnoreCase);
        }

        static void MarkFlameHall(int x, int y)
        {
            FlameHall.Spawn(WorldGrid.Center(x, y));
        }

        static void StampHall(WorldGrid grid, int gap0, int gap1, int mid, int half, bool eastWest, MaterialId hall, bool kindle)
        {
            if (gap0 > gap1)
            {
                return;
            }

            for (var along = gap0; along <= gap1; along++)
            {
                for (var side = -half - 1; side <= half + 1; side++)
                {
                    var x = eastWest ? along : mid + side;
                    var y = eastWest ? mid + side : along;
                    if (Mathf.Abs(side) <= half)
                    {
                        OpenPassage(grid, x, y, hall, kindle);
                    }
                    else
                    {
                        SealHallEdge(grid, x, y);
                    }
                }
            }
        }

        static void OpenPassage(WorldGrid grid, int x, int y, MaterialId hall, bool kindle = false)
        {
            var tile = grid.Get(x, y);
            if (tile != null && tile.Kind == TileKind.Door)
            {
                return;
            }

            grid.Set(x, y, TileKind.Floor, hall);
            if (kindle)
            {
                grid.Get(x, y)?.Kindle();
            }
        }

        static void SealHallEdge(WorldGrid grid, int x, int y)
        {
            var tile = grid.Get(x, y);
            if (tile != null && (tile.Kind == TileKind.Floor || tile.Kind == TileKind.Bridge || tile.Kind == TileKind.Door))
            {
                return;
            }

            grid.Set(x, y, TileKind.Wall, MaterialId.Stone);
        }

        static WorldTile[] PlaceExit(WorldGrid grid, Vector2Int origin, int width, int height, string exit, MaterialId material)
        {
            var midX = origin.x + width / 2;
            var midY = origin.y + height / 2;
            WorldTile left;
            WorldTile leaf;
            WorldTile right;
            switch ((exit ?? "east").ToLowerInvariant())
            {
                case "west":
                    left = grid.Set(origin.x, midY - 1, TileKind.Door, material);
                    leaf = grid.Set(origin.x, midY, TileKind.Door, material);
                    right = grid.Set(origin.x, midY + 1, TileKind.Door, material);
                    break;
                case "north":
                    left = grid.Set(midX - 1, origin.y + height - 1, TileKind.Door, material);
                    leaf = grid.Set(midX, origin.y + height - 1, TileKind.Door, material);
                    right = grid.Set(midX + 1, origin.y + height - 1, TileKind.Door, material);
                    break;
                case "south":
                    left = grid.Set(midX - 1, origin.y, TileKind.Door, material);
                    leaf = grid.Set(midX, origin.y, TileKind.Door, material);
                    right = grid.Set(midX + 1, origin.y, TileKind.Door, material);
                    break;
                default:
                    left = grid.Set(origin.x + width - 1, midY - 1, TileKind.Door, material);
                    leaf = grid.Set(origin.x + width - 1, midY, TileKind.Door, material);
                    right = grid.Set(origin.x + width - 1, midY + 1, TileKind.Door, material);
                    break;
            }

            left.MarkDoor(DoorFace.Jamb);
            leaf.MarkDoor(DoorFace.Leaf);
            right.MarkDoor(DoorFace.Jamb);
            return new[] { left, leaf, right };
        }

        static MapRoom FindRoom(MapRoom[] rooms, string id)
        {
            if (rooms == null || string.IsNullOrEmpty(id))
            {
                return null;
            }

            for (var i = 0; i < rooms.Length; i++)
            {
                if (rooms[i] != null && rooms[i].id == id)
                {
                    return rooms[i];
                }
            }

            return null;
        }

        static void ApplyItem(MapProp prop, CatalogItem item)
        {
            if (string.IsNullOrEmpty(prop.type) || prop.type == "item")
            {
                switch ((item.kind ?? string.Empty).ToLowerInvariant())
                {
                    case "mite":
                    case "torch":
                    case "rod":
                    case "chasm":
                    case "charm":
                    case "barrier":
                    case "gate":
                    case "charge-gate":
                    case "electric-gate":
                    case "crystal":
                        prop.type = item.kind;
                        break;
                    default:
                        prop.type = "item";
                        break;
                }
            }

            if (string.IsNullOrEmpty(prop.displayName))
            {
                prop.displayName = item.name;
            }

            if (string.IsNullOrEmpty(prop.sprite))
            {
                prop.sprite = item.sprite;
            }

            if ((prop.formula == null || prop.formula.Length == 0) && item.formula != null)
            {
                prop.formula = item.formula;
            }

            if ((prop.keys == null || prop.keys.Length == 0) && item.keys != null)
            {
                prop.keys = item.keys;
            }

            if (string.IsNullOrEmpty(prop.formulaId))
            {
                prop.formulaId = item.id;
            }
        }

        static string NameOf(MapProp prop, string fallback) =>
            string.IsNullOrEmpty(prop.displayName) ? fallback : prop.displayName;

        static void PlaceLesson(Vector3 origin, RuneId[] runes, Vector3 dir)
        {
            if (runes == null || runes.Length == 0)
            {
                return;
            }

            var step = dir.sqrMagnitude > 0.01f ? dir.normalized : Vector3.right;
            for (var i = 0; i < runes.Length; i++)
            {
                if (runes[i] != RuneId.None)
                {
                    RuneStele.Raise(origin + step * i, runes[i]);
                }
            }
        }

        static string IdOf(MapProp prop, string fallback) =>
            string.IsNullOrEmpty(prop.formulaId) ? fallback : prop.formulaId;

        static RuneId FirstRune(string[] names)
        {
            var runes = ParseRunes(names);
            return runes != null && runes.Length > 0 ? runes[0] : RuneId.None;
        }

        static RuneId[] ParseRunes(string[] names, params RuneId[] fallback)
        {
            if (names == null || names.Length == 0)
            {
                return fallback;
            }

            var runes = new RuneId[names.Length];
            var count = 0;
            for (var i = 0; i < names.Length; i++)
            {
                var rune = MapFile.ParseRune(names[i]);
                if (rune != RuneId.None)
                {
                    runes[count++] = rune;
                }
            }

            if (count == 0)
            {
                return fallback;
            }

            if (count != runes.Length)
            {
                System.Array.Resize(ref runes, count);
            }

            return runes;
        }

        static SpellId[] ParseKeys(string[] names, SpellId[] fallback)
        {
            if (names == null || names.Length == 0)
            {
                return fallback;
            }

            var keys = new List<SpellId>(names.Length);
            for (var i = 0; i < names.Length; i++)
            {
                var spell = MapFile.ParseSpell(names[i]);
                if (spell != SpellId.None)
                {
                    keys.Add(spell);
                }
            }

            return keys.Count > 0 ? keys.ToArray() : fallback;
        }
    }
}
