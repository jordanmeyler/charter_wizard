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
