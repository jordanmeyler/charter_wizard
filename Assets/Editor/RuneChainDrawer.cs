#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomPropertyDrawer(typeof(RuneChainAttribute))]
    public sealed class RuneChainDrawer : PropertyDrawer
    {
        const float Chip = 36f;
        static RuneId Pick = RuneId.Fire;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property == null || !property.isArray)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            var line = EditorGUIUtility.singleLineHeight;
            return line * 3f + Chip + 10f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || !property.isArray)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            var line = EditorGUIUtility.singleLineHeight;
            var y = position.y;
            EditorGUI.LabelField(new Rect(position.x, y, position.width, line), label.text, EditorStyles.boldLabel);
            y += line;
            var phrase = property.arraySize == 0
                ? "No marks yet — choose one and Add. Do not type names."
                : WorkingNames.RunePhrase(RunePicker.ReadRunes(property));
            EditorGUI.LabelField(new Rect(position.x, y, position.width, line), phrase, EditorStyles.miniLabel);
            y += line + 2f;

            if (property.arraySize == 0)
            {
                EditorGUI.LabelField(new Rect(position.x, y, position.width, Chip), "(empty)");
            }
            else
            {
                for (var i = 0; i < property.arraySize; i++)
                {
                    var rune = (RuneId)property.GetArrayElementAtIndex(i).intValue;
                    var chip = new Rect(position.x + i * (Chip + 4f), y, Chip, Chip);
                    EditorGUI.DrawRect(chip, RunePalette.Card(rune, true));
                    RuneMark.DrawGui(chip, rune, RunePalette.MarkInk(rune));
                    if (GUI.Button(chip, GUIContent.none, GUIStyle.none))
                    {
                        property.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }

            y += Chip + 4f;
            var popup = new Rect(position.x, y, Mathf.Max(80f, position.width - 170f), line);
            Pick = (RuneId)EditorGUI.EnumPopup(popup, Pick);
            if (GUI.Button(new Rect(position.xMax - 166f, y, 100f, line), "Add " + RuneCatalog.NameOf(Pick)))
            {
                var at = property.arraySize;
                property.arraySize = at + 1;
                property.GetArrayElementAtIndex(at).intValue = (int)Pick;
            }

            if (property.arraySize > 0 &&
                GUI.Button(new Rect(position.xMax - 62f, y, 62f, line), "Clear"))
            {
                property.arraySize = 0;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
