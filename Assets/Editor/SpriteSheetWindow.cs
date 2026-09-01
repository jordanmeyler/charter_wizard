#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Window &gt; Rune Magic &gt; Sprite Sheet.
    /// Slices a texture into named clips for Play.
    /// </summary>
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
                "Drop a sheet, set cell size, list clips as name,start,count,fps. Saves under Assets/Resources/SpriteSheets so Play can find adept-walk or fireball-shot.\n\n" +
                "Preferred: Create → Rune Magic → Look (or Window → Rune Magic → Looks) and drag Unity-sliced sprites onto Frames. You can also drag sprites onto a clip's Sprites array on this asset.",
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
