#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    public static class TilemapAuthoring
    {
        public const string TileRoot = "Assets/Tiles";
        public const string FloorFolder = "Assets/Tiles/Floor";
        public const string WallFolder = "Assets/Tiles/Wall";
        public const string SpecialFolder = "Assets/Tiles/Special";
        public const string CoverFolder = "Assets/Tiles/Cover";
        public const string PaletteFolder = "Assets/Tiles/Palettes";
        public const string PalettePath = "Assets/Tiles/Palettes/Rune Palette.prefab";

        [MenuItem("GameObject/Rune Magic/Painted Map", false, 9)]
        public static void CreatePaintedMap()
        {
            EnsureTiles();
            var root = new GameObject("Map", typeof(Grid));
            var grid = root.GetComponent<Grid>();
            grid.cellSize = Vector3.one;
            var layer = new GameObject("Tiles");
            layer.transform.SetParent(root.transform, false);
            var map = layer.AddComponent<Tilemap>();
            map.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            var renderer = layer.AddComponent<TilemapRenderer>();
            renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            renderer.sortingOrder = 0;

            var wallsLayer = AddTileLayer(root.transform, "Walls", 2);
            var decorLayer = AddTileLayer(root.transform, "Environment Details", 3);
            var coverLayer = AddTileLayer(root.transform, "Cover", 4);

            var authoring = root.AddComponent<LevelAuthoring>();
            authoring.tiles = LevelTileSource.Tilemap;
            authoring.includeJsonProps = false;
            authoring.tilemap = map;
            authoring.walls = wallsLayer;
            authoring.overlays = coverLayer;
            authoring.decor = decorLayer;
            authoring.roomName = "Painted Map";
            StampShell(map, 13, 11);

            var spawn = new GameObject("Spawn");
            spawn.transform.SetParent(root.transform, false);
            spawn.transform.position = WorldGrid.Center(2, 5);
            authoring.spawnPoint = spawn.transform;

            Undo.RegisterCreatedObjectUndo(root, "Painted Map");
            Selection.activeGameObject = layer;
            EditorGUIUtility.PingObject(layer);
            Debug.Log("Painted Map created. Paint floors on Tiles, walls on Walls, props on Environment Details, ice/fire on Cover.");
        }

        static Tilemap AddTileLayer(Transform parent, string name, int order)
        {
            var host = new GameObject(name);
            host.transform.SetParent(parent, false);
            var map = host.AddComponent<Tilemap>();
            map.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            var renderer = host.AddComponent<TilemapRenderer>();
            renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            renderer.sortingOrder = order;
            return map;
        }

        [InitializeOnLoadMethod]
        static void AutoEnsureTiles()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                var stone = AssetDatabase.LoadAssetAtPath<WorldPaintTile>(FloorFolder + "/Floor-Stone.asset");
                var palette = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
                if (stone != null && palette != null)
                {
                    if (stone.sprite == null)
                    {
                        BindPackSprites();
                    }

                    return;
                }

                EnsureTiles();
                BindPackSprites();
            };
        }

        [MenuItem("Window/Rune Magic/Create Tile Palette")]
        public static void MenuEnsureTiles()
        {
            EnsureTiles();
            BindPackSprites();
            EditorUtility.DisplayDialog(
                "Tile Palette",
                "Tiles are in Assets/Tiles (Floor, Wall, Special, Cover).\n\n" +
                "Window → 2D → Tile Palette → Open Palette → Rune Palette.\n" +
                "Select the Tiles object in the scene and paint walk cells.\n" +
                "Select Cover to paint ice / fire / lightning / aura.\n" +
                "Click a tile asset to change its material, kind, cover, or sprite.",
                "OK");
        }

        [MenuItem("Window/Rune Magic/Bind Pack Sprites")]
        public static void MenuBindPackSprites()
        {
            EnsureTiles();
            var count = BindPackSprites();
            EditorUtility.DisplayDialog(
                "Pack sprites",
                count + " tile brushes now use sliced ElvGames sprites.\n\n" +
                "Open Window → 2D → Tile Palette → Rune Palette.\n" +
                "Select Map/Tiles, press F to frame the starter room, and paint.",
                "OK");
        }

        public static void EnsureTiles()
        {
            EnsureFolder("Assets/Tiles");
            EnsureFolder(FloorFolder);
            EnsureFolder(WallFolder);
            EnsureFolder(SpecialFolder);
            EnsureFolder(CoverFolder);
            EnsureFolder(PaletteFolder);
            TileAtlas.Ensure();

            var painted = new List<WorldPaintTile>();
            foreach (MaterialId material in Enum.GetValues(typeof(MaterialId)))
            {
                if (material == MaterialId.None)
                {
                    continue;
                }

                painted.Add(WriteTile(FloorFolder, "Floor-" + material, material, TileKind.Floor));
                if (material != MaterialId.Void)
                {
                    painted.Add(WriteTile(WallFolder, "Wall-" + material, material, TileKind.Wall));
                }
            }

            painted.Add(WriteTile(SpecialFolder, "Pit", MaterialId.Void, TileKind.Pit));
            painted.Add(WriteTile(SpecialFolder, "Door", MaterialId.Stone, TileKind.Door));
            painted.Add(WriteTile(SpecialFolder, "Bridge", MaterialId.Stone, TileKind.Bridge));
            painted.Add(WriteTile(CoverFolder, "Cover-Ice", MaterialId.Stone, TileKind.Floor, TileCover.Ice));
            painted.Add(WriteTile(CoverFolder, "Cover-Fire", MaterialId.Dirt, TileKind.Floor, TileCover.Fire));
            painted.Add(WriteTile(CoverFolder, "Cover-Lightning", MaterialId.Stone, TileKind.Floor, TileCover.Lightning));
            painted.Add(WriteTile(CoverFolder, "Cover-Water", MaterialId.Stone, TileKind.Floor, TileCover.Water));
            painted.Add(WriteTile(CoverFolder, "Cover-Vine", MaterialId.Dirt, TileKind.Floor, TileCover.Vine));
            painted.Add(WriteTile(CoverFolder, "Cover-Miasma", MaterialId.Miasma, TileKind.Floor, TileCover.Miasma, TileAura.Miasma));
            painted.Add(WriteTile(CoverFolder, "Cover-Fog", MaterialId.Cloud, TileKind.Floor, TileCover.Fog, TileAura.Fog));
            painted.Add(WriteTile(CoverFolder, "Cover-Cracks", MaterialId.Stone, TileKind.Floor, TileCover.Cracks));
            painted.Add(WriteTile(CoverFolder, "Cover-Seal", MaterialId.Stone, TileKind.Floor, TileCover.Seal));
            painted.Add(WriteTile(CoverFolder, "Aura-Fire", MaterialId.Stone, TileKind.Floor, TileCover.Fire, TileAura.Fire));
            painted.Add(WriteTile(CoverFolder, "Aura-Miasma", MaterialId.Stone, TileKind.Floor, TileCover.Miasma, TileAura.Miasma));
            painted.Add(WriteTile(CoverFolder, "Aura-Fog", MaterialId.Stone, TileKind.Floor, TileCover.Fog, TileAura.Fog));
            WritePalette(painted);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void StampShell(Tilemap map, int width, int height)
        {
            var floor = AssetDatabase.LoadAssetAtPath<WorldPaintTile>(FloorFolder + "/Floor-Stone.asset");
            var wall = AssetDatabase.LoadAssetAtPath<WorldPaintTile>(WallFolder + "/Wall-Stone.asset");
            if (floor == null || wall == null)
            {
                return;
            }

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    map.SetTile(new Vector3Int(x, y, 0), edge ? wall : floor);
                }
            }
        }

        static WorldPaintTile WriteTile(
            string folder,
            string name,
            MaterialId material,
            TileKind kind,
            TileCover cover = TileCover.None,
            TileAura aura = TileAura.None)
        {
            var path = folder + "/" + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<WorldPaintTile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<WorldPaintTile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.material = material;
            tile.kind = kind;
            tile.cover = cover;
            tile.aura = aura;
            if (tile.sprite == null)
            {
                tile.sprite = PackSprite(name) ?? tile.PreviewSprite();
            }

            EditorUtility.SetDirty(tile);
            return tile;
        }

        public static int BindPackSprites()
        {
            var changed = 0;
            foreach (var path in AssetDatabase.FindAssets("t:WorldPaintTile", new[] { TileRoot }))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(path);
                var tile = AssetDatabase.LoadAssetAtPath<WorldPaintTile>(assetPath);
                if (tile == null)
                {
                    continue;
                }

                var sprite = PackSprite(tile.name) ?? tile.PreviewSprite();
                if (sprite == null || tile.sprite == sprite)
                {
                    continue;
                }

                tile.sprite = sprite;
                EditorUtility.SetDirty(tile);
                changed++;
            }

            AssetDatabase.SaveAssets();
            return changed;
        }

        static readonly Dictionary<string, string> PackTiles = new()
        {
            ["Floor-Stone"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Crypt/Tiles/RA_Crypt_1.asset",
            ["Floor-Dirt"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Caverns/Tiles/RA_Cavern_4.asset",
            ["Floor-Water"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Caverns/Tiles/RA_Cavern_24.asset",
            ["Floor-Mud"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Caverns/Tiles/RA_Cavern_16.asset",
            ["Floor-Ash"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Hell/Tiles/RA_Hell_20.asset",
            ["Floor-Ice"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Sanctuary/Tiles/RA_Sanctuary_0.asset",
            ["Floor-Lava"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Hell/Tiles/RA_Hell_21.asset",
            ["Floor-Moss"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Jungle/Tiles/RA_Jungle_0.asset",
            ["Floor-Plant"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Jungle/Tiles/RA_Jungle_1.asset",
            ["Floor-Grove"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Jungle/Tiles/RA_Jungle_2.asset",
            ["Wall-Stone"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Crypt/Tiles/RA_Crypt_31.asset",
            ["Wall-Moss"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Jungle/Tiles/RA_Jungle_10.asset",
            ["Wall-Ice"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Sanctuary/Tiles/RA_Sanctuary_16.asset",
            ["Wall-Lava"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Hell/Tiles/RA_Hell_8.asset",
            ["Door"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Crypt/Tiles/RA_Crypt_80.asset",
            ["Pit"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Caverns/Tiles/RA_Cavern_1.asset",
            ["Bridge"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Caverns/Tiles/RA_Cavern_40.asset",
            ["Cover-Ice"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Sanctuary/Tiles/RA_Sanctuary_0.asset",
            ["Cover-Fire"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Hell/Tiles/RA_Hell_21.asset",
            ["Cover-Water"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Caverns/Tiles/RA_Cavern_24.asset",
            ["Cover-Vine"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Jungle/Tiles/RA_Jungle_5.asset",
            ["Cover-Lightning"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Atlantis/Tiles/RA_Atlantis_20.asset",
            ["Cover-Seal"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Atlantis/Tiles/RA_Atlantis_16.asset",
            ["Cover-Cracks"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Crypt/Tiles/RA_Crypt_50.asset",
            ["Aura-Fire"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Hell/Tiles/RA_Hell_22.asset",
            ["Aura-Miasma"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Jungle/Tiles/RA_Jungle_3.asset",
            ["Aura-Fog"] = "Assets/ElvGames/Rogue Adventure/Tilesets/Hell/Tiles/RA_Hell_10.asset",
        };

        static Sprite PackSprite(string name)
        {
            if (!PackTiles.TryGetValue(name, out var path))
            {
                if (name.StartsWith("Wall-"))
                {
                    path = PackTiles["Wall-Stone"];
                }
                else if (name.StartsWith("Floor-") || name.StartsWith("Cover-") || name.StartsWith("Aura-"))
                {
                    path = PackTiles["Floor-Stone"];
                }
                else
                {
                    return null;
                }
            }

            var pack = AssetDatabase.LoadAssetAtPath<Tile>(path);
            return pack != null ? pack.sprite : null;
        }

        static void WritePalette(List<WorldPaintTile> tiles)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
            if (existing != null)
            {
                return;
            }

            var root = new GameObject("Rune Palette", typeof(Grid));
            root.GetComponent<Grid>().cellSize = Vector3.one;
            var layer = new GameObject("Layer");
            layer.transform.SetParent(root.transform, false);
            var map = layer.AddComponent<Tilemap>();
            map.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            layer.AddComponent<TilemapRenderer>();
            const int cols = 8;
            for (var i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] != null)
                {
                    map.SetTile(new Vector3Int(i % cols, -(i / cols), 0), tiles[i]);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, PalettePath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(leaf))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }

    [CustomEditor(typeof(WorldPaintTile))]
    public sealed class WorldPaintTileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var tile = (WorldPaintTile)target;
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                if (tile.sprite == null)
                {
                    tile.sprite = tile.PreviewSprite();
                }

                EditorUtility.SetDirty(tile);
            }

            EditorGUILayout.HelpBox(
                "Paint this from Window → 2D → Tile Palette. Kind = Floor is walkable ground. Kind = None is look only — not a floor, on any layer. Cover and aura are overlays — paint those on the Cover Tilemap so they sit on top of an existing floor. Drag a sliced sprite onto Sprite to replace the atlas look.",
                MessageType.Info);
            if (GUILayout.Button("Refresh sprite from atlas"))
            {
                tile.sprite = tile.PreviewSprite();
                EditorUtility.SetDirty(tile);
            }
        }
    }
}
#endif
