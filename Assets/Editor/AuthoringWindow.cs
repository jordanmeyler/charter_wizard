#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Window &gt; Rune Magic &gt; Authoring.
    /// Creates placeable prefabs and snaps them to the grid.
    /// </summary>
    public sealed class AuthoringWindow : EditorWindow
    {
        const string PrefabFolder = "Assets/Prefabs";

        Vector2 _scroll;

        [MenuItem("Window/Rune Magic/Authoring")]
        public static void Open()
        {
            GetWindow<AuthoringWindow>("Authoring");
        }

        [MenuItem("GameObject/Rune Magic/Level Authoring", false, 10)]
        public static void CreateLevelHost()
        {
            var host = new GameObject("Level Authoring");
            host.AddComponent<LevelAuthoring>();
            Selection.activeGameObject = host;
            Undo.RegisterCreatedObjectUndo(host, "Level Authoring");
        }

        [MenuItem("GameObject/Rune Magic/Snap Selection To Grid", false, 11)]
        public static void SnapSelection()
        {
            foreach (var transform in Selection.transforms)
            {
                Undo.RecordObject(transform, "Snap to grid");
                transform.position = AuthoringUtil.Snap(transform.position);
            }
        }

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.HelpBox(
                "Build the map like a normal Unity 2D tilemap, then drop objects on it.\n\n" +
                "1. Tiles live in Assets/Tiles (Floor / Wall / Special / Cover). Create tile palette if the Rune Palette is missing.\n" +
                "2. Main already has a Map (Grid + Tiles + Environment Details + Cover). Extra Floor / Walls / Coverings children are fine — Play merges them. A cell is floor only if you stamp Kind = Floor or paint a Floor brush. Interactables are GameObjects, not a tile layer.\n" +
                "3. Window → 2D → Tile Palette → open Rune Palette. Select Tiles and paint. Select Environment Details for plants and furniture. Select Cover for ice / fire / aura.\n" +
                "4. Or paint looks first from any ElvGames palette, then Window → Rune Magic → Tile Properties and click cells to set kind / material / cover / blocks. Looks are not floor until stamped. Select Environment Details, check Blocks, and drag across a cluster to add collision.\n" +
                "5. Click a tile asset to change material, kind, cover, aura, or sprite.\n" +
                "6. Drag prefabs from Assets/Prefabs (Items/Fire Stone, Gate, Door). Authoring Place and GameObject → Rune Magic instantiate those same prefabs. A Door has Closed and Open sprites; drag it onto a Gate.\n" +
                "7. ElvGames palettes also paint — Play reads those sprites. Enemies are under GameObject → Rune Magic → Enemies.\n" +
                "8. Play. The painted map becomes the live grid. JSON floors are not loaded.",
                MessageType.Info);

            if (GUILayout.Button("Tile Properties (assign after painting)"))
            {
                TilePropertyPaint.Open();
            }

            if (GUILayout.Button("Create tile palette (Floor / Wall / Special)"))
            {
                TilemapAuthoring.EnsureTiles();
            }

            if (GUILayout.Button("Add painted map to scene"))
            {
                TilemapAuthoring.CreatePaintedMap();
            }

            if (GUILayout.Button("Create / refresh prefabs"))
            {
                CreatePrefabs();
            }

            if (GUILayout.Button("Stamp Foundation into this scene"))
            {
                StampFoundation.Stamp();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stones — drag from Assets/Prefabs/Items", EditorStyles.boldLabel);
            for (var i = 0; i < Stones.Length; i++)
            {
                DrawPlace("Items/" + Stones[i].FileName, Stones[i].CatalogId);
            }

            EditorGUILayout.Space();
            DrawPlace("Item", "blank WorldItem — only if you need a new catalog id");
            DrawPlace("Decor", "WorldDecor — sprite id, blocking prop");
            DrawPlace("Mite", "EncounterLock — formula, keys, attack, grant");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pack enemies", EditorStyles.boldLabel);
            for (var i = 0; i < PackEnemies.All.Length; i++)
            {
                var spec = PackEnemies.All[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(spec.Name, GUILayout.Width(72));
                EditorGUILayout.LabelField(spec.SpriteId + " — drop in the scene", EditorStyles.miniLabel);
                if (GUILayout.Button("Place", GUILayout.Width(56)))
                {
                    var world = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                        ? SceneView.lastActiveSceneView.pivot
                        : Vector3.zero);
                    var encounter = PackEnemies.Spawn(spec, world);
                    Undo.RegisterCreatedObjectUndo(encounter.gameObject, "Place " + spec.Name);
                    Selection.activeGameObject = encounter.gameObject;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            DrawPlace("Torch", "TorchFixture — keys, lit frames");
            DrawPlace("Rod", "LightningConduit — spark lock");
            DrawPlace("Gate", "SocketGate — requires pack items, opens Door objects");
            DrawPlace("Door", "WorldDoor — closed / open sprites, blocks when shut");
            DrawPlace("Barrier", "BarrierLock — cover cells, clear material");
            DrawPlace("Plaque", "HintPlaque — readable text");
            DrawPlace("Crystal", "SpawnCrystal — death / Yield return");
            DrawPlace("Charm", "FreeCharm — teaches Fire · Mercury");
            DrawPlace("Rune", "RuneStringSource — a written sentence in the field");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Inscriptions", GUILayout.Width(72));
            EditorGUILayout.LabelField("Every rune, floating — click Scene to place", EditorStyles.miniLabel);
            if (GUILayout.Button("Place", GUILayout.Width(56)))
            {
                RunePlaceWindow.Open();
            }

            EditorGUILayout.EndHorizontal();
            DrawPlace("Arrows", "ArrowVolley — shots down a lane");
            DrawPlace("Chasm", "PitChasm — lock over nearby pits");
            DrawPlace("Fog", "RoomFog — standing breath / poison");
            DrawPlace("Flame Hall", "FlameHall — names the water-ward lesson");

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Level Authoring to scene"))
            {
                CreateLevelHost();
            }

            if (GUILayout.Button("Snap selection to grid"))
            {
                SnapSelection();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Open sprite-sheet importer"))
            {
                SpriteSheetWindow.Open();
            }

            EditorGUILayout.EndScrollView();
        }

        public readonly struct StoneSpec
        {
            public StoneSpec(string fileName, string catalogId, string spriteId)
            {
                FileName = fileName;
                CatalogId = catalogId;
                SpriteId = spriteId;
            }

            public string FileName { get; }
            public string CatalogId { get; }
            public string SpriteId { get; }
        }

        public static readonly StoneSpec[] Stones =
        {
            new("Fire Stone", "fire-stone", "stone-fire"),
            new("Water Stone", "water-stone", "stone-water"),
            new("Earth Stone", "earth-stone", "stone-earth"),
            new("Air Stone", "air-stone", "stone-air"),
            new("Body Stone", "body-stone", "stone-body"),
            new("Spirit Stone", "spirit-stone", "stone-spirit"),
            new("Mind Stone", "mind-stone", "stone-mind"),
            new("Grove Stone", "grove-stone", "stone-grove"),
            new("Flood Stone", "flood-stone", "stone-flood"),
            new("Spark Stone", "spark-stone", "stone-spark")
        };

        static void DrawPlace(string prefabName, string hint)
        {
            EditorGUILayout.BeginHorizontal();
            var label = prefabName.StartsWith("Items/", System.StringComparison.Ordinal)
                ? prefabName.Substring(6)
                : prefabName;
            EditorGUILayout.LabelField(label, GUILayout.Width(88));
            EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);
            if (GUILayout.Button("Place", GUILayout.Width(56)))
            {
                PlacePrefab(prefabName);
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void PlacePrefab(string name)
        {
            if (TryPlace(name))
            {
                return;
            }

            EditorUtility.DisplayDialog("Authoring", "Missing prefab " + name, "OK");
        }

        public static bool TryPlace(string name)
        {
            CreatePrefabs();
            var prefab = LoadPrefab(name);
            if (prefab == null)
            {
                return false;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            Undo.RegisterCreatedObjectUndo(instance, "Place " + prefab.name);
            Selection.activeGameObject = instance;
            return true;
        }

        static GameObject LoadPrefab(string name)
        {
            var trimmed = (name ?? string.Empty).Trim();
            if (trimmed.StartsWith("Items/", System.StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(6);
            }

            var paths = new[]
            {
                $"{PrefabFolder}/{trimmed}.prefab",
                $"{PrefabFolder}/Items/{trimmed}.prefab"
            };
            for (var i = 0; i < paths.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        public static void CreatePrefabs()
        {
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            Write("Item", typeof(WorldItem));
            Write("Decor", typeof(WorldDecor));
            Write("Mite", typeof(EncounterLock));
            Write("Torch", typeof(TorchFixture));
            Write("Rod", typeof(LightningConduit));
            Write("Gate", typeof(SocketGate));
            Write("Door", typeof(WorldDoor));
            Write("Barrier", typeof(BarrierLock));
            Write("Plaque", typeof(HintPlaque));
            Write("Crystal", typeof(SpawnCrystal));
            Write("Charm", typeof(FreeCharm));
            Write("Rune", typeof(RuneStringSource));
            Write("Inscription", typeof(RuneStele));
            Write("Pillar", typeof(RuneStele));
            Write("Arrows", typeof(ArrowVolley));
            Write("Chasm", typeof(PitChasm));
            Write("Fog", typeof(RoomFog));
            Write("Flame Hall", typeof(FlameHall));
            Write("Adept", typeof(AdeptAvatar));
            if (!AssetDatabase.IsValidFolder(PrefabFolder + "/Items"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Items");
            }

            for (var i = 0; i < Stones.Length; i++)
            {
                WriteStone(Stones[i]);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void WriteStone(StoneSpec spec)
        {
            var path = $"{PrefabFolder}/Items/{spec.FileName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && existing.GetComponent<WorldItem>() != null)
            {
                return;
            }

            var host = new GameObject(spec.FileName);
            var item = host.AddComponent<WorldItem>();
            host.AddComponent<SpriteRenderer>();
            var so = new SerializedObject(item);
            so.FindProperty("catalogId").stringValue = spec.CatalogId;
            so.FindProperty("displayName").stringValue = spec.FileName;
            so.FindProperty("spriteId").stringValue = spec.SpriteId;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(host, path);
            DestroyImmediate(host);
        }

        static void Write(string name, System.Type type)
        {
            var path = $"{PrefabFolder}/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && existing.GetComponent(type) != null)
            {
                return;
            }

            var host = new GameObject(name);
            host.AddComponent(type);
            host.AddComponent<SpriteRenderer>();
            PrefabUtility.SaveAsPrefabAsset(host, path);
            DestroyImmediate(host);
        }
    }

    public sealed class SpriteSheetWindow : EditorWindow
    {
        Texture2D _texture;
        string _id = "adept";
        int _cellWidth = 16;
        int _cellHeight = 16;
        float _ppu = 16f;
        Vector2 _pivot = new(0.5f, 0.5f);
        string _clips = "idle,0,4,8\nwalk,4,4,10\nmelt,8,4,12";

        [MenuItem("Window/Rune Magic/Sprite Sheet")]
        public static void Open()
        {
            GetWindow<SpriteSheetWindow>("Sprite Sheet");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Drop a sheet, set cell size, list clips as name,start,count,fps. Saves under Assets/Resources/SpriteSheets so Play can find adept-walk or ice-melt.",
                MessageType.Info);
            _texture = (Texture2D)EditorGUILayout.ObjectField("Sheet", _texture, typeof(Texture2D), false);
            _id = EditorGUILayout.TextField("Id", _id);
            _cellWidth = EditorGUILayout.IntField("Cell width", _cellWidth);
            _cellHeight = EditorGUILayout.IntField("Cell height", _cellHeight);
            _ppu = EditorGUILayout.FloatField("Pixels per unit", _ppu);
            _pivot = EditorGUILayout.Vector2Field("Pivot", _pivot);
            EditorGUILayout.LabelField("Clips (name,start,count,fps)");
            _clips = EditorGUILayout.TextArea(_clips, GUILayout.MinHeight(72));
            if (GUILayout.Button("Create sprite sheet asset") && _texture != null)
            {
                Save();
            }
        }

        void Save()
        {
            const string folder = "Assets/Resources/SpriteSheets";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "SpriteSheets");
            }

            var path = $"{folder}/{_id}.asset";
            var sheet = AssetDatabase.LoadAssetAtPath<SpriteSheet>(path);
            if (sheet == null)
            {
                sheet = CreateInstance<SpriteSheet>();
                AssetDatabase.CreateAsset(sheet, path);
            }

            sheet.id = _id;
            sheet.texture = _texture;
            sheet.cellWidth = Mathf.Max(1, _cellWidth);
            sheet.cellHeight = Mathf.Max(1, _cellHeight);
            sheet.pixelsPerUnit = _ppu;
            sheet.pivot = _pivot;
            sheet.clips = ParseClips(_clips);
            EditorUtility.SetDirty(sheet);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(sheet);
        }

        static SpriteSheetClip[] ParseClips(string text)
        {
            var lines = (text ?? string.Empty).Split(new[] { '\n', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            var list = new System.Collections.Generic.List<SpriteSheetClip>();
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 3)
                {
                    continue;
                }

                var clip = new SpriteSheetClip { name = parts[0].Trim() };
                int.TryParse(parts[1].Trim(), out clip.start);
                int.TryParse(parts[2].Trim(), out clip.count);
                clip.fps = 8f;
                if (parts.Length > 3)
                {
                    float.TryParse(parts[3].Trim(), out clip.fps);
                }

                if (clip.count <= 0)
                {
                    clip.count = 1;
                }

                list.Add(clip);
            }

            return list.ToArray();
        }
    }
}
#endif
