using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A teaching stone. Floor inscriptions and standing pillars both
    /// show a mark beside a picture, and speak that rune into the weave.
    /// </summary>
    public sealed class RuneStele : MonoBehaviour, IRuneSource, ILookable
    {
        public enum Kind
        {
            Floor,
            Pillar
        }

        [Header("Authoring")]
        [SerializeField] RuneId authoredRune = RuneId.Fire;
        [SerializeField] Kind authoredForm = Kind.Floor;

        public RuneId Rune { get; private set; }
        public Kind Form { get; private set; }
        bool _wired;

        public bool IsEmitting => Rune != RuneId.None;
        public Vector3 WorldOrigin => transform.position;
        public Vector3 WorldPosition => transform.position;
        public float LookRadius => Form == Kind.Pillar ? 0.7f : 0.55f;
        public bool CanLook => Rune != RuneId.None;
        public string LookText => Sight.OfRune(Rune);
        public float VoiceRadius => Form == Kind.Pillar ? 3.4f : 2.6f;
        public float VoiceWeight => 1.8f;
        public RuneSourceKind SourceKind => RuneSourceKind.String;

        Transform _picture;
        TextMesh _name;

        public static RuneStele Inscribe(Vector3 origin, RuneId rune)
        {
            return Spawn(origin, rune, Kind.Floor);
        }

        public static RuneStele Raise(Vector3 origin, RuneId rune)
        {
            return Spawn(origin, rune, Kind.Pillar);
        }

        static RuneStele Spawn(Vector3 origin, RuneId rune, Kind form)
        {
            var host = new GameObject(form == Kind.Pillar ? "RunePillar" : "RuneInscription");
            host.transform.position = origin;
            var stele = host.AddComponent<RuneStele>();
            stele.Bind(rune, form);
            return stele;
        }

        public void BindFromAuthoring()
        {
            if (_wired)
            {
                return;
            }

            Bind(authoredRune, authoredForm);
        }

        public void Bind(RuneId rune, Kind form)
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            authoredRune = rune;
            authoredForm = form;
            Rune = rune;
            Form = form;
            if (form == Kind.Pillar)
            {
                RuneSign.MountPillar(transform, rune);
                _name = RuneSign.NamePlate(transform, rune, new Vector3(0f, 1.55f, 0f));
            }
            else
            {
                RuneSign.MountFloor(transform, rune);
                _name = RuneSign.NamePlate(transform, rune, new Vector3(0f, 0.42f, 0f));
            }

            _picture = transform.Find("Nature");
            Lookables.Register(this);
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
        }

        void LateUpdate()
        {
            if (_name != null)
            {
                _name.gameObject.SetActive(GlyphView.IsDevelop);
            }

            RuneSign.Pulse(_picture, Rune, Time.time, Form == Kind.Pillar ? 0.62f : 0.55f);
        }

        public void Collect(List<RuneId> buffer)
        {
            if (IsEmitting)
            {
                buffer.Add(Rune);
            }
        }
    }
}
