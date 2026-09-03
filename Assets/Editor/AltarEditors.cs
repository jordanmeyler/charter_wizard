#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(WorldInteract))]
    public sealed class WorldInteractEditor : Editor
    {
        RuneId _recipePick = RuneId.Fire;
        RuneId _viaPick = RuneId.Spark;
        int _catalogIndex;
        bool _showFallback;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "You do not type rune names. Load a written working to fill the marks, or click marks in the grid and Add them in order. The same working can have two writings — set both, or leave Other writing empty and the catalog's other chain is shown.",
                MessageType.Info);

            DrawCatalogLoad();

            EditorGUILayout.Space();
            RunePicker.DrawSequence(
                serializedObject.FindProperty("recipe"),
                "Recipe",
                "The sentence prayer shows and Cast aims. Click marks, then Add.",
                ref _recipePick);

            EditorGUILayout.Space();
            RunePicker.DrawSequence(
                serializedObject.FindProperty("via"),
                "Other writing",
                "Optional second way to write the same working (Spark · Mercury beside Fire · Air · Mercury).",
                ref _viaPick);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("verb"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("look"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));

            _showFallback = EditorGUILayout.Foldout(_showFallback, "Fallback name (only if Recipe is empty)");
            if (_showFallback)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("spell"),
                    new GUIContent("Fallback name", "Old field. Used only when Recipe has no marks."));
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawCatalogLoad()
        {
            CatalogBook.EnsureLoaded();
            var catalog = SpellCodex.All;
            var labels = new string[catalog.Count + 1];
            labels[0] = "(load a written working — fills the runes)";
            for (var i = 0; i < catalog.Count; i++)
            {
                var entry = catalog[i];
                var line = entry.Name + "   " + WorkingNames.RunePhrase(entry.RecipeRunes);
                if (entry.ViaRunes != null && entry.ViaRunes.Count > 0)
                {
                    line += "   /   " + WorkingNames.RunePhrase(entry.ViaRunes);
                }

                labels[i + 1] = line;
            }

            EditorGUILayout.LabelField("Load writings", EditorStyles.boldLabel);
            _catalogIndex = EditorGUILayout.Popup(_catalogIndex, labels);
            EditorGUI.BeginDisabledGroup(_catalogIndex <= 0);
            if (GUILayout.Button("Fill Recipe and Other writing from that working"))
            {
                var entry = catalog[_catalogIndex - 1];
                RunePicker.WriteRunes(serializedObject.FindProperty("recipe"), entry.RecipeRunes);
                RunePicker.WriteRunes(serializedObject.FindProperty("via"), entry.ViaRunes);
                serializedObject.FindProperty("spell").stringValue = string.Empty;
            }

            EditorGUI.EndDisabledGroup();
        }
    }

    [CustomEditor(typeof(ElementalAltar))]
    public sealed class ElementalAltarEditor : Editor
    {
        RuneId _sourcePick = RuneId.Fire;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Click the born mark. Sources fill from the birth table — Spark gives Fire · Air, you do not type them. Override Sources only if you want a different writing.",
                MessageType.Info);

            var resultProp = serializedObject.FindProperty("result");
            var result = (RuneId)resultProp.intValue;
            EditorGUILayout.LabelField("Result", RuneCatalog.NameOf(result));
            if (ChainBook.TryBirth(result, out var birth) && birth.Count > 0)
            {
                EditorGUILayout.LabelField(
                    "Birth  ·  " + WorkingNames.RunePhrase(birth) + "  =  " + RuneCatalog.NameOf(result),
                    EditorStyles.miniLabel);
            }

            RunePicker.Draw(ref result);
            if ((int)result != resultProp.intValue)
            {
                resultProp.intValue = (int)result;
            }

            EditorGUILayout.Space();
            RunePicker.DrawSequence(
                serializedObject.FindProperty("sources"),
                "Sources",
                "Leave empty to use the birth recipe. Click marks and Add only to override.",
                ref _sourcePick);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));

            if (serializedObject.ApplyModifiedProperties())
            {
                var altar = (ElementalAltar)target;
                altar.Author((RuneId)resultProp.intValue, RunePicker.ReadRunes(serializedObject.FindProperty("sources")));
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Snap to grid"))
            {
                var altar = (ElementalAltar)target;
                Undo.RecordObject(altar.transform, "Snap elemental altar");
                altar.transform.position = AuthoringUtil.Snap(altar.transform.position);
            }
        }

        void OnSceneGUI()
        {
            var altar = (ElementalAltar)target;
            var snapped = AuthoringUtil.Snap(altar.transform.position);
            snapped.z = 0f;
            if (snapped != altar.transform.position)
            {
                Undo.RecordObject(altar.transform, "Snap elemental altar");
                altar.transform.position = snapped;
            }
        }
    }
}
#endif
