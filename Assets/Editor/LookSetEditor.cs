#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(LookSet))]
    public sealed class LookSetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Assign Unity sprites (slice the texture in the Sprite Editor, then drag the slices here). " +
                "One sprite is a still. Several loop at FPS.\n\n" +
                "Play finds this Look by Id. Common ids:\n" +
                "  wall / wall-ice / wall-timber / wall-plant\n" +
                "  bridge / bridge-ice / pillar / pillar-ice\n" +
                "  floor-dirt / floor-stone / pit / door\n" +
                "  tile-fire / tile-wet / tile-charge / cover-ice\n" +
                "  fireball-shot / douse-shot / fx-fire\n\n" +
                "Floor and Wall stamps never use these. Only spell-made bodies, leftovers, and shots do.",
                MessageType.Info);
            DrawDefaultInspector();
        }
    }
}
#endif
