#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(EncounterLock))]
    public sealed class EncounterLockEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Every enemy is a lock. Place a Golem or Warden prefab (or GameObject → Rune Magic → Enemies), then dress it here.\n\n" +
                "Look — drag sliced Unity sprites onto Portrait / Idle Frames / Attack Frames. Pack art lives in Assets/ElvGames/Rogue Adventure/Enemies. A = idle, C = slam or cast, D = resolve. Sprite Id (enemy-011, enemy-012) is the fallback if those arrays are empty.\n\n" +
                "Attack — Golem slams anyone in reach. Wizard spends Cast Seconds writing a fireball (empty Cast writes Fire · Mercury). Archer looses a shot. None only wanders.\n\n" +
                "See ENEMIES.md.",
                MessageType.Info);

            EditorGUILayout.LabelField("Lock", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredName"), new GUIContent("Name"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredId"), new GUIContent("Id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("formula"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keys"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredEnsouled"), new GUIContent("Ensouled"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredBlocking"), new GUIContent("Blocking"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("grant"), new GUIContent("Grant item"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Look", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resolveFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleClip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackClip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resolveClip"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill empty frames from pack"))
            {
                EnemyArtBind.Apply((EncounterLock)target, overwrite: false);
                serializedObject.Update();
            }

            if (GUILayout.Button("Replace frames from pack"))
            {
                if (EditorUtility.DisplayDialog(
                    "Replace enemy frames",
                    "Overwrite Portrait, Idle, Attack, and Resolve with the ElvGames slices for this Sprite Id?",
                    "Replace",
                    "Cancel"))
                {
                    EnemyArtBind.Apply((EncounterLock)target, overwrite: true);
                    serializedObject.Update();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attack", EditorStyles.boldLabel);
            var kindProp = serializedObject.FindProperty("authoredAttack");
            var attackProp = serializedObject.FindProperty("attack");
            var shown = (CombatKind)kindProp.enumValueIndex;
            if (shown == CombatKind.None)
            {
                shown = PackEnemies.KindOf(attackProp.stringValue);
            }

            EditorGUI.BeginChangeCheck();
            shown = (CombatKind)EditorGUILayout.EnumPopup(
                new GUIContent("Attack", "Golem slams. Wizard writes a fireball. Archer looses a shot."),
                shown);
            if (EditorGUI.EndChangeCheck())
            {
                kindProp.enumValueIndex = (int)shown;
                attackProp.stringValue = PackEnemies.AttackName(shown);
            }

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("authoredCastSeconds"),
                new GUIContent("Cast seconds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cast"), new GUIContent("Cast recipe"), true);
            EditorGUILayout.HelpBox(
                shown == CombatKind.Golem
                    ? "Slam in reach. Hop or Stoneskin survives it. Attack Frames play during the windup."
                    : shown == CombatKind.Wizard
                        ? "They commit facing when the sentence starts. Empty Cast recipe writes Fire · Mercury. A wall breaks the shot."
                        : shown == CombatKind.Archer
                            ? "They loose an arrow. Empty Cast recipe writes Earth · Mercury."
                            : "No strike. Use this for the Silent Court's stone men — mind work only.",
                MessageType.None);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Snap to grid"))
            {
                var encounter = (EncounterLock)target;
                Undo.RecordObject(encounter.transform, "Snap enemy");
                encounter.transform.position = AuthoringUtil.Snap(encounter.transform.position);
            }
        }
    }
}
#endif
