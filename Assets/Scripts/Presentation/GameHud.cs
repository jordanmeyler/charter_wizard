using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class GameHud : MonoBehaviour
    {
        public const float BarHeight = 96f;

        SanctumDirector _director;
        Vector2 _pauseScroll;

        public void Bind(SanctumDirector director)
        {
            _director = director;
        }

        public static bool PointerOverChrome(PlayMode mode)
        {
            var mouse = Input.mousePosition;
            if (mouse.y <= BarHeight + 8f)
            {
                return true;
            }

            if (mode == PlayMode.Charter || mode == PlayMode.Grimoire || mode == PlayMode.Paused)
            {
                return true;
            }

            if (mode == PlayMode.Aiming && mouse.y <= BarHeight + 108f)
            {
                return true;
            }

            return mouse.x <= 560f && mouse.y >= Screen.height - 168f;
        }

        void OnGUI()
        {
            if (_director == null)
            {
                return;
            }

            if (_director.Mode == PlayMode.Paused)
            {
                DrawPause();
                DrawSpellBar();
                return;
            }

            if (_director.Mode == PlayMode.Grimoire)
            {
                DrawGrimoire();
                DrawSpellBar();
                return;
            }

            if (_director.Mode == PlayMode.Charter)
            {
                DrawCharter();
                DrawSpellBar();
                return;
            }

            if (_director.Mode == PlayMode.Aiming)
            {
                DrawWorldChrome();
                DrawAimDock();
                DrawSpellBar();
                return;
            }

            DrawWorldChrome();
            DrawSpellBar();
        }

        void DrawWorldChrome()
        {
            DrawPanel(12, 12, 540, 132);
            var title = Label(22, FontStyle.Bold, Color.white);
            var body = Label(15, FontStyle.Normal, new Color(0.88f, 0.9f, 0.95f));

            GUI.Label(new Rect(28, 18, 520, 26), "Rune Magic", title);
            GUI.Label(new Rect(28, 44, 520, 22), RoomLine(), body);
            GUI.Label(new Rect(28, 66, 520, 22), TargetLine(), body);
            GUI.Label(new Rect(28, 90, 510, 46), _director.LastLog, body);
        }

        void DrawCharter()
        {
            DrawVeil(new Color(0.03f, 0.04f, 0.07f, 0.38f));

            var title = Label(26, FontStyle.Bold, Color.white);
            var body = Label(15, FontStyle.Normal, new Color(0.86f, 0.88f, 0.94f));
            var hint = Label(13, FontStyle.Normal, new Color(0.7f, 0.74f, 0.82f));

            GUI.Label(new Rect(28, 16, 800, 32), "The Charter", title);
            GUI.Label(new Rect(28, 50, 900, 22),
                "A chain is a sentence. Two runes birth a join or wait. Cast when the story is finished.",
                body);
            GUI.Label(new Rect(28, 74, 900, 20),
                $"Stance: {_director.Composer.Stance}   ·   Tab/Q flip   ·   Space close   ·   Esc / Grimoire",
                hint);

            DrawRuneWall();
            DrawComposeDock();
        }

        void DrawRuneWall()
        {
            var runes = _director.VisibleRunes;
            const float left = 28f;
            const float top = 108f;
            const float size = 92f;
            const float gap = 12f;
            var columns = Mathf.Max(1, Mathf.FloorToInt((Screen.width - 56f) / (size + gap)));

            for (var i = 0; i < runes.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                var rect = new Rect(left + col * (size + gap), top + row * (size + gap), size, size);
                DrawRuneCard(rect, runes[i], () => _director.AddRune(runes[i]));
            }
        }

        void DrawComposeDock()
        {
            var dockHeight = 188f;
            var dockTop = Screen.height - dockHeight - BarHeight;
            DrawPanel(0, dockTop, Screen.width, dockHeight);

            var body = Label(14, FontStyle.Normal, new Color(0.86f, 0.88f, 0.94f));
            var accent = Label(16, FontStyle.Bold, new Color(0.9f, 0.82f, 0.55f));
            GUI.Label(new Rect(24, dockTop + 10, 640, 22), "String", accent);
            GUI.Label(new Rect(24, dockTop + 86, Screen.width - 48, 22),
                _director.Composer.Describe(), body);

            DrawDraftSlots(dockTop + 36);
            DrawCharterActions(dockTop + 112);
        }

        void DrawDraftSlots(float y)
        {
            const float slot = 56f;
            const float gap = 8f;
            var startX = 24f;
            for (var i = 0; i < SpellComposer.MaxSlots; i++)
            {
                var rect = new Rect(startX + i * (slot + gap), y, slot, slot);
                if (i < _director.Composer.Count)
                {
                    var index = i;
                    DrawRuneCard(rect, _director.Composer.Slots[i], () => _director.RemoveDraftFrom(index));
                }
                else
                {
                    DrawEmptySlot(rect, (i + 1).ToString());
                }
            }
        }

        void DrawCharterActions(float y)
        {
            var canAct = !_director.Composer.IsEmpty;
            if (DrawAction(new Rect(24, y, 160, 42), "Cast", canAct, new Color(0.72f, 0.28f, 0.22f)))
            {
                _director.CastDraft();
            }

            if (DrawAction(new Rect(196, y, 160, 42), "Store", canAct, new Color(0.28f, 0.38f, 0.62f)))
            {
                _director.StoreDraft();
            }

            if (DrawAction(new Rect(368, y, 120, 42), "Clear", canAct, new Color(0.22f, 0.22f, 0.26f)))
            {
                _director.ClearDraft();
            }

            var held = _director.Held.Occupied ? _director.Held.Name : "empty";
            var body = Label(14, FontStyle.Normal, new Color(0.84f, 0.86f, 0.92f));
            GUI.Label(new Rect(508, y + 2, 360, 38),
                $"Held: {held}\nOne slot. Store rewrites it.", body);

            if (_director.Held.Occupied &&
                DrawAction(new Rect(Screen.width - 184, y, 160, 42), "Aim held", true, new Color(0.42f, 0.3f, 0.18f)))
            {
                _director.CastHeld();
            }
        }

        void DrawSpellBar()
        {
            var y = Screen.height - BarHeight;
            DrawPanel(0, y, Screen.width, BarHeight);

            var title = Label(18, FontStyle.Bold, new Color(0.95f, 0.86f, 0.52f));
            var body = Label(14, FontStyle.Normal, new Color(0.86f, 0.88f, 0.94f));
            var hint = Label(13, FontStyle.Normal, new Color(0.7f, 0.74f, 0.82f));

            var slot = new Rect(16, y + 12, 280, BarHeight - 24);
            DrawHeldSlot(slot);

            GUI.Label(new Rect(312, y + 14, 420, 24), "Stored spell", title);
            if (_director.Held.Occupied)
            {
                GUI.Label(new Rect(312, y + 40, 440, 40),
                    $"{_director.Held.Name}  ·  {_director.Held.Stance}\nClick the slot or press F, then choose a form and aim.",
                    body);
            }
            else
            {
                GUI.Label(new Rect(312, y + 40, 440, 40),
                    "Empty. Space opens the Charter. String runes, then Store.",
                    body);
            }

            GUI.Label(new Rect(760, y + 16, Mathf.Max(160f, Screen.width - 980f), 64),
                "WASD move · Space Charter · Cast then aim · Esc / Grimoire",
                hint);

            var grim = new Rect(Screen.width - 188, y + 22, 168, 52);
            var open = _director.Mode == PlayMode.Grimoire;
            if (DrawAction(grim, open ? "Close book" : "Grimoire", true,
                    open ? new Color(0.55f, 0.42f, 0.18f) : new Color(0.32f, 0.24f, 0.42f)))
            {
                if (open)
                {
                    _director.CloseGrimoire();
                }
                else
                {
                    _director.OpenGrimoire();
                }
            }
        }

        void DrawAimDock()
        {
            var height = 92f;
            var y = Screen.height - BarHeight - height;
            DrawPanel(0, y, Screen.width, height);

            var title = Label(18, FontStyle.Bold, new Color(0.95f, 0.86f, 0.52f));
            var body = Label(13, FontStyle.Normal, new Color(0.84f, 0.86f, 0.92f));
            GUI.Label(new Rect(16, y + 8, 720, 22),
                $"Aim  ·  {_director.PendingPreview}  ·  {_director.PendingStance}", title);

            var shapes = _director.AvailableShapes;
            if (shapes.Count == 0)
            {
                GUI.Label(new Rect(16, y + 36, 640, 40),
                    _director.PendingStance == CastingStance.Free
                        ? "No natural form. Pick any formation — Free borrows a written spell of that type."
                        : "No natural form. Click the world to fizzle, or Esc to keep the string.",
                    body);
            }
            else
            {
                var x = 16f;
                foreach (var shape in shapes)
                {
                    var chosen = _director.ChosenShape == shape;
                    var color = chosen
                        ? new Color(0.72f, 0.42f, 0.18f)
                        : new Color(0.22f, 0.32f, 0.48f);
                    var picked = shape;
                    if (DrawAction(new Rect(x, y + 36, 118, 42), SpellFormations.NameOf(shape), true, color))
                    {
                        _director.ChooseShape(picked);
                    }

                    x += 128f;
                }

                var hint = _director.ChosenShape == SpellShape.None
                    ? "Pick a formation, then click the world."
                    : SpellFormations.Get(_director.ChosenShape).Hint;
                GUI.Label(new Rect(x + 8, y + 40, 420, 36), hint, body);
            }

            if (DrawAction(new Rect(Screen.width - 176, y + 36, 160, 42), "Cancel", true, new Color(0.28f, 0.22f, 0.22f)))
            {
                _director.CancelAim();
            }
        }

        void DrawHeldSlot(Rect rect)
        {
            var occupied = _director.Held.Occupied;
            var fill = occupied
                ? new Color(0.62f, 0.28f, 0.16f, 0.95f)
                : new Color(0.12f, 0.13f, 0.18f, 0.9f);
            var previous = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.86f, 0.4f, occupied ? 0.85f : 0.2f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3f), Texture2D.whiteTexture);
            GUI.color = previous;

            var name = Label(20, FontStyle.Bold, occupied ? Color.white : new Color(0.55f, 0.58f, 0.66f));
            name.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect, occupied ? _director.Held.Name : "No spell stored", name);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none) && occupied && !_director.Busy)
            {
                _director.CastHeld();
            }
        }

        void DrawGrimoire()
        {
            DrawVeil(new Color(0.02f, 0.02f, 0.05f, 0.78f));
            var title = Label(28, FontStyle.Bold, Color.white);
            var subtitle = Label(15, FontStyle.Normal, new Color(0.78f, 0.8f, 0.88f));
            var heading = Label(17, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var row = Label(14, FontStyle.Normal, new Color(0.88f, 0.9f, 0.94f));
            var muted = Label(13, FontStyle.Italic, new Color(0.68f, 0.7f, 0.78f));

            GUI.Label(new Rect(40, 20, 800, 34), "Grimoire", title);
            GUI.Label(new Rect(40, 56, 980, 22),
                "Ordinary book has no Death. Life marks a living recipe. 41–50 are Free / grave-work. Esc closes.",
                subtitle);

            var view = new Rect(40, 92, Screen.width - 80, Screen.height - BarHeight - 112);
            var innerHeight = CodexHeight();
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, view.width - 24, innerHeight));
            var y = DrawCodex(0f, heading, row, muted);

            y += 16f;
            GUI.Label(new Rect(0, y, 700, 24), "Material joins", heading);
            y += 28f;
            foreach (var blend in MaterialTree.All)
            {
                var tone = blend.Result.Kind == BlendKind.Violent ? "violent" : "stable";
                GUI.Label(new Rect(0, y, 820, 20),
                    $"{RuneCatalog.NameOf(blend.Left)} + {RuneCatalog.NameOf(blend.Right)} → {RuneCatalog.NameOf(blend.Result.Result)}   ({tone})",
                    row);
                y += 22f;
            }

            GUI.EndScrollView();
        }

        void DrawPause()
        {
            DrawVeil(new Color(0.02f, 0.02f, 0.04f, 0.86f));
            var title = Label(28, FontStyle.Bold, Color.white);
            var subtitle = Label(15, FontStyle.Normal, new Color(0.78f, 0.8f, 0.88f));
            var heading = Label(17, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var row = Label(14, FontStyle.Normal, new Color(0.88f, 0.9f, 0.94f));
            var muted = Label(13, FontStyle.Italic, new Color(0.68f, 0.7f, 0.78f));

            GUI.Label(new Rect(40, 24, 800, 34), "Paused — written spells", title);
            GUI.Label(new Rect(40, 62, 980, 22),
                "Developer ledger. Full codex draft plus material joins. Esc resumes.",
                subtitle);

            var view = new Rect(40, 100, Screen.width - 80, Screen.height - 140);
            var innerHeight = CodexHeight();
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, view.width - 24, innerHeight));
            var y = DrawCodex(0f, heading, row, muted);

            y += 16f;
            GUI.Label(new Rect(0, y, 700, 24), "Material joins", heading);
            y += 28f;
            foreach (var blend in MaterialTree.All)
            {
                var tone = blend.Result.Kind == BlendKind.Violent ? "violent" : "stable";
                GUI.Label(new Rect(0, y, 820, 20),
                    $"{RuneCatalog.NameOf(blend.Left)} + {RuneCatalog.NameOf(blend.Right)} → {RuneCatalog.NameOf(blend.Result.Result)}   ({tone})",
                    row);
                y += 22f;
            }

            GUI.EndScrollView();
        }

        float DrawCodex(float y, GUIStyle heading, GUIStyle row, GUIStyle muted)
        {
            GUI.Label(new Rect(0, y, 900, 22), "The eleven basic runes", heading);
            y += 24f;
            GUI.Label(new Rect(0, y, 980, 18), "Roots: Fire (hunger) · Air (breath) · Earth (rest) · Water (yield)", row);
            y += 18f;
            GUI.Label(new Rect(0, y, 980, 18), "Operators: Salt (a body) · Mercury (going) · Sulphur (passion / mind)", row);
            y += 18f;
            GUI.Label(new Rect(0, y, 980, 18), "Veils: Light (shown) · Dark (withheld)     Modifiers: Life (marks a living recipe) · Death (grave / Free only)", row);
            y += 18f;
            GUI.Label(new Rect(0, y, 980, 18), "A join is a new rune. Fire·Air→Spark. Ice is Water·Salt·Earth (not Death). Plant·Life→Grove.", row);
            y += 28f;

            SpellBook? current = null;
            foreach (var entry in SpellCodex.All)
            {
                if (current != entry.Book)
                {
                    if (current != null)
                    {
                        y += 10f;
                    }

                    current = entry.Book;
                    GUI.Label(new Rect(0, y, 900, 22), SpellCodex.BookName(entry.Book), heading);
                    y += 24f;
                }

                GUI.Label(new Rect(0, y, 24, 18), entry.Number.ToString(), muted);
                var title = string.IsNullOrEmpty(entry.Gate) ? entry.Name : $"{entry.Name}  ({entry.Gate})";
                GUI.Label(new Rect(26, y, 130, 18), title, row);
                GUI.Label(new Rect(160, y, 340, 18), entry.Want, row);
                var chain = string.IsNullOrEmpty(entry.Via)
                    ? entry.Recipe
                    : $"{entry.Recipe}   =   {entry.Via}";
                GUI.Label(new Rect(506, y, 330, 18), chain, muted);
                GUI.Label(new Rect(840, y, 60, 18), entry.Form, muted);
                GUI.Label(new Rect(904, y, 80, 18), entry.Outcome.ToString(), muted);
                y += 20f;
            }

            return y;
        }

        static float CodexHeight()
        {
            return 160f + SpellCodex.All.Count * 20f + 12 * 28f + MaterialTree.All.Count * 22f;
        }

        void DrawRuneCard(Rect rect, RuneId rune, System.Action onClick)
        {
            var fill = Color.Lerp(RunePalette.Of(rune), new Color(0.08f, 0.08f, 0.1f), 0.25f);
            fill.a = 0.92f;
            var previous = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.35f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), Texture2D.whiteTexture);
            GUI.color = previous;

            var glyph = Label(rect.height > 70f ? 22 : 16, FontStyle.Bold, Color.white);
            glyph.alignment = TextAnchor.MiddleCenter;
            var name = Label(rect.height > 70f ? 12 : 10, FontStyle.Normal, new Color(0.1f, 0.08f, 0.08f));
            name.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, rect.height * 0.45f),
                RuneCatalog.GlyphOf(rune), glyph);
            GUI.Label(new Rect(rect.x, rect.y + rect.height * 0.5f, rect.width, rect.height * 0.42f),
                RuneCatalog.NameOf(rune), name);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                onClick?.Invoke();
            }
        }

        static void DrawEmptySlot(Rect rect, string caption)
        {
            var previous = GUI.color;
            GUI.color = new Color(0.12f, 0.13f, 0.18f, 0.7f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.12f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1f), Texture2D.whiteTexture);
            GUI.color = previous;
            var label = Label(12, FontStyle.Normal, new Color(0.45f, 0.48f, 0.55f));
            label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect, caption, label);
        }

        static bool DrawAction(Rect rect, string text, bool enabled, Color color)
        {
            var previous = GUI.enabled;
            GUI.enabled = enabled;
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            var clicked = GUI.Button(rect, text, style);
            GUI.backgroundColor = bg;
            GUI.enabled = previous;
            return clicked && enabled;
        }

        string RoomLine()
        {
            var room = _director.CurrentRoom != null ? _director.CurrentRoom.Name : "Sanctum";
            var tile = _director.Underfoot != null ? _director.Underfoot.Def.DisplayName : "empty air";
            return $"Room: {room}   Underfoot: {tile}";
        }

        string TargetLine()
        {
            var target = _director.CurrentTarget;
            if (target == null)
            {
                return "No lock in reach. Walk up to a creature or fixture, then compose.";
            }

            var reading = _director.Grimoire.KnowsInterpretation(target.FormulaId)
                ? $"Known reading: {target.DisplayName}."
                : "The formula is visible. Its meaning is not yet yours.";
            var ready = _director.Held.Occupied
                ? $"Press F or click the slot to choose how {_director.Held.Name} aims."
                : "Space to compose a key, or click the lock.";
            return $"{target.DisplayName}  {{{target.FormulaText()}}}  — {reading} {ready}";
        }

        static GUIStyle Label(int size, FontStyle style, Color color)
        {
            var label = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = style,
                wordWrap = true
            };
            label.normal.textColor = color;
            return label;
        }

        static void DrawPanel(float x, float y, float width, float height)
        {
            var color = GUI.color;
            GUI.color = new Color(0.05f, 0.06f, 0.1f, 0.82f);
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
            GUI.color = color;
        }

        static void DrawVeil(Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
