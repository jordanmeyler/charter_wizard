using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class GameHud : MonoBehaviour
    {
        SanctumDirector _director;
        Vector2 _pauseScroll;

        public void Bind(SanctumDirector director)
        {
            _director = director;
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
                return;
            }

            if (_director.Mode == PlayMode.Charter)
            {
                DrawCharter();
                return;
            }

            DrawWorldChrome();
        }

        void DrawWorldChrome()
        {
            DrawPanel(12, 12, 540, 148);
            var title = Label(22, FontStyle.Bold, Color.white);
            var body = Label(15, FontStyle.Normal, new Color(0.88f, 0.9f, 0.95f));

            GUI.Label(new Rect(28, 20, 520, 28), "Rune Magic", title);
            GUI.Label(new Rect(28, 48, 520, 22), RoomLine(), body);
            GUI.Label(new Rect(28, 70, 520, 22), TargetLine(), body);
            GUI.Label(new Rect(28, 94, 510, 54), _director.LastLog, body);

            DrawHeldBar(Screen.height - 86);
        }

        void DrawCharter()
        {
            DrawPanel(12, 12, 620, 78);
            var title = Label(22, FontStyle.Bold, Color.white);
            var hint = Label(14, FontStyle.Normal, new Color(0.78f, 0.8f, 0.88f));
            GUI.Label(new Rect(28, 18, 590, 28), "The Charter — click a rune to string it", title);
            GUI.Label(new Rect(28, 48, 590, 32),
                $"Stance: {_director.Composer.Stance}   ·   Tab/Q flip   ·   Space close   ·   Esc recipes",
                hint);

            DrawComposeDock();
        }

        void DrawComposeDock()
        {
            var dockHeight = 188f;
            var dockTop = Screen.height - dockHeight;
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
                DrawAction(new Rect(Screen.width - 184, y, 160, 42), "Release held", true, new Color(0.42f, 0.3f, 0.18f)))
            {
                _director.CastHeld();
                _director.CloseCharter();
            }
        }

        void DrawHeldBar(float y)
        {
            DrawPanel(12, y, 760, 74);
            var body = Label(16, FontStyle.Bold, new Color(0.92f, 0.86f, 0.62f));
            var detail = Label(14, FontStyle.Normal, new Color(0.82f, 0.84f, 0.9f));
            var hint = Label(13, FontStyle.Normal, new Color(0.72f, 0.74f, 0.8f));
            if (_director.Held.Occupied)
            {
                GUI.Label(new Rect(28, y + 10, 360, 24), $"Held: {_director.Held.Name}", body);
                GUI.Label(new Rect(280, y + 10, 470, 24),
                    $"{_director.Held.Stance}  ·  F to release", detail);
            }
            else
            {
                GUI.Label(new Rect(28, y + 10, 720, 24),
                    "Held: empty    Space opens the Charter to compose.", detail);
            }

            GUI.Label(new Rect(28, y + 40, 720, 22),
                "WASD move  ·  Space read the field  ·  F release held spell  ·  Esc recipes",
                hint);
        }

        void DrawPause()
        {
            DrawVeil(new Color(0.04f, 0.05f, 0.08f, 0.55f));
            var panel = new Rect(24, 20, Mathf.Min(920, Screen.width - 48), Screen.height - 40);
            DrawPanel(panel.x, panel.y, panel.width, panel.height);

            var title = Label(26, FontStyle.Bold, Color.white);
            var subtitle = Label(15, FontStyle.Normal, new Color(0.82f, 0.84f, 0.9f));
            var heading = Label(18, FontStyle.Bold, new Color(0.95f, 0.84f, 0.45f));
            var name = Label(16, FontStyle.Bold, new Color(1f, 0.92f, 0.7f));
            var row = Label(15, FontStyle.Normal, Color.white);
            var muted = Label(14, FontStyle.Normal, new Color(0.75f, 0.78f, 0.85f));

            GUI.Label(new Rect(panel.x + 20, panel.y + 14, panel.width - 40, 32),
                "Paused — every written spell", title);
            GUI.Label(new Rect(panel.x + 20, panel.y + 48, panel.width - 40, 22),
                "Developer ledger. Esc resumes.", subtitle);

            var view = new Rect(panel.x + 16, panel.y + 80, panel.width - 32, panel.height - 100);
            var innerHeight = 56f + SpellGrammarCount() * 44f + 48f + MaterialTree.All.Count * 26f;
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, view.width - 24, innerHeight));

            GUI.Label(new Rect(0, 0, 700, 26), "Spells", heading);
            var y = 30f;
            var recipes = new List<SpellRecipe>(SpellGrammar.All);
            recipes.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            foreach (var recipe in recipes)
            {
                GUI.Label(new Rect(0, y, 240, 22), recipe.Name, name);
                GUI.Label(new Rect(250, y, 420, 22), SpellGrammar.RecipeLine(recipe), row);
                GUI.Label(new Rect(0, y + 20, view.width - 30, 20), recipe.Effect, muted);
                y += 44f;
            }

            y += 12f;
            GUI.Label(new Rect(0, y, 700, 26), "Material joins", heading);
            y += 30f;
            foreach (var blend in MaterialTree.All)
            {
                var tone = blend.Result.Kind == BlendKind.Violent ? "violent" : "stable";
                GUI.Label(new Rect(0, y, view.width - 30, 22),
                    $"{RuneCatalog.NameOf(blend.Left)} + {RuneCatalog.NameOf(blend.Right)} → {RuneCatalog.NameOf(blend.Result.Result)}   ({tone})",
                    row);
                y += 26f;
            }

            GUI.EndScrollView();
        }

        static int SpellGrammarCount()
        {
            var count = 0;
            foreach (var _ in SpellGrammar.All)
            {
                count++;
            }

            return count;
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
                return "No lock in reach. You may still compose into the open field.";
            }

            var reading = _director.Grimoire.KnowsInterpretation(target.FormulaId)
                ? $"Known reading: {target.DisplayName}."
                : "The formula is visible. Its meaning is not yet yours.";
            return $"{target.DisplayName}  {{{target.FormulaText()}}}  — {reading}";
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
