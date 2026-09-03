#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(WorldInteract))]
    public sealed class WorldInteractEditor : Editor
    {
        int _catalogIndex;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "You do not type rune names. Load a written working, or Add marks on Recipe / Other writing. Those fields are at the top of this component.",
                MessageType.Info);
            DrawCatalogLoad();
            EditorGUILayout.Space();
            DrawPropertiesExcluding(serializedObject, "m_Script");
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
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Click the born mark. Sources fill from the birth table — Spark gives Fire · Air. You do not type them. Override Sources only for a different writing.",
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
            DrawPropertiesExcluding(serializedObject, "m_Script", "result");
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
