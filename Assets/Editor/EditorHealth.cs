#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Unity compiles every .cs file under Assets. A copy of an Editor
    /// window under Animations (or Prefabs / Tiles / Scenes) becomes a
    /// second type in the same compile and throws CS0111.
    /// </summary>
    [InitializeOnLoad]
    static class EditorHealth
    {
        static readonly string[] NeverScripts =
        {
            "Assets/Animations",
            "Assets/Resources/Animations",
            "Assets/Prefabs",
            "Assets/Tiles",
            "Assets/Scenes"
        };

        [InitializeOnLoadMethod]
        static void Hook()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.delayCall += Scan;
        }

        static void Scan()
        {
            for (var i = 0; i < NeverScripts.Length; i++)
            {
                var folder = NeverScripts[i];
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    continue;
                }

                var guids = AssetDatabase.FindAssets("t:MonoScript", new[] { folder });
                for (var g = 0; g < (guids != null ? guids.Length : 0); g++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[g]);
                    if (string.IsNullOrEmpty(path) || !path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Debug.LogError(
                        "EditorHealth: C# script " + path +
                        " is under " + folder +
                        ". Unity will compile it as a second copy and can CS0111. " +
                        "Delete that file. Editor windows stay in Assets/Editor; " +
                        "clips stay in Assets/Animations.");
                }
            }
        }
    }
}
#endif
