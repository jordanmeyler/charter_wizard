#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Scene-view outline for each stamp. Adjacent cells of the same stamp
    /// share one glow so the painted tiles stay readable.
    /// Lives only in Assets/Editor — do not copy this script under
    /// Animations or any other asset folder.
    /// </summary>
    [InitializeOnLoad]
    static class RuneStampOverlay
    {
        const string EnabledPref = "RuneMagic.ShowStampOverlay.v2";
        const string LookOnlyPref = "RuneMagic.ShowStampLookOnly";

        static readonly Color Pit = new(0.95f, 0.2f, 0.75f, 1f);
        static readonly Color BlankPit = new(0.72f, 0.12f, 0.55f, 1f);
        static readonly Color Door = new(0.95f, 0.62f, 0.12f, 1f);
        static readonly Color Bridge = new(0.78f, 0.58f, 0.28f, 1f);
        static readonly Color Blocks = new(1f, 0.22f, 0.22f, 1f);
        static readonly Color AuraMiasma = new(0.3f, 0.88f, 0.22f, 1f);
        static readonly Color AuraFire = new(1f, 0.4f, 0.08f, 1f);
        static readonly Color AuraFog = new(0.82f, 0.86f, 0.92f, 1f);
        static readonly Color CoverIce = new(0.55f, 0.88f, 1f, 1f);
        static readonly Color CoverFire = new(1f, 0.48f, 0.12f, 1f);
        static readonly Color CoverLightning = new(1f, 0.92f, 0.2f, 1f);
        static readonly Color CoverWater = new(0.2f, 0.52f, 1f, 1f);
        static readonly Color CoverVine = new(0.38f, 0.78f, 0.22f, 1f);
        static readonly Color CoverOther = new(0.75f, 0.42f, 0.9f, 1f);
        static readonly Color LookOnly = new(1f, 0.86f, 0.15f, 1f);
        static readonly Color[] MaterialTones;
        static readonly List<Color> SeenColors = new();
        static readonly List<string> SeenLabels = new();
        static readonly Dictionary<int, int> PerMapCount = new();
        static readonly HashSet<Vector3Int> OccupiedCells = new();
        static Vector2 _legendScroll;
        public static int LastStampCount { get; private set; }

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledPref, true);
            set
            {
                if (EditorPrefs.GetBool(EnabledPref, true) == value)
                {
                    return;
                }

                EditorPrefs.SetBool(EnabledPref, value);
                SceneView.RepaintAll();
            }
        }

        public static bool ShowLookOnly
        {
            get => EditorPrefs.GetBool(LookOnlyPref, false);
            set
            {
                if (EditorPrefs.GetBool(LookOnlyPref, false) == value)
                {
                    return;
                }

                EditorPrefs.SetBool(LookOnlyPref, value);
                SceneView.RepaintAll();
            }
        }

        [InitializeOnLoadMethod]
        static void HookScene()
        {
            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;
        }

        [MenuItem("Window/Rune Magic/Show Stamps", priority = 1)]
        static void ToggleMenu()
        {
            Enabled = !Enabled;
        }

        [MenuItem("Window/Rune Magic/Show Stamps", true)]
        static bool ToggleMenuValidate()
        {
            Menu.SetChecked("Window/Rune Magic/Show Stamps", Enabled);
            return true;
        }

        static RuneStampOverlay()
        {
            HookScene();
            var max = 0;
            foreach (MaterialId id in System.Enum.GetValues(typeof(MaterialId)))
            {
                max = Mathf.Max(max, (int)id);
            }

            MaterialTones = new Color[max + 1];
            Tone(MaterialId.None, 0.55f, 0.55f, 0.58f);
            Tone(MaterialId.Stone, 0.95f, 0.82f, 0.38f);
            Tone(MaterialId.Ash, 0.68f, 0.6f, 0.54f);
            Tone(MaterialId.Timber, 0.8f, 0.5f, 0.18f);
            Tone(MaterialId.Hearth, 0.88f, 0.32f, 0.2f);
            Tone(MaterialId.Ember, 1f, 0.36f, 0.08f);
            Tone(MaterialId.Damp, 0.32f, 0.55f, 0.78f);
            Tone(MaterialId.Vein, 0.95f, 0.86f, 0.22f);
            Tone(MaterialId.Scoured, 0.62f, 0.7f, 0.76f);
            Tone(MaterialId.Moss, 0.42f, 0.72f, 0.22f);
            Tone(MaterialId.Metal, 0.7f, 0.76f, 0.86f);
            Tone(MaterialId.SaltCrust, 0.92f, 0.88f, 0.78f);
            Tone(MaterialId.Void, 0.48f, 0.16f, 0.55f);
            Tone(MaterialId.Ice, 0.35f, 0.9f, 1f);
            Tone(MaterialId.Sand, 0.9f, 0.74f, 0.32f);
            Tone(MaterialId.Mud, 0.5f, 0.32f, 0.16f);
            Tone(MaterialId.Lava, 1f, 0.28f, 0.05f);
            Tone(MaterialId.Steam, 0.78f, 0.86f, 0.9f);
            Tone(MaterialId.Dust, 0.74f, 0.64f, 0.46f);
            Tone(MaterialId.Glass, 0.32f, 0.78f, 0.84f);
            Tone(MaterialId.Crystal, 0.74f, 0.42f, 0.96f);
            Tone(MaterialId.Obsidian, 0.32f, 0.2f, 0.52f);
            Tone(MaterialId.Grove, 0.18f, 0.62f, 0.26f);
            Tone(MaterialId.Cloud, 0.84f, 0.9f, 0.98f);
            Tone(MaterialId.Rain, 0.28f, 0.48f, 0.8f);
            Tone(MaterialId.Snow, 0.95f, 0.97f, 1f);
            Tone(MaterialId.Glacier, 0.58f, 0.8f, 0.92f);
            Tone(MaterialId.Acid, 0.72f, 0.95f, 0.12f);
            Tone(MaterialId.Water, 0.12f, 0.46f, 0.98f);
            Tone(MaterialId.Plant, 0.28f, 0.82f, 0.3f);
            Tone(MaterialId.Dirt, 0.72f, 0.42f, 0.18f);
            Tone(MaterialId.Oil, 0.42f, 0.3f, 0.08f);
            Tone(MaterialId.Miasma, 0.4f, 0.72f, 0.12f);
            Tone(MaterialId.Wardstone, 0.56f, 0.4f, 0.78f);
            Tone(MaterialId.Aegis, 0.86f, 0.82f, 0.28f);
        }

        static void Tone(MaterialId id, float r, float g, float b)
        {
            MaterialTones[(int)id] = new Color(r, g, b, 1f);
        }

        static void OnScene(SceneView view)
        {
            if (!Enabled || Application.isPlaying || Event.current.type != EventType.Repaint)
            {
                return;
            }

            var maps = CollectMaps();
            DrawCells(maps);
            DrawOpenPits(maps);
            DrawSceneLegend();
        }

        public static string Describe(TileBase tile)
        {
            if (tile is WorldPaintTile paint)
            {
                var text = paint.kind + " / " + paint.material;
                if (paint.ResolvedCover() != TileCover.None)
                {
                    text += " / " + paint.ResolvedCover();
                }

                if (paint.blocks)
                {
                    text += " / blocks";
                }

                return text;
            }

            return tile != null ? tile.name + " (look only)" : "";
        }

        public static void DrawLegendGui()
        {
            var n = LastStampCount;
            EditorGUILayout.LabelField(n > 0
                ? n + " stamped cells in the Scene view"
                : "No stamped cells in view — stamp a cell or turn on look-only outlines.");
            EditorGUILayout.LabelField("Materials", EditorStyles.miniBoldLabel);
            _legendScroll = EditorGUILayout.BeginScrollView(_legendScroll, GUILayout.MaxHeight(220f));
            foreach (MaterialId id in System.Enum.GetValues(typeof(MaterialId)))
            {
                if (id == MaterialId.None)
                {
                    continue;
                }

                Swatch(MaterialColor(id), MaterialCatalog.Of(id).Name);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Other stamps", EditorStyles.miniBoldLabel);
            Swatch(Pit, "Pit");
            Swatch(BlankPit, "Blank space (pit at Play)");
            Swatch(Door, "Door");
            Swatch(AuraMiasma, "Miasma cover");
            Swatch(CoverIce, "Ice cover");
            Swatch(Blocks, "Blocks");
            if (ShowLookOnly)
            {
                Swatch(LookOnly, "Look only — Play guesses");
            }
        }

        static void Swatch(Color color, string label)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = GUILayoutUtility.GetRect(14f, 14f, GUILayout.Width(14f), GUILayout.Height(14f));
            EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, 1f));
            GUILayout.Label(label, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        static void DrawCells(List<Tilemap> maps)
        {
            SeenColors.Clear();
            SeenLabels.Clear();
            LastStampCount = 0;
            var oldZ = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            for (var i = 0; i < maps.Count; i++)
            {
                DrawMap(maps[i]);
            }

            Handles.zTest = oldZ;
        }

        static void DrawMap(Tilemap map)
        {
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var tile = map.GetTile(cell);
                if (tile == null || !TryColor(tile, out var color))
                {
                    continue;
                }

                LastStampCount++;
                Note(color, StampLabel(tile));
                DrawCellGlow(map, cell, color);
            }
        }

        static void DrawOpenPits(List<Tilemap> maps)
        {
            OccupiedCells.Clear();
            Tilemap guide = null;
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;
            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                if (map == null || LayerOrder(map) > 2)
                {
                    continue;
                }

                guide = guide != null ? guide : map;
                foreach (var cell in map.cellBounds.allPositionsWithin)
                {
                    if (map.GetTile(cell) == null)
                    {
                        continue;
                    }

                    OccupiedCells.Add(new Vector3Int(cell.x, cell.y, 0));
                    minX = Mathf.Min(minX, cell.x);
                    minY = Mathf.Min(minY, cell.y);
                    maxX = Mathf.Max(maxX, cell.x);
                    maxY = Mathf.Max(maxY, cell.y);
                }
            }

            if (guide == null || OccupiedCells.Count == 0)
            {
                return;
            }

            var oldZ = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            var any = false;
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (OccupiedCells.Contains(cell))
                    {
                        continue;
                    }

                    any = true;
                    DrawCellGlow(guide, cell, BlankPit);
                }
            }

            Handles.zTest = oldZ;
            if (any)
            {
                Note(BlankPit, "Blank space (pit at Play)");
            }
        }

        static void DrawCellGlow(Tilemap map, Vector3Int cell, Color color)
        {
            var center = map.GetCellCenterWorld(cell);
            var size = map.layoutGrid != null ? map.layoutGrid.cellSize : map.cellSize;
            var half = new Vector3(size.x * 0.48f, size.y * 0.48f, 0f);
            var min = center - half;
            var max = center + half;
            var verts = new[]
            {
                new Vector3(min.x, min.y, 0f),
                new Vector3(max.x, min.y, 0f),
                new Vector3(max.x, max.y, 0f),
                new Vector3(min.x, max.y, 0f)
            };
            Handles.DrawSolidRectangleWithOutline(
                verts,
                new Color(color.r, color.g, color.b, 0.16f),
                new Color(color.r, color.g, color.b, 0.95f));
        }

        public static void DrawGizmosFor(Tilemap map)
        {
            if (!Enabled || map == null || Application.isPlaying)
            {
                return;
            }

            var n = 0;
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var tile = map.GetTile(cell);
                if (tile == null || !TryColor(tile, out var color))
                {
                    continue;
                }

                n++;
                Note(color, StampLabel(tile));
                var center = map.GetCellCenterWorld(cell);
                var next = map.GetCellCenterWorld(cell + Vector3Int.right);
                var up = map.GetCellCenterWorld(cell + Vector3Int.up);
                var size = new Vector3(
                    Mathf.Max(0.2f, Vector3.Distance(center, next) * 0.92f),
                    Mathf.Max(0.2f, Vector3.Distance(center, up) * 0.92f),
                    0.02f);
                Gizmos.color = new Color(color.r, color.g, color.b, 0.22f);
                Gizmos.DrawCube(center, size);
                Gizmos.color = new Color(color.r, color.g, color.b, 1f);
                Gizmos.DrawWireCube(center, size);
            }

            PerMapCount[map.GetInstanceID()] = n;
            var total = 0;
            foreach (var pair in PerMapCount)
            {
                total += pair.Value;
            }

            LastStampCount = total;
        }

        static List<Tilemap> CollectMaps()
        {
            var found = Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var maps = new List<Tilemap>(found.Length);
            for (var i = 0; i < found.Length; i++)
            {
                if (found[i] != null && found[i].gameObject.scene.IsValid())
                {
                    maps.Add(found[i]);
                }
            }

            maps.Sort((a, b) => LayerOrder(a).CompareTo(LayerOrder(b)));
            return maps;
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
                name.IndexOf("environment", System.StringComparison.Ordinal) >= 0)
            {
                return 2;
            }

            if (name.IndexOf("wall", System.StringComparison.Ordinal) >= 0)
            {
                return 1;
            }

            return 0;
        }

        static bool TryColor(TileBase tile, out Color color)
        {
            if (tile is not WorldPaintTile paint)
            {
                color = LookOnly;
                return ShowLookOnly;
            }

            if (paint.kind == TileKind.Pit)
            {
                color = Pit;
                return true;
            }

            if (paint.kind == TileKind.Door)
            {
                color = Door;
                return true;
            }

            if (paint.kind == TileKind.Bridge)
            {
                color = Bridge;
                return true;
            }

            if (paint.blocks)
            {
                color = Blocks;
                return true;
            }

            if (paint.aura == TileAura.Fire)
            {
                color = AuraFire;
                return true;
            }

            switch (paint.ResolvedCover() != TileCover.None
                ? paint.ResolvedCover()
                : WorldPaintTile.CoverFromMaterial(paint.material))
            {
                case TileCover.Ice:
                    color = CoverIce;
                    return true;
                case TileCover.Fire:
                    color = CoverFire;
                    return true;
                case TileCover.Lightning:
                    color = CoverLightning;
                    return true;
                case TileCover.Water:
                    color = CoverWater;
                    return true;
                case TileCover.Vine:
                    color = CoverVine;
                    return true;
                case TileCover.Miasma:
                    color = AuraMiasma;
                    return true;
                case TileCover.Fog:
                    color = AuraFog;
                    return true;
                case TileCover.Cracks:
                case TileCover.Seal:
                    color = CoverOther;
                    return true;
            }

            if (paint.kind == TileKind.None)
            {
                if (paint.material != MaterialId.None && paint.material != MaterialId.Stone)
                {
                    color = MaterialColor(paint.material);
                    return true;
                }

                color = LookOnly;
                return ShowLookOnly;
            }

            color = MaterialColor(paint.material);
            if (paint.kind == TileKind.Wall)
            {
                color = Color.Lerp(color, new Color(0.12f, 0.12f, 0.14f), 0.32f);
            }

            return true;
        }

        static Color MaterialColor(MaterialId id)
        {
            var index = (int)id;
            if (index >= 0 && index < MaterialTones.Length && MaterialTones[index].a > 0f)
            {
                return MaterialTones[index];
            }

            var tone = MaterialCatalog.Of(id).FloorTone;
            Color.RGBToHSV(tone, out var h, out var s, out var v);
            return Color.HSVToRGB(h, Mathf.Max(0.45f, s), Mathf.Max(0.72f, v));
        }

        static string StampLabel(TileBase tile)
        {
            if (tile is WorldPaintTile paint)
            {
                if (paint.kind == TileKind.Pit)
                {
                    return "Pit";
                }

                if (paint.kind == TileKind.Door)
                {
                    return "Door";
                }

                if (paint.blocks)
                {
                    return "Blocks";
                }

                if (paint.aura == TileAura.Fire)
                {
                    return "Kindled";
                }

                if (paint.ResolvedCover() != TileCover.None)
                {
                    return paint.ResolvedCover() == TileCover.Fire
                        ? "Fire mark"
                        : paint.ResolvedCover() + " cover";
                }

                if (paint.kind == TileKind.None)
                {
                    return paint.material != MaterialId.None && paint.material != MaterialId.Stone
                        ? MaterialCatalog.Of(paint.material).Name + " (look)"
                        : "Look only";
                }

                return paint.kind + " / " + MaterialCatalog.Of(paint.material).Name;
            }

            return "Look only";
        }

        static void Note(Color color, string label)
        {
            for (var i = 0; i < SeenLabels.Count; i++)
            {
                if (SeenLabels[i] == label)
                {
                    return;
                }
            }

            SeenLabels.Add(label);
            SeenColors.Add(color);
        }

        static void DrawSceneLegend()
        {
            // Repaint-only Scene GUI cannot use GUILayout (needs a Layout pass).
            Handles.BeginGUI();
            var count = Mathf.Min(SeenLabels.Count, 16);
            var extra = SeenLabels.Count - count;
            var row = 16f;
            var height = 28f + count * row + (extra > 0 ? row : 0f);
            var box = new Rect(12f, 12f, 188f, height);
            EditorGUI.DrawRect(box, new Color(0.08f, 0.08f, 0.1f, 0.72f));

            var x = box.x + 8f;
            var y = box.y + 6f;
            var width = box.width - 16f;
            GUI.Label(new Rect(x, y, width, 18f), "Stamps", EditorStyles.boldLabel);
            y += 18f;
            for (var i = 0; i < count; i++)
            {
                var color = SeenColors[i];
                EditorGUI.DrawRect(new Rect(x, y + 3f, 10f, 10f), new Color(color.r, color.g, color.b, 1f));
                GUI.Label(new Rect(x + 14f, y, width - 14f, row), SeenLabels[i], EditorStyles.miniLabel);
                y += row;
            }

            if (extra > 0)
            {
                GUI.Label(new Rect(x, y, width, row), "+" + extra + " more in Tile Properties", EditorStyles.miniLabel);
            }

            Handles.EndGUI();
        }
    }

    [Overlay(typeof(SceneView), "rune-magic-stamps", "Rune Stamps")]
    sealed class StampSceneOverlay : IMGUIOverlay
    {
        public override void OnCreated()
        {
            displayed = true;
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            var on = EditorGUILayout.ToggleLeft("Show stamp colours", RuneStampOverlay.Enabled);
            if (EditorGUI.EndChangeCheck())
            {
                RuneStampOverlay.Enabled = on;
            }

            EditorGUI.BeginChangeCheck();
            var look = EditorGUILayout.ToggleLeft("Look-only cells", RuneStampOverlay.ShowLookOnly);
            if (EditorGUI.EndChangeCheck())
            {
                RuneStampOverlay.ShowLookOnly = look;
            }

            GUILayout.Label(
                RuneStampOverlay.LastStampCount > 0
                    ? RuneStampOverlay.LastStampCount + " stamped cells"
                    : "Waiting for Scene gizmos…",
                EditorStyles.miniLabel);
        }
    }

    static class StampGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active |
                   GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawTilemap(Tilemap map, GizmoType type)
        {
            RuneStampOverlay.DrawGizmosFor(map);
        }
    }
}
#endif
