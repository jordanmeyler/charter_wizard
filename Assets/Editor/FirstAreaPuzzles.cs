#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// First-area locks: ice-caged fire stone, miasma + air stone,
    /// a small pit + earth stone. Places at the Scene view pivot,
    /// or at an explicit cell.
    /// </summary>
    public static class FirstAreaPuzzles
    {
        static readonly Vector2Int[] IceRing =
        {
            new(-1, -1), new(0, -1), new(1, -1),
            new(-1, 0), new(1, 0)
        };

        static readonly string[] IceFormula = { "Water", "Earth" };

        static readonly string[] IceKeys =
        {
            "Fireball", "FlamePillar", "Melt", "Ignite",
            "SunLance", "Scald", "Witchfire", "Thaw"
        };

        [MenuItem("GameObject/Rune Magic/First Area/Ice-caged Fire Stone", false, 70)]
        public static void PlaceFire() => PlaceFireAt(Pivot());

        [MenuItem("GameObject/Rune Magic/First Area/Miasma + Air Stone", false, 71)]
        public static void PlaceAir() => PlaceAirAt(Pivot());

        [MenuItem("GameObject/Rune Magic/First Area/Pit + Earth Stone", false, 72)]
        public static void PlaceEarth() => PlaceEarthAt(Pivot());

        public static void PlaceFireAt(Vector3 world)
        {
            world = AuthoringUtil.Snap(world);
            var stone = PlaceItem("Fire Stone", "fire-stone", "stone-fire", world);
            var cage = PlaceHost("Ice Cage", typeof(BarrierLock), world);
            Configure(cage.GetComponent<BarrierLock>(), so =>
            {
                Set(so, "authoredName", "Ice cage");
                Set(so, "authoredId", "ice-cage");
                Set(so, "spriteId", "ice-block");
                Set(so, "clearMaterial", "Stone");
                Set(so, "note", "Hunger finds the hard water. A stone of fire sits free.");
                Set(so, "stampIceWalls", true);
                Set(so, "formula", IceFormula);
                Set(so, "keys", IceKeys);
                Set(so, "coverCells", IceRing);
            });
            Selection.activeGameObject = stone;
            Debug.Log("Ice-caged fire stone at " + world +
                      ". The U of ice opens north. Any fire-bearing sentence melts it.");
        }

        public static void PlaceAirAt(Vector3 world)
        {
            world = AuthoringUtil.Snap(world);
            var fog = PlaceHost("Miasma", typeof(RoomFog), world);
            Configure(fog.GetComponent<RoomFog>(), so =>
            {
                Set(so, "authoredName", "Miasma");
                Set(so, "authoredId", "miasma");
                Set(so, "spriteId", "poison-fog");
                Set(so, "note", "Breath sent. The foul air forgets the room.");
                Set(so, "radius", 2);
                Set(so, "formula", new[] { "Air" });
                Set(so, "retreatOffset", new Vector2Int(-3, 0));
            });
            var cell = AuthoringUtil.CellOf(world);
            PlaceItem("Air Stone", "air-stone", "stone-air", WorldGrid.Center(cell.x + 1, cell.y));
            Selection.activeGameObject = fog;
            Debug.Log("Miasma + air stone at " + world +
                      ". Walking in throws you back. Gust or Gale clears it.");
        }

        public static void PlaceEarthAt(Vector3 world)
        {
            world = AuthoringUtil.Snap(world);
            var pit = PlaceHost("Pit", typeof(PitChasm), world);
            Configure(pit.GetComponent<PitChasm>(), so =>
            {
                Set(so, "authoredName", "Pit");
                Set(so, "authoredId", "pit");
                Set(so, "carvePits", true);
            });
            var cell = AuthoringUtil.CellOf(world);
            PlaceItem("Earth Stone", "earth-stone", "stone-earth", WorldGrid.Center(cell.x, cell.y + 1));
            Selection.activeGameObject = pit;
            Debug.Log("Pit + earth stone at " + world +
                      ". Hop (Air · Salt · Air) or Earth-pillar (Earth · Salt) crosses it.");
        }

        static Vector3 Pivot()
        {
            return SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero;
        }

        static GameObject PlaceItem(string name, string catalogId, string spriteId, Vector3 world)
        {
            var host = PlaceHost(name, typeof(WorldItem), world);
            Configure(host.GetComponent<WorldItem>(), so =>
            {
                Set(so, "catalogId", catalogId);
                Set(so, "spriteId", spriteId);
            });
            return host;
        }

        static GameObject PlaceHost(string name, System.Type type, Vector3 world)
        {
            var host = new GameObject(name);
            host.AddComponent(type);
            if (host.GetComponent<SpriteRenderer>() == null)
            {
                host.AddComponent<SpriteRenderer>();
            }

            var authoring = Object.FindFirstObjectByType<LevelAuthoring>();
            if (authoring != null)
            {
                host.transform.SetParent(authoring.transform, true);
            }

            host.transform.position = world;
            Undo.RegisterCreatedObjectUndo(host, "Place " + name);
            return host;
        }

        static void Configure(Object target, System.Action<SerializedObject> write)
        {
            if (target == null)
            {
                return;
            }

            var so = new SerializedObject(target);
            write(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Set(SerializedObject so, string field, string value)
        {
            var property = so.FindProperty(field);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        static void Set(SerializedObject so, string field, bool value)
        {
            var property = so.FindProperty(field);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        static void Set(SerializedObject so, string field, int value)
        {
            var property = so.FindProperty(field);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        static void Set(SerializedObject so, string field, string[] values)
        {
            var property = so.FindProperty(field);
            if (property == null)
            {
                return;
            }

            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).stringValue = values[i];
            }
        }

        static void Set(SerializedObject so, string field, Vector2Int value)
        {
            var property = so.FindProperty(field);
            if (property != null)
            {
                property.vector2IntValue = value;
            }
        }

        static void Set(SerializedObject so, string field, Vector2Int[] values)
        {
            var property = so.FindProperty(field);
            if (property == null)
            {
                return;
            }

            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).vector2IntValue = values[i];
            }
        }
    }
}
#endif
