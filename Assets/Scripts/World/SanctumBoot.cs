using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// AfterSceneLoad is too early for a full tile bake — textures,
    /// 2D colliders, and Destroy() replacement are safer from Start.
    /// </summary>
    public sealed class SanctumBoot : MonoBehaviour
    {
        void Start()
        {
            try
            {
                Bootstrap.Run();
            }
            finally
            {
                Destroy(gameObject);
            }
        }
    }
}
