#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Window &gt; Rune Magic &gt; Catalog. Opens the master recipe and art files.
    /// </summary>
    public sealed class CatalogWindow : EditorWindow
    {
        [MenuItem("Window/Rune Magic/Catalog")]
        public static void Open()
        {
            GetWindow<CatalogWindow>("Catalog");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Recipes live in Assets/Resources/Catalog/spells.json — that is the master book the game loads. Sprites and items live in art.json. Stones are prefabs under Assets/Prefabs/Items — drag them into the Scene. Sprite sheets: Window → Rune Magic → Sprite Sheet.",
                MessageType.Info);

            if (GUILayout.Button("Open Authoring (place items)"))
            {
                AuthoringWindow.Open();
            }

            if (GUILayout.Button("Open Sprite Sheet importer"))
            {
                SpriteSheetWindow.Open();
            }

            if (GUILayout.Button("Open spells.json (recipes + joins)"))
            {
                Ping("Assets/Resources/Catalog/spells.json");
            }

            if (GUILayout.Button("Open art.json (sprites + items)"))
            {
                Ping("Assets/Resources/Catalog/art.json");
            }

            if (GUILayout.Button("Reveal catalog-editor.html"))
            {
                EditorUtility.RevealInFinder(Path.Combine(Application.dataPath, "../Tools/catalog-editor.html"));
            }
        }

        static void Ping(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                EditorUtility.DisplayDialog("Catalog", "Missing " + assetPath, "OK");
            }
        }
    }
}
#endif
