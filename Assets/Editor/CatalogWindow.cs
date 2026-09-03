#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Window &gt; Rune Magic &gt; Catalog. Item descriptions and
    /// jumps to the master recipe / art files.
    /// </summary>
    public sealed class CatalogWindow : EditorWindow
    {
        List<ArtCatalog.Draft> _drafts;
        Vector2 _scroll;
        string _status = "Edit a Description and Save. Stones, charms, and other pack items.";

        [MenuItem("Window/Rune Magic/Catalog")]
        public static void Open()
        {
            var window = GetWindow<CatalogWindow>("Catalog");
            window.minSize = new Vector2(520, 420);
            window.Reload();
        }

        void OnEnable()
        {
            Reload();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Description is what the pack and You see show. Pickup line is spoken when the item is taken. " +
                "Save writes both art.json and the matching prefab (Fire Stone, Water Stone, …). " +
                "You can also type a Description on a stone in the Inspector.",
                MessageType.Info);

            if (GUILayout.Button("Reload item descriptions"))
            {
                Reload();
            }

            if (_drafts == null || _drafts.Count == 0)
            {
                EditorGUILayout.LabelField("No pack items found.", EditorStyles.miniLabel);
            }
            else
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                for (var i = 0; i < _drafts.Count; i++)
                {
                    DrawDraft(_drafts[i]);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save descriptions"))
            {
                Save();
            }

            if (GUILayout.Button("Open Authoring (place items)"))
            {
                AuthoringWindow.Open();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(_status, EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Files", EditorStyles.boldLabel);
            if (GUILayout.Button("Open spells.json (recipes + joins)"))
            {
                Ping("Assets/Resources/Catalog/spells.json");
            }

            if (GUILayout.Button("Open art.json (sprites + items)"))
            {
                Ping(ArtCatalog.ArtPath);
            }

            if (GUILayout.Button("Reveal catalog-editor.html"))
            {
                EditorUtility.RevealInFinder(Path.Combine(Application.dataPath, "../Tools/catalog-editor.html"));
            }

            if (GUILayout.Button("Open Adept Animator (Hero_22)"))
            {
                AdeptAnimatorBuilder.Open();
            }

            if (GUILayout.Button("Open Sprite Sheet importer"))
            {
                SpriteSheetWindow.Open();
            }
        }

        void DrawDraft(ArtCatalog.Draft draft)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(draft.Name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(draft.Kind + "  ·  " + draft.Id, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(draft.PrefabPath))
            {
                EditorGUILayout.LabelField(draft.PrefabPath, EditorStyles.miniLabel);
            }

            EditorGUILayout.LabelField("Description (pack and You see)");
            draft.Look = EditorGUILayout.TextArea(draft.Look ?? string.Empty, GUILayout.MinHeight(48));
            EditorGUILayout.LabelField("Pickup line");
            draft.Note = EditorGUILayout.TextArea(draft.Note ?? string.Empty, GUILayout.MinHeight(32));
            if (!string.IsNullOrEmpty(draft.Look))
            {
                EditorGUILayout.LabelField(Sight.YouSee(draft.Look), EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        void Reload()
        {
            _drafts = ArtCatalog.Load();
            _status = (_drafts != null ? _drafts.Count : 0) + " pack items. Edit a Description and Save.";
            Repaint();
        }

        void Save()
        {
            if (ArtCatalog.Save(_drafts, out var error))
            {
                _status = "Saved descriptions to art.json and matching prefabs.";
            }
            else
            {
                _status = error ?? "Save failed.";
                EditorUtility.DisplayDialog("Catalog", _status, "OK");
            }

            Repaint();
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
