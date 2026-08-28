#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Mark chips for inscriptions and Cover stamps. Same generated
    /// signs as Play.
    /// </summary>
    public static class RunePicker
    {
        const float Cell = 44f;

        public static void Draw(ref RuneId current)
        {
            Draw(RuneCatalog.PlaceableRunes(), ref current);
        }

        static readonly TileCover[] LookOnly = { TileCover.Cracks, TileCover.Seal };

        public static void DrawCover(ref TileCover current)
        {
            var spoken = CoverCatalog.Spoken;
            var total = spoken.Length + LookOnly.Length + 1;
            var wide = Columns();
            var rows = Mathf.CeilToInt(total / (float)wide);
            var height = rows * (Cell + 18f);
            var area = GUILayoutUtility.GetRect(wide * (Cell + 4f), height);
            DrawNone(new Rect(area.x, area.y, Cell, Cell), ref current);
            for (var i = 0; i < spoken.Length; i++)
            {
                DrawCoverCell(CellRect(area, wide, i + 1), spoken[i], ref current);
            }

            for (var i = 0; i < LookOnly.Length; i++)
            {
                DrawLookOnly(CellRect(area, wide, spoken.Length + 1 + i), LookOnly[i], ref current);
            }
        }

        static Rect CellRect(Rect area, int wide, int slot)
        {
            var col = slot % wide;
            var row = slot / wide;
            return new Rect(area.x + col * (Cell + 4f), area.y + row * (Cell + 18f), Cell, Cell);
        }

        static void DrawLookOnly(Rect rect, TileCover cover, ref TileCover current)
        {
            var selected = current == cover;
            EditorGUI.DrawRect(rect, selected
                ? new Color(0.92f, 0.78f, 0.28f, 0.55f)
                : new Color(0.2f, 0.18f, 0.16f, 0.85f));
            Label(rect, cover.ToString(), selected);
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                current = cover;
            }
        }

        static void Draw(RuneId[] runes, ref RuneId current)
        {
            var wide = Columns();
            var rows = Mathf.CeilToInt(runes.Length / (float)wide);
            var height = rows * (Cell + 18f);
            var area = GUILayoutUtility.GetRect(wide * (Cell + 4f), height);
            for (var i = 0; i < runes.Length; i++)
            {
                var rune = runes[i];
                var col = i % wide;
                var row = i / wide;
                var rect = new Rect(area.x + col * (Cell + 4f), area.y + row * (Cell + 18f), Cell, Cell);
                var fill = current == rune
                    ? new Color(0.92f, 0.78f, 0.28f, 0.55f)
                    : RunePalette.Card(rune, true);
                EditorGUI.DrawRect(rect, fill);
                RuneMark.DrawGui(rect, rune, RunePalette.MarkInk(rune));
                Label(rect, RuneCatalog.NameOf(rune), current == rune);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    current = rune;
                }
            }
        }

        static void DrawNone(Rect rect, ref TileCover current)
        {
            var selected = current == TileCover.None;
            EditorGUI.DrawRect(rect, selected
                ? new Color(0.92f, 0.78f, 0.28f, 0.55f)
                : new Color(0.16f, 0.15f, 0.14f, 0.8f));
            Label(rect, "None", selected);
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                current = TileCover.None;
            }
        }

        static void DrawCoverCell(Rect rect, TileCover cover, ref TileCover current)
        {
            var rune = CoverCatalog.RuneOf(cover);
            var selected = current == cover;
            var fill = selected
                ? new Color(0.92f, 0.78f, 0.28f, 0.55f)
                : RunePalette.Card(rune, true);
            EditorGUI.DrawRect(rect, fill);
            RuneMark.DrawGui(rect, rune, RunePalette.MarkInk(rune));
            Label(rect, RuneCatalog.NameOf(rune), selected);
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                current = cover;
            }
        }

        static void Label(Rect rect, string text, bool selected)
        {
            var label = new Rect(rect.x, rect.yMax, rect.width, 16f);
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 9
            };
            style.normal.textColor = selected
                ? new Color(0.95f, 0.86f, 0.45f)
                : new Color(0.82f, 0.78f, 0.7f);
            GUI.Label(label, text, style);
        }

        static int Columns()
        {
            return Mathf.Max(4, Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 28f) / (Cell + 4f)));
        }
    }
}
#endif
