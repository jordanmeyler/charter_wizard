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

        public static bool PointerOverChrome(PlayMode mode) => BlocksWorldPick(mode);

        public static bool BlocksWorldPick(PlayMode mode)
        {
            var mouse = Input.mousePosition;
            if (mouse.y <= BarHeight + 8f)
            {
                return true;
            }

            if (mode == PlayMode.Grimoire || mode == PlayMode.Paused)
            {
                return true;
            }

            if (mode == PlayMode.Aiming && mouse.y <= BarHeight + 120f)
            {
                return true;
            }

            if (mouse.x <= 560f && mouse.y >= Screen.height - 168f)
            {
                return true;
            }

            if (mode == PlayMode.Charter)
            {
                return true;
            }

            return false;
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
            DrawPanel(12, 12, 560, 144);
            var title = Label(22, FontStyle.Bold, Color.white);
            var body = Label(15, FontStyle.Normal, new Color(0.88f, 0.9f, 0.95f));

            GUI.Label(new Rect(28, 18, 520, 26), "Rune Magic", title);
            GUI.Label(new Rect(28, 44, 520, 22), RoomLine(), body);
            GUI.Label(new Rect(28, 66, 520, 22), FieldLine(), body);
            GUI.Label(new Rect(28, 88, 510, 48), TargetAndLog(), body);
        }

        void DrawCharter()
        {
            DrawVeil(new Color(0.03f, 0.04f, 0.07f, 0.38f));

            var title = Label(26, FontStyle.Bold, Color.white);
            var body = Label(15, FontStyle.Normal, new Color(0.86f, 0.88f, 0.94f));
            var hint = Label(13, FontStyle.Normal, new Color(0.7f, 0.74f, 0.82f));

            GUI.Label(new Rect(28, 16, 800, 32), "The Charter", title);
            GUI.Label(new Rect(28, 50, 980, 22),
                "The wall is the eleven — only what is on screen can be drawn. Gold ring = a join (Plant is Water · Earth · Salt). Hover the weave to hold it still.",
                body);
            GUI.Label(new Rect(28, 74, 980, 20),
                "F / Enter Charter Cast   ·   X Free Cast   ·   R Store (Charter only)   ·   Space close   ·   Esc / Grimoire",
                hint);

            var wallBottom = DrawRuneWall();
            DrawRoomWeave(wallBottom + 6f);
            DrawComposeDock();
        }

        float DrawRuneWall()
        {
            var runes = _director.VisibleRunes;
            const float left = 28f;
            const float top = 98f;
            const float size = 68f;
            const float gap = 8f;
            var columns = Mathf.Max(1, Mathf.FloorToInt((Screen.width - 56f) / (size + gap)));
            var rows = 1;
            for (var i = 0; i < runes.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                rows = row + 1;
                var rect = new Rect(left + col * (size + gap), top + row * (size + gap), size, size);
                var rune = runes[i];
                DrawRuneCard(rect, rune, () => _director.AddRune(rune), _director.InVicinity(rune));
            }

            return top + rows * (size + gap);
        }

        void DrawRoomWeave(float top)
        {
            var tapestry = _director.Tapestry;
            var dockTop = Screen.height - 214f - BarHeight;
            var height = Mathf.Max(72f, dockTop - top - 8f);
            var panel = new Rect(16, top, Screen.width - 32, height);
            DrawPanel(panel.x, panel.y, panel.width, panel.height);

            var heading = Label(15, FontStyle.Bold, new Color(0.9f, 0.82f, 0.55f));
            var hint = Label(13, FontStyle.Normal, new Color(0.72f, 0.76f, 0.84f));
            GUI.Label(new Rect(28, top + 8, 720, 20), "The room’s weave", heading);
            var reading = tapestry != null && !string.IsNullOrEmpty(tapestry.Reading)
                ? tapestry.Reading
                : "the field is quiet";
            GUI.Label(new Rect(220, top + 8, Screen.width - 260, 20), reading, hint);

            var rows = RuneTapestry.Rows;
            var cols = RuneTapestry.Cols;
            var gap = 5f;
            var gridTop = top + 32f;
            var cell = Mathf.Min(56f, (height - 40f) / rows - gap);
            cell = Mathf.Max(34f, cell);
            var stride = cell + gap;
            var width = cols * stride;
            var left = Mathf.Max(28f, (Screen.width - width) * 0.5f);
            var band = new Rect(left - 2f, gridTop - 2f, width + 4f, rows * stride + 4f);
            var mouse = Event.current != null ? Event.current.mousePosition : Vector2.zero;
            if (tapestry != null)
            {
                tapestry.HoverPaused = band.Contains(mouse);
            }

            var slide = tapestry != null ? tapestry.Scroll - Mathf.Floor(tapestry.Scroll) : 0f;
            var shift = slide * stride;

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col <= cols; col++)
                {
                    var x = left + col * stride - shift;
                    var rect = new Rect(x, gridTop + row * stride, cell, cell);
                    if (rect.xMax < band.xMin || rect.x > band.xMax)
                    {
                        continue;
                    }

                    var glyph = tapestry != null
                        ? tapestry.Cell(row, col)
                        : new WeaveGlyph(RuneId.None, MaterialId.None, WeaveKind.Tear);
                    if (glyph.IsTear)
                    {
                        DrawEmptySlot(rect, "tear");
                        continue;
                    }

                    var rune = glyph.Rune;
                    DrawRuneCard(rect, rune, () => _director.WeaveFromField(rune), true);
                }
            }
        }

        void DrawComposeDock()
        {
            var dockHeight = 214f;
            var dockTop = Screen.height - dockHeight - BarHeight;
            DrawPanel(0, dockTop, Screen.width, dockHeight);

            var body = Label(14, FontStyle.Normal, new Color(0.86f, 0.88f, 0.94f));
            var accent = Label(16, FontStyle.Bold, new Color(0.9f, 0.82f, 0.55f));
            GUI.Label(new Rect(24, dockTop + 10, 640, 22), "String", accent);
            GUI.Label(new Rect(24, dockTop + 86, Screen.width - 48, 20),
                _director.Composer.Describe(), body);
            GUI.Label(new Rect(24, dockTop + 106, Screen.width - 48, 20),
                _director.Composer.DescribeFree(_director.Attunement), body);

            DrawDraftSlots(dockTop + 36);
            DrawCharterActions(dockTop + 132);
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
                    DrawRuneCard(rect, _director.Composer.Slots[i], () => _director.RemoveDraftFrom(index), true);
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
            if (DrawAction(new Rect(24, y, 148, 42), "Charter Cast", canAct, new Color(0.72f, 0.28f, 0.22f)))
            {
                _director.CastCharter();
            }

            if (DrawAction(new Rect(180, y, 132, 42), "Store", canAct, new Color(0.28f, 0.38f, 0.62f)))
            {
                _director.StoreDraft();
            }

            if (DrawAction(new Rect(320, y, 148, 42), "Free Cast", canAct, new Color(0.52f, 0.22f, 0.48f)))
            {
                _director.CastFree();
            }

            if (DrawAction(new Rect(476, y, 100, 42), "Clear", canAct, new Color(0.22f, 0.22f, 0.26f)))
            {
                _director.ClearDraft();
            }

            var held = _director.Held.Occupied ? _director.Held.Name : "empty";
            var body = Label(13, FontStyle.Normal, new Color(0.84f, 0.86f, 0.92f));
            GUI.Label(new Rect(588, y + 2, 280, 38),
                $"Held: {held}\nCharter only. Free cannot be stored.", body);

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
                    $"{_director.Held.Name}  ·  Charter\nClick the slot or press F, then aim. The chain already wrote the form.",
                    body);
            }
            else
            {
                GUI.Label(new Rect(312, y + 40, 440, 40),
                    "Empty. Store is a Charter benefit. Free is wild and cannot be held.",
                    body);
            }

            GUI.Label(new Rect(760, y + 16, Mathf.Max(160f, Screen.width - 980f), 64),
                "WASD move · Space Charter · F Charter Cast · X Free · R Store · Esc / Grimoire",
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

            var shape = _director.ChosenShape;
            if (shape == SpellShape.None)
            {
                GUI.Label(new Rect(16, y + 36, 720, 40),
                    "The chain did not write a form. Click the world to fizzle, or Esc to keep the string.",
                    body);
            }
            else
            {
                var def = SpellFormations.Get(shape);
                GUI.Label(new Rect(16, y + 36, 720, 40),
                    $"{def.Name} is in the sentence. {def.Hint}",
                    body);
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
                $"{_director.Attunement.Notes()}   ·   Click a name to string it. Materials sit with the spells. 41–50: Death / Free. Esc closes.",
                subtitle);

            var view = new Rect(40, 92, Screen.width - 80, Screen.height - BarHeight - 112);
            var innerHeight = CodexHeight();
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, view.width - 24, innerHeight));
            var y = DrawCodex(0f, heading, row, muted, loadable: true);

            y = DrawMaterialsLedger(y, heading, row, muted);

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
                "Developer ledger. Written spells, world materials, and joins. Esc resumes.",
                subtitle);

            var view = new Rect(40, 100, Screen.width - 80, Screen.height - 140);
            var innerHeight = CodexHeight();
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, view.width - 24, innerHeight));
            var y = DrawCodex(0f, heading, row, muted, loadable: false);

            y = DrawMaterialsLedger(y, heading, row, muted);

            GUI.EndScrollView();
        }

        float DrawMaterialsLedger(float y, GUIStyle heading, GUIStyle row, GUIStyle muted)
        {
            y += 16f;
            GUI.Label(new Rect(0, y, 700, 24), "World materials", heading);
            y += 22f;
            GUI.Label(new Rect(0, y, 980, 18),
                "Stamp MaterialId on a tile. The Charter weave reads the full signature, not just a root.",
                muted);
            y += 22f;
            foreach (var material in MaterialCatalog.All)
            {
                GUI.Label(new Rect(0, y, 150, 20), material.Name, row);
                GUI.Label(new Rect(160, y, 280, 20), material.SignatureText(), muted);
                GUI.Label(new Rect(450, y, 620, 20), material.Note, row);
                y += 20f;
            }

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

            return y;
        }

        float DrawCodex(float y, GUIStyle heading, GUIStyle row, GUIStyle muted, bool loadable)
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
                var nameRect = new Rect(26, y, 130, 18);
                if (loadable && GUI.Button(nameRect, title, row))
                {
                    _director.LoadCodex(entry.Number);
                }
                else if (!loadable)
                {
                    GUI.Label(nameRect, title, row);
                }
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
            return 220f + SpellCodex.All.Count * 20f + 12 * 28f
                + MaterialCatalog.All.Count * 20f + MaterialTree.All.Count * 22f;
        }

        void DrawRuneCard(Rect rect, RuneId rune, System.Action onClick, bool available)
        {
            var wrought = ChainBook.IsWrought(rune);
            var fill = Color.Lerp(RunePalette.Of(rune), new Color(0.08f, 0.08f, 0.1f), available ? 0.25f : 0.72f);
            fill.a = available ? 0.92f : 0.35f;
            var previous = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, available ? 0.35f : 0.08f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), Texture2D.whiteTexture);
            GUI.color = previous;

            if (wrought && available)
            {
                DrawWroughtMark(rect);
            }

            var glyphColor = available ? Color.white : new Color(0.55f, 0.56f, 0.6f, 0.45f);
            var birth = wrought ? ChainBook.BirthText(rune) : string.Empty;
            var main = !string.IsNullOrEmpty(birth) ? birth : RuneCatalog.GlyphOf(rune);
            var glyph = Label(rect.height > 70f ? 18 : 12, FontStyle.Bold, glyphColor);
            glyph.alignment = TextAnchor.MiddleCenter;
            glyph.wordWrap = true;
            var name = Label(rect.height > 70f ? 12 : 10, FontStyle.Normal,
                available ? new Color(0.1f, 0.08f, 0.08f) : new Color(0.45f, 0.46f, 0.5f, 0.55f));
            name.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x + 3, rect.y + 6, rect.width - 6, rect.height * 0.5f),
                main, glyph);
            var caption = available ? RuneCatalog.NameOf(rune) : "not in view";
            GUI.Label(new Rect(rect.x, rect.y + rect.height * 0.52f, rect.width, rect.height * 0.4f),
                caption, name);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                onClick?.Invoke();
            }
        }

        static void DrawWroughtMark(Rect rect)
        {
            var previous = GUI.color;
            var gold = new Color(0.98f, 0.82f, 0.32f, 0.95f);
            GUI.color = gold;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 3f, rect.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 3f, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - 3f, rect.y, 3f, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x + 6f, rect.yMax - 8f, rect.width - 12f, 2f), Texture2D.whiteTexture);
            GUI.color = previous;
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
            return $"{room}   ·   underfoot: {tile}";
        }

        string FieldLine()
        {
            return "The weave waits in the Charter. Only what is on screen can be drawn.";
        }

        string TargetAndLog()
        {
            var target = _director.CurrentTarget;
            var lockLine = target == null
                ? "Space opens the Charter. You can only draw runes that are on the screen."
                : $"{target.DisplayName}  {{{target.FormulaText()}}}";
            return $"{lockLine}\n{_director.LastLog}";
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
