using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class GameHud : MonoBehaviour
    {
        enum LedgerPage
        {
            Recent,
            Grimoire
        }

        public const float BarHeight = 96f;
        public const float LedgerWidth = 400f;
        public const float LedgerMaxHeight = 460f;
        static readonly Color CharterSuccess = new(0.28f, 0.82f, 0.42f);
        static readonly Color FreeSuccess = new(0.72f, 0.36f, 0.92f);
        static Texture2D _castIcon;
        static Texture2D _plusIcon;

        SanctumDirector _director;
        Vector2 _pauseScroll;
        Vector2 _packScroll;
        Vector2 _ledgerScroll;
        Vector2 _bookScroll;
        LedgerPage _ledgerPage;
        static Rect _ledgerGui;
        static GameHud _instance;
        int _namingLedger = -1;
        string _namingText = string.Empty;
        bool _focusName;

        public static bool EditingName { get; private set; }

        public void Bind(SanctumDirector director)
        {
            _director = director;
            _instance = this;
        }

        void OnEnable()
        {
            _instance = this;
        }

        void OnDisable()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            if (EditingName)
            {
                CloseNaming();
            }
        }

        public static void CancelNaming()
        {
            _instance?.CloseNaming();
        }

        public static bool PointerOverChrome(PlayMode mode) => BlocksWorldPick(mode);

        public static bool BlocksWorldPick(PlayMode mode)
        {
            if (EditingName)
            {
                return true;
            }

            var mouse = Input.mousePosition;
            if (mouse.y <= BarHeight + 8f)
            {
                return true;
            }

            if (mode == PlayMode.Grimoire || mode == PlayMode.Paused || mode == PlayMode.Inventory)
            {
                return true;
            }

            if (mode == PlayMode.Aiming && mouse.y <= BarHeight + 120f)
            {
                return true;
            }

            if (mouse.x <= 560f && mouse.y >= Screen.height - 196f)
            {
                return true;
            }

            if (mode == PlayMode.Charter)
            {
                return true;
            }

            if (_ledgerGui.width > 1f)
            {
                var gui = new Vector2(mouse.x, Screen.height - mouse.y);
                if (_ledgerGui.Contains(gui))
                {
                    return true;
                }
            }

            return false;
        }

        void OnGUI()
        {
            _ledgerGui = default;
            EditingName = _namingLedger >= 0;
            if (_director == null)
            {
                return;
            }

            var previousEnabled = GUI.enabled;
            if (EditingName)
            {
                GUI.enabled = false;
            }

            if (_director.Mode == PlayMode.Paused)
            {
                DrawPause();
                DrawSpellBar();
            }
            else if (_director.Mode == PlayMode.Grimoire)
            {
                DrawGrimoire();
                DrawSpellBar();
            }
            else if (_director.Mode == PlayMode.Inventory)
            {
                DrawInventory();
                DrawSpellBar();
            }
            else if (_director.Mode == PlayMode.Charter)
            {
                DrawCharter();
                DrawCastLedger();
                DrawSpellBar();
            }
            else if (_director.Mode == PlayMode.Aiming)
            {
                DrawWorldChrome();
                DrawAimDock();
                DrawCastLedger();
                DrawSpellBar();
            }
            else
            {
                DrawWorldChrome();
                DrawCastLedger();
                DrawSpellBar();
            }

            GUI.enabled = previousEnabled;
            if (EditingName)
            {
                DrawKeepModal();
            }

            DrawDeathNotice();
        }

        void DrawDeathNotice()
        {
            if (_director == null || !_director.DeathNoticeUp)
            {
                return;
            }

            var cause = _director.LastDeath;
            var width = Mathf.Min(560f, Screen.width - 32f);
            var height = cause.HasRunes ? 168f : 112f;
            var panel = new Rect((Screen.width - width) * 0.5f, 214f, width, height);
            var previous = GUI.color;
            GUI.color = new Color(0.12f, 0.04f, 0.04f, 0.92f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.92f, 0.32f, 0.22f, 0.85f);
            DrawFrame(panel, 2f);
            GUI.color = previous;

            var title = Label(20, FontStyle.Bold, new Color(1f, 0.78f, 0.52f));
            title.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(panel.x + 16, panel.y + 10, panel.width - 32, 28),
                cause.Banner, title);

            var body = Label(15, FontStyle.Normal, new Color(0.94f, 0.88f, 0.8f));
            body.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(panel.x + 20, panel.y + 42, panel.width - 40, 40),
                cause.Detail, body);

            if (!cause.HasRunes)
            {
                return;
            }

            var runes = cause.Runes;
            const float gap = 10f;
            var mark = 44f;
            var row = runes.Length * mark + (runes.Length - 1) * gap;
            var start = panel.x + (panel.width - row) * 0.5f;
            var y = panel.y + 92f;
            for (var i = 0; i < runes.Length; i++)
            {
                DrawMiniMark(new Rect(start + i * (mark + gap), y, mark, mark), runes[i]);
            }

            if (GlyphView.IsDevelop)
            {
                var names = Label(13, FontStyle.Italic, new Color(0.86f, 0.78f, 0.58f));
                names.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(panel.x + 16, panel.y + 140, panel.width - 32, 20),
                    WorkingNames.RunePhrase(runes), names);
            }
        }

        void DrawWorldChrome()
        {
            DrawPanel(12, 12, 560, 196);
            var title = Label(22, FontStyle.Bold, Color.white);
            var body = Label(15, FontStyle.Normal, new Color(0.88f, 0.9f, 0.95f));
            var look = Label(15, FontStyle.Italic, new Color(0.9f, 0.86f, 0.74f));

            GUI.Label(new Rect(28, 18, 400, 26), "Rune Magic", title);
            GUI.Label(new Rect(28, 44, 400, 22), RoomLine(), body);
            if (DrawAction(new Rect(430, 40, 58, 26), "Look", true, new Color(0.32f, 0.28f, 0.18f)))
            {
                _director.OpenInventory();
            }

            if (DrawAction(new Rect(492, 40, 58, 26), "Yield", true, new Color(0.38f, 0.16f, 0.16f)))
            {
                _director.YieldSelf();
            }

            var y = 70f;
            var statuses = _director.PlayerStatuses();
            if (!string.IsNullOrEmpty(statuses))
            {
                GUI.Label(new Rect(28, y, 510, 20), "On you: " + statuses,
                    Label(14, FontStyle.Italic, new Color(0.95f, 0.78f, 0.42f)));
                y += 22f;
            }

            y = DrawVitalMeters(y);

            var holding = _director.ConcentrationLine();
            if (!string.IsNullOrEmpty(holding))
            {
                GUI.Label(new Rect(28, y, 510, 20), "Concentrating: " + holding,
                    Label(14, FontStyle.Italic, new Color(0.82f, 0.68f, 0.95f)));
                y += 22f;
            }

            GUI.Label(new Rect(28, y, 510, 44), _director.SightLine, look);
            GUI.Label(new Rect(28, y + 46, 510, 196 - y - 54), _director.LastLog, body);
        }

        float DrawVitalMeters(float y)
        {
            var host = StatusHost.On(AdeptAvatar.Find());
            if (host == null)
            {
                return y;
            }

            y = DrawVitalMeter(y, host, StatusId.Burning, new Color(1f, 0.42f, 0.12f), "Burning");
            y = DrawVitalMeter(y, host, StatusId.Poisoned, new Color(0.42f, 0.82f, 0.22f), "Poison");
            return y;
        }

        float DrawVitalMeter(float y, StatusHost host, StatusId id, Color color, string label)
        {
            if (host == null || !host.Has(id))
            {
                return y;
            }

            var left = host.MeterLeft(id);
            var frac = host.MeterFraction(id);
            var previous = GUI.color;
            GUI.Label(new Rect(28, y, 90, 14), $"{label} {left:0.0}",
                Label(12, FontStyle.Bold, color));
            GUI.color = new Color(0.12f, 0.1f, 0.08f, 0.85f);
            GUI.DrawTexture(new Rect(120, y + 3, 200, 8), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.DrawTexture(new Rect(120, y + 3, 200f * frac, 8), Texture2D.whiteTexture);
            GUI.color = previous;
            return y + 16f;
        }

        void DrawCastLedger()
        {
            const float pad = 12f;
            const float row = 36f;
            const float header = 58f;
            const int visibleRows = 12;
            var count = _ledgerPage == LedgerPage.Recent
                ? _director.Ledger.Recent.Count
                : BookRowCount();
            var inner = Mathf.Max(row, count * row);
            var height = count == 0
                ? header + 48f
                : Mathf.Min(LedgerMaxHeight, header + Mathf.Min(inner, visibleRows * row) + 10f);

            var panel = new Rect(Screen.width - LedgerWidth - pad, pad, LedgerWidth, height);
            _ledgerGui = panel;
            DrawPanel(panel.x, panel.y, panel.width, panel.height);
            DrawLedgerTabs(panel);

            DrawSprite(new Rect(panel.x + 212, panel.y + 10, 12, 12),
                SpriteFactory.Circle(CharterSuccess, 24), Color.white);
            DrawSprite(new Rect(panel.x + 268, panel.y + 10, 12, 12),
                SpriteFactory.Circle(FreeSuccess, 24), Color.white);
            var key = Label(11, FontStyle.Normal, new Color(0.7f, 0.72f, 0.8f));
            GUI.Label(new Rect(panel.x + 226, panel.y + 8, 42, 16), "Charter", key);
            GUI.Label(new Rect(panel.x + 282, panel.y + 8, 36, 16), "Free", key);

            var view = new Rect(panel.x + 8, panel.y + header - 4, panel.width - 16, height - header);
            if (_ledgerPage == LedgerPage.Recent)
            {
                DrawRecentPage(view, row, inner);
            }
            else
            {
                DrawGrimoirePage(view, row, inner);
            }
        }

        void DrawLedgerTabs(Rect panel)
        {
            const float tabW = 88f;
            const float tabH = 24f;
            var recent = new Rect(panel.x + 8, panel.y + 6, tabW, tabH);
            var book = new Rect(panel.x + 8 + tabW + 6, panel.y + 6, tabW, tabH);
            if (DrawTab(recent, "Recent", _ledgerPage == LedgerPage.Recent))
            {
                _ledgerPage = LedgerPage.Recent;
            }

            if (DrawTab(book, "Grimoire", _ledgerPage == LedgerPage.Grimoire))
            {
                _ledgerPage = LedgerPage.Grimoire;
            }
        }

        void DrawRecentPage(Rect view, float row, float inner)
        {
            var entries = _director.Ledger.Recent;
            if (entries.Count == 0)
            {
                var muted = Label(13, FontStyle.Italic, new Color(0.68f, 0.7f, 0.78f));
                GUI.Label(new Rect(view.x + 4, view.y + 2, view.width - 8, 24),
                    "Nothing attempted yet.", muted);
                return;
            }

            _ledgerScroll = GUI.BeginScrollView(view, _ledgerScroll, new Rect(0, 0, view.width - 18, inner));
            for (var i = 0; i < entries.Count; i++)
            {
                var index = i;
                var attempt = entries[i];
                DrawCastRow(new Rect(0, i * row, view.width - 18, row - 2), attempt,
                    () => _director.CastRecent(index),
                    () => BeginNaming(index, attempt));
            }

            GUI.EndScrollView();
        }

        void DrawGrimoirePage(Rect view, float row, float inner)
        {
            if (GlyphView.IsDevelop)
            {
                DrawDevelopBookPage(view, row, inner);
                return;
            }

            var kept = _director.Grimoire.KeptWorkings;
            if (kept.Count == 0)
            {
                var muted = Label(13, FontStyle.Italic, new Color(0.68f, 0.7f, 0.78f));
                GUI.Label(new Rect(view.x + 4, view.y + 2, view.width - 8, 44),
                    "Nothing kept yet. Name a working from Recent to write it here.", muted);
                return;
            }

            _bookScroll = GUI.BeginScrollView(view, _bookScroll, new Rect(0, 0, view.width - 18, inner));
            for (var i = 0; i < kept.Count; i++)
            {
                var index = i;
                DrawCastRow(new Rect(0, i * row, view.width - 18, row - 2), FromKept(kept[i]),
                    () => _director.CastKept(index), null, showRunes: true);
            }

            GUI.EndScrollView();
        }

        void DrawDevelopBookPage(Rect view, float row, float inner)
        {
            var catalog = SpellCodex.All;
            var groups = RuneCatalog.LedgerGroups();
            _bookScroll = GUI.BeginScrollView(view, _bookScroll, new Rect(0, 0, view.width - 18, inner));
            for (var i = 0; i < catalog.Count; i++)
            {
                var entry = catalog[i];
                var number = entry.Number;
                DrawCastRow(new Rect(0, i * row, view.width - 18, row - 2), FromCodex(entry),
                    () => _director.LoadCodex(number), null, showRunes: true);
            }

            var y = catalog.Count * row + 6f;
            var heading = Label(12, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var nameStyle = Label(12, FontStyle.Bold, new Color(0.9f, 0.92f, 0.96f));
            var recipeStyle = Label(11, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            for (var g = 0; g < groups.Count; g++)
            {
                var group = groups[g];
                GUI.Label(new Rect(4, y, view.width - 24, 18), group.Title, heading);
                y += row;
                for (var i = 0; i < group.Runes.Count; i++)
                {
                    var rune = group.Runes[i];
                    var line = new Rect(0, y, view.width - 18, row - 2);
                    var previous = GUI.color;
                    GUI.color = new Color(0.1f, 0.11f, 0.14f, 0.55f);
                    GUI.DrawTexture(line, Texture2D.whiteTexture);
                    GUI.color = previous;
                    DrawMiniMark(new Rect(line.x + 4, line.y + 4, 24, 24), rune);
                    GUI.Label(new Rect(line.x + 34, line.y + 2, 90, 16), RuneCatalog.NameOf(rune), nameStyle);
                    var born = ChainBook.BirthNameText(rune);
                    GUI.Label(new Rect(line.x + 34, line.y + 16, line.width - 70, 16),
                        string.IsNullOrEmpty(born) ? "—" : born, recipeStyle);
                    if (ChainBook.IsWrought(rune))
                    {
                        var play = new Rect(line.xMax - 32, line.y + 3, 28, line.height - 6);
                        if (DrawIconAction(play, CastIcon(), true, new Color(0.22f, 0.32f, 0.42f)))
                        {
                            _director.LoadBirth(rune);
                        }
                    }

                    y += row;
                }
            }

            GUI.EndScrollView();
        }

        int BookRowCount()
        {
            if (!GlyphView.IsDevelop)
            {
                return _director.Grimoire.KeptWorkings.Count;
            }

            return SpellCodex.All.Count + LedgerRuneRows();
        }

        static int LedgerRuneRows()
        {
            var rows = 0;
            var groups = RuneCatalog.LedgerGroups();
            for (var i = 0; i < groups.Count; i++)
            {
                rows += 1 + groups[i].Runes.Count;
            }

            return rows;
        }

        static CastAttempt FromKept(KeptWorking kept) =>
            new(kept.Stance, kept.Runes, true, kept.Spell, kept.GivenName, saved: true);

        CastAttempt FromCodex(CodexEntry entry)
        {
            var runes = ToRuneArray(entry.RecipeRunes);
            var stance = entry.FreeOnly ? CastingStance.Free : CastingStance.Charter;
            return new CastAttempt(stance, runes, true, entry.Spell, entry.Name,
                saved: _director.Grimoire.Keeps(entry.Spell));
        }

        static RuneId[] ToRuneArray(IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return System.Array.Empty<RuneId>();
            }

            if (runes is RuneId[] array)
            {
                return array;
            }

            var copy = new RuneId[runes.Count];
            for (var i = 0; i < runes.Count; i++)
            {
                copy[i] = runes[i];
            }

            return copy;
        }

        void DrawCastRow(
            Rect rect,
            CastAttempt attempt,
            System.Action onCast,
            System.Action onKeep,
            bool showRunes = false)
        {
            var previous = GUI.color;
            GUI.color = attempt.Saved
                ? new Color(0.32f, 0.24f, 0.1f, 0.85f)
                : new Color(0.1f, 0.11f, 0.14f, 0.55f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;

            DrawVerdict(new Rect(rect.x + 4, rect.y + 6, 22, 22), attempt);

            const float icon = 28f;
            var keepRect = onKeep != null
                ? new Rect(rect.xMax - icon - 4, rect.y + 3, icon, rect.height - 6)
                : default;
            var castRect = new Rect(
                (onKeep != null ? keepRect.x : rect.xMax) - icon - 4,
                rect.y + 3, icon, rect.height - 6);
            var named = !string.IsNullOrEmpty(attempt.GivenName);
            var labelWidth = named ? 70f : 0f;
            if (named)
            {
                var ink = Label(10, FontStyle.Normal, new Color(0.82f, 0.78f, 0.6f));
                ink.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(castRect.x - labelWidth - 4, rect.y, labelWidth, rect.height),
                    attempt.GivenName, ink);
            }

            var runes = attempt.Runes;
            var count = runes != null ? Mathf.Max(1, runes.Length) : 1;
            var room = castRect.x - labelWidth - (rect.x + 32) - 12f;
            var mark = Mathf.Clamp(Mathf.Floor((room - (count - 1) * 3f) / count), 14f, 22f);
            var start = rect.x + 32;
            var hide = !showRunes && attempt.HideRunes;
            if (runes == null || runes.Length == 0)
            {
                DrawBlockedMark(new Rect(start, rect.y + (rect.height - mark) * 0.5f, mark, mark));
            }
            else
            {
                for (var i = 0; i < runes.Length; i++)
                {
                    var slot = new Rect(start + i * (mark + 3), rect.y + (rect.height - mark) * 0.5f, mark, mark);
                    if (hide)
                    {
                        DrawBlockedMark(slot);
                    }
                    else
                    {
                        DrawMiniMark(slot, runes[i]);
                    }
                }
            }

            var canSend = attempt.Worked && attempt.Runes != null && attempt.Runes.Length > 0;
            if (DrawIconAction(castRect, CastIcon(), canSend, new Color(0.22f, 0.32f, 0.42f)))
            {
                onCast?.Invoke();
            }

            if (onKeep != null && DrawIconAction(keepRect, PlusIcon(),
                    attempt.Worked && attempt.Spell != SpellId.None,
                    attempt.Saved ? new Color(0.42f, 0.32f, 0.14f) : new Color(0.28f, 0.24f, 0.18f)))
            {
                onKeep();
            }
        }

        void BeginNaming(int index, CastAttempt attempt)
        {
            _namingLedger = index;
            if (!string.IsNullOrEmpty(attempt.GivenName))
            {
                _namingText = attempt.GivenName;
            }
            else if (GlyphView.IsDevelop && attempt.Spell != SpellId.None
                && SpellCodex.TryGet(attempt.Spell, out var named))
            {
                _namingText = named.Name;
            }
            else
            {
                _namingText = string.Empty;
            }

            EditingName = true;
            _focusName = true;
            _director.PauseForNaming();
        }

        void CloseNaming()
        {
            _namingLedger = -1;
            _namingText = string.Empty;
            EditingName = false;
            GUI.FocusControl(null);
            _director?.ResumeFromNaming();
        }

        void ConfirmNaming()
        {
            if (_namingLedger < 0)
            {
                return;
            }

            _director.KeepRecent(_namingLedger, _namingText);
            CloseNaming();
        }

        void DrawKeepModal()
        {
            var entries = _director.Ledger.Recent;
            if (_namingLedger < 0 || _namingLedger >= entries.Count)
            {
                CloseNaming();
                return;
            }

            var attempt = entries[_namingLedger];
            DrawVeil(new Color(0.02f, 0.02f, 0.05f, 0.78f));

            const float width = 520f;
            const float height = 292f;
            var modal = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f - 24f, width, height);
            var ev = Event.current;
            if (ev != null && ev.type == EventType.MouseDown && !modal.Contains(ev.mousePosition))
            {
                ev.Use();
            }

            DrawPanel(modal.x, modal.y, modal.width, modal.height);

            var title = Label(22, FontStyle.Bold, Color.white);
            var body = Label(14, FontStyle.Normal, new Color(0.82f, 0.84f, 0.9f));
            var hint = Label(13, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            GUI.Label(new Rect(modal.x + 24, modal.y + 16, modal.width - 48, 28), "Keep this working", title);
            GUI.Label(new Rect(modal.x + 24, modal.y + 46, modal.width - 48, 20),
                "Name the spell. The runes you used stay on the page.", body);

            var stanceColor = attempt.Stance == CastingStance.Free ? FreeSuccess : CharterSuccess;
            DrawSprite(new Rect(modal.x + 24, modal.y + 76, 18, 18), SpriteFactory.Circle(stanceColor, 32), Color.white);
            var stance = Label(14, FontStyle.Bold, stanceColor);
            GUI.Label(new Rect(modal.x + 48, modal.y + 74, 200, 22),
                attempt.Stance == CastingStance.Free ? "Free" : "Charter", stance);

            DrawRuneCombo(new Rect(modal.x + 24, modal.y + 104, modal.width - 48, 56), attempt);

            GUI.Label(new Rect(modal.x + 24, modal.y + 168, 120, 20), "Spell name", hint);
            GUI.SetNextControlName("KeepSpellName");
            _namingText = GUI.TextField(
                new Rect(modal.x + 24, modal.y + 190, modal.width - 48, 32),
                _namingText ?? string.Empty);
            if (_focusName)
            {
                GUI.FocusControl("KeepSpellName");
                if (ev != null && ev.type == EventType.Repaint)
                {
                    _focusName = false;
                }
            }

            var submit = ev != null && ev.type == EventType.KeyDown
                && (ev.keyCode == KeyCode.Return || ev.keyCode == KeyCode.KeypadEnter);
            var cancel = ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape;
            if (submit || cancel)
            {
                ev.Use();
            }

            if (DrawAction(new Rect(modal.x + 24, modal.y + 236, 120, 38), "Cancel", true,
                    new Color(0.22f, 0.22f, 0.26f)) || cancel)
            {
                CloseNaming();
                return;
            }

            if (DrawAction(new Rect(modal.xMax - 144, modal.y + 236, 120, 38), "Keep", true,
                    new Color(0.42f, 0.32f, 0.14f)) || submit)
            {
                ConfirmNaming();
            }
        }

        void DrawRuneCombo(Rect rect, CastAttempt attempt)
        {
            var previous = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 0.7f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;

            var runes = attempt.Runes;
            if (runes == null || runes.Length == 0)
            {
                var muted = Label(13, FontStyle.Italic, new Color(0.68f, 0.7f, 0.78f));
                muted.alignment = TextAnchor.MiddleCenter;
                GUI.Label(rect, "No marks were written.", muted);
                return;
            }

            const float gap = 8f;
            var mark = Mathf.Min(48f, (rect.width - 24f - (runes.Length - 1) * gap) / runes.Length);
            var start = rect.x + (rect.width - (mark * runes.Length + gap * (runes.Length - 1))) * 0.5f;
            var y = rect.y + (rect.height - mark) * 0.5f;
            for (var i = 0; i < runes.Length; i++)
            {
                var slot = new Rect(start + i * (mark + gap), y, mark, mark);
                DrawMiniMark(slot, runes[i]);
            }
        }

        static void DrawVerdict(Rect rect, CastAttempt attempt)
        {
            if (!attempt.Worked)
            {
                var previous = GUI.color;
                GUI.color = new Color(0.24f, 0.1f, 0.1f, 0.95f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = previous;
                var mark = Label(18, FontStyle.Bold, new Color(0.95f, 0.38f, 0.32f));
                mark.alignment = TextAnchor.MiddleCenter;
                GUI.Label(rect, "✕", mark);
                return;
            }

            var color = attempt.Stance == CastingStance.Free ? FreeSuccess : CharterSuccess;
            DrawSprite(rect, SpriteFactory.Circle(color, 32), Color.white);
        }

        static void DrawMiniMark(Rect rect, RuneId rune)
        {
            var previous = GUI.color;
            GUI.color = GlyphView.Slate;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
            RuneMark.DrawGui(rect, rune, RunePalette.MarkInk(rune));
        }

        static void DrawBlockedMark(Rect rect)
        {
            var previous = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(0.28f, 0.22f, 0.22f, 0.9f);
            GUI.DrawTexture(new Rect(rect.x + 3, rect.y + rect.height * 0.5f - 1.5f, rect.width - 6, 3f),
                Texture2D.whiteTexture);
            GUI.color = previous;
        }

        void DrawInventory()
        {
            DrawVeil(new Color(0.03f, 0.03f, 0.05f, 0.8f));
            var title = Label(28, FontStyle.Bold, Color.white);
            var subtitle = Label(15, FontStyle.Normal, new Color(0.78f, 0.8f, 0.88f));
            var heading = Label(17, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var row = Label(16, FontStyle.Normal, new Color(0.88f, 0.9f, 0.94f));
            var muted = Label(14, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            var look = Label(16, FontStyle.Normal, new Color(0.9f, 0.86f, 0.74f));

            GUI.Label(new Rect(40, 20, 800, 34), "Pack", title);
            GUI.Label(new Rect(40, 56, 980, 22),
                "Stones, keys, and what you have borrowed. Click one to look. Arrows move. Esc or I closes.",
                subtitle);

            var pack = _director.Pack;
            var list = new Rect(40, 96, 360, Screen.height - BarHeight - 120);
            var pane = new Rect(420, 96, Screen.width - 460, Screen.height - BarHeight - 120);
            DrawPanel(list.x, list.y, list.width, list.height);
            DrawPanel(pane.x, pane.y, pane.width, pane.height);

            if (pack == null || pack.Empty)
            {
                GUI.Label(new Rect(list.x + 16, list.y + 16, list.width - 32, 80),
                    "Nothing sits here yet. Stones are keys. Charms, wards, and mediums will join them.",
                    muted);
                GUI.Label(new Rect(pane.x + 24, pane.y + 24, pane.width - 48, 80),
                    "Pick something up, then look.", muted);
                return;
            }

            GUI.Label(new Rect(list.x + 16, list.y + 12, list.width - 32, 22), "Carried", heading);
            var inner = pack.Held.Count * 64f + 8f;
            var view = new Rect(list.x + 8, list.y + 40, list.width - 16, list.height - 52);
            _packScroll = GUI.BeginScrollView(view, _packScroll, new Rect(0, 0, view.width - 18, inner));
            var y = 0f;
            for (var i = 0; i < pack.Held.Count; i++)
            {
                var item = pack.Held[i];
                var slot = new Rect(4, y, view.width - 26, 56);
                var chosen = i == pack.SelectedIndex;
                var previous = GUI.color;
                GUI.color = chosen
                    ? new Color(0.42f, 0.32f, 0.16f, 0.95f)
                    : new Color(0.12f, 0.13f, 0.18f, 0.9f);
                GUI.DrawTexture(slot, Texture2D.whiteTexture);
                GUI.color = previous;
                DrawItemSprite(new Rect(slot.x + 8, slot.y + 8, 40, 40), item);
                GUI.Label(new Rect(slot.x + 56, slot.y + 8, slot.width - 68, 22),
                    string.IsNullOrEmpty(item.name) ? item.id : item.name, row);
                GUI.Label(new Rect(slot.x + 56, slot.y + 30, slot.width - 68, 20),
                    AdeptPack.KindLabel(item), muted);
                var index = i;
                if (GUI.Button(slot, GUIContent.none, GUIStyle.none))
                {
                    _director.SelectPack(index);
                }

                y += 64;
            }

            GUI.EndScrollView();

            var selected = pack.Selected;
            if (selected == null)
            {
                GUI.Label(new Rect(pane.x + 24, pane.y + 24, pane.width - 48, 40),
                    "Click a thing to look at it.", muted);
                return;
            }

            DrawItemSprite(new Rect(pane.x + 24, pane.y + 24, 96, 96), selected);
            GUI.Label(new Rect(pane.x + 136, pane.y + 28, pane.width - 168, 32),
                string.IsNullOrEmpty(selected.name) ? selected.id : selected.name, title);
            GUI.Label(new Rect(pane.x + 136, pane.y + 64, pane.width - 168, 22),
                AdeptPack.KindLabel(selected), heading);
            if (!string.IsNullOrEmpty(selected.teachesSpell))
            {
                GUI.Label(new Rect(pane.x + 136, pane.y + 88, pane.width - 168, 20),
                    $"Borrowed: {selected.teachesSpell}", muted);
            }

            GUI.Label(new Rect(pane.x + 24, pane.y + 140, pane.width - 48, pane.height - 164),
                AdeptPack.LookText(selected), look);
        }

        static void DrawItemSprite(Rect rect, CatalogItem item)
        {
            DrawSprite(rect, SpriteFactory.Named(item != null ? item.sprite : null), Color.white);
        }

        static void DrawSprite(Rect rect, Sprite sprite, Color tint)
        {
            if (sprite == null || sprite.texture == null)
            {
                return;
            }

            var texture = sprite.texture;
            var source = sprite.textureRect;
            var uv = new Rect(
                source.x / texture.width,
                source.y / texture.height,
                source.width / texture.width,
                source.height / texture.height);
            var previous = GUI.color;
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(rect, texture, uv);
            GUI.color = previous;
        }

        void DrawCharter()
        {
            DrawVeil(new Color(0.03f, 0.04f, 0.07f, 0.38f));

            var title = Label(26, FontStyle.Bold, Color.white);
            var body = Label(15, FontStyle.Normal, new Color(0.86f, 0.88f, 0.94f));
            var hint = Label(13, FontStyle.Normal, new Color(0.7f, 0.74f, 0.82f));

            GUI.Label(new Rect(28, 16, 800, 32), "The Charter", title);
            GUI.Label(new Rect(28, 50, 980, 22),
                GlyphView.Speak(
                    "Walk while the wall is open. Every root mark is on the wall. The weave is only what the screen is speaking — hover it to still the belt. Grey on the wall means that join is not in view. Spark, Ice, and Plant stand as themselves in the weave. What you have strung stays until you cast or close. You are mind · body · soul.",
                    "Walk while the wall is open. Draw from the wall or the weave, or click a mark the room is speaking. Hover the weave to still it. Right-click a mark to remember it. What you have strung stays until you cast or close."),
                body);
            GUI.Label(new Rect(28, 74, 980, 20),
                GlyphView.Speak(
                    "F / Enter Charter Cast   ·   X Free Cast   ·   R Store (Charter only)   ·   Space close   ·   Esc / Grimoire   ·   F1 Play",
                    "F / Enter Charter Cast   ·   X Free Cast   ·   R Store   ·   Right-click keep   ·   Space close   ·   F1 Develop"),
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
            if (runes.Count == 0)
            {
                var hint = Label(14, FontStyle.Italic, new Color(0.72f, 0.74f, 0.8f));
                GUI.Label(new Rect(left, top, Screen.width - 56f, size),
                    "The wall is empty. Right-click a mark in the weave to keep it here.",
                    hint);
                return top + size;
            }

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

            const float pad = 12f;
            const float header = 36f;
            const float lip = 3f;
            var rows = RuneTapestry.Rows;
            var well = new Rect(
                panel.x + pad,
                top + header,
                panel.width - pad * 2f,
                Mathf.Max(1f, height - header - pad));
            var inner = new Rect(
                well.x + lip,
                well.y + lip,
                Mathf.Max(1f, well.width - lip * 2f),
                Mathf.Max(1f, well.height - lip * 2f));
            var cellH = inner.height / rows;
            var cols = Mathf.Clamp(
                Mathf.RoundToInt(inner.width / Mathf.Max(1f, cellH)),
                8,
                24);
            if (tapestry != null)
            {
                tapestry.Columns = cols;
            }

            var cellW = inner.width / cols;
            DrawWeaveWell(well);

            var spoken = tapestry != null && tapestry.Sequence.Count > 0;
            var mouse = Event.current != null ? Event.current.mousePosition : Vector2.zero;
            if (tapestry != null)
            {
                tapestry.HoverPaused = spoken && well.Contains(mouse);
            }

            if (!spoken)
            {
                var quiet = Label(14, FontStyle.Italic, new Color(0.68f, 0.7f, 0.78f));
                quiet.alignment = TextAnchor.MiddleCenter;
                GUI.Label(inner, "nothing on the screen speaks", quiet);
                return;
            }

            var slide = tapestry != null ? tapestry.Scroll - Mathf.Floor(tapestry.Scroll) : 0f;
            var shift = slide * cellW;

            GUI.BeginGroup(inner);
            for (var row = 0; row < rows; row++)
            {
                var right = RuneTapestry.GoesRight(row);
                var rowShift = right ? shift : -shift;
                var placed = new List<(Rect Rect, WeaveGlyph Glyph)>(cols + 2);
                for (var col = -1; col <= cols; col++)
                {
                    var x = col * cellW + rowShift;
                    var rect = new Rect(x, row * cellH, cellW, cellH);
                    if (rect.xMax <= 0f || rect.x >= inner.width)
                    {
                        continue;
                    }

                    var glyph = tapestry != null
                        ? tapestry.Cell(row, col)
                        : new WeaveGlyph(RuneId.None, MaterialId.None, WeaveKind.Tear);
                    placed.Add((rect, glyph));
                }

                DrawJoinChunks(placed);
                for (var i = 0; i < placed.Count; i++)
                {
                    var glyph = placed[i].Glyph;
                    var rect = placed[i].Rect;
                    if (glyph.IsTear)
                    {
                        DrawEmptySlot(rect, "tear");
                        continue;
                    }

                    var shown = glyph.Shown;
                    var chunk = glyph.IsGroup ? glyph.Rune : RuneId.None;
                    var pick = glyph.IsGroup && ChainBook.IsWrought(glyph.Rune)
                        ? glyph.Rune
                        : shown;
                    DrawRuneCard(rect, shown, () => _director.WeaveFromField(pick), true, true, chunk);
                }
            }

            GUI.EndGroup();
        }

        static void DrawWeaveWell(Rect well)
        {
            var previous = GUI.color;
            GUI.color = new Color(0.02f, 0.02f, 0.04f, 0.96f);
            GUI.DrawTexture(well, Texture2D.whiteTexture);
            GUI.color = new Color(0.96f, 0.82f, 0.38f, 0.92f);
            DrawFrame(well, 3f);
            GUI.color = new Color(1f, 0.92f, 0.7f, 0.18f);
            GUI.DrawTexture(new Rect(well.x + 3f, well.y + 2f, well.width - 6f, 2f), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        void DrawJoinChunks(List<(Rect Rect, WeaveGlyph Glyph)> placed)
        {
            var i = 0;
            while (i < placed.Count)
            {
                var glyph = placed[i].Glyph;
                if (!glyph.IsGroup)
                {
                    i++;
                    continue;
                }

                var id = glyph.GroupId;
                var start = i;
                var bounds = placed[i].Rect;
                i++;
                while (i < placed.Count && placed[i].Glyph.GroupId == id)
                {
                    bounds = Union(bounds, placed[i].Rect);
                    i++;
                }

                if (i - start < 2 && glyph.GroupSize < 2)
                {
                    continue;
                }

                DrawJoinSlab(bounds, glyph);
                var previous = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.22f);
                for (var n = start + 1; n < i; n++)
                {
                    var seam = placed[n].Rect;
                    GUI.DrawTexture(new Rect(seam.x - 2.5f, seam.y + 6f, 1f, seam.height - 12f),
                        Texture2D.whiteTexture);
                }

                GUI.color = previous;
            }
        }

        void DrawJoinSlab(Rect bounds, WeaveGlyph glyph)
        {
            const float pad = 4f;
            const float banner = 16f;
            var slab = new Rect(
                bounds.x - pad,
                bounds.y - banner,
                bounds.width + pad * 2f,
                bounds.height + banner + pad);

            var wash = GlyphView.IsPlay
                ? GlyphView.JoinWash
                : Color.Lerp(RunePalette.Of(glyph.Rune), new Color(0.06f, 0.05f, 0.04f), 0.18f);
            wash.a = 0.92f;
            var previous = GUI.color;
            GUI.color = wash;
            GUI.DrawTexture(slab, Texture2D.whiteTexture);

            var rim = glyph.Living
                ? new Color(0.62f, 0.92f, 0.42f, 0.96f)
                : new Color(0.98f, 0.82f, 0.32f, 0.96f);
            GUI.color = rim;
            DrawFrame(slab, 3f);
            GUI.DrawTexture(new Rect(slab.x + 4f, bounds.y - 2f, slab.width - 8f, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(slab.x + 8f, slab.yMax - 6f, slab.width - 16f, 2f), Texture2D.whiteTexture);
            GUI.color = previous;

            if (GlyphView.IsPlay)
            {
                return;
            }

            var caption = !string.IsNullOrEmpty(glyph.GroupTitle)
                ? glyph.GroupTitle
                : RuneCatalog.NameOf(glyph.Rune);
            var title = Label(12, FontStyle.Bold, rim);
            title.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(slab.x, slab.y, slab.width, banner), caption, title);
        }

        static Rect Union(Rect a, Rect b)
        {
            var x = Mathf.Min(a.x, b.x);
            var y = Mathf.Min(a.y, b.y);
            var xMax = Mathf.Max(a.xMax, b.xMax);
            var yMax = Mathf.Max(a.yMax, b.yMax);
            return new Rect(x, y, xMax - x, yMax - y);
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

            var held = GlyphView.HeldName(_director.Held);
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

            GUI.Label(new Rect(312, y + 14, 300, 24), "Stored spell", title);
            if (_director.Held.Occupied)
            {
                GUI.Label(new Rect(312, y + 40, 300, 40),
                    GlyphView.Speak(
                        $"{_director.Held.Name}  ·  Charter\nClick the slot or press F, then aim.",
                        "A held working. Click the slot or press F, then aim."),
                    body);
            }
            else
            {
                GUI.Label(new Rect(312, y + 40, 300, 40),
                    "Empty. Store is a Charter benefit. Free cannot be held.",
                    body);
            }

            GUI.Label(new Rect(620, y + 16, Mathf.Max(120f, Screen.width - 1200f), 64),
                "WASD · Space Charter · F Cast · X Free · R Store · I Pack · F1 " +
                (GlyphView.IsDevelop ? "Play" : "Develop"),
                hint);

            var packOpen = _director.Mode == PlayMode.Inventory;
            var grimOpen = _director.Mode == PlayMode.Grimoire;
            if (DrawAction(new Rect(Screen.width - 508, y + 22, 128, 52),
                    GlyphView.IsDevelop ? "Develop" : "Play", true,
                    GlyphView.IsDevelop
                        ? new Color(0.42f, 0.28f, 0.16f)
                        : new Color(0.18f, 0.22f, 0.3f)))
            {
                _director.ToggleSight();
            }

            if (DrawAction(new Rect(Screen.width - 368, y + 22, 168, 52),
                    packOpen ? "Close pack" : "Pack", true,
                    packOpen ? new Color(0.42f, 0.32f, 0.16f) : new Color(0.28f, 0.26f, 0.2f)))
            {
                if (packOpen)
                {
                    _director.CloseInventory();
                }
                else
                {
                    _director.OpenInventory();
                }
            }

            var grim = new Rect(Screen.width - 188, y + 22, 168, 52);
            if (DrawAction(grim, grimOpen ? "Close book" : "Grimoire", true,
                    grimOpen ? new Color(0.55f, 0.42f, 0.18f) : new Color(0.32f, 0.24f, 0.42f)))
            {
                if (grimOpen)
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
            var aimTitle = GlyphView.IsDevelop
                ? $"Aim  ·  {_director.PendingPreview}  ·  {_director.PendingStance}"
                : _director.ChosenShape == SpellShape.None
                    ? "Aim  ·  the sentence did not hold"
                    : string.IsNullOrEmpty(_director.PendingPreview)
                        ? "Aim  ·  a working"
                        : "Aim  ·  " + _director.PendingPreview;
            GUI.Label(new Rect(16, y + 8, 720, 22), aimTitle, title);

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
                var hint = string.IsNullOrEmpty(_director.AimHint) ? def.Hint : _director.AimHint;
                GUI.Label(new Rect(16, y + 36, 720, 40),
                    $"{def.Name} is in the sentence. {hint}",
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

            if (occupied && GlyphView.IsPlay)
            {
                DrawHeldMarks(rect, _director.Held.Composition);
            }
            else
            {
                var name = Label(20, FontStyle.Bold, occupied ? Color.white : new Color(0.55f, 0.58f, 0.66f));
                name.alignment = TextAnchor.MiddleCenter;
                GUI.Label(rect, occupied ? _director.Held.Name : "No spell stored", name);
            }

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
                GlyphView.Speak(
                    $"{_director.Attunement.Notes()}   ·   Every rune and written spell. Click a join (Metal is Lava · Spark · Earth) or a spell to string it. The eleven roots are always ready. Esc closes.",
                    "Your book. Workings you have kept, and marks you remember. Click a page to send it. The eleven roots are always ready. Esc closes."),
                subtitle);

            var view = new Rect(40, 92, Screen.width - 80, Screen.height - BarHeight - 112);
            if (GlyphView.IsPlay)
            {
                DrawPlayGrimoire(view, heading, muted);
                return;
            }

            var innerHeight = CodexHeight() + 160f;
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, view.width - 24, innerHeight));
            var y = DrawKeptMarksInline(0f);
            y = DrawRuneIndex(y, heading, row, muted, loadable: true);
            y = DrawCodex(y, heading, row, muted, loadable: true);
            y = DrawMaterialsLedger(y, heading, row, muted);
            GUI.EndScrollView();
        }

        void DrawPlayGrimoire(Rect view, GUIStyle heading, GUIStyle muted)
        {
            var kept = _director.Grimoire.KeptWorkings;
            var innerHeight = 80f + _director.Memory.Kept.Count * 56f + Mathf.Max(1, kept.Count) * 40f;
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, view.width - 24, innerHeight));
            var y = DrawKeptMarksInline(0f);
            y += 8f;
            GUI.Label(new Rect(0, y, 700, 22), "Kept workings", heading);
            y += 26f;
            if (kept.Count == 0)
            {
                GUI.Label(new Rect(0, y, 900, 22),
                    "Nothing kept yet. Name a working from Recent casts to write it here.", muted);
                GUI.EndScrollView();
                return;
            }

            const float rowH = 38f;
            for (var i = 0; i < kept.Count; i++)
            {
                var index = i;
                DrawCastRow(new Rect(0, y, Mathf.Min(720f, view.width - 40f), rowH - 2), FromKept(kept[i]),
                    () => _director.CastKept(index), null, showRunes: true);
                y += rowH;
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

            GUI.Label(new Rect(40, 24, 800, 34), "Paused — developer ledger", title);
            GUI.Label(new Rect(40, 62, 980, 22),
                "Developer ledger. Every named rune and how it is born, the written spells, and world materials. F1 toggles Play sight. Esc resumes.",
                subtitle);

            var view = new Rect(40, 100, Screen.width - 80, Screen.height - 140);
            var innerHeight = CodexHeight();
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, view.width - 24, innerHeight));
            var y = DrawRuneIndex(0f, heading, row, muted, loadable: false);
            y = DrawCodex(y, heading, row, muted, loadable: false);
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

            return y;
        }

        float DrawRuneIndex(float y, GUIStyle heading, GUIStyle row, GUIStyle muted, bool loadable)
        {
            CatalogBook.EnsureLoaded();
            y += 8f;
            GUI.Label(new Rect(0, y, 900, 22), $"All runes · {RuneCatalog.All.Count} named", heading);
            y += 22f;
            GUI.Label(new Rect(0, y, 1100, 18),
                "Every named mark. A join is born from others — Metal is Lava · Spark · Earth, Spark is Fire · Air. Click a wrought name to string it.",
                muted);
            y += 20f;
            if (loadable)
            {
                GUI.Label(new Rect(0, y, 1100, 18),
                    "Click a wrought name to string the recipe. The eleven roots are always ready.",
                    muted);
                y += 20f;
            }

            var groups = RuneCatalog.LedgerGroups();
            for (var i = 0; i < groups.Count; i++)
            {
                y = DrawRuneGroup(y, heading, row, muted, loadable, groups[i].Title, groups[i].Runes);
            }

            return y;
        }

        float DrawRuneGroup(
            float y,
            GUIStyle heading,
            GUIStyle row,
            GUIStyle muted,
            bool loadable,
            string title,
            IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return y;
            }

            GUI.Label(new Rect(0, y, 900, 22), title, heading);
            y += 24f;
            for (var i = 0; i < runes.Count; i++)
            {
                y = DrawRuneLine(y, row, muted, loadable, runes[i]);
            }

            y += 10f;
            return y;
        }

        float DrawRuneLine(float y, GUIStyle row, GUIStyle muted, bool loadable, RuneId rune)
        {
            if (rune == RuneId.None || !RuneCatalog.TryGet(rune, out var def))
            {
                return y;
            }

            var nameRect = new Rect(0, y, 130, 18);
            var born = ChainBook.BirthNameText(rune);
            if (string.IsNullOrEmpty(born))
            {
                born = "—";
            }

            if (loadable && ChainBook.IsWrought(rune) && GUI.Button(nameRect, def.Name, row))
            {
                _director.LoadBirth(rune);
            }
            else
            {
                GUI.Label(nameRect, def.Name, row);
            }

            GUI.Label(new Rect(136, y, 260, 18), born, muted);
            GUI.Label(new Rect(404, y, 700, 18), def.Meaning, row);
            return y + 20f;
        }

        float DrawCodex(float y, GUIStyle heading, GUIStyle row, GUIStyle muted, bool loadable)
        {
            y += 8f;
            GUI.Label(new Rect(0, y, 900, 22), $"Written spells · {SpellCodex.All.Count}", heading);
            y += 22f;
            GUI.Label(new Rect(0, y, 1100, 18),
                "Every catalog chain. Joins fold (Fire · Air is Spark). A via-form is the same story from a wrought rune already in the field.",
                muted);
            y += 24f;

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

                if (_director.Grimoire.Keeps(entry.Spell))
                {
                    var previous = GUI.color;
                    GUI.color = new Color(0.42f, 0.32f, 0.1f, 0.55f);
                    GUI.DrawTexture(new Rect(0, y, 990, 20), Texture2D.whiteTexture);
                    GUI.color = previous;
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
            CatalogBook.EnsureLoaded();
            var groups = RuneCatalog.LedgerGroups();
            var runes = 0;
            for (var i = 0; i < groups.Count; i++)
            {
                runes += groups[i].Runes.Count;
            }

            var spells = SpellCodex.All.Count;
            var materials = MaterialCatalog.All.Count;
            return 280f + groups.Count * 34f + runes * 20f + spells * 20f + 12 * 28f
                + materials * 20f + 200f;
        }

        void DrawRuneCard(Rect rect, RuneId rune, System.Action onClick, bool available, bool oneSpace = false, RuneId chunk = RuneId.None)
        {
            var play = GlyphView.IsPlay;
            var wrought = !oneSpace && ChainBook.IsWrought(rune);
            var inChunk = chunk != RuneId.None;
            var tone = RunePalette.Of(rune);
            var fill = play
                ? (available ? GlyphView.Slate : new Color(0.1f, 0.1f, 0.12f, 0.4f))
                : inChunk
                    ? PaneOn(RunePalette.Of(chunk))
                    : RunePalette.Card(rune, available);
            if (!play && inChunk)
            {
                fill.a = 0.2f;
            }

            var previous = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            if (play)
            {
                GUI.color = new Color(1f, 1f, 1f, available ? 0.28f : 0.08f);
                GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, 2f), Texture2D.whiteTexture);
            }
            else
            {
                var rim = tone;
                rim.a = available ? (inChunk ? 0.8f : 0.95f) : 0.28f;
                GUI.color = rim;
                DrawFrame(rect, inChunk ? 1f : 2f);
                if (!inChunk)
                {
                    GUI.color = new Color(1f, 1f, 1f, available ? 0.28f : 0.08f);
                    GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, 2f), Texture2D.whiteTexture);
                }
            }

            GUI.color = previous;

            if (_director.Memory.Knows(rune))
            {
                DrawKeptPin(rect);
            }

            if (wrought && available && !play)
            {
                DrawWroughtMark(rect);
            }

            var showName = !play && rect.height > 40f;
            var markRect = showName
                ? new Rect(rect.x, rect.y + 2f, rect.width, rect.height * 0.6f)
                : rect;
            RuneMark.DrawGui(markRect, rune, RunePalette.MarkInk(rune, available));

            if (showName)
            {
                var captionInk = inChunk
                    ? InkOn(RunePalette.Of(chunk))
                    : RunePalette.Caption(rune, available);
                var name = Label(rect.height > 70f ? 12 : 10, FontStyle.Bold, captionInk);
                name.alignment = TextAnchor.MiddleCenter;
                var caption = available ? RuneCatalog.NameOf(rune) : "not in view";
                GUI.Label(new Rect(rect.x + 2f, rect.y + rect.height * 0.62f, rect.width - 4f, rect.height * 0.34f),
                    caption, name);
            }

            var ev = Event.current;
            if (!EditingName && ev != null && rect.Contains(ev.mousePosition) && ev.type == EventType.MouseDown)
            {
                if (ev.button == 1 || (ev.button == 0 && ev.shift))
                {
                    ev.Use();
                    _director.RememberRune(rune);
                    return;
                }
            }

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                onClick?.Invoke();
            }
        }

        float DrawKeptMarksInline(float y)
        {
            var heading = Label(17, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            GUI.Label(new Rect(0, y, 700, 22), "Kept marks", heading);
            y += 26f;
            var kept = _director.Memory.Kept;
            if (kept.Count == 0)
            {
                var muted = Label(14, FontStyle.Italic, new Color(0.72f, 0.74f, 0.82f));
                GUI.Label(new Rect(0, y, 900, 20),
                    "No wall marks kept yet. Right-click a mark in the Charter weave to remember it.",
                    muted);
                return y + 28f;
            }

            const float size = 48f;
            const float gap = 8f;
            for (var i = 0; i < kept.Count; i++)
            {
                var rect = new Rect(i * (size + gap), y, size, size);
                var rune = kept[i];
                DrawRuneCard(rect, rune, () =>
                {
                    _director.CloseGrimoire();
                    _director.OpenCharter();
                    _director.AddRune(rune);
                }, true);
            }

            return y + size + 16f;
        }

        void DrawKeptMarks(Rect view)
        {
            var kept = _director.Memory.Kept;
            if (kept.Count == 0)
            {
                var muted = Label(16, FontStyle.Italic, new Color(0.72f, 0.74f, 0.82f));
                GUI.Label(new Rect(view.x + 12, view.y + 12, view.width - 24, 80),
                    "Nothing is kept yet. Open the Charter and right-click a mark in the weave to remember it.",
                    muted);
                return;
            }

            const float size = 72f;
            const float gap = 12f;
            var columns = Mathf.Max(1, Mathf.FloorToInt((view.width - 24f) / (size + gap)));
            for (var i = 0; i < kept.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                var rect = new Rect(view.x + 12 + col * (size + gap), view.y + 12 + row * (size + gap), size, size);
                var rune = kept[i];
                DrawRuneCard(rect, rune, () =>
                {
                    _director.CloseGrimoire();
                    _director.OpenCharter();
                    _director.AddRune(rune);
                }, true);
            }
        }

        void DrawHeldMarks(Rect rect, Composition composition)
        {
            var sequence = composition.Sequence;
            if (sequence == null || sequence.Length == 0)
            {
                return;
            }

            var slot = Mathf.Min(36f, (rect.width - 16f) / sequence.Length);
            var start = rect.x + (rect.width - slot * sequence.Length) * 0.5f;
            var y = rect.y + (rect.height - slot) * 0.5f;
            for (var i = 0; i < sequence.Length; i++)
            {
                RuneMark.DrawGui(new Rect(start + i * slot, y, slot, slot), sequence[i],
                    RunePalette.MarkInk(sequence[i]));
            }
        }

        static void DrawKeptPin(Rect rect)
        {
            var previous = GUI.color;
            GUI.color = new Color(0.95f, 0.82f, 0.4f, 0.95f);
            GUI.DrawTexture(new Rect(rect.xMax - 10f, rect.y + 4f, 6f, 6f), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        static void DrawWroughtMark(Rect rect)
        {
            var previous = GUI.color;
            var gold = new Color(0.98f, 0.82f, 0.32f, 0.95f);
            GUI.color = gold;
            DrawFrame(rect, 3f);
            GUI.DrawTexture(new Rect(rect.x + 6f, rect.yMax - 8f, rect.width - 12f, 2f), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        static void DrawFrame(Rect rect, float thickness)
        {
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        }

        static Color InkOn(Color fill)
        {
            var luma = fill.r * 0.3f + fill.g * 0.59f + fill.b * 0.11f;
            return luma < 0.55f
                ? new Color(0.98f, 0.96f, 0.88f)
                : new Color(0.1f, 0.08f, 0.06f);
        }

        static Color PaneOn(Color fill)
        {
            var luma = fill.r * 0.3f + fill.g * 0.59f + fill.b * 0.11f;
            return luma < 0.45f
                ? Color.Lerp(fill, Color.white, 0.22f)
                : Color.Lerp(fill, Color.black, 0.18f);
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

        static bool DrawTab(Rect rect, string text, bool selected)
        {
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = selected
                ? new Color(0.55f, 0.42f, 0.18f)
                : new Color(0.16f, 0.15f, 0.2f);
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            var clicked = GUI.Button(rect, text, style);
            GUI.backgroundColor = bg;
            return clicked;
        }

        static bool DrawAction(Rect rect, string text, bool enabled, Color color)
        {
            var previous = GUI.enabled;
            GUI.enabled = previous && enabled;
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
            return clicked && enabled && previous;
        }

        static bool DrawIconAction(Rect rect, Texture2D icon, bool enabled, Color color)
        {
            var previous = GUI.enabled;
            GUI.enabled = previous && enabled;
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            var style = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(0, 0, 0, 0)
            };
            var clicked = GUI.Button(rect, GUIContent.none, style);
            GUI.backgroundColor = bg;
            if (icon != null)
            {
                var pad = Mathf.Max(5f, rect.height * 0.18f);
                var slot = new Rect(rect.x + pad, rect.y + pad, rect.width - pad * 2f, rect.height - pad * 2f);
                var tint = GUI.color;
                GUI.color = enabled && previous ? Color.white : new Color(1f, 1f, 1f, 0.3f);
                GUI.DrawTexture(slot, icon, ScaleMode.ScaleToFit, true);
                GUI.color = tint;
            }

            GUI.enabled = previous;
            return clicked && enabled && previous;
        }

        static Texture2D CastIcon()
        {
            if (_castIcon != null)
            {
                return _castIcon;
            }

            var canvas = new PixelCanvas(24);
            canvas.FillTriangle(6, 3, 6, 20, 20, 12, Color.white);
            _castIcon = canvas.ToTexture();
            return _castIcon;
        }

        static Texture2D PlusIcon()
        {
            if (_plusIcon != null)
            {
                return _plusIcon;
            }

            var canvas = new PixelCanvas(24);
            canvas.Fill(4, 10, 16, 4, Color.white);
            canvas.Fill(10, 4, 4, 16, Color.white);
            _plusIcon = canvas.ToTexture();
            return _plusIcon;
        }

        string RoomLine()
        {
            var room = _director.CurrentRoom != null ? _director.CurrentRoom.Name : "Sanctum";
            if (GlyphView.IsPlay)
            {
                return room;
            }

            var tile = _director.Underfoot != null ? _director.Underfoot.Def.DisplayName : "empty air";
            return $"{room}   ·   underfoot: {tile}";
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
            GUI.color = new Color(0.05f, 0.045f, 0.07f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
            GUI.color = new Color(0.92f, 0.74f, 0.32f, 0.55f);
            DrawFrame(new Rect(x, y, width, height), 2f);
            GUI.color = new Color(1f, 0.92f, 0.7f, 0.12f);
            GUI.DrawTexture(new Rect(x + 3, y + 2, width - 6, 2f), Texture2D.whiteTexture);
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
