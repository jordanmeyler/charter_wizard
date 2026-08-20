using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The constant matrix: the same runes stream for novice and master.
    /// Casting reads the field; it is not tile-based.
    /// </summary>
    public sealed class RuneField : MonoBehaviour
    {
        public static readonly RuneId[] StartingStream =
        {
            RuneId.Fire, RuneId.Air, RuneId.Earth, RuneId.Water,
            RuneId.Salt, RuneId.Mercury, RuneId.Sulphur
        };

        SpellComposer _composer;
        System.Action<string> _log;

        public void Bind(SpellComposer composer, System.Action<string> log)
        {
            _composer = composer;
            _log = log;
            for (var i = 0; i < StartingStream.Length; i++)
            {
                SpawnOrb(StartingStream[i], i);
            }
        }

        public void Select(RuneId rune)
        {
            _composer.TryAdd(rune, out var note);
            _log?.Invoke(note);
        }

        void SpawnOrb(RuneId rune, int index)
        {
            var orb = new GameObject($"Rune_{RuneCatalog.NameOf(rune)}");
            orb.transform.SetParent(transform, false);
            var collider = orb.AddComponent<CircleCollider2D>();
            collider.radius = 0.28f;
            var view = orb.AddComponent<RuneOrb>();
            var angle = index * Mathf.PI * 2f / StartingStream.Length;
            var radius = 1.55f + (index % 2) * 0.28f;
            var speed = 0.55f + index * 0.04f;
            if (index % 2 == 1)
            {
                speed = -speed;
            }

            view.Bind(this, rune, angle, radius, speed);
        }
    }
}
