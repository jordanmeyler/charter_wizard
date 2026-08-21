using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The first standing body. Death sends the adept back here.
    /// </summary>
    public sealed class SpawnCrystal : MonoBehaviour
    {
        public static SpawnCrystal Spawn(Vector3 world)
        {
            var host = new GameObject("SpawnCrystal");
            host.transform.position = world;
            var crystal = host.AddComponent<SpawnCrystal>();
            crystal.Bind();
            return crystal;
        }

        void Bind()
        {
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Named("spawn-crystal");
            renderer.sortingOrder = 6;
            FixtureGlow.Attach(transform, new Color(0.72f, 0.55f, 1f, 0.7f), 1.8f, 0.18f);
            WorldLabel.Attach(transform, "Anchor", new Vector3(0f, 0.95f, 0f),
                new Color(0.86f, 0.78f, 1f));
        }

        void Update()
        {
            transform.localScale = Vector3.one * (0.96f + Mathf.Sin(Time.time * 2.2f) * 0.04f);
        }
    }
}
