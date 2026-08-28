using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Builds a playable floor from a painted Tilemap and scene objects.
    /// JSON maps are leftover and are not loaded unless you set
    /// <see cref="LevelAuthoring.tiles"/> to a named map.
    /// </summary>
    public static class SceneLevel
    {
        public static SanctumBuild Construct()
        {
            var authoring = Object.FindFirstObjectByType<LevelAuthoring>();
            var build = authoring != null ? BuildFrom(authoring) : DefaultTiles();
            return MergeScene(build, authoring);
        }

        static SanctumBuild DefaultTiles()
        {
            if (TilemapLevel.HasPaintedMap())
            {
                return TilemapLevel.Bake(null);
            }

            if (Object.FindFirstObjectByType<WorldGrid>() != null)
            {
                return FromExistingGrid(null);
            }

            Debug.LogWarning(
                "No painted Tilemap in the scene. Add GameObject → Rune Magic → Painted Map, then paint. Using an empty stone court.");
            return SanctumLayout.FallbackCourt();
        }

        static SanctumBuild BuildFrom(LevelAuthoring spec)
        {
            switch (spec.tiles)
            {
                case LevelTileSource.NamedMap:
                    {
                        var map = MapFile.Load(string.IsNullOrEmpty(spec.mapId) ? "foundation" : spec.mapId);
                        if (map != null)
                        {
                            return MapBuilder.Build(map, spec.includeJsonProps);
                        }

                        goto case LevelTileSource.RoomShell;
                    }
                case LevelTileSource.RoomShell:
                    return BuildShell(spec);
                case LevelTileSource.SceneGrid:
                    return FromExistingGrid(spec);
                case LevelTileSource.Tilemap:
                    {
                        var baked = TilemapLevel.Bake(spec);
                        return baked ?? BuildShell(spec);
                    }
                default:
                    {
                        var baked = TilemapLevel.Bake(spec);
                        return baked ?? BuildShell(spec);
                    }
            }
        }

        static SanctumBuild BuildShell(LevelAuthoring spec)
        {
            var name = string.IsNullOrEmpty(spec.roomName) ? "Authored Room" : spec.roomName;
            var root = new GameObject(name);
            var grid = root.AddComponent<WorldGrid>();
            var width = Mathf.Max(3, spec.roomWidth);
            var height = Mathf.Max(3, spec.roomHeight);
            grid.RoomShell(0, 0, width - 1, height - 1, spec.wall, spec.floor);
            grid.DressLooks();
            WorldSim.Ensure(grid);
            var spawn = spec.spawnPoint != null
                ? spec.spawnPoint.position
                : WorldGrid.Center(2, height / 2);
            var room = new RoomInfo(name, name, new RectInt(0, 0, width, height), spawn);
            return new SanctumBuild
            {
                Grid = grid,
                Spawn = spawn,
                Locks = System.Array.Empty<ISpellLock>(),
                Rooms = new[] { room },
                Charm = null
            };
        }

        static SanctumBuild FromExistingGrid(LevelAuthoring spec)
        {
            var grid = Object.FindFirstObjectByType<WorldGrid>();
            if (grid == null)
            {
                return spec != null ? BuildShell(spec) : SanctumLayout.Construct();
            }

            var bounds = BoundsOf(grid);
            var spawn = spec != null && spec.spawnPoint != null
                ? spec.spawnPoint.position
                : WorldGrid.Center(bounds.xMin + 2, bounds.yMin + bounds.height / 2);
            var room = new RoomInfo(grid.gameObject.name, grid.gameObject.name, bounds, spawn);
            WorldSim.Ensure(grid);
            return new SanctumBuild
            {
                Grid = grid,
                Spawn = spawn,
                Locks = System.Array.Empty<ISpellLock>(),
                Rooms = new[] { room },
                Charm = null
            };
        }

        static RectInt BoundsOf(WorldGrid grid)
        {
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;
            var any = false;
            foreach (var tile in grid.All)
            {
                if (tile == null)
                {
                    continue;
                }

                any = true;
                minX = Mathf.Min(minX, tile.Coord.x);
                minY = Mathf.Min(minY, tile.Coord.y);
                maxX = Mathf.Max(maxX, tile.Coord.x);
                maxY = Mathf.Max(maxY, tile.Coord.y);
            }

            return any
                ? new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1)
                : new RectInt(0, 0, 13, 11);
        }

        static SanctumBuild MergeScene(SanctumBuild build, LevelAuthoring authoring)
        {
            if (build == null)
            {
                return null;
            }

            if (authoring != null && authoring.spawnPoint != null)
            {
                build.Spawn = authoring.spawnPoint.position;
            }

            var crystal = Object.FindFirstObjectByType<SpawnCrystal>();
            if (crystal != null)
            {
                crystal.EnsureBound();
                if (authoring == null || authoring.spawnPoint == null)
                {
                    build.Spawn = crystal.transform.position;
                }
            }

            var plaques = Object.FindObjectsByType<HintPlaque>(FindObjectsSortMode.None);
            for (var i = 0; i < plaques.Length; i++)
            {
                plaques[i].EnsureBound();
            }

            var decors = Object.FindObjectsByType<WorldDecor>(FindObjectsSortMode.None);
            for (var i = 0; i < decors.Length; i++)
            {
                decors[i].BindFromAuthoring();
            }

            var doors = Object.FindObjectsByType<WorldDoor>(FindObjectsSortMode.None);
            for (var i = 0; i < doors.Length; i++)
            {
                doors[i].BindFromAuthoring(build.Grid);
            }

            var steles = Object.FindObjectsByType<RuneStele>(FindObjectsSortMode.None);
            for (var i = 0; i < steles.Length; i++)
            {
                steles[i].BindFromAuthoring();
            }

            var strings = Object.FindObjectsByType<RuneStringSource>(FindObjectsSortMode.None);
            for (var i = 0; i < strings.Length; i++)
            {
                strings[i].BindFromAuthoring();
            }

            var halls = Object.FindObjectsByType<FlameHall>(FindObjectsSortMode.None);
            for (var i = 0; i < halls.Length; i++)
            {
                halls[i].BindFromAuthoring();
            }

            var locks = new List<ISpellLock>();
            if (build.Locks != null)
            {
                locks.AddRange(build.Locks);
            }

            var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (behaviour is not ISpellLock spellLock)
                {
                    continue;
                }

                BindLock(behaviour, build.Grid);
                if (!locks.Contains(spellLock))
                {
                    locks.Add(spellLock);
                }
            }

            build.Locks = locks.ToArray();
            return build;
        }

        static void BindLock(MonoBehaviour behaviour, WorldGrid grid)
        {
            switch (behaviour)
            {
                case EncounterLock mite:
                    mite.BindFromAuthoring();
                    break;
                case TorchFixture torch:
                    torch.BindFromAuthoring();
                    break;
                case LightningConduit rod:
                    rod.BindFromAuthoring();
                    break;
                case SocketGate gate:
                    gate.BindFromAuthoring(grid);
                    break;
                case BarrierLock barrier:
                    barrier.BindFromAuthoring(grid);
                    break;
                case PitChasm chasm:
                    chasm.BindFromAuthoring(grid);
                    break;
                case ArrowVolley arrows:
                    arrows.BindFromAuthoring(grid);
                    break;
                case RoomFog fog:
                    fog.BindFromAuthoring(grid);
                    break;
            }
        }
    }
}
