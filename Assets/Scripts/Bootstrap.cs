using UnityEngine;

namespace CharterWizard
{
    /// <summary>
    /// Starts the game even if the scene is empty. Press Play and the courtyard appears.
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindFirstObjectByType<GameDirector>() != null)
            {
                return;
            }

            var host = new GameObject("GameDirector");
            host.AddComponent<GameDirector>();
        }
    }
}
