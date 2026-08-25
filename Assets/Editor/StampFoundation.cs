#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Paints Floor 1 into the open scene so you can keep placing
    /// tiles and objects in the Scene view.
    /// </summary>
    public static class StampFoundation
    {
        [MenuItem("Window/Rune Magic/Stamp Foundation Into Scene")]
        public static void Stamp()
        {
            var map = MapFile.Load("foundation");
            if (map == null)
            {
                EditorUtility.DisplayDialog("Stamp Foundation", "Missing Resources/Maps/foundation.json", "OK");
                return;
            }

            TilemapAuthoring.EnsureTiles();
            var authoring = Object.FindFirstObjectByType<LevelAuthoring>();
            if (authoring == null)
            {
                TilemapAuthoring.CreatePaintedMap();
                authoring = Object.FindFirstObjectByType<LevelAuthoring>();
            }

            if (authoring == null || authoring.tilemap == null)
            {
                EditorUtility.DisplayDialog("Stamp Foundation", "No painted map in the scene.", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(authoring.gameObject, "Stamp Foundation");
            var floors = authoring.tilemap;
            var cover = authoring.overlays;
            floors.ClearAllTiles();
            if (cover != null)
            {
                cover.ClearAllTiles();
            }

            ClearAuthored(authoring.transform);
            var cells = new Dictionary<Vector3Int, WorldPaintTile>();
            var overlays = new Dictionary<Vector3Int, WorldPaintTile>();
            PaintMap(map, cells, overlays);
            foreach (var pair in cells)
            {
                floors.SetTile(pair.Key, pair.Value);
            }

            if (cover != null)
            {
                foreach (var pair in overlays)
                {
                    cover.SetTile(pair.Key, pair.Value);
                }
            }

            if (map.spawn != null && authoring.spawnPoint != null)
            {
                authoring.spawnPoint.position = WorldGrid.Center(map.spawn.x, map.spawn.y);
            }

            PlaceProps(map, authoring.transform);
            EditorUtility.SetDirty(floors);
            if (cover != null)
            {
                EditorUtility.SetDirty(cover);
            }

            Debug.Log("Stamped Foundation: " + cells.Count + " walk cells. Paint and place more from GameObject → Rune Magic.");
        }

        static void PaintMap(
            MapFile map,
            Dictionary<Vector3Int, WorldPaintTile> cells,
            Dictionary<Vector3Int, WorldPaintTile> overlays)
        {
            if (map.rooms == null)
            {
                return;
            }

            for (var i = 0; i < map.rooms.Length; i++)
            {
                PaintRoom(map.rooms[i], cells, overlays);
            }

            if (map.halls == null)
            {
                return;
            }

            for (var i = 0; i < map.halls.Length; i++)
            {
                PaintHall(map, map.halls[i], cells, overlays);
            }
        }

        static void PaintRoom(
            MapRoom spec,
            Dictionary<Vector3Int, WorldPaintTile> cells,
            Dictionary<Vector3Int, WorldPaintTile> overlays)
        {
            if (spec == null)
            {
                return;
            }

            var origin = spec.origin != null ? spec.origin.Cell : Vector2Int.zero;
            var width = Mathf.Max(3, spec.width);
            var height = Mathf.Max(3, spec.height);
            var wall = Brush(TileKind.Wall, MapFile.ParseMaterial(spec.wall));
            var floor = Brush(TileKind.Floor, MapFile.ParseMaterial(spec.floor));
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    cells[new Vector3Int(origin.x + x, origin.y + y, 0)] = edge ? wall : floor;
                }
            }

            if (!string.IsNullOrEmpty(spec.exit) && spec.exit != "none")
            {
                PaintExit(origin, width, height, spec.exit, MapFile.ParseMaterial(spec.wall), cells);
            }

            if (spec.stamps == null)
            {
                return;
            }

            for (var i = 0; i < spec.stamps.Length; i++)
            {
                var stamp = spec.stamps[i];
                if (stamp == null || stamp.cells == null)
                {
                    continue;
                }

                var kind = MapFile.ParseKind(stamp.kind);
                var material = MapFile.ParseMaterial(stamp.material);
                var brush = Brush(kind, material);
                var overlay = OverlayBrush(stamp.aura, stamp.cover);
                for (var c = 0; c + 1 < stamp.cells.Length; c += 2)
                {
                    var pos = new Vector3Int(origin.x + stamp.cells[c], origin.y + stamp.cells[c + 1], 0);
                    cells[pos] = brush;
                    if (overlay != null)
                    {
                        overlays[pos] = overlay;
                    }
                }
            }
        }

        static void PaintExit(
            Vector2Int origin,
            int width,
            int height,
            string exit,
            MaterialId material,
            Dictionary<Vector3Int, WorldPaintTile> cells)
        {
            var door = Brush(TileKind.Door, material);
            var midX = origin.x + width / 2;
            var midY = origin.y + height / 2;
            switch ((exit ?? "east").ToLowerInvariant())
            {
                case "west":
                    SetDoor(cells, door, origin.x, midY - 1, origin.x, midY, origin.x, midY + 1);
                    break;
                case "north":
                    SetDoor(cells, door, midX - 1, origin.y + height - 1, midX, origin.y + height - 1, midX + 1, origin.y + height - 1);
                    break;
                case "south":
                    SetDoor(cells, door, midX - 1, origin.y, midX, origin.y, midX + 1, origin.y);
                    break;
                default:
                    SetDoor(cells, door, origin.x + width - 1, midY - 1, origin.x + width - 1, midY, origin.x + width - 1, midY + 1);
                    break;
            }
        }

        static void SetDoor(Dictionary<Vector3Int, WorldPaintTile> cells, WorldPaintTile door, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            cells[new Vector3Int(x0, y0, 0)] = door;
            cells[new Vector3Int(x1, y1, 0)] = door;
            cells[new Vector3Int(x2, y2, 0)] = door;
        }

        static void PaintHall(
            MapFile map,
            MapHall hall,
            Dictionary<Vector3Int, WorldPaintTile> cells,
            Dictionary<Vector3Int, WorldPaintTile> overlays)
        {
            var from = FindRoom(map.rooms, hall.from);
            var to = FindRoom(map.rooms, hall.to);
            if (from == null || to == null || from.origin == null || to.origin == null)
            {
                return;
            }

            var material = MapFile.ParseMaterial(hall.material);
            var floor = Brush(TileKind.Floor, material);
            var wall = Brush(TileKind.Wall, MaterialId.Stone);
            var fire = string.Equals(hall.hazard, "fire", System.StringComparison.OrdinalIgnoreCase)
                ? OverlayBrush("fire", null)
                : null;
            const int half = 1;
            if (to.origin.x > from.origin.x + from.width - 1)
            {
                var y0 = Mathf.Max(from.origin.y + 1, to.origin.y + 1);
                var y1 = Mathf.Min(from.origin.y + from.height - 2, to.origin.y + to.height - 2);
                var mid = y0 <= y1 ? (y0 + y1) / 2 : to.origin.y + to.height / 2;
                StampHall(cells, overlays, from.origin.x + from.width, to.origin.x - 1, mid, half, true, floor, wall, fire);
                Open(cells, from.origin.x + from.width - 1, mid, floor);
                Open(cells, to.origin.x, mid, floor);
                return;
            }

            if (to.origin.y > from.origin.y + from.height - 1)
            {
                var x0 = Mathf.Max(from.origin.x + 1, to.origin.x + 1);
                var x1 = Mathf.Min(from.origin.x + from.width - 2, to.origin.x + to.width - 2);
                var mid = x0 <= x1 ? (x0 + x1) / 2 : to.origin.x + to.width / 2;
                StampHall(cells, overlays, from.origin.y + from.height, to.origin.y - 1, mid, half, false, floor, wall, fire);
                Open(cells, mid, from.origin.y + from.height - 1, floor);
                Open(cells, mid, to.origin.y, floor);
            }
        }

        static void StampHall(
            Dictionary<Vector3Int, WorldPaintTile> cells,
            Dictionary<Vector3Int, WorldPaintTile> overlays,
            int gap0,
            int gap1,
            int mid,
            int half,
            bool eastWest,
            WorldPaintTile floor,
            WorldPaintTile wall,
            WorldPaintTile fire)
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
                    var pos = new Vector3Int(x, y, 0);
                    if (Mathf.Abs(side) <= half)
                    {
                        cells[pos] = floor;
                        if (fire != null)
                        {
                            overlays[pos] = fire;
                        }
                    }
                    else if (!cells.TryGetValue(pos, out var existing) || existing == null || existing.kind == TileKind.Wall)
                    {
                        cells[pos] = wall;
                    }
                }
            }
        }

        static void Open(Dictionary<Vector3Int, WorldPaintTile> cells, int x, int y, WorldPaintTile floor)
        {
            for (var d = -1; d <= 1; d++)
            {
                cells[new Vector3Int(x, y + d, 0)] = floor;
                cells[new Vector3Int(x + d, y, 0)] = floor;
            }
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

        static void PlaceProps(MapFile map, Transform parent)
        {
            if (map.rooms == null)
            {
                return;
            }

            for (var r = 0; r < map.rooms.Length; r++)
            {
                var spec = map.rooms[r];
                if (spec?.props == null || spec.origin == null)
                {
                    continue;
                }

                var origin = spec.origin.Cell;
                for (var i = 0; i < spec.props.Length; i++)
                {
                    PlaceProp(spec.props[i], origin, parent);
                }
            }

            if (map.spawn != null)
            {
                var crystal = new GameObject("Crystal");
                crystal.transform.SetParent(parent, false);
                crystal.transform.position = WorldGrid.Center(map.spawn.x, map.spawn.y);
                crystal.AddComponent<SpawnCrystal>();
                crystal.AddComponent<SpriteRenderer>();
                Undo.RegisterCreatedObjectUndo(crystal, "Stamp Crystal");
            }
        }

        static void PlaceProp(MapProp prop, Vector2Int origin, Transform parent)
        {
            if (prop == null || string.IsNullOrEmpty(prop.type))
            {
                return;
            }

            var world = WorldGrid.Center(origin.x + prop.x, origin.y + prop.y);
            var type = prop.type.ToLowerInvariant();
            GameObject host = null;
            switch (type)
            {
                case "mite":
                case "lock":
                    host = PackEnemies.Spawn(MatchEnemy(prop), world).gameObject;
                    break;
                case "item":
                    host = new GameObject(string.IsNullOrEmpty(prop.item) ? "Item" : prop.item);
                    var item = host.AddComponent<WorldItem>();
                    SetString(item, "catalogId", prop.item);
                    SetString(item, "spriteId", prop.sprite);
                    break;
                case "decor":
                case "pillar":
                case "stele":
                    host = new GameObject("Decor");
                    var decor = host.AddComponent<WorldDecor>();
                    SetString(decor, "spriteId", string.IsNullOrEmpty(prop.sprite) ? "pillar" : prop.sprite);
                    break;
                case "plaque":
                case "inscription":
                    host = new GameObject("Plaque");
                    var plaque = host.AddComponent<HintPlaque>();
                    SetString(plaque, "text", prop.text);
                    break;
                case "torch":
                    host = new GameObject("Torch");
                    host.AddComponent<TorchFixture>();
                    break;
                case "gate":
                    host = new GameObject("Gate");
                    var gate = host.AddComponent<SocketGate>();
                    SetString(gate, "authoredName", prop.displayName);
                    break;
                case "charm":
                    host = new GameObject("Charm");
                    host.AddComponent<FreeCharm>();
                    break;
                default:
                    return;
            }

            if (host == null)
            {
                return;
            }

            host.transform.SetParent(parent, false);
            host.transform.position = world;
            if (host.GetComponent<SpriteRenderer>() == null)
            {
                host.AddComponent<SpriteRenderer>();
            }

            Undo.RegisterCreatedObjectUndo(host, "Stamp " + type);
        }

        static PackEnemies.Spec MatchEnemy(MapProp prop)
        {
            var id = (prop.formulaId ?? prop.sprite ?? prop.displayName ?? string.Empty).ToLowerInvariant();
            if (id.Contains("golem") || id.Contains("fire"))
            {
                return PackEnemies.All[10];
            }

            if (id.Contains("warden") || id.Contains("spirit"))
            {
                return PackEnemies.All[11];
            }

            if (id.Contains("stone"))
            {
                return PackEnemies.All[1];
            }

            if (id.Contains("ice"))
            {
                return PackEnemies.All[3];
            }

            if (id.Contains("air") || id.Contains("bat"))
            {
                return PackEnemies.All[8];
            }

            return PackEnemies.All[0];
        }

        static void SetString(Object target, string field, string value)
        {
            if (target == null || string.IsNullOrEmpty(value))
            {
                return;
            }

            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property != null)
            {
                property.stringValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void ClearAuthored(Transform root)
        {
            var doomed = new List<GameObject>();
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is EncounterLock or WorldItem or WorldDecor or HintPlaque or
                    TorchFixture or SocketGate or FreeCharm or SpawnCrystal or BarrierLock)
                {
                    doomed.Add(behaviour.gameObject);
                }
            }

            for (var i = 0; i < doomed.Count; i++)
            {
                Undo.DestroyObjectImmediate(doomed[i]);
            }
        }

        static WorldPaintTile Brush(TileKind kind, MaterialId material)
        {
            var folder = kind == TileKind.Wall
                ? TilemapAuthoring.WallFolder
                : kind == TileKind.Floor
                    ? TilemapAuthoring.FloorFolder
                    : TilemapAuthoring.SpecialFolder;
            var name = kind == TileKind.Wall
                ? "Wall-" + material
                : kind == TileKind.Floor
                    ? (material == MaterialId.Void ? "Pit" : "Floor-" + material)
                    : kind == TileKind.Door
                        ? "Door"
                        : kind == TileKind.Pit
                            ? "Pit"
                            : "Bridge";
            if (kind == TileKind.Pit)
            {
                folder = TilemapAuthoring.SpecialFolder;
                name = "Pit";
            }

            var tile = AssetDatabase.LoadAssetAtPath<WorldPaintTile>(folder + "/" + name + ".asset");
            return tile != null ? tile : AssetDatabase.LoadAssetAtPath<WorldPaintTile>(TilemapAuthoring.FloorFolder + "/Floor-Stone.asset");
        }

        static WorldPaintTile OverlayBrush(string aura, string cover)
        {
            var key = (cover ?? aura ?? string.Empty).Trim().ToLowerInvariant();
            var name = key switch
            {
                "ice" => "Cover-Ice",
                "fire" => "Cover-Fire",
                "lightning" => "Cover-Lightning",
                "water" => "Cover-Water",
                "vine" => "Cover-Vine",
                "cracks" or "crack" => "Cover-Cracks",
                "seal" => "Cover-Seal",
                "miasma" or "poison" => "Aura-Miasma",
                "fog" => "Aura-Fog",
                _ => null
            };
            return string.IsNullOrEmpty(name)
                ? null
                : AssetDatabase.LoadAssetAtPath<WorldPaintTile>(TilemapAuthoring.CoverFolder + "/" + name + ".asset");
        }
    }
}
#endif
