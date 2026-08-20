using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class SanctumBuild
    {
        public WorldGrid Grid;
        public Vector3 Spawn;
        public ISpellLock[] Locks;
        public RoomInfo[] Rooms;
        public GameObject Charm;
    }

    /// <summary>
    /// Four tutorial rooms: a many-keyed beast, a cold torch, a pit that wants earth,
    /// then a storm rod that wants a blended Spark bolt.
    /// </summary>
    public static class SanctumLayout
    {
        const int RoomW = 13;
        const int RoomH = 11;
        const int HallW = 4;

        static readonly SpellId[] MiteKeys =
        {
            SpellId.Fireball, SpellId.FlamePillar, SpellId.WaterJet, SpellId.IcePillar,
            SpellId.LightningBolt, SpellId.LiveFloor, SpellId.Jolt,
            SpellId.HurledStone, SpellId.StonePillar, SpellId.Dread,
            SpellId.Gale, SpellId.Scald, SpellId.ScatterDust,
            SpellId.SunLance, SpellId.Drive, SpellId.BrilliantArc,
            SpellId.ChainLightning, SpellId.Thunderclap, SpellId.StormCall, SpellId.LavaFlood,
            SpellId.IceSpear, SpellId.Rage, SpellId.Blight, SpellId.Unmake, SpellId.LastBreath
        };

        static readonly SpellId[] TorchKeys =
        {
            SpellId.Fireball, SpellId.FlamePillar, SpellId.Frenzy, SpellId.Snuff, SpellId.SunLance, SpellId.Smother,
            SpellId.Ignite, SpellId.Melt
        };

        static readonly SpellId[] PitKeys =
        {
            SpellId.HurledStone, SpellId.StonePillar, SpellId.RaisedEarth,
            SpellId.Pit, SpellId.Bridge, SpellId.Wall
        };

        static readonly SpellId[] RodKeys =
        {
            SpellId.LightningBolt, SpellId.LiveFloor, SpellId.Jolt, SpellId.BrilliantArc, SpellId.Blackout,
            SpellId.ChainLightning, SpellId.StormCall, SpellId.Thunderclap
        };

        public static SanctumBuild Construct()
        {
            var root = new GameObject("SanctumGrid");
            var grid = root.AddComponent<WorldGrid>();
            var rooms = new RoomInfo[4];
            var locks = new ISpellLock[4];

            var r1 = RoomOrigin(0);
            var r2 = RoomOrigin(1);
            var r3 = RoomOrigin(2);
            var r4 = RoomOrigin(3);

            rooms[0] = BuildAshCourt(grid, r1, locks, 0, out var charm);
            rooms[1] = BuildWickChapel(grid, r2, locks, 1);
            Connect(grid, r1, r2, RuneId.Earth);
            rooms[2] = BuildTheDrop(grid, r3, locks, 2);
            Connect(grid, r2, r3, RuneId.Plant);
            rooms[3] = BuildStormCell(grid, r4, locks, 3);
            Connect(grid, r3, r4, RuneId.Earth);

            return new SanctumBuild
            {
                Grid = grid,
                Spawn = WorldGrid.Center(r1.x + 2, r1.y + 5),
                Locks = locks,
                Rooms = rooms,
                Charm = charm
            };
        }

        static Vector2Int RoomOrigin(int index)
        {
            return new Vector2Int(index * (RoomW + HallW), 0);
        }

        static RoomInfo BuildAshCourt(WorldGrid grid, Vector2Int o, ISpellLock[] locks, int index, out GameObject charm)
        {
            grid.RoomShell(o.x, o.y, o.x + RoomW - 1, o.y + RoomH - 1, RuneId.Earth, RuneId.Earth);
            var room = new RoomInfo("ash-court", "Ash Court",
                new RectInt(o.x, o.y, RoomW, RoomH),
                WorldGrid.Center(o.x + 2, o.y + 5));

            room.ExitDoors = PlaceExit(grid, o, RuneId.Earth);

            HintPlaque.Spawn(WorldGrid.Center(o.x + 3, o.y + 8), "A lock with many keys.");

            charm = new GameObject("FreeCharm");
            charm.transform.position = WorldGrid.Center(o.x + 3, o.y + 3);

            var mite = SpawnLock(
                "Ash Mite",
                "ash-mite",
                WorldGrid.Center(o.x + 7, o.y + 6),
                new[] { RuneId.Fire, RuneId.Salt },
                MiteKeys,
                ensouled: false);
            room.Lock = mite;
            locks[index] = mite;
            return room;
        }

        static RoomInfo BuildWickChapel(WorldGrid grid, Vector2Int o, ISpellLock[] locks, int index)
        {
            grid.RoomShell(o.x, o.y, o.x + RoomW - 1, o.y + RoomH - 1, RuneId.Plant, RuneId.Plant);
            grid.Set(o.x + 6, o.y + 6, TileKind.Floor, RuneId.Fire);
            grid.Set(o.x + 5, o.y + 5, TileKind.Floor, RuneId.Fire);
            grid.Set(o.x + 7, o.y + 5, TileKind.Floor, RuneId.Fire);
            grid.Set(o.x + 6, o.y + 4, TileKind.Floor, RuneId.Fire);

            var room = new RoomInfo("wick-chapel", "Wick Chapel",
                new RectInt(o.x, o.y, RoomW, RoomH),
                WorldGrid.Center(o.x + 2, o.y + 5));
            room.ExitDoors = PlaceExit(grid, o, RuneId.Plant);

            HintPlaque.Spawn(WorldGrid.Center(o.x + 3, o.y + 8), "Fire wants a form — then a place.");

            var torchObject = new GameObject("ColdTorch");
            torchObject.transform.position = WorldGrid.Center(o.x + 6, o.y + 5);
            var torch = torchObject.AddComponent<TorchFixture>();
            torch.Bind("Cold Torch", "cold-torch", TorchKeys);
            room.Lock = torch;
            locks[index] = torch;
            return room;
        }

        static RoomInfo BuildTheDrop(WorldGrid grid, Vector2Int o, ISpellLock[] locks, int index)
        {
            grid.RoomShell(o.x, o.y, o.x + RoomW - 1, o.y + RoomH - 1, RuneId.Earth, RuneId.Earth);

            var pits = new List<Vector2Int>();
            for (var x = o.x + 5; x <= o.x + 7; x++)
            {
                for (var y = o.y + 1; y <= o.y + RoomH - 2; y++)
                {
                    grid.Set(x, y, TileKind.Pit, RuneId.None);
                    pits.Add(new Vector2Int(x, y));
                }
            }

            var room = new RoomInfo("the-drop", "The Drop",
                new RectInt(o.x, o.y, RoomW, RoomH),
                WorldGrid.Center(o.x + 2, o.y + 5));
            room.ExitDoors = PlaceExit(grid, o, RuneId.Earth);

            HintPlaque.Spawn(WorldGrid.Center(o.x + 2, o.y + 8), "Earth × Mercury, then Shot — or Salt as a pillar.");

            var pitMark = new GameObject("PitMark");
            pitMark.transform.position = WorldGrid.Center(o.x + 6, o.y + 2);
            WorldLabel.Attach(pitMark.transform, "PIT", Vector3.zero, new Color(1f, 0.45f, 0.25f));

            var chasmObject = new GameObject("Chasm");
            chasmObject.transform.position = WorldGrid.Center(o.x + 4, o.y + 5);
            var chasm = chasmObject.AddComponent<PitChasm>();
            chasm.Bind("Chasm", "chasm", PitKeys, grid, pits);
            room.Lock = chasm;
            locks[index] = chasm;
            return room;
        }

        static RoomInfo BuildStormCell(WorldGrid grid, Vector2Int o, ISpellLock[] locks, int index)
        {
            grid.RoomShell(o.x, o.y, o.x + RoomW - 1, o.y + RoomH - 1, RuneId.Earth, RuneId.Spark);
            grid.Set(o.x + 6, o.y + 5, TileKind.Floor, RuneId.Air);
            grid.Set(o.x + 5, o.y + 5, TileKind.Floor, RuneId.Fire);
            grid.Set(o.x + 7, o.y + 5, TileKind.Floor, RuneId.Fire);

            var room = new RoomInfo("storm-cell", "Storm Cell",
                new RectInt(o.x, o.y, RoomW, RoomH),
                WorldGrid.Center(o.x + 2, o.y + 5));

            HintPlaque.Spawn(WorldGrid.Center(o.x + 3, o.y + 8), "Fire + Air = Spark.");

            var rodObject = new GameObject("StormRod");
            rodObject.transform.position = WorldGrid.Center(o.x + 6, o.y + 5);
            var rod = rodObject.AddComponent<LightningConduit>();
            rod.Bind("Storm Rod", "storm-rod", RodKeys);
            room.Lock = rod;
            locks[index] = rod;
            return room;
        }

        static void Connect(WorldGrid grid, Vector2Int from, Vector2Int to, RuneId hallElement)
        {
            var hallX0 = from.x + RoomW;
            var hallX1 = to.x - 1;
            var y0 = 0;
            var y1 = RoomH - 1;
            grid.Fill(hallX0, y0, hallX1, y1, TileKind.Wall, RuneId.Earth);
            grid.Fill(hallX0, 4, hallX1, 6, TileKind.Floor, hallElement);
            grid.Set(to.x, 4, TileKind.Floor, hallElement);
            grid.Set(to.x, 5, TileKind.Floor, hallElement);
            grid.Set(to.x, 6, TileKind.Floor, hallElement);
        }

        static WorldTile[] PlaceExit(WorldGrid grid, Vector2Int origin, RuneId element)
        {
            return new[]
            {
                grid.Set(origin.x + RoomW - 1, origin.y + 4, TileKind.Door, element),
                grid.Set(origin.x + RoomW - 1, origin.y + 5, TileKind.Door, element),
                grid.Set(origin.x + RoomW - 1, origin.y + 6, TileKind.Door, element)
            };
        }

        static EncounterLock SpawnLock(
            string displayName,
            string formulaId,
            Vector3 position,
            RuneId[] formula,
            SpellId[] keys,
            bool ensouled)
        {
            var actor = new GameObject(displayName);
            actor.transform.position = position;
            var encounter = actor.AddComponent<EncounterLock>();
            encounter.Bind(displayName, formulaId, formula, keys, ensouled);
            return encounter;
        }
    }
}
