#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Hide a Tilemap in the Scene view so you can paint or stamp the
    /// layers under it. Uses Scene Visibility (the Hierarchy eye) — the
    /// GameObject stays on, Play still bakes every layer.
    /// </summary>
    static class TileLayerVisibility
    {
        public static bool IsHidden(Tilemap map)
        {
            return map != null && IsHidden(map.gameObject);
        }

        public static bool IsHidden(GameObject host)
        {
            return host != null
                && SceneVisibilityManager.instance != null
                && SceneVisibilityManager.instance.IsHidden(host);
        }

        public static void SetHidden(GameObject host, bool hidden)
        {
            if (host == null || SceneVisibilityManager.instance == null)
            {
                return;
            }

            if (hidden)
            {
                SceneVisibilityManager.instance.Hide(host, false);
            }
            else
            {
                SceneVisibilityManager.instance.Show(host, false);
            }

            SceneView.RepaintAll();
        }

        public static List<Tilemap> Collect(LevelAuthoring spec = null)
        {
            spec ??= Object.FindFirstObjectByType<LevelAuthoring>();
            var maps = new List<Tilemap>();
            AddUnique(maps, spec != null ? spec.tilemap : null);
            AddUnique(maps, spec != null ? spec.walls : null);
            AddUnique(maps, spec != null ? spec.overlays : null);
            AddUnique(maps, spec != null ? spec.decor : null);

            var primary = spec != null && spec.tilemap != null
                ? spec.tilemap
                : TilemapLevel.FindPaintedMap();
            var parent = primary != null && primary.transform.parent != null
                ? primary.transform.parent
                : spec != null ? spec.transform : null;
            if (parent != null)
            {
                var children = parent.GetComponentsInChildren<Tilemap>(true);
                for (var i = 0; i < children.Length; i++)
                {
                    AddUnique(maps, children[i]);
                }
            }

            maps.Sort(CompareLayers);
            return maps;
        }

        public static void DrawGui(bool compact = false)
        {
            var maps = Collect();
            if (maps.Count == 0)
            {
                if (!compact)
                {
                    EditorGUILayout.HelpBox("No painted Map in the scene.", MessageType.None);
                }

                return;
            }

            if (!compact)
            {
                EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Uncheck a layer to hide it in the Scene view so you can paint or stamp the tiles under it. Hidden layers still bake at Play. Same as the Hierarchy eye. Click a name to select that Tilemap.",
                    MessageType.None);
            }

            var selected = Selection.activeGameObject;
            for (var i = 0; i < maps.Count; i++)
            {
                DrawRow(maps, maps[i], selected, compact);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(compact ? "Show all" : "Show all layers"))
            {
                ShowAll(maps);
            }

            if (!compact && GUILayout.Button("Hide selected", GUILayout.Width(110)))
            {
                HideSelected(maps);
            }

            EditorGUILayout.EndHorizontal();
        }

        static void DrawRow(List<Tilemap> maps, Tilemap map, GameObject selected, bool compact)
        {
            var host = map.gameObject;
            EditorGUILayout.BeginHorizontal();
            var hidden = IsHidden(host);
            EditorGUI.BeginChangeCheck();
            var visible = EditorGUILayout.Toggle(
                new GUIContent(string.Empty, hidden
                    ? "Show this layer in the Scene view"
                    : "Hide this layer in the Scene view so you can work on the tiles under it"),
                !hidden,
                GUILayout.Width(18));
            if (EditorGUI.EndChangeCheck())
            {
                SetHidden(host, !visible);
            }

            var on = selected == host;
            var was = GUI.backgroundColor;
            if (on)
            {
                GUI.backgroundColor = new Color(0.72f, 0.55f, 1f, 1f);
            }
            else if (hidden)
            {
                GUI.backgroundColor = new Color(0.55f, 0.55f, 0.58f, 1f);
            }

            if (GUILayout.Toggle(on, host.name, "Button") && !on)
            {
                Selection.activeGameObject = host;
                EditorGUIUtility.PingObject(host);
                if (hidden)
                {
                    SetHidden(host, false);
                }
            }

            GUI.backgroundColor = was;
            if (GUILayout.Button(
                new GUIContent(compact ? "S" : "Solo", "Hide every other layer and select this one"),
                GUILayout.Width(compact ? 22 : 44)))
            {
                Solo(maps, map);
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void ShowAll(List<Tilemap> maps = null)
        {
            maps ??= Collect();
            for (var i = 0; i < maps.Count; i++)
            {
                SetHidden(maps[i].gameObject, false);
            }
        }

        public static void HideSelected(List<Tilemap> maps = null)
        {
            maps ??= Collect();
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                return;
            }

            for (var i = 0; i < maps.Count; i++)
            {
                if (maps[i].gameObject == selected)
                {
                    SetHidden(selected, true);
                    return;
                }
            }
        }

        static void Solo(List<Tilemap> maps, Tilemap keep)
        {
            for (var i = 0; i < maps.Count; i++)
            {
                SetHidden(maps[i].gameObject, maps[i] != keep);
            }

            if (keep != null)
            {
                Selection.activeGameObject = keep.gameObject;
                EditorGUIUtility.PingObject(keep.gameObject);
            }
        }

        static void AddUnique(List<Tilemap> maps, Tilemap map)
        {
            if (map != null && map.gameObject.scene.IsValid() && !maps.Contains(map))
            {
                maps.Add(map);
            }
        }

        static int CompareLayers(Tilemap a, Tilemap b)
        {
            var order = LayerOrder(a).CompareTo(LayerOrder(b));
            if (order != 0)
            {
                return order;
            }

            var sort = SortingOrder(a).CompareTo(SortingOrder(b));
            return sort != 0 ? sort : string.CompareOrdinal(a.gameObject.name, b.gameObject.name);
        }

        static int LayerOrder(Tilemap map)
        {
            var name = map.gameObject.name.ToLowerInvariant();
            if (name.IndexOf("cover", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("overlay", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("aura", System.StringComparison.Ordinal) >= 0)
            {
                return 3;
            }

            if (name.IndexOf("detail", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("decor", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("environment", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("enviroment", System.StringComparison.Ordinal) >= 0 ||
                name.IndexOf("enviromental", System.StringComparison.Ordinal) >= 0)
            {
                return 2;
            }

            if (name.IndexOf("wall", System.StringComparison.Ordinal) >= 0)
            {
                return 1;
            }

            return 0;
        }

        static int SortingOrder(Tilemap map)
        {
            var renderer = map != null ? map.GetComponent<TilemapRenderer>() : null;
            return renderer != null ? renderer.sortingOrder : 0;
        }

        [MenuItem("Window/Rune Magic/Show All Tile Layers", priority = 2)]
        static void MenuShowAll()
        {
            ShowAll();
        }

        [MenuItem("Window/Rune Magic/Hide Selected Tile Layer", priority = 3)]
        static void MenuHideSelected()
        {
            HideSelected();
        }

        [MenuItem("Window/Rune Magic/Hide Selected Tile Layer", true)]
        static bool MenuHideSelectedValidate()
        {
            var selected = Selection.activeGameObject;
            if (selected == null || selected.GetComponent<Tilemap>() == null)
            {
                return false;
            }

            var maps = Collect();
            for (var i = 0; i < maps.Count; i++)
            {
                if (maps[i].gameObject == selected)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Overlay(typeof(SceneView), "rune-magic-layers", "Rune Layers")]
    sealed class LayerSceneOverlay : IMGUIOverlay
    {
        public override void OnCreated()
        {
            displayed = true;
        }

        public override void OnGUI()
        {
            TileLayerVisibility.DrawGui(compact: true);
        }
    }

    [CustomEditor(typeof(LevelAuthoring))]
    public sealed class LevelAuthoringEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            TileLayerVisibility.DrawGui();
        }
    }
}
#endif
