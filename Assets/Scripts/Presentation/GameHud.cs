using UnityEngine;

namespace RuneMagic
{
    public sealed class GameHud : MonoBehaviour
    {
        SanctumDirector _director;
        bool _grimoireOpen;

        public void Bind(SanctumDirector director)
        {
            _director = director;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                _grimoireOpen = !_grimoireOpen;
            }
        }

        void OnGUI()
        {
            if (_director == null)
            {
                return;
            }

            DrawPanel(12, 12, 540, 196);
            var title = Label(22, FontStyle.Bold, Color.white);
            var body = Label(15, FontStyle.Normal, new Color(0.88f, 0.9f, 0.95f));
            var accent = Label(15, FontStyle.Bold, StanceColor());

            GUI.Label(new Rect(28, 20, 520, 28), "Rune Magic", title);
            GUI.Label(new Rect(28, 48, 520, 22), RoomLine(), body);
            GUI.Label(new Rect(28, 70, 520, 22), $"Stance: {_director.Composer.Stance}   Taint: {_director.Taint:0.00}", accent);
            GUI.Label(new Rect(28, 92, 520, 22), $"Compose: {_director.Composer.SlotSummary()}", body);
            GUI.Label(new Rect(28, 114, 520, 22), TargetLine(), body);
            GUI.Label(new Rect(28, 136, 510, 58), _director.LastLog, body);

            DrawPanel(12, Screen.height - 78, 640, 66);
            GUI.Label(new Rect(28, Screen.height - 70, 620, 50),
                "WASD move  ·  Click or 1–7 take a rune  ·  Tab/Q Charter/Free  ·  F cast  ·  C clear  ·  G grimoire",
                body);

            if (_grimoireOpen)
            {
                DrawGrimoire();
            }
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

        void DrawGrimoire()
        {
            DrawPanel(Screen.width - 460, 12, 448, 360);
            var title = Label(18, FontStyle.Bold, Color.white);
            var body = Label(14, FontStyle.Normal, new Color(0.86f, 0.88f, 0.92f));
            GUI.Label(new Rect(Screen.width - 444, 24, 420, 24), "Grimoire — known recipes", title);

            var y = 56f;
            var any = false;
            foreach (var recipe in SpellGrammar.All)
            {
                if (!_director.Grimoire.KnowsRecipe(recipe.Material, recipe.Aspect))
                {
                    continue;
                }

                any = true;
                GUI.Label(new Rect(Screen.width - 444, y, 420, 20),
                    $"{recipe.Name}   {SpellGrammar.FormulaText(recipe.Material, recipe.Aspect)}", body);
                y += 22f;
            }

            if (!any)
            {
                GUI.Label(new Rect(Screen.width - 444, y, 420, 60),
                    "Empty. Walk the Free charm, or compose until a form writes itself.", body);
            }
        }

        Color StanceColor()
        {
            return _director.Composer.Stance == CastingStance.Charter
                ? new Color(0.75f, 0.86f, 1f)
                : new Color(1f, 0.72f, 0.45f);
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
            GUI.color = new Color(0.05f, 0.06f, 0.1f, 0.78f);
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
            GUI.color = color;
        }
    }
}
