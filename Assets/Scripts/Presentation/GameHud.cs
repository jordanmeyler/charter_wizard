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

        enum BookPage
        {
            Workings,
            Runes,
            Spells,
            World
        }

        public const float BarHeight = 96f;
        public const float LedgerWidth = 420f;
        public const float LedgerMaxHeight = 480f;
        public const float InfoWidth = 440f;
        const float InfoPad = 12f;
        const float InfoInner = 16f;
        const float InfoHeader = 36f;
        const float StatusRow = 20f;
        const float LogLineHeight = 20f;
        const float LogViewHeight = 148f;
        const float LedgerRow = 58f;
        const float BookRow = 68f;
        const float BookRuneCard = 176f;
        const float BookRuneCardH = 92f;
        const float DraftSlotPreferred = 52f;
        const float DraftSlotMin = 36f;
        const float DraftSlotGap = 8f;
        const float DraftSlotSide = 24f;
        const float DraftRoleHeight = 18f;
        const float DraftSlotTop = 30f;
        const float DraftAfterSlots = 128f;
        static readonly Color CharterSuccess = new(0.28f, 0.82f, 0.42f);
        static readonly Color FreeSuccess = new(0.72f, 0.36f, 0.92f);
        static Texture2D _castIcon;
        static Texture2D _plusIcon;

        SanctumDirector _director;
        Vector2 _pauseScroll;
        Vector2 _packScroll;
        Vector2 _ledgerScroll;
        Vector2 _bookScroll;
        Vector2 _speechScroll;
        Vector2 _logScroll;
        string _logged = string.Empty;
        int _logSeen;
        float _speechViewHeight;
        float _speechInnerHeight;
        LedgerPage _ledgerPage;
        BookPage _bookPage;
        string _bookQuery = string.Empty;
        RuneId _bookFilter = RuneId.None;
        bool _focusBookSearch;
        static Rect _ledgerGui;
        static Rect _infoGui;
        static Rect _interactGui;
        static GameHud _instance;
        enum NameTarget
        {
            None,
            Recent,
            Kept
        }

        int _namingIndex = -1;
        NameTarget _nameTarget;
        string _namingText = string.Empty;
        bool _focusName;
        bool _ledgerCollapsed;
        bool _infoCollapsed;
        readonly List<HudStatus> _hudStatuses = new();
        bool _revealing;
        PrayerWorking _revealed;
        readonly HashSet<string> _namedOffers = new();
        bool _speaking;
        string _speechTitle = string.Empty;
        string _speechSpeaker = string.Empty;
        string[] _speechPages = System.Array.Empty<string>();
        int _speechPage;

        public static bool EditingName { get; private set; }
        public static bool RevealingSpell { get; private set; }
        public static bool ShowingSpeech { get; private set; }
        public static bool EditingBookSearch { get; private set; }
        public static bool HoldsPlay => EditingName || RevealingSpell || ShowingSpeech;

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

            if (RevealingSpell)
            {
                CloseReveal();
            }

            if (ShowingSpeech)
            {
                CloseSpeech();
            }
        }

        public static void CancelNaming()
        {
            _instance?.CloseNaming();
        }

        public static void CancelHeldMenu()
        {
            if (_instance == null)
            {
                return;
            }

            if (ShowingSpeech)
            {
                _instance.CloseSpeech();
                return;
            }

            if (RevealingSpell)
            {
                _instance.CloseReveal();
                return;
            }

            _instance.CloseNaming();
        }

        public static void CancelBookSearch()
        {
            _instance?.ClearBookSearch();
        }

        public static void ShowSpeech(string title, string speaker, IReadOnlyList<string> pages)
        {
            _instance?.OpenSpeech(title, speaker, pages);
        }

        public static void AdvanceHeldSpeech()
        {
            _instance?.AdvanceSpeech();
        }

        public static void RevealWorking(CodexEntry entry)
        {
            _instance?.OpenReveal(PrayerReveal.FromEntry(entry));
        }

        public static void RevealWorking(PrayerWorking working)
        {
            _instance?.OpenReveal(working);
        }

        public static void OfferKeepLatest(Composition composition)
        {
            _instance?.TryOfferKeep(composition);
        }

        public static bool PointerOverChrome(PlayMode mode) => BlocksWorldPick(mode);

        public static bool BlocksWorldPick(PlayMode mode)
        {
            if (HoldsPlay)
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

            if (mode == PlayMode.Charter)
            {
                return true;
            }

            var gui = new Vector2(mouse.x, Screen.height - mouse.y);
            if (_infoGui.width > 1f && _infoGui.Contains(gui))
            {
                return true;
            }

            if (_ledgerGui.width > 1f && _ledgerGui.Contains(gui))
            {
                return true;
            }

            if (_interactGui.width > 1f && _interactGui.Contains(gui))
            {
                return true;
            }

            return false;
        }

        void OnGUI()
        {
            _ledgerGui = default;
            _infoGui = default;
            _interactGui = default;
            EditingName = _nameTarget != NameTarget.None && _namingIndex >= 0;
            RevealingSpell = _revealing;
            ShowingSpeech = _speaking;
            EditingBookSearch = false;
            if (_director == null)
            {
                return;
            }

            var previousEnabled = GUI.enabled;
            if (HoldsPlay)
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
            if (!HoldsPlay)
            {
                DrawCastNotice();
            }

            if (RevealingSpell)
            {
                DrawPrayerModal();
            }
            else if (EditingName)
            {
                DrawKeepModal();
            }
            else if (ShowingSpeech)
            {
                DrawSpeechModal();
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

        void DrawCastNotice()
        {
            if (_director == null || !_director.HasCastNotice)
            {
                return;
            }

            var missing = _director.CastNoticeRunes;
            var markRow = missing != null && missing.Count > 0;
            var ledgerOpen = _director.Mode == PlayMode.Charter
                || _director.Mode == PlayMode.Exploring
                || _director.Mode == PlayMode.Aiming;
            var right = ledgerOpen ? Screen.width - LedgerWidth - 28f : Screen.width - 24f;
            var width = Mathf.Min(720f, Mathf.Max(280f, right - 24f));
            var height = markRow ? 88f : 64f;
            var y = _director.Mode == PlayMode.Exploring || _director.Mode == PlayMode.Aiming
                ? (_infoGui.height > 1f ? _infoGui.yMax + 12f : 56f)
                : 92f;
            var x = ledgerOpen ? 16f : (Screen.width - width) * 0.5f;
            var panel = new Rect(x, y, width, height);
            DrawPanel(panel.x, panel.y, panel.width, panel.height);
            var previous = GUI.color;
            GUI.color = new Color(0.95f, 0.72f, 0.28f, 0.95f);
            DrawFrame(panel, 2f);
            GUI.color = previous;

            var body = Label(15, FontStyle.Bold, new Color(0.98f, 0.9f, 0.62f));
            body.alignment = TextAnchor.MiddleCenter;
            var textTop = markRow ? panel.y + 6f : panel.y;
            var textHeight = markRow ? 28f : panel.height;
            GUI.Label(new Rect(panel.x + 12, textTop, panel.width - 24, textHeight),
                _director.CastNotice, body);

            if (!markRow)
            {
                return;
            }

            const float slot = 36f;
            const float gap = 8f;
            var start = panel.x + (panel.width - missing.Count * (slot + gap) + gap) * 0.5f;
            var markY = panel.y + 42f;
            for (var i = 0; i < missing.Count; i++)
            {
                var rect = new Rect(start + i * (slot + gap), markY, slot, slot);
                var previousFill = GUI.color;
                GUI.color = new Color(0.12f, 0.1f, 0.08f, 0.9f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = new Color(0.95f, 0.72f, 0.28f, 0.85f);
                DrawFrame(rect, 1.5f);
                GUI.color = previousFill;
                RuneMark.DrawGui(rect, missing[i], RunePalette.MarkInk(missing[i]));
            }
        }

        void DrawWorldChrome()
        {
            var innerW = InfoWidth - InfoInner * 2f;
            var title = Label(15, FontStyle.Bold, Color.white);
            title.wordWrap = false;
            title.clipping = TextClipping.Clip;
            var body = Label(14, FontStyle.Normal, new Color(0.88f, 0.9f, 0.95f));
            var lookStyle = Label(14, FontStyle.Italic, new Color(0.9f, 0.86f, 0.74f));
            var holdStyle = Label(13, FontStyle.Italic, new Color(0.82f, 0.68f, 0.95f));

            var look = _director.SightLine ?? string.Empty;
            var holding = _director.ConcentrationLine();
            CollectHudStatuses();
            var lookH = _infoCollapsed ? 0f : MeasureWrapped(lookStyle, look, innerW, 20f, 56f);
            var holdText = !_infoCollapsed && !string.IsNullOrEmpty(holding)
                ? "Hold: " + holding
                : string.Empty;
            var holdH = MeasureWrapped(holdStyle, holdText, innerW, 18f, 36f);
            var statusH = Mathf.Max(StatusRow, _hudStatuses.Count * StatusRow);
            var logH = _infoCollapsed
                ? 0f
                : Mathf.Clamp(Screen.height * 0.2f, 110f, LogViewHeight);

            var height = InfoHeader + 8f + statusH;
            if (lookH > 0f)
            {
                height += lookH + 4f;
            }

            if (holdH > 0f)
            {
                height += holdH + 4f;
            }

            if (logH > 0f)
            {
                height += 22f + logH;
            }

            _infoGui = new Rect(InfoPad, InfoPad, InfoWidth, height);
            DrawPanel(_infoGui.x, _infoGui.y, _infoGui.width, _infoGui.height);

            var collapse = new Rect(_infoGui.xMax - 36, _infoGui.y + 6, 28, 24);
            if (DrawTab(collapse, _infoCollapsed ? "+" : "–", _infoCollapsed))
            {
                _infoCollapsed = !_infoCollapsed;
            }

            var head = _infoCollapsed ? look : RoomLine();
            GUI.Label(new Rect(_infoGui.x + InfoInner, _infoGui.y + 6, innerW - 32, 24),
                head, title);

            var y = _infoGui.y + InfoHeader;
            DrawHudStatuses(new Rect(_infoGui.x + InfoInner, y, innerW, statusH));
            y += statusH + 4f;

            if (lookH > 0f)
            {
                GUI.Label(new Rect(_infoGui.x + InfoInner, y, innerW, lookH), look, lookStyle);
                y += lookH + 4f;
            }

            if (holdH > 0f)
            {
                GUI.Label(new Rect(_infoGui.x + InfoInner, y, innerW, holdH), holdText, holdStyle);
                y += holdH + 4f;
            }

            if (logH > 0f)
            {
                var logHead = Label(12, FontStyle.Bold, new Color(0.78f, 0.72f, 0.5f));
                GUI.Label(new Rect(_infoGui.x + InfoInner, y, innerW, 18), "Log", logHead);
                y += 18f;
                DrawRunningLog(new Rect(_infoGui.x + InfoInner, y, innerW, logH), body);
            }

            DrawInteractPrompt();
        }

        struct HudStatus
        {
            public string Label;
            public Color Color;
        }

        void CollectHudStatuses()
        {
            _hudStatuses.Clear();
            var host = StatusHost.On(AdeptAvatar.Find());
            if (host != null)
            {
                var active = host.Active;
                for (var i = 0; i < active.Count; i++)
                {
                    var effect = active[i];
                    if (effect == null || effect.Remaining <= 0f)
                    {
                        continue;
                    }

                    _hudStatuses.Add(new HudStatus
                    {
                        Label = StatusLabel(effect),
                        Color = effect.Spec.Tint
                    });
                }
            }

            if (_hudStatuses.Count == 0)
            {
                _hudStatuses.Add(new HudStatus
                {
                    Label = "Everything's ok",
                    Color = new Color(0.55f, 0.82f, 0.58f)
                });
            }
        }

        static string StatusLabel(StatusInstance effect)
        {
            var name = TitleStatus(effect.Spec.Name);
            if (effect.Spec.NeedsConcentration || float.IsInfinity(effect.Remaining))
            {
                return name;
            }

            return $"{name} {effect.Remaining:0.0}";
        }

        static string TitleStatus(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        void DrawHudStatuses(Rect view)
        {
            var row = StatusRow;
            for (var i = 0; i < _hudStatuses.Count; i++)
            {
                var status = _hudStatuses[i];
                var rect = new Rect(view.x, view.y + i * row, view.width, row);
                GUI.Label(rect, status.Label, Label(14, FontStyle.Bold, status.Color));
            }
        }

        static float MeasureWrapped(GUIStyle style, string text, float width, float min, float max)
        {
            if (string.IsNullOrWhiteSpace(text) || style == null)
            {
                return 0f;
            }

            return Mathf.Clamp(style.CalcHeight(new GUIContent(text), width), min, max);
        }

        void DrawLogBox(Rect view, string message, GUIStyle body)
        {
            DrawRunningLog(view, body);
        }

        void DrawRunningLog(Rect view, GUIStyle body)
        {
            if (view.height < 12f)
            {
                return;
            }

            var lines = _director.LogLines;
            var count = lines != null ? lines.Count : 0;
            if (count == 0)
            {
                GUI.Label(view, _director.LastLog ?? string.Empty, body);
                return;
            }

            if (_logSeen != count)
            {
                _logSeen = count;
                _logged = _director.LastLog ?? string.Empty;
                _logScroll.y = float.MaxValue;
            }

            var innerWidth = Mathf.Max(40f, view.width - 18f);
            var lineStyle = new GUIStyle(body)
            {
                fontSize = 13,
                wordWrap = true
            };
            var stale = new GUIStyle(lineStyle);
            stale.normal.textColor = new Color(0.7f, 0.72f, 0.8f);
            var latest = new GUIStyle(lineStyle);
            latest.normal.textColor = new Color(0.94f, 0.9f, 0.72f);
            latest.fontStyle = FontStyle.Bold;

            var heights = new float[count];
            var innerHeight = 4f;
            for (var i = 0; i < count; i++)
            {
                var text = lines[i] ?? string.Empty;
                heights[i] = Mathf.Max(LogLineHeight, lineStyle.CalcHeight(new GUIContent(text), innerWidth));
                innerHeight += heights[i] + 4f;
            }

            var previous = GUI.color;
            GUI.color = new Color(0.04f, 0.04f, 0.06f, 0.55f);
            GUI.DrawTexture(view, Texture2D.whiteTexture);
            GUI.color = previous;

            if (innerHeight <= view.height + 1f)
            {
                var y = view.y + 2f;
                for (var i = 0; i < count; i++)
                {
                    GUI.Label(new Rect(view.x + 4f, y, innerWidth, heights[i]),
                        lines[i], i == count - 1 ? latest : stale);
                    y += heights[i] + 4f;
                }

                return;
            }

            _logScroll = GUI.BeginScrollView(view, _logScroll, new Rect(0, 0, innerWidth, innerHeight));
            var rowY = 2f;
            for (var i = 0; i < count; i++)
            {
                GUI.Label(new Rect(4f, rowY, innerWidth, heights[i]),
                    lines[i], i == count - 1 ? latest : stale);
                rowY += heights[i] + 4f;
            }

            GUI.EndScrollView();
        }

        void DrawInteractPrompt()
        {
            var nearby = _director.NearbyInteract;
            if (nearby == null || _director.Mode != PlayMode.Exploring || HoldsPlay)
            {
                return;
            }

            var verb = string.IsNullOrWhiteSpace(nearby.InteractVerb) ? "Interact" : nearby.InteractVerb;
            var prompt = Sight.InteractPrompt(verb);
            var ink = Label(16, FontStyle.Bold, new Color(0.98f, 0.9f, 0.62f));
            ink.alignment = TextAnchor.MiddleCenter;
            var width = Mathf.Clamp(ink.CalcSize(new GUIContent(prompt)).x + 36f, 220f, 420f);
            var height = 40f;
            if (!TryInteractPromptRect(nearby, width, height, out var rect))
            {
                return;
            }

            _interactGui = rect;
            DrawPanel(rect.x, rect.y, rect.width, rect.height);
            var previous = GUI.color;
            GUI.color = new Color(0.95f, 0.82f, 0.4f, 0.85f);
            DrawFrame(rect, 1.5f);
            GUI.color = previous;
            GUI.Label(rect, prompt, ink);
            var ev = Event.current;
            if (ev != null && ev.type == EventType.MouseDown && ev.button == 0 && rect.Contains(ev.mousePosition))
            {
                ev.Use();
                _director.UseNearbyInteract();
            }
        }

        static bool TryInteractPromptRect(IInteractable nearby, float width, float height, out Rect rect)
        {
            var cam = Camera.main;
            Vector2 center;
            if (cam != null)
            {
                var screen = cam.WorldToScreenPoint(nearby.WorldPosition + new Vector3(0f, 0.95f, 0f));
                if (screen.z <= 0.05f)
                {
                    rect = default;
                    return false;
                }

                center = new Vector2(screen.x, Screen.height - screen.y);
            }
            else
            {
                center = new Vector2(Screen.width * 0.5f, Screen.height - BarHeight - 40f);
            }

            rect = new Rect(center.x - width * 0.5f, center.y - height - 6f, width, height);
            rect.x = Mathf.Clamp(rect.x, 12f, Screen.width - width - 12f);
            var minY = _infoGui.height > 1f ? _infoGui.yMax + 8f : 12f;
            rect.y = Mathf.Clamp(rect.y, minY, Screen.height - BarHeight - height - 12f);
            return true;
        }

        void DrawCastLedger()
        {
            const float pad = 12f;
            const float header = 58f;
            const int visibleRows = 8;
            var row = LedgerRow;
            var count = _ledgerCollapsed
                ? 1
                : _ledgerPage == LedgerPage.Recent
                    ? _director.Ledger.Recent.Count
                    : BookRowCount();
            var inner = Mathf.Max(row, count * row);
            var height = _ledgerCollapsed
                ? header + row + 10f
                : count == 0
                    ? header + 52f
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
            if (_ledgerCollapsed)
            {
                DrawCollapsedLedger(view, row);
                return;
            }

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
                _ledgerCollapsed = false;
            }

            if (DrawTab(book, "Grimoire", _ledgerPage == LedgerPage.Grimoire))
            {
                _ledgerPage = LedgerPage.Grimoire;
                _ledgerCollapsed = false;
            }

            var collapse = new Rect(panel.xMax - 36, panel.y + 6, 28, tabH);
            if (DrawTab(collapse, _ledgerCollapsed ? "+" : "–", _ledgerCollapsed))
            {
                _ledgerCollapsed = !_ledgerCollapsed;
            }
        }

        void DrawCollapsedLedger(Rect view, float row)
        {
            var entries = _director.Ledger.Recent;
            if (entries.Count == 0)
            {
                var muted = Label(13, FontStyle.Italic, new Color(0.68f, 0.7f, 0.78f));
                GUI.Label(new Rect(view.x + 4, view.y + 2, view.width - 8, 24),
                    "Nothing attempted yet.", muted);
                return;
            }

            DrawCastRow(new Rect(view.x, view.y, view.width, row - 2), entries[0],
                () => _director.CastRecent(0),
                () => BeginNaming(0, entries[0]));
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
                    "Nothing kept yet. With Add new spells on, a working that holds is written here without a name. Or Keep one from Recent.", muted);
                return;
            }

            _bookScroll = GUI.BeginScrollView(view, _bookScroll, new Rect(0, 0, view.width - 18, inner));
            for (var i = 0; i < kept.Count; i++)
            {
                var index = i;
                DrawCastRow(new Rect(0, i * row, view.width - 18, row - 2), FromKept(kept[i]),
                    () => _director.CastKept(index),
                    () => BeginRenameKept(index, FromKept(kept[index])), showRunes: true);
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

            const float icon = 30f;
            var keepRect = onKeep != null
                ? new Rect(rect.xMax - icon - 4, rect.y + (rect.height - icon) * 0.5f, icon, icon)
                : default;
            var castRect = new Rect(
                (onKeep != null ? keepRect.x : rect.xMax) - icon - 4,
                rect.y + (rect.height - icon) * 0.5f, icon, icon);

            var name = RecipeLabel(attempt);
            var stanceColor = attempt.Stance == CastingStance.Free ? FreeSuccess : CharterSuccess;
            var stance = Label(11, FontStyle.Bold, stanceColor);
            GUI.Label(new Rect(rect.x + 30, rect.y + 2, 70, 16),
                attempt.Stance == CastingStance.Free ? "Free" : "Charter", stance);
            var title = Label(13, FontStyle.Bold, new Color(0.94f, 0.92f, 0.8f));
            GUI.Label(new Rect(rect.x + 86, rect.y + 1, castRect.x - (rect.x + 90), 18), name, title);

            var runes = attempt.Runes;
            var hide = !showRunes && attempt.HideRunes;
            var mark = Mathf.Min(26f, rect.height - 24f);
            var start = rect.x + 30;
            var room = castRect.x - start - 8f;
            if (runes != null && runes.Length > 0)
            {
                mark = Mathf.Clamp(Mathf.Floor((room - (runes.Length - 1) * 4f) / runes.Length), 16f, 26f);
            }

            if (runes == null || runes.Length == 0)
            {
                DrawBlockedMark(new Rect(start, rect.y + 20, mark, mark));
            }
            else
            {
                var caption = Label(9, FontStyle.Normal, new Color(0.72f, 0.74f, 0.8f));
                caption.alignment = TextAnchor.UpperCenter;
                for (var i = 0; i < runes.Length; i++)
                {
                    var slot = new Rect(start + i * (mark + 4), rect.y + 18, mark, mark);
                    if (hide)
                    {
                        DrawBlockedMark(slot);
                        continue;
                    }

                    var available = _director.RunePresent(runes[i]);
                    DrawMiniMark(slot, runes[i], available);
                    if (GlyphView.IsDevelop)
                    {
                        GUI.Label(new Rect(slot.x - 6, slot.yMax - 2, slot.width + 12, 14),
                            RuneCatalog.NameOf(runes[i]), caption);
                    }
                }
            }

            var canSend = attempt.Worked && attempt.Runes != null && attempt.Runes.Length > 0;
            if (DrawIconAction(castRect, CastIcon(), canSend, new Color(0.22f, 0.32f, 0.42f)))
            {
                onCast?.Invoke();
            }

            var canKeep = attempt.Worked && attempt.Runes != null && attempt.Runes.Length > 0;
            if (onKeep != null && DrawIconAction(keepRect, PlusIcon(),
                    canKeep,
                    attempt.Saved ? new Color(0.42f, 0.32f, 0.14f) : new Color(0.28f, 0.24f, 0.18f)))
            {
                onKeep();
            }
        }

        static string RecipeLabel(CastAttempt attempt)
        {
            if (!string.IsNullOrEmpty(attempt.GivenName))
            {
                return attempt.GivenName;
            }

            if (!attempt.Worked)
            {
                return GlyphView.Speak("did not hold", "fizzled");
            }

            return GlyphView.Speak(WorkingNames.RunePhrase(attempt.Runes), "working");
        }

        void BeginNaming(int index, CastAttempt attempt)
        {
            BeginNaming(NameTarget.Recent, index, attempt);
        }

        void BeginRenameKept(int index, CastAttempt attempt)
        {
            BeginNaming(NameTarget.Kept, index, attempt);
        }

        void BeginNaming(NameTarget target, int index, CastAttempt attempt)
        {
            _nameTarget = target;
            _namingIndex = index;
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
            _nameTarget = NameTarget.None;
            _namingIndex = -1;
            _namingText = string.Empty;
            EditingName = false;
            GUI.FocusControl(null);
            _director?.ResumeFromNaming();
        }

        void ConfirmNaming()
        {
            if (_namingIndex < 0)
            {
                return;
            }

            if (_nameTarget == NameTarget.Kept)
            {
                _director.RenameKept(_namingIndex, _namingText);
            }
            else
            {
                _director.KeepRecent(_namingIndex, _namingText);
            }

            CloseNaming();
        }

        void TryOfferKeep(Composition composition)
        {
            if (!GameSettings.PromptNewSpells || composition.Sequence == null || composition.Sequence.Length == 0)
            {
                return;
            }

            var runes = composition.Sequence;
            var key = WorkingNames.Key(runes);
            if (string.IsNullOrEmpty(key) || _namedOffers.Contains(key))
            {
                return;
            }

            if (_director.Grimoire.KeepsComposition(runes) || _director.Grimoire.Names.TryGet(runes, out _))
            {
                return;
            }

            var entries = _director.Ledger.Recent;
            if (entries.Count == 0 || !entries[0].Worked
                || !WorkingNames.SameComposition(entries[0].Runes, runes))
            {
                return;
            }

            _namedOffers.Add(key);
            _director.KeepRecent(0, string.Empty);
        }

        void OpenReveal(PrayerWorking working)
        {
            if (!working.HasContent)
            {
                return;
            }

            if (EditingName)
            {
                CloseNaming();
            }

            _revealed = working;
            _revealing = true;
            RevealingSpell = true;
            _director.PauseForNaming();
        }

        void CloseReveal()
        {
            _revealing = false;
            RevealingSpell = false;
            _revealed = default;
            _director?.ResumeFromNaming();
        }

        void OpenSpeech(string title, string speaker, IReadOnlyList<string> pages)
        {
            var lines = WorldSpeech.CollectPages(null, pages);
            if (lines.Count == 0)
            {
                return;
            }

            if (EditingName)
            {
                CloseNaming();
            }

            if (RevealingSpell)
            {
                CloseReveal();
            }

            _speechTitle = title ?? string.Empty;
            _speechSpeaker = speaker ?? string.Empty;
            _speechPages = lines.ToArray();
            _speechPage = 0;
            ResetSpeechScroll();
            _speaking = true;
            ShowingSpeech = true;
            _director?.PauseForNaming();
        }

        void CloseSpeech()
        {
            _speaking = false;
            ShowingSpeech = false;
            _speechTitle = string.Empty;
            _speechSpeaker = string.Empty;
            _speechPages = System.Array.Empty<string>();
            _speechPage = 0;
            ResetSpeechScroll();
            _director?.ResumeFromNaming();
        }

        void AdvanceSpeech()
        {
            if (!_speaking)
            {
                return;
            }

            if (SpeechHasUnreadScroll())
            {
                ScrollSpeechPage();
                return;
            }

            if (_speechPage + 1 < _speechPages.Length)
            {
                _speechPage++;
                ResetSpeechScroll();
                return;
            }

            CloseSpeech();
        }

        void ResetSpeechScroll()
        {
            _speechScroll = Vector2.zero;
            _speechViewHeight = 0f;
            _speechInnerHeight = 0f;
        }

        bool SpeechHasUnreadScroll()
        {
            return _speechInnerHeight > _speechViewHeight + 2f
                && _speechScroll.y + _speechViewHeight < _speechInnerHeight - 6f;
        }

        void ScrollSpeechPage()
        {
            var step = Mathf.Max(48f, _speechViewHeight * 0.85f);
            var max = Mathf.Max(0f, _speechInnerHeight - _speechViewHeight);
            _speechScroll.y = Mathf.Min(_speechScroll.y + step, max);
        }

        bool TryNamingAttempt(out CastAttempt attempt)
        {
            if (_nameTarget == NameTarget.Recent)
            {
                var entries = _director.Ledger.Recent;
                if (_namingIndex >= 0 && _namingIndex < entries.Count)
                {
                    attempt = entries[_namingIndex];
                    return true;
                }
            }
            else if (_nameTarget == NameTarget.Kept
                && _director.Grimoire.TryGetKept(_namingIndex, out var kept))
            {
                attempt = FromKept(kept);
                return true;
            }

            attempt = default;
            return false;
        }

        void DrawKeepModal()
        {
            if (!TryNamingAttempt(out var attempt))
            {
                CloseNaming();
                return;
            }

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
            GUI.Label(new Rect(modal.x + 24, modal.y + 16, modal.width - 48, 28),
                _nameTarget == NameTarget.Kept ? "Name this spell" : "Keep this working", title);
            GUI.Label(new Rect(modal.x + 24, modal.y + 46, modal.width - 48, 20),
                _nameTarget == NameTarget.Kept
                    ? "Rename the page. Leave it blank to keep the marks without a name."
                    : "Name the spell. The runes you used stay on the page.", body);

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

            if (DrawAction(new Rect(modal.xMax - 144, modal.y + 236, 120, 38),
                    _nameTarget == NameTarget.Kept ? "Save" : "Keep", true,
                    new Color(0.42f, 0.32f, 0.14f)) || submit)
            {
                ConfirmNaming();
            }
        }

        void DrawPrayerModal()
        {
            if (!_revealed.HasContent)
            {
                CloseReveal();
                return;
            }

            var recipe = _revealed.HasRecipe;
            var other = _revealed.HasVia;
            var birth = _revealed.HasBirth;
            DrawVeil(new Color(0.02f, 0.02f, 0.05f, 0.78f));
            const float width = 560f;
            var height = 200f;
            if (recipe)
            {
                height += 148f;
            }

            if (other)
            {
                height += 152f;
            }

            if (birth)
            {
                height += recipe ? 168f : 148f;
            }

            var modal = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f - 24f, width, height);
            var ev = Event.current;
            if (ev != null && ev.type == EventType.MouseDown && !modal.Contains(ev.mousePosition))
            {
                ev.Use();
            }

            DrawPanel(modal.x, modal.y, modal.width, modal.height);
            var title = Label(22, FontStyle.Bold, Color.white);
            var body = Label(14, FontStyle.Normal, new Color(0.82f, 0.84f, 0.9f));
            var legend = Label(12, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            GUI.Label(new Rect(modal.x + 24, modal.y + 16, modal.width - 48, 28),
                recipe
                    ? GlyphView.Speak("A working is shown", "A working is shown")
                    : GlyphView.Speak("A join is shown", "A join is shown"), title);
            GUI.Label(new Rect(modal.x + 24, modal.y + 46, modal.width - 48, 40),
                recipe && GlyphView.IsDevelop && _revealed.Entry.Spell != SpellId.None
                    ? $"{_revealed.Entry.Name} — {_revealed.Entry.Want}"
                    : birth && !recipe
                        ? GlyphView.Speak(
                            "These marks become one.",
                            "These marks become one.")
                        : "Elemental is a material. Catalyst is mind, body, or soul. Special is anima, animus, aether, life, or death.",
                body);
            GUI.Label(new Rect(modal.x + 24, modal.y + 88, modal.width - 48, 18),
                birth && !recipe
                    ? "Sources on the left. The born mark on the right."
                    : other
                        ? "The same working can be written more than one way."
                        : "Each mark is labelled elemental, catalyst, or special.", legend);

            var y = modal.y + 112f;
            if (recipe)
            {
                var recipeHeight = other || birth ? 124f : 132f;
                DrawRevealedRunes(new Rect(modal.x + 24, y, modal.width - 48, recipeHeight), _revealed.Recipe);
                y += recipeHeight + 8f;
            }

            if (other)
            {
                var or = Label(13, FontStyle.Italic, new Color(0.86f, 0.8f, 0.58f));
                or.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(modal.x + 24, y, modal.width - 48, 18), "or", or);
                y += 20f;
                DrawRevealedRunes(new Rect(modal.x + 24, y, modal.width - 48, 124f), _revealed.Via);
                y += 132f;
            }

            if (birth)
            {
                if (recipe)
                {
                    var join = Label(13, FontStyle.Italic, new Color(0.86f, 0.8f, 0.58f));
                    join.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(modal.x + 24, y, modal.width - 48, 18),
                        GlyphView.Speak("these marks become", "these marks become"), join);
                    y += 20f;
                }

                DrawBirthEquation(
                    new Rect(modal.x + 24, y, modal.width - 48, 124f),
                    _revealed.BirthSources,
                    _revealed.BirthResult);
                y += 132f;
            }

            var buttonsY = y + 8f;
            var cancel = ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape;
            if (cancel)
            {
                ev.Use();
            }

            if (DrawAction(new Rect(modal.x + 24, buttonsY, 148, 42), "Continue", true,
                    new Color(0.22f, 0.22f, 0.26f)) || cancel)
            {
                CloseReveal();
                return;
            }

            if (recipe && DrawAction(new Rect(modal.xMax - 172, buttonsY, 148, 42), "Cast", true,
                    new Color(0.72f, 0.28f, 0.22f)))
            {
                var shown = _revealed;
                CloseReveal();
                _director.CastRevealed(shown);
            }
        }

        void DrawSpeechModal()
        {
            if (_speechPages == null || _speechPages.Length == 0)
            {
                CloseSpeech();
                return;
            }

            _speechPage = Mathf.Clamp(_speechPage, 0, _speechPages.Length - 1);
            var page = _speechPages[_speechPage];
            var hasMore = _speechPage + 1 < _speechPages.Length;

            DrawVeil(new Color(0.02f, 0.02f, 0.05f, 0.78f));
            const float width = 560f;
            var body = Label(16, FontStyle.Normal, new Color(0.88f, 0.9f, 0.95f));
            var textWidth = width - 66f;
            var textHeight = Mathf.Max(body.CalcHeight(new GUIContent(page), textWidth), 1f);
            var maxBody = Mathf.Clamp(Screen.height * 0.38f, 160f, 280f);
            var bodyHeight = Mathf.Clamp(textHeight, 72f, maxBody);
            var speakerRow = !string.IsNullOrWhiteSpace(_speechSpeaker) ? 26f : 0f;
            var titleRow = !string.IsNullOrWhiteSpace(_speechTitle) ? 32f : 0f;
            var height = 118f + titleRow + speakerRow + bodyHeight;
            var modal = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f - 24f, width, height);
            var ev = Event.current;
            if (ev != null && ev.type == EventType.MouseDown && !modal.Contains(ev.mousePosition))
            {
                ev.Use();
            }

            DrawPanel(modal.x, modal.y, modal.width, modal.height);

            var y = modal.y + 16f;
            if (titleRow > 0f)
            {
                var heading = Label(22, FontStyle.Bold, Color.white);
                GUI.Label(new Rect(modal.x + 24, y, modal.width - 48, 28), _speechTitle, heading);
                y += titleRow;
            }

            if (speakerRow > 0f)
            {
                var who = Label(15, FontStyle.Italic, new Color(0.92f, 0.82f, 0.5f));
                GUI.Label(new Rect(modal.x + 24, y, modal.width - 48, 22), _speechSpeaker, who);
                y += speakerRow;
            }

            var bodyRect = new Rect(modal.x + 24, y, modal.width - 48, bodyHeight);
            DrawSpeechBody(bodyRect, page, body, textWidth, textHeight);
            HandleSpeechScrollKeys(ev);

            var cancel = ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape;
            if (cancel)
            {
                ev.Use();
                CloseSpeech();
                return;
            }

            var unread = SpeechHasUnreadScroll();
            var action = unread ? "More" : hasMore ? "Next" : "Continue";
            if (DrawAction(new Rect(modal.xMax - 172, modal.yMax - 58, 148, 42), action, true,
                    new Color(0.42f, 0.32f, 0.14f)))
            {
                AdvanceSpeech();
            }

            var mark = Label(12, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            mark.alignment = TextAnchor.MiddleLeft;
            var note = _speechPages.Length > 1
                ? $"{_speechPage + 1} / {_speechPages.Length}"
                : string.Empty;
            if (unread)
            {
                note = string.IsNullOrEmpty(note) ? "Scroll for more" : note + "  ·  scroll";
            }

            if (!string.IsNullOrEmpty(note))
            {
                GUI.Label(new Rect(modal.x + 24, modal.yMax - 50, 280, 28), note, mark);
            }
        }

        void DrawSpeechBody(Rect view, string page, GUIStyle body, float textWidth, float textHeight)
        {
            _speechViewHeight = view.height;
            _speechInnerHeight = textHeight;
            if (textHeight <= view.height + 1f)
            {
                GUI.Label(view, page, body);
                _speechScroll = Vector2.zero;
                return;
            }

            _speechScroll = GUI.BeginScrollView(view, _speechScroll, new Rect(0, 0, textWidth, textHeight));
            GUI.Label(new Rect(0, 0, textWidth, textHeight), page, body);
            GUI.EndScrollView();
        }

        void HandleSpeechScrollKeys(Event ev)
        {
            if (ev == null || ev.type != EventType.KeyDown)
            {
                return;
            }

            var step = 0f;
            if (ev.keyCode == KeyCode.DownArrow)
            {
                step = 32f;
            }
            else if (ev.keyCode == KeyCode.UpArrow)
            {
                step = -32f;
            }
            else if (ev.keyCode == KeyCode.PageDown)
            {
                step = Mathf.Max(48f, _speechViewHeight * 0.85f);
            }
            else if (ev.keyCode == KeyCode.PageUp)
            {
                step = -Mathf.Max(48f, _speechViewHeight * 0.85f);
            }

            if (Mathf.Approximately(step, 0f))
            {
                return;
            }

            ev.Use();
            var max = Mathf.Max(0f, _speechInnerHeight - _speechViewHeight);
            _speechScroll.y = Mathf.Clamp(_speechScroll.y + step, 0f, max);
        }

        void DrawBirthEquation(Rect rect, IReadOnlyList<RuneId> sources, RuneId result)
        {
            var previous = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 0.7f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;

            var count = 0;
            if (sources != null)
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (sources[i] != RuneId.None)
                    {
                        count++;
                    }
                }
            }

            var hasResult = result != RuneId.None;
            var slots = count + (count > 0 && hasResult ? 1 : 0) + (hasResult ? 1 : 0);
            if (slots == 0)
            {
                return;
            }

            const float gap = 10f;
            var mark = Mathf.Min(56f, (rect.width - 24f - (slots - 1) * gap) / slots);
            var start = rect.x + (rect.width - (mark * slots + gap * (slots - 1))) * 0.5f;
            var y = rect.y + 16f;
            var role = Label(11, FontStyle.Bold, new Color(0.86f, 0.8f, 0.58f));
            role.alignment = TextAnchor.UpperCenter;
            var caption = Label(11, FontStyle.Normal, new Color(0.78f, 0.8f, 0.88f));
            caption.alignment = TextAnchor.UpperCenter;
            var x = start;
            if (sources != null)
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (sources[i] == RuneId.None)
                    {
                        continue;
                    }

                    DrawRevealedMark(new Rect(x, y, mark, mark), sources[i], role, caption);
                    x += mark + gap;
                }
            }

            if (count > 0 && hasResult)
            {
                var equals = Label(28, FontStyle.Bold, new Color(0.92f, 0.86f, 0.62f));
                equals.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(x, y, mark, mark), "=", equals);
                x += mark + gap;
            }

            if (hasResult)
            {
                DrawRevealedMark(new Rect(x, y, mark, mark), result, role, caption);
            }
        }

        void DrawRevealedMark(Rect slot, RuneId rune, GUIStyle role, GUIStyle caption)
        {
            DrawMiniMark(slot, rune);
            GUI.Label(new Rect(slot.x - 8, slot.yMax + 2, slot.width + 16, 16),
                RuneCatalog.StringRole(rune), role);
            if (GlyphView.IsDevelop)
            {
                GUI.Label(new Rect(slot.x - 8, slot.yMax + 16, slot.width + 16, 16),
                    RuneCatalog.NameOf(rune), caption);
            }
        }

        void DrawRevealedRunes(Rect rect, IReadOnlyList<RuneId> runes)
        {
            var previous = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 0.7f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
            if (runes == null || runes.Count == 0)
            {
                return;
            }

            const float gap = 10f;
            var mark = Mathf.Min(56f, (rect.width - 24f - (runes.Count - 1) * gap) / runes.Count);
            var start = rect.x + (rect.width - (mark * runes.Count + gap * (runes.Count - 1))) * 0.5f;
            var y = rect.y + 16f;
            var role = Label(11, FontStyle.Bold, new Color(0.86f, 0.8f, 0.58f));
            role.alignment = TextAnchor.UpperCenter;
            var caption = Label(11, FontStyle.Normal, new Color(0.78f, 0.8f, 0.88f));
            caption.alignment = TextAnchor.UpperCenter;
            for (var i = 0; i < runes.Count; i++)
            {
                var slot = new Rect(start + i * (mark + gap), y, mark, mark);
                DrawMiniMark(slot, runes[i]);
                GUI.Label(new Rect(slot.x - 8, slot.yMax + 2, slot.width + 16, 16),
                    RuneCatalog.StringRole(runes[i]), role);
                if (GlyphView.IsDevelop)
                {
                    GUI.Label(new Rect(slot.x - 8, slot.yMax + 16, slot.width + 16, 16),
                        RuneCatalog.NameOf(runes[i]), caption);
                }
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

        static void DrawMiniMark(Rect rect, RuneId rune, bool available = true)
        {
            var previous = GUI.color;
            GUI.color = available
                ? GlyphView.Slate
                : new Color(0.07f, 0.07f, 0.09f, 0.72f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            if (!available)
            {
                GUI.color = new Color(0.16f, 0.16f, 0.2f, 0.9f);
                DrawFrame(rect, 1f);
            }

            GUI.color = previous;
            RuneMark.DrawGui(rect, rune, RunePalette.MarkInk(rune, available));
            if (!available)
            {
                var wash = GUI.color;
                GUI.color = new Color(0.02f, 0.02f, 0.04f, 0.45f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = wash;
            }
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
                    "Time holds while this menu is open. Every root mark is on the wall. The weave is only what the screen is speaking — hover a mark to still the belt and see where it is from. Each available rune appears often enough to click; more copies follow how often that material is on screen, and uncommon marks take a larger share. Grey on the wall means that join is not in view. Charter Cast closes the menu: time runs, and you stand until you click. You are mind · body · soul.",
                    "Time holds while this menu is open. Draw marks from the grid, or send a kept working from the Grimoire if those marks are around. Hover the weave to still it and see where a mark is from. Charter Cast closes the menu: time runs, and you stand until you click."),
                body);
            GUI.Label(new Rect(28, 74, 980, 20),
                GlyphView.Speak(
                    "F / Enter Charter Cast   ·   X Free Cast   ·   R Store (Charter only)   ·   Space close   ·   Esc pause   ·   G Grimoire   ·   F1 Play",
                    "F / Enter Charter Cast   ·   X Free Cast   ·   R Store   ·   Space close   ·   Esc pause   ·   G Grimoire   ·   F1 Develop"),
                hint);

            var weaveTop = GlyphView.IsPlay ? 98f : DrawRuneWall();
            if (GlyphView.IsPlay && _director.HasCastNotice)
            {
                weaveTop = Mathf.Max(weaveTop, 186f);
            }

            DrawRoomWeave(weaveTop + 6f);
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
            var dockTop = Screen.height - ComposeDockHeight() - BarHeight;
            var height = Mathf.Max(72f, dockTop - top - 8f);
            var panel = new Rect(16, top, Screen.width - 32, height);
            DrawPanel(panel.x, panel.y, panel.width, panel.height);

            var heading = Label(15, FontStyle.Bold, new Color(0.9f, 0.82f, 0.55f));
            var hint = Label(13, FontStyle.Normal, new Color(0.72f, 0.76f, 0.84f));
            GUI.Label(new Rect(28, top + 8, 160, 20), "The room’s weave", heading);
            var reading = tapestry != null && !string.IsNullOrEmpty(tapestry.Reading)
                ? tapestry.Reading
                : "the field is quiet";
            GUI.Label(new Rect(196, top + 8, Screen.width - 480, 20), reading, hint);

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
            WeaveGlyph? hovered = null;

            GUI.BeginGroup(inner);
            var local = Event.current != null ? Event.current.mousePosition : Vector2.zero;
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
                    if (rect.Contains(local))
                    {
                        hovered = glyph;
                    }

                    if (glyph.IsTear)
                    {
                        DrawEmptySlot(rect, "tear");
                        continue;
                    }

                    // Groups are a visual hint. The recipe always takes the mark you click.
                    var shown = glyph.Shown;
                    var chunk = glyph.IsGroup ? glyph.Rune : RuneId.None;
                    DrawRuneCard(rect, shown, () => _director.WeaveFromField(shown), true, true, chunk);
                }
            }

            GUI.EndGroup();

            if (hovered.HasValue)
            {
                DrawWeaveOrigin(new Rect(Screen.width - 292, top + 8, 260, 20), hovered.Value);
            }
        }

        static void DrawWeaveOrigin(Rect rect, WeaveGlyph glyph)
        {
            var origin = RoomSentence.OriginOf(glyph);
            var mark = glyph.IsTear
                ? "tear"
                : RuneCatalog.NameOf(glyph.Shown);
            var line = GlyphView.Speak(
                mark + " · from " + origin,
                "from " + origin);
            var hint = Label(13, FontStyle.Italic, new Color(0.86f, 0.8f, 0.58f));
            hint.alignment = TextAnchor.MiddleRight;
            GUI.Label(rect, line, hint);
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
            const float banner = 14f;
            var slab = bounds;

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
            GUI.DrawTexture(new Rect(slab.x + 4f, slab.y + banner, slab.width - 8f, 2f), Texture2D.whiteTexture);
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

        static float ComposeDockHeight()
        {
            MeasureDraftSlots(out var slot, out _, out _, out var rows);
            return DraftSlotTop + rows * (slot + DraftRoleHeight + 4f) + DraftAfterSlots;
        }

        static void MeasureDraftSlots(out float slot, out float gap, out int columns, out int rows)
        {
            gap = DraftSlotGap;
            var available = Mathf.Max(DraftSlotPreferred, Screen.width - DraftSlotSide * 2f);
            slot = DraftSlotPreferred;
            columns = Mathf.Max(1, Mathf.FloorToInt((available + gap) / (slot + gap)));
            columns = Mathf.Min(columns, SpellComposer.MaxSlots);
            if (columns < SpellComposer.MaxSlots)
            {
                var compact = (available - (SpellComposer.MaxSlots - 1) * gap) / SpellComposer.MaxSlots;
                if (compact >= DraftSlotMin)
                {
                    slot = compact;
                    columns = SpellComposer.MaxSlots;
                }
            }

            rows = Mathf.CeilToInt(SpellComposer.MaxSlots / (float)columns);
        }

        void DrawComposeDock()
        {
            MeasureDraftSlots(out var slot, out _, out _, out var rows);
            var slotsBlock = DraftSlotTop + rows * (slot + DraftRoleHeight + 4f);
            var legendY = slotsBlock + 4f;
            var previewY = legendY + 18f;
            var actionsY = previewY + 24f;
            var dockHeight = ComposeDockHeight();
            var dockTop = Screen.height - dockHeight - BarHeight;
            DrawPanel(0, dockTop, Screen.width, dockHeight);

            var body = Label(14, FontStyle.Normal, new Color(0.86f, 0.88f, 0.94f));
            var accent = Label(16, FontStyle.Bold, new Color(0.9f, 0.82f, 0.55f));
            var legend = Label(12, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            GUI.Label(new Rect(24, dockTop + 8, 640, 20), "String", accent);
            DrawDraftSlots(dockTop + DraftSlotTop);
            GUI.Label(new Rect(24, dockTop + legendY, Screen.width - 48, 18),
                "Elemental is a material. Catalyst is mind, body, or soul. Special is anima, animus, aether, life, or death.",
                legend);
            GUI.Label(new Rect(24, dockTop + previewY, Screen.width - 48, 18),
                _director.Composer.DescribeFree(_director.Attunement), body);

            DrawCharterActions(dockTop + actionsY);
        }

        void DrawDraftSlots(float y)
        {
            MeasureDraftSlots(out var slot, out var gap, out var columns, out _);
            var role = Label(11, FontStyle.Bold, new Color(0.86f, 0.8f, 0.58f));
            role.alignment = TextAnchor.MiddleCenter;
            for (var i = 0; i < SpellComposer.MaxSlots; i++)
            {
                var col = i % columns;
                var row = i / columns;
                var rect = new Rect(
                    DraftSlotSide + col * (slot + gap),
                    y + row * (slot + DraftRoleHeight + 4f),
                    slot, slot);
                if (i < _director.Composer.Count)
                {
                    var index = i;
                    var rune = _director.Composer.Slots[i];
                    DrawRuneCard(rect, rune, () => _director.RemoveDraftFrom(index), true);
                    GUI.Label(new Rect(rect.x - 4, rect.yMax + 1, rect.width + 8, DraftRoleHeight),
                        RuneCatalog.StringRole(rune), role);
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

            const float pad = 12f;
            const float gap = 8f;
            var heldW = Mathf.Clamp(Screen.width * 0.16f, 168f, 220f);
            var slot = new Rect(pad, y + 12, heldW, BarHeight - 24);
            DrawHeldSlot(slot);

            var exploring = _director.Mode == PlayMode.Exploring;
            var charter = _director.Mode == PlayMode.Charter;
            var aiming = _director.Mode == PlayMode.Aiming;
            var packOpen = _director.Mode == PlayMode.Inventory;
            var grimOpen = _director.Mode == PlayMode.Grimoire;
            var paused = _director.Mode == PlayMode.Paused;
            var inWorld = exploring || charter || aiming;
            var nearby = _director.NearbyInteract;
            var interactLabel = nearby == null
                ? "Use"
                : string.IsNullOrWhiteSpace(nearby.InteractVerb) ? "Use" : nearby.InteractVerb;
            var draft = !_director.Composer.IsEmpty;
            var held = _director.Held.Occupied;

            var actions = new[]
            {
                new BarAction(
                    charter ? "Close" : aiming ? "Cancel" : "Charter",
                    "Space",
                    inWorld || grimOpen || packOpen,
                    charter || aiming
                        ? new Color(0.42f, 0.3f, 0.16f)
                        : new Color(0.26f, 0.28f, 0.4f),
                    () => _director.ToggleCharter()),
                new BarAction(
                    "Cast",
                    "F",
                    (charter && draft) || ((exploring || charter) && held),
                    new Color(0.62f, 0.28f, 0.2f),
                    () =>
                    {
                        if (charter && draft)
                        {
                            _director.CastCharter();
                        }
                        else
                        {
                            _director.CastHeld();
                        }
                    }),
                new BarAction(
                    "Free",
                    "X",
                    charter && draft,
                    new Color(0.52f, 0.22f, 0.48f),
                    () => _director.CastFree()),
                new BarAction(
                    "Store",
                    "R",
                    charter && draft,
                    new Color(0.28f, 0.38f, 0.62f),
                    () => _director.StoreDraft()),
                new BarAction(
                    interactLabel,
                    "E",
                    exploring && nearby != null,
                    new Color(0.42f, 0.36f, 0.16f),
                    () => _director.UseNearbyInteract()),
                new BarAction(
                    "Yield",
                    "K",
                    exploring || charter,
                    new Color(0.28f, 0.2f, 0.2f),
                    () => _director.YieldSelf())
            };

            var menus = new[]
            {
                new BarAction(
                    paused ? "Resume" : "Pause",
                    "Esc",
                    true,
                    paused ? new Color(0.28f, 0.38f, 0.22f) : new Color(0.2f, 0.22f, 0.28f),
                    () => _director.TogglePause()),
                new BarAction(
                    GlyphView.IsDevelop ? "Develop" : "Play",
                    "F1",
                    true,
                    GlyphView.IsDevelop
                        ? new Color(0.42f, 0.28f, 0.16f)
                        : new Color(0.18f, 0.22f, 0.3f),
                    () => _director.ToggleSight()),
                new BarAction(
                    packOpen ? "Close" : "Pack",
                    "I",
                    true,
                    packOpen ? new Color(0.42f, 0.32f, 0.16f) : new Color(0.28f, 0.26f, 0.2f),
                    () =>
                    {
                        if (packOpen)
                        {
                            _director.CloseInventory();
                        }
                        else
                        {
                            if (paused)
                            {
                                _director.TogglePause();
                            }

                            _director.OpenInventory();
                        }
                    }),
                new BarAction(
                    grimOpen ? "Close" : "Book",
                    "G",
                    true,
                    grimOpen ? new Color(0.55f, 0.42f, 0.18f) : new Color(0.32f, 0.24f, 0.42f),
                    () =>
                    {
                        if (grimOpen)
                        {
                            _director.CloseGrimoire();
                        }
                        else
                        {
                            if (paused)
                            {
                                _director.TogglePause();
                            }

                            _director.OpenGrimoire();
                        }
                    })
            };

            var count = actions.Length + menus.Length;
            var left = slot.xMax + gap;
            var right = Screen.width - pad;
            var width = right - left - (count - 1) * gap;
            var btnW = Mathf.Clamp(width / count, 70f, 116f);
            var btnH = 68f;
            var btnY = y + (BarHeight - btnH) * 0.5f;
            var x = left;
            for (var i = 0; i < actions.Length; i++)
            {
                DrawBarAction(new Rect(x, btnY, btnW, btnH), actions[i]);
                x += btnW + gap;
            }

            x = right - menus.Length * (btnW + gap) + gap;
            if (x < left + actions.Length * (btnW + gap))
            {
                x = left + actions.Length * (btnW + gap);
            }

            for (var i = 0; i < menus.Length; i++)
            {
                DrawBarAction(new Rect(x, btnY, btnW, btnH), menus[i]);
                x += btnW + gap;
            }
        }

        readonly struct BarAction
        {
            public BarAction(string title, string key, bool enabled, Color color, System.Action onClick)
            {
                Title = title;
                Key = key;
                Enabled = enabled;
                Color = color;
                OnClick = onClick;
            }

            public string Title { get; }
            public string Key { get; }
            public bool Enabled { get; }
            public Color Color { get; }
            public System.Action OnClick { get; }
        }

        static void DrawBarAction(Rect rect, BarAction action)
        {
            if (DrawControl(rect, action.Title, action.Key, action.Enabled, action.Color))
            {
                action.OnClick?.Invoke();
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
            DrawVeil(new Color(0.02f, 0.02f, 0.05f, 0.86f));
            if (GlyphView.IsPlay && _bookPage != BookPage.Workings)
            {
                _bookPage = BookPage.Workings;
            }

            var header = DrawGrimoireChrome();
            var view = new Rect(16f, header, Screen.width - 32f, Screen.height - BarHeight - header - 8f);
            if (view.height < 48f)
            {
                return;
            }

            switch (_bookPage)
            {
                case BookPage.Runes:
                    DrawBookPage(view, MeasureRuneBook, DrawRuneBook);
                    break;
                case BookPage.Spells:
                    DrawBookPage(view, MeasureSpellBook, DrawSpellBook);
                    break;
                case BookPage.World:
                    DrawBookPage(view, MeasureWorldBook, DrawWorldBook);
                    break;
                default:
                    DrawBookPage(view, MeasureWorkingsBook, DrawWorkingsBook);
                    break;
            }
        }

        float DrawGrimoireChrome()
        {
            var title = Label(26, FontStyle.Bold, Color.white);
            var subtitle = Label(14, FontStyle.Normal, new Color(0.8f, 0.82f, 0.9f));
            GUI.Label(new Rect(20, 10, 280, 32), "Grimoire", title);
            GUI.Label(new Rect(300, 16, Screen.width - 320, 22),
                GlyphView.Speak(
                    "Search the book, or filter a rune. Click a join or a spell to string it. Esc closes.",
                    "Search your kept pages, or filter a rune. Click a page to send it if those marks are around. Esc closes."),
                subtitle);

            var tabY = 48f;
            var tabH = 30f;
            var tabW = 108f;
            var x = 20f;
            if (DrawTab(new Rect(x, tabY, tabW, tabH), "Workings", _bookPage == BookPage.Workings))
            {
                _bookPage = BookPage.Workings;
                _pauseScroll = Vector2.zero;
            }

            x += tabW + 6f;
            if (GlyphView.IsDevelop)
            {
                if (DrawTab(new Rect(x, tabY, tabW, tabH), "Runes", _bookPage == BookPage.Runes))
                {
                    _bookPage = BookPage.Runes;
                    _pauseScroll = Vector2.zero;
                }

                x += tabW + 6f;
                if (DrawTab(new Rect(x, tabY, tabW, tabH), "Spells", _bookPage == BookPage.Spells))
                {
                    _bookPage = BookPage.Spells;
                    _pauseScroll = Vector2.zero;
                }

                x += tabW + 6f;
                if (DrawTab(new Rect(x, tabY, tabW, tabH), "World", _bookPage == BookPage.World))
                {
                    _bookPage = BookPage.World;
                    _pauseScroll = Vector2.zero;
                }

                x += tabW + 8f;
            }

            var search = new Rect(Mathf.Max(x + 8f, Screen.width - 420f), tabY, 280f, tabH);
            if (search.x < x + 8f)
            {
                search.x = x + 8f;
                search.width = Mathf.Max(160f, Screen.width - search.x - 28f);
            }

            var fieldFill = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            GUI.DrawTexture(search, Texture2D.whiteTexture);
            GUI.color = fieldFill;
            var field = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 8, 4, 4)
            };
            field.normal.textColor = new Color(0.92f, 0.9f, 0.82f);
            GUI.SetNextControlName("GrimoireSearch");
            var next = GUI.TextField(search, _bookQuery ?? string.Empty, field);
            if (next != _bookQuery)
            {
                _bookQuery = next;
                _pauseScroll = Vector2.zero;
            }

            EditingBookSearch = GUI.GetNameOfFocusedControl() == "GrimoireSearch";
            if (_focusBookSearch)
            {
                GUI.FocusControl("GrimoireSearch");
                if (Event.current != null && Event.current.type == EventType.Repaint)
                {
                    _focusBookSearch = false;
                }
            }

            var hint = Label(12, FontStyle.Italic, new Color(0.62f, 0.64f, 0.72f));
            if (string.IsNullOrEmpty(_bookQuery) && !EditingBookSearch)
            {
                GUI.Label(new Rect(search.x + 8f, search.y + 6f, search.width - 16f, 18f),
                    "Search name, recipe, meaning…", hint);
            }

            var filterY = tabY + tabH + 10f;
            var filterH = DrawRuneFilters(new Rect(20f, filterY, Screen.width - 40f, 80f));
            return filterY + filterH + 10f;
        }

        float DrawRuneFilters(Rect view)
        {
            var chipH = 32f;
            var gap = 6f;
            var allW = 52f;
            var x = view.x;
            var y = view.y;
            if (DrawTab(new Rect(x, y, allW, chipH), "All", _bookFilter == RuneId.None))
            {
                _bookFilter = RuneId.None;
                _pauseScroll = Vector2.zero;
            }

            x += allW + gap;
            var nameStyle = Label(12, FontStyle.Bold, new Color(0.9f, 0.88f, 0.78f));
            nameStyle.alignment = TextAnchor.MiddleLeft;
            var runes = GrimoireQuery.FilterRunes;
            for (var i = 0; i < runes.Length; i++)
            {
                var rune = runes[i];
                var label = RuneCatalog.NameOf(rune);
                var chipW = Mathf.Clamp(nameStyle.CalcSize(new GUIContent(label)).x + 36f, 68f, 110f);
                if (x + chipW > view.xMax)
                {
                    x = view.x;
                    y += chipH + 6f;
                }

                var rect = new Rect(x, y, chipW, chipH);
                var on = _bookFilter == rune;
                var previous = GUI.color;
                GUI.color = on
                    ? new Color(0.42f, 0.32f, 0.12f, 0.95f)
                    : new Color(0.1f, 0.11f, 0.14f, 0.8f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = on
                    ? new Color(0.95f, 0.82f, 0.4f, 0.95f)
                    : new Color(0.55f, 0.56f, 0.62f, 0.7f);
                DrawFrame(rect, on ? 2f : 1f);
                GUI.color = previous;
                DrawMiniMark(new Rect(rect.x + 5f, rect.y + 4f, 24f, 24f), rune, true);
                GUI.Label(new Rect(rect.x + 30f, rect.y, rect.width - 34f, chipH), label, nameStyle);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    _bookFilter = on ? RuneId.None : rune;
                    _pauseScroll = Vector2.zero;
                }

                x += chipW + gap;
            }

            return y + chipH - view.y;
        }

        void DrawBookPage(Rect view, System.Func<float, float> measure, System.Action<float, float> draw)
        {
            var fullHeight = measure(view.width);
            if (fullHeight <= view.height + 1f)
            {
                _pauseScroll = Vector2.zero;
                GUI.BeginGroup(view);
                draw(view.width, 0f);
                GUI.EndGroup();
                return;
            }

            var innerW = view.width - 22f;
            var innerH = measure(innerW);
            _pauseScroll = GUI.BeginScrollView(view, _pauseScroll, new Rect(0, 0, innerW, innerH));
            draw(innerW, 0f);
            GUI.EndScrollView();
        }

        void DrawWorkingsBook(float width, float y)
        {
            var heading = Label(18, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var muted = Label(15, FontStyle.Italic, new Color(0.72f, 0.74f, 0.82f));
            y = DrawKeptMarksInline(y);
            y += 8f;
            GUI.Label(new Rect(0, y, width, 24), "Kept workings", heading);
            y += 28f;

            var kept = _director.Grimoire.KeptWorkings;
            var shown = 0;
            for (var i = 0; i < kept.Count; i++)
            {
                if (!GrimoireQuery.MatchesWorking(kept[i], _bookQuery, _bookFilter))
                {
                    continue;
                }

                var index = i;
                DrawCastRow(new Rect(0, y, width, BookRow - 4f), FromKept(kept[i]),
                    () => _director.CastKept(index),
                    () => BeginRenameKept(index, FromKept(kept[index])), showRunes: true);
                y += BookRow;
                shown++;
            }

            if (kept.Count == 0)
            {
                GUI.Label(new Rect(0, y, width, 48),
                    "Nothing kept yet. With Add new spells on, a working that holds is written here without a name. Or Keep one from Recent.",
                    muted);
            }
            else if (shown == 0)
            {
                GUI.Label(new Rect(0, y, width, 28), "No kept page matches that search.", muted);
            }
        }

        float MeasureWorkingsBook(float width)
        {
            var kept = _director.Grimoire.KeptWorkings;
            var rows = 0;
            for (var i = 0; i < kept.Count; i++)
            {
                if (GrimoireQuery.MatchesWorking(kept[i], _bookQuery, _bookFilter))
                {
                    rows++;
                }
            }

            var marksH = _director.Memory.Kept.Count == 0 ? 76f : 112f;
            var emptyH = kept.Count == 0 || rows == 0 ? 48f : 0f;
            return marksH + 36f + emptyH + rows * BookRow;
        }

        void DrawRuneBook(float width, float y)
        {
            CatalogBook.EnsureLoaded();
            var heading = Label(18, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var muted = Label(14, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            var shown = 0;
            var groups = RuneCatalog.LedgerGroups();
            for (var g = 0; g < groups.Count; g++)
            {
                var group = groups[g];
                var matches = 0;
                for (var i = 0; i < group.Runes.Count; i++)
                {
                    if (GrimoireQuery.MatchesRune(group.Runes[i], _bookQuery, _bookFilter))
                    {
                        matches++;
                    }
                }

                if (matches == 0)
                {
                    continue;
                }

                GUI.Label(new Rect(0, y, width, 24), group.Title, heading);
                y += 28f;
                y = DrawRuneCardGrid(y, width, group.Runes, true);
                shown += matches;
                y += 12f;
            }

            if (shown == 0)
            {
                GUI.Label(new Rect(0, y, width, 28), "No rune matches that search.", muted);
            }
        }

        float MeasureRuneBook(float width)
        {
            CatalogBook.EnsureLoaded();
            var columns = Mathf.Max(1, Mathf.FloorToInt((width + 10f) / (BookRuneCard + 10f)));
            var height = 8f;
            var groups = RuneCatalog.LedgerGroups();
            for (var g = 0; g < groups.Count; g++)
            {
                var matches = 0;
                var runes = groups[g].Runes;
                for (var i = 0; i < runes.Count; i++)
                {
                    if (GrimoireQuery.MatchesRune(runes[i], _bookQuery, _bookFilter))
                    {
                        matches++;
                    }
                }

                if (matches == 0)
                {
                    continue;
                }

                var rows = Mathf.CeilToInt(matches / (float)columns);
                height += 40f + rows * (BookRuneCardH + 10f);
            }

            return Mathf.Max(48f, height);
        }

        float DrawRuneCardGrid(float y, float width, IReadOnlyList<RuneId> runes, bool loadable)
        {
            var columns = Mathf.Max(1, Mathf.FloorToInt((width + 10f) / (BookRuneCard + 10f)));
            var col = 0;
            var name = Label(15, FontStyle.Bold, new Color(0.94f, 0.92f, 0.82f));
            var birth = Label(12, FontStyle.Italic, new Color(0.72f, 0.74f, 0.82f));
            var meaning = Label(12, FontStyle.Normal, new Color(0.8f, 0.82f, 0.88f));
            meaning.clipping = TextClipping.Clip;
            for (var i = 0; i < runes.Count; i++)
            {
                var rune = runes[i];
                if (!GrimoireQuery.MatchesRune(rune, _bookQuery, _bookFilter)
                    || !RuneCatalog.TryGet(rune, out var def))
                {
                    continue;
                }

                var rect = new Rect(col * (BookRuneCard + 10f), y, BookRuneCard, BookRuneCardH);
                var previous = GUI.color;
                GUI.color = new Color(0.1f, 0.11f, 0.15f, 0.82f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = new Color(0.92f, 0.74f, 0.32f, 0.28f);
                DrawFrame(rect, 1f);
                GUI.color = previous;
                DrawMiniMark(new Rect(rect.x + 10f, rect.y + 14f, 36f, 36f), rune, true);
                GUI.Label(new Rect(rect.x + 54f, rect.y + 8f, rect.width - 62f, 20f), def.Name, name);
                var born = ChainBook.BirthNameText(rune);
                GUI.Label(new Rect(rect.x + 54f, rect.y + 28f, rect.width - 62f, 16f),
                    string.IsNullOrEmpty(born) ? "root mark" : born, birth);
                GUI.Label(new Rect(rect.x + 10f, rect.y + 56f, rect.width - 20f, 30f), def.Meaning, meaning);
                if (loadable && ChainBook.IsWrought(rune) && GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    _director.LoadBirth(rune);
                }
                else if (!ChainBook.IsWrought(rune))
                {
                    GUI.Button(rect, GUIContent.none, GUIStyle.none);
                }

                col++;
                if (col >= columns)
                {
                    col = 0;
                    y += BookRuneCardH + 10f;
                }
            }

            if (col > 0)
            {
                y += BookRuneCardH + 10f;
            }

            return y;
        }

        void DrawSpellBook(float width, float y)
        {
            CatalogBook.EnsureLoaded();
            var heading = Label(18, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var muted = Label(14, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            var shown = 0;
            SpellBook? current = null;
            foreach (var entry in SpellCodex.All)
            {
                if (!GrimoireQuery.MatchesSpell(entry, _bookQuery, _bookFilter))
                {
                    continue;
                }

                if (current != entry.Book)
                {
                    if (current != null)
                    {
                        y += 8f;
                    }

                    current = entry.Book;
                    GUI.Label(new Rect(0, y, width, 24), SpellCodex.BookName(entry.Book), heading);
                    y += 28f;
                }

                DrawSpellCard(new Rect(0, y, width, BookRow - 4f), entry);
                y += BookRow;
                shown++;
            }

            if (shown == 0)
            {
                GUI.Label(new Rect(0, y, width, 28), "No written spell matches that search.", muted);
            }
        }

        float MeasureSpellBook(float width)
        {
            CatalogBook.EnsureLoaded();
            SpellBook? current = null;
            var height = 8f;
            var shown = 0;
            foreach (var entry in SpellCodex.All)
            {
                if (!GrimoireQuery.MatchesSpell(entry, _bookQuery, _bookFilter))
                {
                    continue;
                }

                if (current != entry.Book)
                {
                    current = entry.Book;
                    height += shown == 0 ? 28f : 36f;
                }

                height += BookRow;
                shown++;
            }

            return Mathf.Max(48f, height);
        }

        void DrawSpellCard(Rect rect, CodexEntry entry)
        {
            var previous = GUI.color;
            GUI.color = _director.Grimoire.Keeps(entry.Spell)
                ? new Color(0.32f, 0.24f, 0.1f, 0.85f)
                : new Color(0.1f, 0.11f, 0.14f, 0.62f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;

            var number = Label(13, FontStyle.Bold, new Color(0.7f, 0.72f, 0.8f));
            var title = Label(16, FontStyle.Bold, new Color(0.95f, 0.92f, 0.8f));
            var body = Label(13, FontStyle.Normal, new Color(0.82f, 0.84f, 0.9f));
            var muted = Label(12, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            var stanceColor = entry.FreeOnly ? FreeSuccess : CharterSuccess;
            var stance = Label(12, FontStyle.Bold, stanceColor);

            GUI.Label(new Rect(rect.x + 10f, rect.y + 6f, 36f, 20f), entry.Number.ToString(), number);
            var name = string.IsNullOrEmpty(entry.Gate) ? entry.Name : $"{entry.Name}  ({entry.Gate})";
            GUI.Label(new Rect(rect.x + 48f, rect.y + 4f, rect.width * 0.42f, 22f), name, title);
            GUI.Label(new Rect(rect.x + 48f, rect.y + 26f, rect.width * 0.42f, 18f), entry.Want, body);
            GUI.Label(new Rect(rect.xMax - 168f, rect.y + 6f, 70f, 18f),
                entry.FreeOnly ? "Free" : "Charter", stance);
            GUI.Label(new Rect(rect.xMax - 92f, rect.y + 6f, 80f, 18f), entry.Form, muted);

            var runes = entry.RecipeRunes;
            var mark = 26f;
            var start = rect.x + rect.width * 0.48f;
            if (runes != null)
            {
                for (var i = 0; i < runes.Count; i++)
                {
                    DrawMiniMark(new Rect(start + i * (mark + 4f), rect.y + 28f, mark, mark),
                        runes[i], _director.RunePresent(runes[i]));
                }
            }

            var chain = string.IsNullOrEmpty(entry.Via) ? entry.Recipe : $"{entry.Recipe}  =  {entry.Via}";
            GUI.Label(new Rect(start, rect.y + 6f, rect.width * 0.28f, 18f), chain, muted);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                _director.LoadCodex(entry.Number);
            }
        }

        void DrawWorldBook(float width, float y)
        {
            var heading = Label(18, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var muted = Label(14, FontStyle.Italic, new Color(0.7f, 0.72f, 0.8f));
            var name = Label(15, FontStyle.Bold, new Color(0.92f, 0.9f, 0.8f));
            var note = Label(13, FontStyle.Normal, new Color(0.82f, 0.84f, 0.9f));
            GUI.Label(new Rect(0, y, width, 24), "World materials", heading);
            y += 26f;
            GUI.Label(new Rect(0, y, width, 20),
                "Stamp a MaterialId on a tile. The Charter weave reads the full signature.",
                muted);
            y += 28f;

            var shown = 0;
            foreach (var material in MaterialCatalog.All)
            {
                if (!GrimoireQuery.MatchesMaterial(material, _bookQuery, _bookFilter))
                {
                    continue;
                }

                var rect = new Rect(0, y, width, 52f);
                var previous = GUI.color;
                GUI.color = new Color(0.1f, 0.11f, 0.14f, 0.55f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = previous;
                GUI.Label(new Rect(12f, y + 6f, 160f, 20f), material.Name, name);
                GUI.Label(new Rect(180f, y + 6f, width - 200f, 20f), material.Note, note);
                var sig = material.Signature;
                for (var i = 0; i < sig.Count; i++)
                {
                    DrawMiniMark(new Rect(12f + i * 30f, y + 26f, 22f, 22f), sig[i], true);
                }

                y += 56f;
                shown++;
            }

            if (shown == 0)
            {
                GUI.Label(new Rect(0, y, width, 28), "No material matches that search.", muted);
            }
        }

        float MeasureWorldBook(float width)
        {
            var rows = 0;
            foreach (var material in MaterialCatalog.All)
            {
                if (GrimoireQuery.MatchesMaterial(material, _bookQuery, _bookFilter))
                {
                    rows++;
                }
            }

            return 70f + Mathf.Max(1, rows) * 56f;
        }

        void ClearBookSearch()
        {
            if (!string.IsNullOrEmpty(_bookQuery))
            {
                _bookQuery = string.Empty;
                _pauseScroll = Vector2.zero;
                return;
            }

            if (_bookFilter != RuneId.None)
            {
                _bookFilter = RuneId.None;
                _pauseScroll = Vector2.zero;
                return;
            }

            GUI.FocusControl(null);
            EditingBookSearch = false;
        }

        void DrawPause()
        {
            DrawVeil(new Color(0.02f, 0.02f, 0.04f, 0.86f));
            var title = Label(28, FontStyle.Bold, Color.white);
            var subtitle = Label(15, FontStyle.Normal, new Color(0.78f, 0.8f, 0.88f));
            var heading = Label(17, FontStyle.Bold, new Color(0.92f, 0.82f, 0.5f));
            var muted = Label(13, FontStyle.Italic, new Color(0.68f, 0.7f, 0.78f));

            GUI.Label(new Rect(40, 24, 800, 34), "Paused", title);
            GUI.Label(new Rect(40, 62, 980, 22),
                "Settings and a breath. Esc resumes. The Grimoire still opens with G.",
                subtitle);

            var panel = new Rect(40, 100, Mathf.Min(720f, Screen.width - 80f), 280f);
            DrawPanel(panel.x, panel.y, panel.width, panel.height);
            GUI.Label(new Rect(panel.x + 24, panel.y + 16, 400, 24), "Settings", heading);

            var hideBad = DrawSetting(
                new Rect(panel.x + 24, panel.y + 52, panel.width - 48, 72),
                GameSettings.HideBadRecipes,
                "Don't show bad recipes",
                "Recent only keeps the last failed cast, so mistakes do not stack.");
            if (hideBad != GameSettings.HideBadRecipes)
            {
                GameSettings.SetHideBadRecipes(hideBad);
            }

            var prompt = DrawSetting(
                new Rect(panel.x + 24, panel.y + 132, panel.width - 48, 72),
                GameSettings.PromptNewSpells,
                "Add new spells",
                "After a working first holds, it is written in the Grimoire without a name. Rename it from the book. Off: Keep a recipe yourself from Recent.");
            if (prompt != GameSettings.PromptNewSpells)
            {
                GameSettings.SetPromptNewSpells(prompt);
            }

            if (DrawAction(new Rect(panel.x + 24, panel.y + 220, 140, 40), "Resume", true,
                    new Color(0.28f, 0.38f, 0.22f)))
            {
                _director.TogglePause();
            }

            if (DrawAction(new Rect(panel.x + 176, panel.y + 220, 140, 40), "Grimoire", true,
                    new Color(0.32f, 0.24f, 0.42f)))
            {
                _director.TogglePause();
                _director.OpenGrimoire();
            }

            if (GlyphView.IsDevelop)
            {
                GUI.Label(new Rect(40, 400, 980, 40),
                    "Develop sight still keeps the written ledger in the Grimoire. Pause is only settings now.",
                    muted);
            }
        }

        static bool DrawSetting(Rect rect, bool value, string title, string hint)
        {
            var box = new Rect(rect.x, rect.y + 8, 22, 22);
            var next = GUI.Toggle(box, value, GUIContent.none);
            var name = Label(16, FontStyle.Bold, new Color(0.94f, 0.9f, 0.78f));
            var body = Label(13, FontStyle.Normal, new Color(0.76f, 0.78f, 0.86f));
            GUI.Label(new Rect(rect.x + 32, rect.y, rect.width - 32, 24), title, name);
            GUI.Label(new Rect(rect.x + 32, rect.y + 26, rect.width - 32, 40), hint, body);
            return next;
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
            if (!HoldsPlay && ev != null && rect.Contains(ev.mousePosition) && ev.type == EventType.MouseDown)
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
                    "No marks kept yet. Remembering a mark for the wall comes later. Draw from the weave for now.",
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
                }, _director.InVicinity(rune));
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
                    "Nothing is kept yet. Remembering a mark for the wall comes later. Draw from the weave for now.",
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
                }, _director.InVicinity(rune));
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

        static bool DrawControl(Rect rect, string title, string key, bool enabled, Color color)
        {
            var previous = GUI.enabled;
            GUI.enabled = previous && enabled;
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(4, 4, 4, 18)
            };
            var clicked = GUI.Button(rect, title, style);
            GUI.backgroundColor = bg;
            GUI.enabled = previous;

            var hint = Label(11, FontStyle.Normal, enabled
                ? new Color(0.86f, 0.8f, 0.58f)
                : new Color(0.5f, 0.5f, 0.56f));
            hint.alignment = TextAnchor.LowerCenter;
            GUI.Label(new Rect(rect.x + 2f, rect.y, rect.width - 4f, rect.height - 4f), key, hint);
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
