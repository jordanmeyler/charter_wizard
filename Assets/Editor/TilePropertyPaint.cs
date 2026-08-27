#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Rendering;
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
        bool _applyOpacity;
        float _opacity = 0.42f;
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
            _ = StampOverlay.Enabled;
            SceneView.RepaintAll();
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnScene;
            AssetDatabase.SaveAssets();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Stamps show in the Scene view — not the Game tab. Nothing else to turn on. If you still see no colours: click the Scene tab, turn on Gizmos at the top-right of that view, and keep Play off. Window → Rune Magic → Show Stamps should be checked.",
                MessageType.Info);

            _paint = EditorGUILayout.Toggle("Paint in Scene view", _paint);
            _coverLayer = EditorGUILayout.Toggle("Write onto Cover layer", _coverLayer);
            var show = EditorGUILayout.Toggle("Show stamps in Scene view", StampOverlay.Enabled);
            if (show != StampOverlay.Enabled)
            {
                StampOverlay.Enabled = show;
            }

            if (StampOverlay.Enabled)
            {
                var lookOnly = EditorGUILayout.Toggle("Outline look-only cells", StampOverlay.ShowLookOnly);
                if (lookOnly != StampOverlay.ShowLookOnly)
                {
                    StampOverlay.ShowLookOnly = lookOnly;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Legend", EditorStyles.boldLabel);
                StampOverlay.DrawLegendGui();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stamp", EditorStyles.boldLabel);
            DrawField("Kind", ref _applyKind, () => _kind = (TileKind)EditorGUILayout.EnumPopup(_kind));
            DrawField("Material", ref _applyMaterial, () => _material = (MaterialId)EditorGUILayout.EnumPopup(_material));
            DrawField("Cover", ref _applyCover, () => _cover = (TileCover)EditorGUILayout.EnumPopup(_cover));
            DrawField("Aura", ref _applyAura, () => _aura = (TileAura)EditorGUILayout.EnumPopup(_aura));
            DrawField("Blocks", ref _applyBlocks, () => _blocks = EditorGUILayout.Toggle(_blocks));
            DrawField("Opacity", ref _applyOpacity, () =>
            {
                _opacity = EditorGUILayout.Slider(_opacity, 0f, 1f);
            });

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
                    EditorGUILayout.LabelField("Opacity", Mathf.RoundToInt(paint.ResolvedOpacity() * 100f) + "%");
                }
                else if (tile != null)
                {
                    EditorGUILayout.LabelField("Properties", "not set — Play will guess from the name");
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Select the layer first. Environment Details is its own stamp — check only Blocks and drag across a cluster of tables or statues to give that group collision. Timber on a table burns to an ash pile. Ice / fire / vine / aura can sit on the same cell, or toggle Write onto Cover layer so they overlay without touching Kind. Miasma and fog are see-through unless you stamp Opacity. Blank Tiles cells are pits at Play — erase a hole or leave the map edge empty. Stamp Kind=Pit only when you painted a pit look that would otherwise stay floor.",
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

                if (StampOverlay.Enabled)
                {
                    Handles.Label(
                        new Vector3((min.x + max.x) * 0.5f, max.y + 0.08f, 0f),
                        StampOverlay.Describe(tile));
                }
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
            _opacity = paint.opacity > 0.001f ? paint.opacity : paint.ResolvedOpacity();
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
            var opacity = _applyOpacity ? _opacity : (current != null ? current.opacity : 0f);
            if (_coverLayer)
            {
                kind = current != null ? current.kind : TileKind.Floor;
                material = current != null ? current.material : MaterialId.Stone;
                blocks = false;
            }

            var authored = EnsureTile(sprite, kind, material, cover, aura, blocks, opacity);
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
            bool blocks,
            float opacity)
        {
            var key = Key(sprite, kind, material, cover, aura, blocks, opacity);
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
            tile.opacity = opacity;
            tile.color = new Color(1f, 1f, 1f, tile.ResolvedOpacity());
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
            bool blocks,
            float opacity)
        {
            var guid = "none";
            long local = 0;
            if (sprite != null)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out guid, out local);
            }

            var sign = local < 0 ? "n" : "p";
            var solid = blocks ? "block" : "open";
            var fade = Mathf.RoundToInt(Mathf.Clamp01(opacity) * 100f);
            return kind + "_" + material + "_" + cover + "_" + aura + "_" + solid + "_a" + fade + "_" + guid + "_" + sign + Mathf.Abs(local);
        }
    }

    /// <summary>
    /// Scene-view outline for each stamp. Adjacent cells of the same stamp
    /// share one glow so the painted tiles stay readable.
    /// </summary>
    [InitializeOnLoad]
    static class StampOverlay
    {
        const string EnabledPref = "RuneMagic.ShowStampOverlay.v2";
        const string LookOnlyPref = "RuneMagic.ShowStampLookOnly";

        static readonly Color Pit = new(0.95f, 0.2f, 0.75f, 1f);
        static readonly Color BlankPit = new(0.72f, 0.12f, 0.55f, 1f);
        static readonly Color Door = new(0.95f, 0.62f, 0.12f, 1f);
        static readonly Color Bridge = new(0.78f, 0.58f, 0.28f, 1f);
        static readonly Color Blocks = new(1f, 0.22f, 0.22f, 1f);
        static readonly Color AuraMiasma = new(0.3f, 0.88f, 0.22f, 1f);
        static readonly Color AuraFire = new(1f, 0.4f, 0.08f, 1f);
        static readonly Color AuraFog = new(0.82f, 0.86f, 0.92f, 1f);
        static readonly Color CoverIce = new(0.55f, 0.88f, 1f, 1f);
        static readonly Color CoverFire = new(1f, 0.48f, 0.12f, 1f);
        static readonly Color CoverLightning = new(1f, 0.92f, 0.2f, 1f);
        static readonly Color CoverWater = new(0.2f, 0.52f, 1f, 1f);
        static readonly Color CoverVine = new(0.38f, 0.78f, 0.22f, 1f);
        static readonly Color CoverOther = new(0.75f, 0.42f, 0.9f, 1f);
        static readonly Color LookOnly = new(1f, 0.86f, 0.15f, 1f);
        static readonly Color[] MaterialTones;
        static readonly List<Color> SeenColors = new();
        static readonly List<string> SeenLabels = new();
        static readonly Dictionary<int, int> PerMapCount = new();
        static Vector2 _legendScroll;
        public static int LastStampCount { get; private set; }

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledPref, true);
            set
            {
                if (EditorPrefs.GetBool(EnabledPref, true) == value)
                {
                    return;
                }

                EditorPrefs.SetBool(EnabledPref, value);
                SceneView.RepaintAll();
            }
        }

        public static bool ShowLookOnly
        {
            get => EditorPrefs.GetBool(LookOnlyPref, false);
            set
            {
                if (EditorPrefs.GetBool(LookOnlyPref, false) == value)
                {
                    return;
                }

                EditorPrefs.SetBool(LookOnlyPref, value);
                SceneView.RepaintAll();
            }
        }

        [InitializeOnLoadMethod]
        static void HookScene()
        {
            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;
        }

        [MenuItem("Window/Rune Magic/Show Stamps", priority = 1)]
        static void ToggleMenu()
        {
            Enabled = !Enabled;
        }

        [MenuItem("Window/Rune Magic/Show Stamps", true)]
        static bool ToggleMenuValidate()
        {
            Menu.SetChecked("Window/Rune Magic/Show Stamps", Enabled);
            return true;
        }

        static StampOverlay()
        {
            HookScene();
            var max = 0;
            foreach (MaterialId id in System.Enum.GetValues(typeof(MaterialId)))
            {
                max = Mathf.Max(max, (int)id);
            }

            MaterialTones = new Color[max + 1];
            Tone(MaterialId.None, 0.55f, 0.55f, 0.58f);
            Tone(MaterialId.Stone, 0.95f, 0.82f, 0.38f);
            Tone(MaterialId.Ash, 0.68f, 0.6f, 0.54f);
            Tone(MaterialId.Timber, 0.8f, 0.5f, 0.18f);
            Tone(MaterialId.Hearth, 0.88f, 0.32f, 0.2f);
            Tone(MaterialId.Ember, 1f, 0.36f, 0.08f);
            Tone(MaterialId.Damp, 0.32f, 0.55f, 0.78f);
            Tone(MaterialId.Vein, 0.95f, 0.86f, 0.22f);
            Tone(MaterialId.Scoured, 0.62f, 0.7f, 0.76f);
            Tone(MaterialId.Moss, 0.42f, 0.72f, 0.22f);
            Tone(MaterialId.Metal, 0.7f, 0.76f, 0.86f);
            Tone(MaterialId.SaltCrust, 0.92f, 0.88f, 0.78f);
            Tone(MaterialId.Void, 0.48f, 0.16f, 0.55f);
            Tone(MaterialId.Ice, 0.35f, 0.9f, 1f);
            Tone(MaterialId.Sand, 0.9f, 0.74f, 0.32f);
            Tone(MaterialId.Mud, 0.5f, 0.32f, 0.16f);
            Tone(MaterialId.Lava, 1f, 0.28f, 0.05f);
            Tone(MaterialId.Steam, 0.78f, 0.86f, 0.9f);
            Tone(MaterialId.Dust, 0.74f, 0.64f, 0.46f);
            Tone(MaterialId.Glass, 0.32f, 0.78f, 0.84f);
            Tone(MaterialId.Crystal, 0.74f, 0.42f, 0.96f);
            Tone(MaterialId.Obsidian, 0.32f, 0.2f, 0.52f);
            Tone(MaterialId.Grove, 0.18f, 0.62f, 0.26f);
            Tone(MaterialId.Cloud, 0.84f, 0.9f, 0.98f);
            Tone(MaterialId.Rain, 0.28f, 0.48f, 0.8f);
            Tone(MaterialId.Snow, 0.95f, 0.97f, 1f);
            Tone(MaterialId.Glacier, 0.58f, 0.8f, 0.92f);
            Tone(MaterialId.Acid, 0.72f, 0.95f, 0.12f);
            Tone(MaterialId.Water, 0.12f, 0.46f, 0.98f);
            Tone(MaterialId.Plant, 0.28f, 0.82f, 0.3f);
            Tone(MaterialId.Dirt, 0.72f, 0.42f, 0.18f);
            Tone(MaterialId.Oil, 0.42f, 0.3f, 0.08f);
            Tone(MaterialId.Miasma, 0.4f, 0.72f, 0.12f);
            Tone(MaterialId.Wardstone, 0.56f, 0.4f, 0.78f);
            Tone(MaterialId.Aegis, 0.86f, 0.82f, 0.28f);
        }

        static void Tone(MaterialId id, float r, float g, float b)
        {
            MaterialTones[(int)id] = new Color(r, g, b, 1f);
        }

        static void OnScene(SceneView view)
        {
            if (!Enabled || Application.isPlaying || Event.current.type != EventType.Repaint)
            {
                return;
            }

            DrawCells();
            DrawOpenPits();
            DrawSceneLegend();
        }

        public static string Describe(TileBase tile)
        {
            if (tile is WorldPaintTile paint)
            {
                var text = paint.kind + " / " + paint.material;
                if (paint.cover != TileCover.None)
                {
                    text += " / " + paint.cover;
                }

                if (paint.aura != TileAura.None)
                {
                    text += " / " + paint.aura;
                }

                if (paint.blocks)
                {
                    text += " / blocks";
                }

                return text;
            }

            return tile != null ? tile.name + " (look only)" : "";
        }

        public static void DrawLegendGui()
        {
            var n = LastStampCount;
            EditorGUILayout.LabelField(n > 0
                ? n + " stamped cells in the Scene view"
                : "No stamped cells in view — stamp a cell or turn on look-only outlines.");
            EditorGUILayout.LabelField("Materials", EditorStyles.miniBoldLabel);
            _legendScroll = EditorGUILayout.BeginScrollView(_legendScroll, GUILayout.MaxHeight(220f));
            foreach (MaterialId id in System.Enum.GetValues(typeof(MaterialId)))
            {
                if (id == MaterialId.None)
                {
                    continue;
                }

                Swatch(MaterialColor(id), MaterialCatalog.Of(id).Name);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Other stamps", EditorStyles.miniBoldLabel);
            Swatch(Pit, "Pit");
            Swatch(BlankPit, "Blank space (pit at Play)");
            Swatch(Door, "Door");
            Swatch(AuraMiasma, "Miasma (aura)");
            Swatch(CoverIce, "Ice cover");
            Swatch(Blocks, "Blocks");
            if (ShowLookOnly)
            {
                Swatch(LookOnly, "Look only — Play guesses");
            }
        }

        static void Swatch(Color color, string label)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = GUILayoutUtility.GetRect(14f, 14f, GUILayout.Width(14f), GUILayout.Height(14f));
            EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, 1f));
            GUILayout.Label(label, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        static void DrawCells()
        {
            SeenColors.Clear();
            SeenLabels.Clear();
            LastStampCount = 0;
            var maps = CollectMaps();
            var oldZ = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            for (var i = 0; i < maps.Count; i++)
            {
                DrawMap(maps[i]);
            }

            Handles.zTest = oldZ;
        }

        static void DrawMap(Tilemap map)
        {
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var tile = map.GetTile(cell);
                if (tile == null || !TryColor(tile, out var color))
                {
                    continue;
                }

                LastStampCount++;
                Note(color, StampLabel(tile));
                DrawCellGlow(map, cell, color);
            }
        }

        static void DrawOpenPits()
        {
            var maps = CollectMaps();
            Tilemap guide = null;
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;
            var occupied = new HashSet<Vector3Int>();
            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                if (map == null || LayerOrder(map) > 2)
                {
                    continue;
                }

                guide = guide != null ? guide : map;
                foreach (var cell in map.cellBounds.allPositionsWithin)
                {
                    if (map.GetTile(cell) == null)
                    {
                        continue;
                    }

                    occupied.Add(new Vector3Int(cell.x, cell.y, 0));
                    minX = Mathf.Min(minX, cell.x);
                    minY = Mathf.Min(minY, cell.y);
                    maxX = Mathf.Max(maxX, cell.x);
                    maxY = Mathf.Max(maxY, cell.y);
                }
            }

            if (guide == null || occupied.Count == 0)
            {
                return;
            }

            var oldZ = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            var any = false;
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (occupied.Contains(cell))
                    {
                        continue;
                    }

                    any = true;
                    DrawCellGlow(guide, cell, BlankPit);
                }
            }

            Handles.zTest = oldZ;
            if (any)
            {
                Note(BlankPit, "Blank space (pit at Play)");
            }
        }

        static void DrawCellGlow(Tilemap map, Vector3Int cell, Color color)
        {
            var center = map.GetCellCenterWorld(cell);
            var size = map.layoutGrid != null ? map.layoutGrid.cellSize : map.cellSize;
            var half = new Vector3(size.x * 0.48f, size.y * 0.48f, 0f);
            var min = center - half;
            var max = center + half;
            var verts = new[]
            {
                new Vector3(min.x, min.y, 0f),
                new Vector3(max.x, min.y, 0f),
                new Vector3(max.x, max.y, 0f),
                new Vector3(min.x, max.y, 0f)
            };
            Handles.DrawSolidRectangleWithOutline(
                verts,
                new Color(color.r, color.g, color.b, 0.16f),
                new Color(color.r, color.g, color.b, 0.95f));
        }

        public static void DrawGizmosFor(Tilemap map)
        {
            if (!Enabled || map == null || Application.isPlaying)
            {
                return;
            }

            var n = 0;
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var tile = map.GetTile(cell);
                if (tile == null || !TryColor(tile, out var color))
                {
                    continue;
                }

                n++;
                Note(color, StampLabel(tile));
                var center = map.GetCellCenterWorld(cell);
                var next = map.GetCellCenterWorld(cell + Vector3Int.right);
                var up = map.GetCellCenterWorld(cell + Vector3Int.up);
                var size = new Vector3(
                    Mathf.Max(0.2f, Vector3.Distance(center, next) * 0.92f),
                    Mathf.Max(0.2f, Vector3.Distance(center, up) * 0.92f),
                    0.02f);
                Gizmos.color = new Color(color.r, color.g, color.b, 0.22f);
                Gizmos.DrawCube(center, size);
                Gizmos.color = new Color(color.r, color.g, color.b, 1f);
                Gizmos.DrawWireCube(center, size);
            }

            PerMapCount[map.GetInstanceID()] = n;
            var total = 0;
            foreach (var pair in PerMapCount)
            {
                total += pair.Value;
            }

            LastStampCount = total;
        }

        static List<Tilemap> CollectMaps()
        {
            var found = Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var maps = new List<Tilemap>(found.Length);
            for (var i = 0; i < found.Length; i++)
            {
                if (found[i] != null && found[i].gameObject.scene.IsValid())
                {
                    maps.Add(found[i]);
                }
            }

            maps.Sort((a, b) => LayerOrder(a).CompareTo(LayerOrder(b)));
            return maps;
        }

        static int LayerOrder(Tilemap map)
        {
            var name = map.gameObject.name.ToLowerInvariant();
            if (name.IndexOf("cover", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("overlay", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("aura", System.StringComparison.Ordinal) >= 0)
            {
                return 3;
            }

            if (name.IndexOf("detail", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("decor", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("environment", System.StringComparison.Ordinal) >= 0)
            {
                return 2;
            }

            if (name.IndexOf("wall", System.StringComparison.Ordinal) >= 0)
            {
                return 1;
            }

            return 0;
        }

        static bool TryColor(TileBase tile, out Color color)
        {
            if (tile is not WorldPaintTile paint)
            {
                color = LookOnly;
                return ShowLookOnly;
            }

            if (paint.kind == TileKind.Pit)
            {
                color = Pit;
                return true;
            }

            if (paint.kind == TileKind.Door)
            {
                color = Door;
                return true;
            }

            if (paint.kind == TileKind.Bridge)
            {
                color = Bridge;
                return true;
            }

            if (paint.blocks)
            {
                color = Blocks;
                return true;
            }

            if (paint.aura == TileAura.Miasma)
            {
                color = AuraMiasma;
                return true;
            }

            if (paint.aura == TileAura.Fire)
            {
                color = AuraFire;
                return true;
            }

            if (paint.aura == TileAura.Fog)
            {
                color = AuraFog;
                return true;
            }

            switch (paint.cover)
            {
                case TileCover.Ice:
                    color = CoverIce;
                    return true;
                case TileCover.Fire:
                    color = CoverFire;
                    return true;
                case TileCover.Lightning:
                    color = CoverLightning;
                    return true;
                case TileCover.Water:
                    color = CoverWater;
                    return true;
                case TileCover.Vine:
                    color = CoverVine;
                    return true;
                case TileCover.Cracks:
                case TileCover.Seal:
                    color = CoverOther;
                    return true;
            }

            color = MaterialColor(paint.material);
            if (paint.kind == TileKind.Wall)
            {
                color = Color.Lerp(color, new Color(0.12f, 0.12f, 0.14f), 0.32f);
            }

            return true;
        }

        static Color MaterialColor(MaterialId id)
        {
            var index = (int)id;
            if (index >= 0 && index < MaterialTones.Length && MaterialTones[index].a > 0f)
            {
                return MaterialTones[index];
            }

            var tone = MaterialCatalog.Of(id).FloorTone;
            Color.RGBToHSV(tone, out var h, out var s, out var v);
            return Color.HSVToRGB(h, Mathf.Max(0.45f, s), Mathf.Max(0.72f, v));
        }

        static string StampLabel(TileBase tile)
        {
            if (tile is WorldPaintTile paint)
            {
                if (paint.kind == TileKind.Pit)
                {
                    return "Pit";
                }

                if (paint.kind == TileKind.Door)
                {
                    return "Door";
                }

                if (paint.blocks)
                {
                    return "Blocks";
                }

                if (paint.aura != TileAura.None)
                {
                    return paint.aura.ToString();
                }

                if (paint.cover != TileCover.None)
                {
                    return paint.cover + " cover";
                }

                return paint.kind + " / " + MaterialCatalog.Of(paint.material).Name;
            }

            return "Look only";
        }

        static void Note(Color color, string label)
        {
            for (var i = 0; i < SeenLabels.Count; i++)
            {
                if (SeenLabels[i] == label)
                {
                    return;
                }
            }

            SeenLabels.Add(label);
            SeenColors.Add(color);
        }

        static void DrawSceneLegend()
        {
            Handles.BeginGUI();
            var count = Mathf.Min(SeenLabels.Count, 16);
            var extra = SeenLabels.Count - count;
            var height = 28f + count * 16f + (extra > 0 ? 16f : 0f);
            var box = new Rect(12f, 12f, 188f, height);
            EditorGUI.DrawRect(box, new Color(0.08f, 0.08f, 0.1f, 0.72f));
            GUILayout.BeginArea(new Rect(box.x + 8f, box.y + 6f, box.width - 16f, box.height - 10f));
            GUILayout.Label("Stamps", EditorStyles.boldLabel);
            for (var i = 0; i < count; i++)
            {
                SceneSwatch(SeenColors[i], SeenLabels[i]);
            }

            if (extra > 0)
            {
                GUILayout.Label("+" + extra + " more in Tile Properties", EditorStyles.miniLabel);
            }

            GUILayout.EndArea();
            Handles.EndGUI();
        }

        static void SceneSwatch(Color color, string label)
        {
            GUILayout.BeginHorizontal();
            var rect = GUILayoutUtility.GetRect(10f, 10f, GUILayout.Width(10f), GUILayout.Height(10f));
            EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, 1f));
            GUILayout.Label(label, EditorStyles.miniLabel);
            GUILayout.EndHorizontal();
        }
    }

    [Overlay(typeof(SceneView), "rune-magic-stamps", "Rune Stamps")]
    sealed class StampSceneOverlay : IMGUIOverlay
    {
        public override void OnCreated()
        {
            displayed = true;
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            var on = EditorGUILayout.ToggleLeft("Show stamp colours", StampOverlay.Enabled);
            if (EditorGUI.EndChangeCheck())
            {
                StampOverlay.Enabled = on;
            }

            EditorGUI.BeginChangeCheck();
            var look = EditorGUILayout.ToggleLeft("Look-only cells", StampOverlay.ShowLookOnly);
            if (EditorGUI.EndChangeCheck())
            {
                StampOverlay.ShowLookOnly = look;
            }

            GUILayout.Label(
                StampOverlay.LastStampCount > 0
                    ? StampOverlay.LastStampCount + " stamped cells"
                    : "Waiting for Scene gizmos…",
                EditorStyles.miniLabel);
        }
    }

    static class StampGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active |
                   GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawTilemap(Tilemap map, GizmoType type)
        {
            StampOverlay.DrawGizmosFor(map);
        }
    }
}
#endif
