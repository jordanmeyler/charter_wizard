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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Teach a recipe, not a name. Set the runes this slab shows. A second writing is for the same working said another way (Fire · Air · Mercury or Spark · Mercury). Leave Via empty to show the catalog's other chain when there is one. Dress the statue with tiles — this volume does not draw.",
                MessageType.Info);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("verb"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("look"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));

            EditorGUILayout.Space();
            RunePicker.DrawSequence(
                serializedObject.FindProperty("recipe"),
                "Recipe",
                "The sentence prayer shows and Cast aims.",
                ref _recipePick);

            EditorGUILayout.Space();
            RunePicker.DrawSequence(
                serializedObject.FindProperty("via"),
                "Other writing",
                "Optional. Another way to write the same working.",
                ref _viaPick);

            EditorGUILayout.Space();
            var fallback = serializedObject.FindProperty("spell");
            EditorGUILayout.PropertyField(fallback, new GUIContent(
                "Fallback name",
                "Used only when Recipe is empty. Prefer setting runes."));

            serializedObject.ApplyModifiedProperties();
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
                "Shows how an element is made: sources on the left, an equals, then the born mark. Pick Spark and Fire · Air fill in. Override Sources to show a different writing.",
                MessageType.Info);

            var resultProp = serializedObject.FindProperty("result");
            var result = (RuneId)resultProp.intValue;
            EditorGUILayout.LabelField("Result", RuneCatalog.NameOf(result));
            RunePicker.Draw(ref result);
            if ((int)result != resultProp.intValue)
            {
                resultProp.intValue = (int)result;
            }

            EditorGUILayout.Space();
            RunePicker.DrawSequence(
                serializedObject.FindProperty("sources"),
                "Sources",
                "Leave empty to use the birth recipe (Fire · Air for Spark).",
                ref _sourcePick);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));

            if (serializedObject.ApplyModifiedProperties())
            {
                var altar = (ElementalAltar)target;
                altar.Author((RuneId)resultProp.intValue, SourcesOf(serializedObject.FindProperty("sources")));
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

        static RuneId[] SourcesOf(SerializedProperty array)
        {
            if (array == null || !array.isArray || array.arraySize == 0)
            {
                return System.Array.Empty<RuneId>();
            }

            var runes = new RuneId[array.arraySize];
            for (var i = 0; i < array.arraySize; i++)
            {
                runes[i] = (RuneId)array.GetArrayElementAtIndex(i).intValue;
            }

            return runes;
        }
    }
}
#endif
