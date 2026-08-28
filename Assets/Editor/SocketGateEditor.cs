#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(SocketGate))]
    public sealed class SocketGateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "This is the lock, not the door. Requires is a list of pack item ids (fire-stone, …). Drag Door objects into Doors. Drag a sprite onto Portrait to replace the generated gold lock.",
                MessageType.Info);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requires"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("doors"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("finishes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("note"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("doorCells"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Look", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hideLook"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showGlow"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showLabel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pulse"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
