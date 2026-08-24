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

            var authoring = root.AddComponent<LevelAuthoring>();
            authoring.tiles = LevelTileSource.Tilemap;
            authoring.includeJsonProps = false;
            authoring.tilemap = map;
            authoring.roomName = "Painted Map";
            StampShell(map, 13, 11);

            var spawn = new GameObject("Spawn");
            spawn.transform.SetParent(root.transform, false);
            spawn.transform.position = WorldGrid.Center(2, 5);
            authoring.spawnPoint = spawn.transform;

            Undo.RegisterCreatedObjectUndo(root, "Painted Map");
            Selection.activeGameObject = layer;
            EditorGUIUtility.PingObject(layer);
            Debug.Log("Painted Map created. Window → 2D → Tile Palette, open Rune Palette, and paint onto Tiles.");
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

                if (AssetDatabase.LoadAssetAtPath<WorldPaintTile>(FloorFolder + "/Floor-Stone.asset") != null)
                {
                    return;
                }

                EnsureTiles();
            };
        }

        [MenuItem("Window/Rune Magic/Create Tile Palette")]
        public static void MenuEnsureTiles()
        {
            EnsureTiles();
            EditorUtility.DisplayDialog(
                "Tile Palette",
                "Tiles are in Assets/Tiles (Floor, Wall, Special).\n\n" +
                "Window → 2D → Tile Palette → Open Palette → Rune Palette.\n" +
                "Select the Tiles object in the scene and paint.\n" +
                "Click a tile asset to change its material, kind, or sprite.",
                "OK");
        }

        public static void EnsureTiles()
        {
            EnsureFolder("Assets/Tiles");
            EnsureFolder(FloorFolder);
            EnsureFolder(WallFolder);
            EnsureFolder(SpecialFolder);
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

        static WorldPaintTile WriteTile(string folder, string name, MaterialId material, TileKind kind)
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
            if (tile.sprite == null)
            {
                tile.sprite = tile.PreviewSprite();
            }

            EditorUtility.SetDirty(tile);
            return tile;
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
                "Paint this from Window → 2D → Tile Palette. Material and kind are what Play bakes into the grid. Cover is ice / fire / lightning over the walk tile. Drag a sliced sprite onto Sprite to replace the atlas look.",
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
