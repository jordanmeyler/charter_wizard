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
                "Hand-place items in the Scene view. Play keeps them.\n\n" +
                "1. Window → Rune Magic → Authoring → Create prefabs.\n" +
                "2. Drag a prefab into the scene. Snap with GameObject → Rune Magic → Snap Selection To Grid.\n" +
                "3. On the Inspector: catalog id, matter (Ice, Fire…), keys, formula, sprites, change clip.\n" +
                "4. Optional: add Level Authoring to skip JSON props or paint a blank room shell.\n\n" +
                "Sprite sheets: Create → Rune Magic → Sprite Sheet. Put the asset under Resources. Name clips idle, walk, melt, explode. Play looks up adept-walk, ice-melt, fireball-shot.",
                MessageType.Info);

            if (GUILayout.Button("Create / refresh prefabs"))
            {
                CreatePrefabs();
            }

            EditorGUILayout.Space();
            DrawPlace("Item", "WorldItem — catalog id, matter, keys, change clip");
            DrawPlace("Mite", "EncounterLock — formula, keys, attack, grant");
            DrawPlace("Torch", "TorchFixture — keys, lit frames");
            DrawPlace("Rod", "LightningConduit — spark lock");
            DrawPlace("Gate", "SocketGate — requires pack items");
            DrawPlace("Barrier", "BarrierLock — cover cells, clear material");
            DrawPlace("Plaque", "HintPlaque — readable text");
            DrawPlace("Crystal", "SpawnCrystal — death / Yield return");
            DrawPlace("Charm", "FreeCharm — teaches Fire · Mercury");

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

        static void DrawPlace(string prefabName, string hint)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(prefabName, GUILayout.Width(72));
            EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);
            if (GUILayout.Button("Place", GUILayout.Width(56)))
            {
                PlacePrefab(prefabName);
            }

            EditorGUILayout.EndHorizontal();
        }

        static void PlacePrefab(string name)
        {
            CreatePrefabs();
            var path = $"{PrefabFolder}/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Authoring", "Missing " + path, "OK");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            Undo.RegisterCreatedObjectUndo(instance, "Place " + name);
            Selection.activeGameObject = instance;
        }

        public static void CreatePrefabs()
        {
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            Write("Item", typeof(WorldItem));
            Write("Mite", typeof(EncounterLock));
            Write("Torch", typeof(TorchFixture));
            Write("Rod", typeof(LightningConduit));
            Write("Gate", typeof(SocketGate));
            Write("Barrier", typeof(BarrierLock));
            Write("Plaque", typeof(HintPlaque));
            Write("Crystal", typeof(SpawnCrystal));
            Write("Charm", typeof(FreeCharm));
            Write("Adept", typeof(AdeptAvatar));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
        int _cellWidth = 32;
        int _cellHeight = 32;
        float _ppu = 32f;
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
