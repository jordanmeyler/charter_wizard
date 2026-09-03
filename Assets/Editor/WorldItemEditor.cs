#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(WorldItem))]
    public sealed class WorldItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Description is what the pack and You see show when this goes into inventory. " +
                "Leave it empty to use the catalog row for this Catalog Id. " +
                "Window → Rune Magic → Catalog edits every stone in one list.",
                MessageType.Info);

            EditorGUILayout.LabelField("Words", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"), new GUIContent("Name"));
            var look = serializedObject.FindProperty("look");
            EditorGUILayout.PropertyField(look, new GUIContent(
                "Description",
                "Pack inspect and You see. Leave empty to use the catalog row."));
            var note = serializedObject.FindProperty("note");
            EditorGUILayout.PropertyField(note, new GUIContent(
                "Pickup line",
                "Spoken when it goes into the pack. Empty uses the catalog note."));

            var catalogId = serializedObject.FindProperty("catalogId").stringValue;
            CatalogBook.TryItem(catalogId, out var catalog);
            if (catalog != null)
            {
                if (string.IsNullOrEmpty(look.stringValue) && !string.IsNullOrEmpty(catalog.look))
                {
                    EditorGUILayout.HelpBox("Catalog description: " + catalog.look, MessageType.None);
                    if (GUILayout.Button("Copy catalog description here"))
                    {
                        look.stringValue = catalog.look;
                    }
                }
                else if (!string.IsNullOrEmpty(look.stringValue))
                {
                    EditorGUILayout.LabelField(Sight.YouSee(look.stringValue), EditorStyles.wordWrappedMiniLabel);
                }

                if (string.IsNullOrEmpty(note.stringValue) && !string.IsNullOrEmpty(catalog.note))
                {
                    EditorGUILayout.HelpBox("Catalog pickup: " + catalog.note, MessageType.None);
                }

                if (!string.IsNullOrEmpty(look.stringValue)
                    && look.stringValue != catalog.look
                    && GUILayout.Button("Write this Description to the catalog"))
                {
                    serializedObject.ApplyModifiedProperties();
                    var drafts = ArtCatalog.Load();
                    for (var i = 0; i < drafts.Count; i++)
                    {
                        if (drafts[i].Id == catalogId)
                        {
                            drafts[i].Look = look.stringValue;
                            drafts[i].Note = string.IsNullOrEmpty(note.stringValue)
                                ? drafts[i].Note
                                : note.stringValue;
                            break;
                        }
                    }

                    if (ArtCatalog.Save(drafts, out var error))
                    {
                        EditorGUILayout.HelpBox("Wrote this Description to art.json and matching prefabs.", MessageType.Info);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Catalog", error ?? "Save failed.", "OK");
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Authoring", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("catalogId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("changeFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("changeClip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("changeFps"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("matter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fragile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keys"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("teachesSpell"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
