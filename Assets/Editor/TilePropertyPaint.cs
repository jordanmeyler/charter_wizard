#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Paint looks first (any palette), then stamp Kind / Material / Cover /
    /// Aura / Blocks onto those cells without changing the sprite.
    /// </summary>
    public sealed class TilePropertyPaint : EditorWindow
    {
        const string Folder = "Assets/Tiles/Authored";

        static readonly Dictionary<string, WorldPaintTile> Cache = new();

        TileKind _kind = TileKind.Floor;
        MaterialId _material = MaterialId.Stone;
        TileCover _cover = TileCover.None;
        TileAura _aura = TileAura.None;
        bool _blocks;
        bool _applyKind = true;
        bool _applyMaterial = true;
        bool _applyCover;
        bool _applyAura;
        bool _applyBlocks;
        bool _paint;
        bool _coverLayer;
        Tilemap _hoverMap;
        Vector3Int _hoverCell;
        bool _hasHover;

        [MenuItem("Window/Rune Magic/Tile Properties")]
        public static void Open()
        {
            GetWindow<TilePropertyPaint>("Tile Properties");
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnScene;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnScene;
            AssetDatabase.SaveAssets();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Paint the map with any palette first. Then turn on Paint and click those cells to assign gameplay. The picture stays; Kind and Material are what Play uses. Right-click a cell to copy its properties.",
                MessageType.Info);

            _paint = EditorGUILayout.Toggle("Paint in Scene view", _paint);
            _coverLayer = EditorGUILayout.Toggle("Write onto Cover layer", _coverLayer);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stamp", EditorStyles.boldLabel);
            DrawField("Kind", ref _applyKind, () => _kind = (TileKind)EditorGUILayout.EnumPopup(_kind));
            DrawField("Material", ref _applyMaterial, () => _material = (MaterialId)EditorGUILayout.EnumPopup(_material));
            DrawField("Cover", ref _applyCover, () => _cover = (TileCover)EditorGUILayout.EnumPopup(_cover));
            DrawField("Aura", ref _applyAura, () => _aura = (TileAura)EditorGUILayout.EnumPopup(_aura));
            DrawField("Blocks", ref _applyBlocks, () => _blocks = EditorGUILayout.Toggle(_blocks));

            EditorGUILayout.Space();
            if (_hasHover && _hoverMap != null)
            {
                var tile = _hoverMap.GetTile(_hoverCell);
                var paint = tile as WorldPaintTile;
                EditorGUILayout.LabelField("Under cursor", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Cell", _hoverCell.x + ", " + _hoverCell.y);
                EditorGUILayout.LabelField("Look", tile != null ? tile.name : "(empty)");
                if (paint != null)
                {
                    EditorGUILayout.LabelField("Kind", paint.kind.ToString());
                    EditorGUILayout.LabelField("Material", paint.material.ToString());
                    EditorGUILayout.LabelField("Cover", paint.cover.ToString());
                    EditorGUILayout.LabelField("Aura", paint.aura.ToString());
                    EditorGUILayout.LabelField("Blocks", paint.blocks ? "yes" : "no");
                }
                else if (tile != null)
                {
                    EditorGUILayout.LabelField("Properties", "not set — Play will guess from the name");
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Select the layer first. Environment Details is its own stamp — check only Blocks and drag across a cluster of tables or statues to give that group collision. Timber on a table burns to an ash pile. Ice / fire / vine / aura can sit on the same cell, or toggle Write onto Cover layer so they overlay without touching Kind.",
                MessageType.None);
        }

        static void DrawField(string label, ref bool apply, System.Action drawer)
        {
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            EditorGUILayout.PrefixLabel(label);
            using (new EditorGUI.DisabledScope(!apply))
            {
                drawer();
            }

            EditorGUILayout.EndHorizontal();
        }

        void OnScene(SceneView view)
        {
            var map = ResolveMap();
            if (map == null)
            {
                _hasHover = false;
                return;
            }

            var world = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            world.z = 0f;
            var cell = map.WorldToCell(world);
            _hoverMap = map;
            _hoverCell = cell;
            _hasHover = true;
            Repaint();

            var look = _coverLayer ? WalkMap() : map;
            var tile = look != null ? look.GetTile(cell) : map.GetTile(cell);
            if (tile != null)
            {
                var min = map.CellToWorld(cell);
                var max = map.CellToWorld(cell + Vector3Int.one);
                var fill = _paint ? new Color(0.72f, 0.55f, 1f, 0.22f) : new Color(1f, 1f, 1f, 0.06f);
                Handles.DrawSolidRectangleWithOutline(
                    new[]
                    {
                        new Vector3(min.x, min.y, 0f),
                        new Vector3(max.x, min.y, 0f),
                        new Vector3(max.x, max.y, 0f),
                        new Vector3(min.x, max.y, 0f)
                    },
                    fill,
                    new Color(0.72f, 0.55f, 1f, 0.9f));
            }

            if (!_paint)
            {
                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var ev = Event.current;
            if (ev.button == 1 && (ev.type == EventType.MouseDown || ev.type == EventType.MouseDrag))
            {
                Sample(map, cell);
                ev.Use();
                return;
            }

            if (ev.button == 0 && (ev.type == EventType.MouseDown || ev.type == EventType.MouseDrag))
            {
                Stamp(map, cell);
                ev.Use();
            }

            if (ev.type == EventType.MouseUp && ev.button == 0)
            {
                AssetDatabase.SaveAssets();
            }
        }

        void Sample(Tilemap map, Vector3Int cell)
        {
            var paint = map.GetTile<WorldPaintTile>(cell);
            if (paint == null)
            {
                return;
            }

            _kind = paint.kind;
            _material = paint.material;
            _cover = paint.cover;
            _aura = paint.aura;
            _blocks = paint.blocks;
            Repaint();
        }

        void Stamp(Tilemap map, Vector3Int cell)
        {
            var raw = map.GetTile(cell);
            if (raw == null && _coverLayer)
            {
                var walk = WalkMap();
                raw = walk != null ? walk.GetTile(cell) : null;
            }

            if (raw == null)
            {
                return;
            }

            var current = raw as WorldPaintTile;
            var sprite = SpriteOf(raw);
            var kind = _applyKind ? _kind : (current != null ? current.kind : GuessKind(raw));
            var material = _applyMaterial ? _material : (current != null ? current.material : GuessMaterial(raw));
            var cover = _applyCover ? _cover : (current != null ? current.cover : TileCover.None);
            var aura = _applyAura ? _aura : (current != null ? current.aura : TileAura.None);
            var blocks = _applyBlocks ? _blocks : current != null && current.blocks;
            if (_coverLayer)
            {
                kind = current != null ? current.kind : TileKind.Floor;
                material = current != null ? current.material : MaterialId.Stone;
                blocks = false;
            }

            var authored = EnsureTile(sprite, kind, material, cover, aura, blocks);
            if (authored == null || authored == raw)
            {
                return;
            }

            Undo.RecordObject(map, "Assign tile properties");
            map.SetTile(cell, authored);
            EditorUtility.SetDirty(map);
        }

        Tilemap ResolveMap()
        {
            var selected = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<Tilemap>()
                : null;
            if (selected != null && !_coverLayer)
            {
                return selected;
            }

            var authoring = Object.FindFirstObjectByType<LevelAuthoring>();
            if (_coverLayer)
            {
                if (selected != null && selected.gameObject.name.ToLowerInvariant().Contains("cover"))
                {
                    return selected;
                }

                if (authoring != null && authoring.overlays != null)
                {
                    return authoring.overlays;
                }

                var tiles = authoring != null ? authoring.tilemap : TilemapLevel.FindPaintedMap();
                return NamedSibling(tiles, "cover") ?? WalkMap();
            }

            return WalkMap();
        }

        static Tilemap NamedSibling(Tilemap from, string name)
        {
            if (from == null || from.transform.parent == null)
            {
                return null;
            }

            var maps = from.transform.parent.GetComponentsInChildren<Tilemap>(true);
            for (var i = 0; i < maps.Length; i++)
            {
                if (maps[i].gameObject.name.ToLowerInvariant().Contains(name))
                {
                    return maps[i];
                }
            }

            return null;
        }

        static Tilemap WalkMap()
        {
            var selected = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<Tilemap>()
                : null;
            if (selected != null)
            {
                return selected;
            }

            var authoring = Object.FindFirstObjectByType<LevelAuthoring>();
            if (authoring != null && authoring.tilemap != null)
            {
                return authoring.tilemap;
            }

            return TilemapLevel.FindPaintedMap();
        }

        static Sprite SpriteOf(TileBase tile)
        {
            return tile is Tile painted ? painted.sprite : null;
        }

        static TileKind GuessKind(TileBase tile)
        {
            return TilemapLevel.GuessKindForEditor(tile);
        }

        static MaterialId GuessMaterial(TileBase tile)
        {
            return TilemapLevel.GuessMaterialForEditor(tile);
        }

        static WorldPaintTile EnsureTile(
            Sprite sprite,
            TileKind kind,
            MaterialId material,
            TileCover cover,
            TileAura aura,
            bool blocks)
        {
            var key = Key(sprite, kind, material, cover, aura, blocks);
            if (Cache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Tiles"))
            {
                AssetDatabase.CreateFolder("Assets", "Tiles");
            }

            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets/Tiles", "Authored");
            }

            var path = Folder + "/" + key + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<WorldPaintTile>(path);
            if (tile == null)
            {
                tile = CreateInstance<WorldPaintTile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = sprite;
            tile.kind = kind;
            tile.material = material;
            tile.cover = cover;
            tile.aura = aura;
            tile.blocks = blocks;
            EditorUtility.SetDirty(tile);
            Cache[key] = tile;
            return tile;
        }

        static string Key(
            Sprite sprite,
            TileKind kind,
            MaterialId material,
            TileCover cover,
            TileAura aura,
            bool blocks)
        {
            var guid = "none";
            long local = 0;
            if (sprite != null)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out guid, out local);
            }

            var sign = local < 0 ? "n" : "p";
            var solid = blocks ? "block" : "open";
            return kind + "_" + material + "_" + cover + "_" + aura + "_" + solid + "_" + guid + "_" + sign + Mathf.Abs(local);
        }
    }
}
#endif
