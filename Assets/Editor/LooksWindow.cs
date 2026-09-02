#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Window &gt; Rune Magic &gt; Looks.
    /// Creates a Look asset you can drop sprites onto.
    /// </summary>
    public sealed class LooksWindow : EditorWindow
    {
        string _id = "wall-ice";
        float _fps = 8f;
        Vector2 _scroll;

        static readonly string[] Catalog =
        {
            "wall", "wall-ice", "wall-timber", "wall-plant", "wall-moss",
            "bridge", "bridge-ice", "bridge-stone",
            "pillar", "pillar-ice", "fire-pillar", "flame-pillar", "lava-pillar",
            "pillar-fire", "pillar-hearth", "pillar-lava",
            "floor-dirt", "floor-stone", "floor-water", "pit", "door", "door-open",
            "cover-ice", "cover-fire", "cover-vine",
            "tile-fire", "tile-wet", "tile-charge", "tile-grow", "tile-fog", "tile-poison",
            "fireball-shot", "douse-shot", "arrow-shot", "fx-fire", "fx-ice", "fx-earth", "fx-lava"
        };

        [MenuItem("Window/Rune Magic/Looks")]
        public static void Open()
        {
            GetWindow<LooksWindow>("Looks");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "The Unity path: Create → Rune Magic → Look, set Id, drag sliced sprites onto Frames. " +
                "Or make one here. Play uses that clip for the conjured body / leftover / shot with that id. " +
                "Painters stay as fallback when Frames is empty.",
                MessageType.Info);

            _id = EditorGUILayout.TextField("Id", _id);
            _fps = EditorGUILayout.FloatField("FPS", _fps);
            if (GUILayout.Button("Create Look asset") && !string.IsNullOrWhiteSpace(_id))
            {
                CreateLook(_id.Trim(), _fps);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Common ids", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (var i = 0; i < Catalog.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(Catalog[i], GUILayout.Height(18));
                if (GUILayout.Button("Create", GUILayout.Width(64)))
                {
                    _id = Catalog[i];
                    CreateLook(Catalog[i], _fps);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        static void CreateLook(string id, float fps)
        {
            const string folder = "Assets/Resources/Looks";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Looks");
            }

            var path = $"{folder}/{id}.asset";
            var look = AssetDatabase.LoadAssetAtPath<LookSet>(path);
            if (look == null)
            {
                look = CreateInstance<LookSet>();
                AssetDatabase.CreateAsset(look, path);
            }

            look.id = id;
            look.fps = fps > 0f ? fps : 8f;
            if (look.frames == null)
            {
                look.frames = System.Array.Empty<Sprite>();
            }

            EditorUtility.SetDirty(look);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(look);
            Selection.activeObject = look;
        }
    }
}
#endif
