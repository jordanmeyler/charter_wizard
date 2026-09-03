#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(WorldAltar))]
    public sealed class WorldAltarEditor : Editor
    {
        SerializedProperty _teach;
        SerializedProperty _birth;
        SerializedProperty _recipe;
        SerializedProperty _via;
        SerializedProperty _showOther;
        SerializedProperty _result;
        SerializedProperty _sources;
        SerializedProperty _verb;
        SerializedProperty _look;
        SerializedProperty _radius;

        void OnEnable()
        {
            _teach = serializedObject.FindProperty("teachRecipe");
            _birth = serializedObject.FindProperty("showBirth");
            _recipe = serializedObject.FindProperty("recipe");
            _via = serializedObject.FindProperty("via");
            _showOther = serializedObject.FindProperty("showOtherWriting");
            _result = serializedObject.FindProperty("result");
            _sources = serializedObject.FindProperty("sources");
            _verb = serializedObject.FindProperty("verb");
            _look = serializedObject.FindProperty("look");
            _radius = serializedObject.FindProperty("radius");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (_teach == null || _birth == null || _recipe == null)
            {
                DrawDefaultInspector();
                return;
            }

            EditorGUILayout.PropertyField(
                _teach,
                new GUIContent("Teach Recipe", "Pray the authored writing. Cast aims those runes."));
            EditorGUILayout.PropertyField(
                _birth,
                new GUIContent("Show Birth", "World display: sources = result (Fire · Air = Spark)."));

            if (_teach.boolValue)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Prayer", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_recipe, new GUIContent("Recipe"));
                if (_showOther != null)
                {
                    EditorGUILayout.PropertyField(
                        _showOther,
                        new GUIContent(
                            "Show Other Writing",
                            "Prayer also shows a second writing — Via, or the catalog's other writing when Via is empty. Turn off to teach only the Recipe (Earth-pillar stays Earth · Salt, without Stone)."));
                }

                if (_showOther == null || _showOther.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        _via,
                        new GUIContent("Via", "Other writing of the same working. Leave empty to use the catalog's other writing."));
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Only the Recipe is shown. Earth-pillar is Earth · Salt — Stone is the catalog's other writing.",
                        MessageType.None);
                }

                DrawLoadWritings();
            }

            if (_birth.boolValue)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Birth", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(
                    _result,
                    new GUIContent("Result", "Born rune, e.g. Spark. Sources fill from the catalog."));
                EditorGUILayout.PropertyField(
                    _sources,
                    new GUIContent("Sources", "Leave empty to use the catalog parents of Result."));
            }

            if (!_teach.boolValue && !_birth.boolValue)
            {
                EditorGUILayout.HelpBox("Turn on Teach Recipe and/or Show Birth.", MessageType.Info);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.PropertyField(_verb);
            EditorGUILayout.PropertyField(_look);
            if (_radius != null)
            {
                EditorGUILayout.PropertyField(
                    _radius,
                    new GUIContent(
                        "Radius",
                        "Pray reach. Transform or a parent prefab's scale multiplies this. The gold circle in the Scene is that range — no sprite is added."));
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawLoadWritings()
        {
            var names = new List<string> { "Load writings…" };
            var entries = SpellCodex.All;
            for (var i = 0; i < entries.Count; i++)
            {
                names.Add(entries[i].Name);
            }

            var pick = EditorGUILayout.Popup("From catalog", 0, names.ToArray());
            if (pick <= 0)
            {
                return;
            }

            var entry = entries[pick - 1];
            WriteRunes(_recipe, entry.RecipeRunes);
            if (_showOther == null || _showOther.boolValue)
            {
                WriteRunes(_via, entry.ViaRunes);
            }

            serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }

        static void WriteRunes(SerializedProperty list, IReadOnlyList<RuneId> runes)
        {
            list.ClearArray();
            if (runes == null)
            {
                return;
            }

            for (var i = 0; i < runes.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                list.GetArrayElementAtIndex(i).enumValueIndex = (int)runes[i];
            }
        }
    }
}
#endif
