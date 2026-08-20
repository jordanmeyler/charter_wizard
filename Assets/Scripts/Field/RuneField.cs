using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The constant matrix: the same primaries and aspects stream for novice
    /// and master. Nearby tiles and creature formulas add local colour.
    /// Casting reads the field from the Charter wall, not from these orbs.
    /// </summary>
    public sealed class RuneField : MonoBehaviour
    {
        public const float PerceptionRadius = 7.5f;

        public static readonly RuneId[] StartingStream =
        {
            RuneId.Fire, RuneId.Air, RuneId.Earth, RuneId.Water,
            RuneId.Salt, RuneId.Mercury, RuneId.Sulphur
        };

        SanctumDirector _director;
        Transform _orbRoot;

        public void Bind(SanctumDirector director)
        {
            _director = director;
            var root = new GameObject("Orbs");
            root.transform.SetParent(transform, false);
            _orbRoot = root.transform;
            for (var i = 0; i < StartingStream.Length; i++)
            {
                SpawnOrb(StartingStream[i], i);
            }
        }

        public static List<RuneId> Perceive(Vector3 origin, WorldGrid grid, ISpellLock[] locks)
        {
            var seen = new List<RuneId>();
            foreach (var rune in StartingStream)
            {
                AddUnique(seen, rune);
            }

            if (grid != null)
            {
                foreach (var tile in grid.All)
                {
                    if (tile == null || tile.Element == RuneId.None)
                    {
                        continue;
                    }

                    if (Vector2.Distance(origin, tile.transform.position) <= PerceptionRadius)
                    {
                        AddUnique(seen, tile.Element);
                    }
                }
            }

            if (locks != null)
            {
                foreach (var encounter in locks)
                {
                    if (encounter is not EncounterLock living || living.Resolved)
                    {
                        continue;
                    }

                    if (Vector2.Distance(origin, living.WorldPosition) > PerceptionRadius)
                    {
                        continue;
                    }

                    foreach (var rune in living.Formula)
                    {
                        AddUnique(seen, rune);
                    }
                }
            }

            seen.Sort(CompareFieldOrder);
            return seen;
        }

        void LateUpdate()
        {
            if (_orbRoot == null)
            {
                return;
            }

            var show = _director == null || _director.Mode == PlayMode.Exploring;
            if (_orbRoot.gameObject.activeSelf != show)
            {
                _orbRoot.gameObject.SetActive(show);
            }
        }

        void SpawnOrb(RuneId rune, int index)
        {
            var orb = new GameObject($"Rune_{RuneCatalog.NameOf(rune)}");
            orb.transform.SetParent(_orbRoot != null ? _orbRoot : transform, false);
            var collider = orb.AddComponent<CircleCollider2D>();
            collider.radius = 0.28f;
            collider.isTrigger = true;
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

        static void AddUnique(List<RuneId> seen, RuneId rune)
        {
            if (rune == RuneId.None || seen.Contains(rune))
            {
                return;
            }

            seen.Add(rune);
        }

        static int CompareFieldOrder(RuneId left, RuneId right)
        {
            return Rank(left).CompareTo(Rank(right));
        }

        static int Rank(RuneId rune)
        {
            if (RuneCatalog.IsAspect(rune))
            {
                return 200 + (int)rune;
            }

            switch (rune)
            {
                case RuneId.Fire: return 10;
                case RuneId.Air: return 11;
                case RuneId.Earth: return 12;
                case RuneId.Water: return 13;
                default: return 50 + (int)rune;
            }
        }
    }
}
