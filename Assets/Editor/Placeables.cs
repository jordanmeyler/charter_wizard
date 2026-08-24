#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// GameObject → Rune Magic menu items. Places authored objects
    /// without going through the JSON map file.
    /// </summary>
    public static class Placeables
    {
        [MenuItem("GameObject/Rune Magic/Item", false, 20)]
        static void Item() => Spawn("Item", typeof(WorldItem));

        [MenuItem("GameObject/Rune Magic/Decor", false, 21)]
        static void Decor() => Spawn("Decor", typeof(WorldDecor));

        [MenuItem("GameObject/Rune Magic/Mite", false, 22)]
        static void Mite() => Spawn("Mite", typeof(EncounterLock));

        [MenuItem("GameObject/Rune Magic/Torch", false, 23)]
        static void Torch() => Spawn("Torch", typeof(TorchFixture));

        [MenuItem("GameObject/Rune Magic/Rod", false, 24)]
        static void Rod() => Spawn("Rod", typeof(LightningConduit));

        [MenuItem("GameObject/Rune Magic/Gate", false, 25)]
        static void Gate() => Spawn("Gate", typeof(SocketGate));

        [MenuItem("GameObject/Rune Magic/Barrier", false, 26)]
        static void Barrier() => Spawn("Barrier", typeof(BarrierLock));

        [MenuItem("GameObject/Rune Magic/Plaque", false, 27)]
        static void Plaque() => Spawn("Plaque", typeof(HintPlaque));

        [MenuItem("GameObject/Rune Magic/Crystal", false, 28)]
        static void Crystal() => Spawn("Crystal", typeof(SpawnCrystal));

        [MenuItem("GameObject/Rune Magic/Charm", false, 29)]
        static void Charm() => Spawn("Charm", typeof(FreeCharm));

        static void Spawn(string name, System.Type type)
        {
            var host = new GameObject(name);
            host.AddComponent(type);
            if (host.GetComponent<SpriteRenderer>() == null)
            {
                host.AddComponent<SpriteRenderer>();
            }

            host.transform.position = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            Undo.RegisterCreatedObjectUndo(host, "Create " + name);
            Selection.activeGameObject = host;
        }
    }
}
#endif
