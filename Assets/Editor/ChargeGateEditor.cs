#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(ChargeGate))]
    public sealed class ChargeGateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "This is the lock, not the door. Electricity opens it — a bolt, a spark sentence, or charge walking onto its cells. Hide Look (on by default) draws nothing. Uncheck it and set Portrait only if you want one sprite on the Gate itself. Drag Door objects into Doors.",
                MessageType.Info);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keys"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("doors"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sensorCells"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("finishes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("note"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("doorCells"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Look", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("liveFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteLit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hideLook"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showGlow"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showLabel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pulse"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
