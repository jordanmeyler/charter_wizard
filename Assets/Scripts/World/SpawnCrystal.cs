using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The first standing body. Shown — Light sits here. Death
    /// sends the adept back. The pit is the twin: Dark, withheld.
    /// </summary>
    public sealed class SpawnCrystal : MonoBehaviour, ILookable, IRuneSource
    {
        [Header("Authoring")]
        [SerializeField] string spriteId = "spawn-crystal";
        [SerializeField] Sprite portrait;
        [SerializeField] Sprite[] idleFrames;

        bool _wired;

        public Vector3 WorldPosition => transform.position;
        public float LookRadius => 0.7f;
        public bool CanLook => true;
        public string LookText => Sight.OfCrystal();

        public bool IsEmitting => true;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 1.4f;
        public float VoiceWeight => 2.4f;
        public RuneSourceKind SourceKind => RuneSourceKind.Presence;

        public void Collect(List<RuneId> buffer)
        {
            buffer.Add(RuneId.Lumen);
        }

        public static SpawnCrystal Spawn(Vector3 world)
        {
            var host = new GameObject("SpawnCrystal");
            host.transform.position = world;
            var crystal = host.AddComponent<SpawnCrystal>();
            crystal.Bind();
            return crystal;
        }

        public void EnsureBound()
        {
            Bind();
        }

        void Bind()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            AuthoringUtil.ApplyLook(gameObject, 6, string.IsNullOrEmpty(spriteId) ? "spawn-crystal" : spriteId, portrait, idleFrames, 5f);
            if (GetComponentInChildren<FixtureGlow>() == null)
            {
                FixtureGlow.Attach(transform, new Color(0.72f, 0.55f, 1f, 0.7f), 1.8f, 0.18f);
            }

            WorldLabel.Attach(transform, "Anchor", new Vector3(0f, 0.95f, 0f),
                new Color(0.86f, 0.78f, 1f));
            Lookables.Register(this);
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
        }

        void Update()
        {
            transform.localScale = Vector3.one * (0.96f + Mathf.Sin(Time.time * 2.2f) * 0.04f);
        }
    }
}
