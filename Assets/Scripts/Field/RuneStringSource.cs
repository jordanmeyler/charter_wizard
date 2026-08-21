using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// An ordered sentence written into the field. Strands keep sequence so
    /// the player can read it as a line, not a cloud. World events and the
    /// adept's place can hang more of these later.
    /// </summary>
    public sealed class RuneStringSource : MonoBehaviour, IRuneSource
    {
        public int StringId { get; private set; }
        public RuneId[] Sequence { get; private set; }
        public Vector3 Heading { get; private set; }

        public bool IsEmitting => Sequence != null && Sequence.Length > 0;
        public Vector3 WorldOrigin => transform.position;
        public float VoiceRadius => 3.2f;
        public float VoiceWeight => 1.6f;
        public RuneSourceKind SourceKind => RuneSourceKind.String;

        public static RuneStringSource Spawn(Vector3 origin, IReadOnlyList<RuneId> sequence, Vector3 heading)
        {
            var host = new GameObject("RuneString");
            host.transform.position = origin;
            var source = host.AddComponent<RuneStringSource>();
            source.Bind(sequence, heading);
            return source;
        }

        public void Bind(IReadOnlyList<RuneId> sequence, Vector3 heading)
        {
            StringId = GetInstanceID();
            Sequence = sequence != null ? new RuneId[sequence.Count] : System.Array.Empty<RuneId>();
            if (sequence != null)
            {
                for (var i = 0; i < sequence.Count; i++)
                {
                    Sequence[i] = sequence[i];
                }
            }

            Heading = heading.sqrMagnitude < 0.0001f ? Vector3.right : heading.normalized;
            ShowAltar();
        }

        void ShowAltar()
        {
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Plaque();
            renderer.sortingOrder = 4;
            var rune = Sequence.Length > 0 ? Sequence[0] : RuneId.None;
            var tint = rune == RuneId.None ? new Color(0.85f, 0.78f, 0.55f) : RunePalette.Of(rune);
            renderer.color = Color.Lerp(Color.white, tint, 0.35f);
            FixtureGlow.Attach(transform, new Color(tint.r, tint.g, tint.b, 0.55f), 1.35f, 0.1f);

            var names = new string[Sequence.Length];
            for (var i = 0; i < Sequence.Length; i++)
            {
                names[i] = RuneCatalog.NameOf(Sequence[i]).ToUpperInvariant();
            }

            var title = names.Length == 0 ? "RUNE" : string.Join(" · ", names);
            WorldLabel.Attach(transform, title, new Vector3(0f, 0.72f, 0f), tint);
        }

        public void Collect(List<RuneId> buffer)
        {
            if (!IsEmitting)
            {
                return;
            }

            for (var i = 0; i < Sequence.Length; i++)
            {
                if (Sequence[i] != RuneId.None)
                {
                    buffer.Add(Sequence[i]);
                }
            }
        }
    }
}
