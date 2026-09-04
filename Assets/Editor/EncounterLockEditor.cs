#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(EncounterLock))]
    public sealed class EncounterLockEditor : Editor
    {
        static readonly string[] SpellLabels = Labels();
        static readonly SpellId[] SpellIds = Ids();
        int _packIndex;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Every enemy is a lock. Dress the look, then give them a mind.\n\n" +
                "Attacks — add close / mid / long slots. Pick a spell and the runes fill themselves. Leave Attacks empty and the old Attack dropdown still works (Golem slam, Wizard fireball, Archer arrow).\n\n" +
                "Gambits — first matching if/then wins, like FF12. The Mixed Court wall → flame-pillar is the default when this list is empty and they write fire.\n\n" +
                "Nature — change the body from the Inspector. Load nature defaults, then tweak defense and affinities (0 immune … 5 ruin-weak).\n\n" +
                "See ENEMIES.md.",
                MessageType.Info);

            DrawLock();
            DrawLook();
            DrawNature();
            DrawMind();
            DrawAttacks();
            DrawGambits();
            DrawLegacyAttack();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Snap to grid"))
            {
                var encounter = (EncounterLock)target;
                Undo.RecordObject(encounter.transform, "Snap enemy");
                encounter.transform.position = AuthoringUtil.Snap(encounter.transform.position);
            }
        }

        void DrawLock()
        {
            EditorGUILayout.LabelField("Lock", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredName"), new GUIContent("Name"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredId"), new GUIContent("Id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("formula"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keys"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredEnsouled"), new GUIContent("Ensouled"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredBlocking"), new GUIContent("Blocking"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("grant"), new GUIContent("Grant item"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Start from a pack body", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            var names = new string[PackEnemies.All.Length];
            for (var i = 0; i < PackEnemies.All.Length; i++)
            {
                names[i] = PackEnemies.All[i].Name;
            }

            _packIndex = EditorGUILayout.Popup(_packIndex, names);
            if (GUILayout.Button("Apply pack", GUILayout.Width(96)))
            {
                var encounter = (EncounterLock)target;
                Undo.RecordObject(encounter, "Apply pack");
                encounter.ApplyPack(PackEnemies.All[_packIndex]);
                serializedObject.Update();
                EnemyArtBind.Apply(encounter, overwrite: false, PackEnemies.All[_packIndex]);
                serializedObject.Update();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawLook()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Look", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resolveFrames"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleClip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackClip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resolveClip"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill empty frames from pack"))
            {
                EnemyArtBind.Apply((EncounterLock)target, overwrite: false);
                serializedObject.Update();
            }

            if (GUILayout.Button("Replace frames from pack"))
            {
                if (EditorUtility.DisplayDialog(
                    "Replace enemy frames",
                    "Overwrite Portrait, Idle, Attack, and Resolve with the ElvGames slices for this Sprite Id?",
                    "Replace",
                    "Cancel"))
                {
                    EnemyArtBind.Apply((EncounterLock)target, overwrite: true);
                    serializedObject.Update();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawNature()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nature & resistances", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("authoredNature"),
                new GUIContent("Nature", "Auto reads the Id. Golem is earth. Warden is mind if Ensouled."));
            var customDefense = serializedObject.FindProperty("customDefense");
            EditorGUILayout.PropertyField(customDefense, new GUIContent("Override defense"));
            if (customDefense.boolValue)
            {
                EditorGUILayout.IntSlider(serializedObject.FindProperty("authoredDefense"), 0, 10, new GUIContent("Defense"));
            }

            var customPush = serializedObject.FindProperty("customPush");
            EditorGUILayout.PropertyField(customPush, new GUIContent("Override push resist"));
            if (customPush.boolValue)
            {
                EditorGUILayout.IntSlider(serializedObject.FindProperty("authoredPushResist"), 0, 6, new GUIContent("Push resist"));
            }

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("strikeAffinities"),
                new GUIContent("Strike affinities", "0 immune, 1 normal, 5 ruin-weak. Empty uses the nature row."),
                true);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("statusAffinities"),
                new GUIContent("Status affinities"),
                true);

            if (GUILayout.Button("Load nature defaults into affinities"))
            {
                var encounter = (EncounterLock)target;
                Undo.RecordObject(encounter, "Load nature defaults");
                encounter.LoadNatureDefaults();
                serializedObject.Update();
            }

            EditorGUILayout.HelpBox(
                "Affinity 0 shrugs the column off. A stone golem is defense 4 and will not take a fireball. Override Fire to 0 if you want a stubborn earth body that also ignores hunger.",
                MessageType.None);
        }

        void DrawMind()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mind", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("authoredMode"),
                new GUIContent("Mode", "Auto: Golem holds ground, Wizard / Archer stand and write, None wanders. Hunt chases. Skirmish keeps mid / long."));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("closeRange"),
                new GUIContent("Close range", "Slam reach. 0 uses 1.25."));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("midRange"),
                new GUIContent("Mid range", "Mid-band ceiling. 0 uses 4.5."));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("longRange"),
                new GUIContent("Long range", "Sight and long shots. 0 uses 8.2."));
        }

        void DrawAttacks()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attacks", EditorStyles.boldLabel);
            var attacks = serializedObject.FindProperty("attacks");
            EditorGUILayout.HelpBox(
                "One slot per range is enough for most bodies. A golem with a mid fireball and a close slam will shoot, then slam when you step in. Pick a spell to fill the runes.",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add slam"))
            {
                AddSlot(CombatBook.SlamSlot());
            }

            if (GUILayout.Button("Add fireball"))
            {
                AddSlot(CombatBook.SlotFromPage(CombatBook.PageOf(SpellId.Fireball, CombatStrike.Shot)));
            }

            if (GUILayout.Button("Add arrow"))
            {
                AddSlot(CombatBook.SlotFromPage(CombatBook.PageOf(SpellId.WoodArrow, CombatStrike.Shot)));
            }

            if (GUILayout.Button("Add flame-pillar"))
            {
                AddSlot(CombatBook.SlotFromPage(CombatBook.PageOf(SpellId.FlamePillar, CombatStrike.Pillar)));
            }

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Seed attacks from Attack kind"))
            {
                var encounter = (EncounterLock)target;
                Undo.RecordObject(encounter, "Seed attacks");
                encounter.SeedAttacksFromKind();
                serializedObject.Update();
            }

            for (var i = 0; i < attacks.arraySize; i++)
            {
                DrawSlot(attacks.GetArrayElementAtIndex(i), i, attacks);
            }
        }

        void DrawSlot(SerializedProperty slot, int index, SerializedProperty list)
        {
            if (slot == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Slot " + (index + 1), EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(72)))
            {
                list.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(slot.FindPropertyRelative("Name"));
            EditorGUILayout.PropertyField(slot.FindPropertyRelative("Range"));
            var spellProp = slot.FindPropertyRelative("Spell");
            var strikeProp = slot.FindPropertyRelative("Strike");
            var currentSpell = (SpellId)spellProp.intValue;
            var currentStrike = (CombatStrike)strikeProp.enumValueIndex;
            var pageIndex = PageIndex(currentSpell, currentStrike);
            EditorGUI.BeginChangeCheck();
            pageIndex = EditorGUILayout.Popup("Attack / spell", pageIndex, SpellLabels);
            if (EditorGUI.EndChangeCheck())
            {
                var page = CombatBook.Pages[pageIndex];
                spellProp.intValue = (int)page.Spell;
                strikeProp.enumValueIndex = (int)page.Strike;
                slot.FindPropertyRelative("Range").enumValueIndex = (int)page.Range;
                WriteStrings(slot.FindPropertyRelative("Recipe"), CombatBook.RecipeNames(page.Spell));
                var seconds = slot.FindPropertyRelative("CastSeconds");
                if (page.CastSeconds > 0f)
                {
                    seconds.floatValue = page.CastSeconds;
                }

                var name = slot.FindPropertyRelative("Name");
                name.stringValue = page.Name;
            }
            else
            {
                EditorGUILayout.PropertyField(strikeProp, new GUIContent("Strike"));
            }

            EditorGUILayout.PropertyField(slot.FindPropertyRelative("Recipe"), new GUIContent("Recipe"), true);
            var runes = CombatBook.ParseRecipe(ReadStrings(slot.FindPropertyRelative("Recipe")));
            if (runes.Length > 0)
            {
                EditorGUILayout.LabelField(WorkingNames.RunePhrase(runes), EditorStyles.miniLabel);
            }
            else if (currentStrike != CombatStrike.Slam)
            {
                EditorGUILayout.LabelField("No spell — write the runes yourself, or pick Slam.", EditorStyles.miniLabel);
            }

            EditorGUILayout.PropertyField(slot.FindPropertyRelative("CastSeconds"));
            EditorGUILayout.EndVertical();
        }

        void DrawGambits()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gambits", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "If the player raises a wall, then write flame-pillar. First match wins. Player-cast gambits fire when that sentence lands in the same room.",
                MessageType.None);
            if (GUILayout.Button("Add wall → flame-pillar"))
            {
                AddGambit(CombatBook.WallToFlamePillar());
            }

            var gambits = serializedObject.FindProperty("gambits");
            for (var i = 0; i < gambits.arraySize; i++)
            {
                DrawGambit(gambits.GetArrayElementAtIndex(i), i, gambits);
            }
        }

        void DrawGambit(SerializedProperty gambit, int index, SerializedProperty list)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gambit " + (index + 1), EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(72)))
            {
                list.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(gambit.FindPropertyRelative("Name"));
            EditorGUILayout.PropertyField(gambit.FindPropertyRelative("When"));
            var when = (GambitWhen)gambit.FindPropertyRelative("When").enumValueIndex;
            if (when == GambitWhen.PlayerCasts)
            {
                var whenSpell = gambit.FindPropertyRelative("WhenSpell");
                var picked = SpellPopup("If they cast", (SpellId)whenSpell.intValue);
                whenSpell.intValue = (int)picked;
            }

            if (when == GambitWhen.SelfHasStatus || when == GambitWhen.TargetHasStatus)
            {
                EditorGUILayout.PropertyField(gambit.FindPropertyRelative("WhenStatus"));
            }

            EditorGUILayout.PropertyField(gambit.FindPropertyRelative("ThenStrike"), new GUIContent("Then"));
            var thenSpell = gambit.FindPropertyRelative("ThenSpell");
            var then = SpellPopup("Then spell", (SpellId)thenSpell.intValue);
            if (then != (SpellId)thenSpell.intValue)
            {
                thenSpell.intValue = (int)then;
                if (then != SpellId.None)
                {
                    WriteStrings(gambit.FindPropertyRelative("ThenRecipe"), CombatBook.RecipeNames(then));
                    var strike = gambit.FindPropertyRelative("ThenStrike");
                    var page = CombatBook.PageOf(then, CombatStrike.None);
                    if (page.Strike != CombatStrike.None)
                    {
                        strike.enumValueIndex = (int)page.Strike;
                    }
                }
            }

            EditorGUILayout.PropertyField(gambit.FindPropertyRelative("ThenRecipe"), new GUIContent("Then recipe"), true);
            EditorGUILayout.PropertyField(gambit.FindPropertyRelative("Once"));
            EditorGUILayout.EndVertical();
        }

        void DrawLegacyAttack()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fallback Attack", EditorStyles.boldLabel);
            var kindProp = serializedObject.FindProperty("authoredAttack");
            var attackProp = serializedObject.FindProperty("attack");
            var shown = (CombatKind)kindProp.enumValueIndex;
            if (shown == CombatKind.None)
            {
                shown = PackEnemies.KindOf(attackProp.stringValue);
            }

            EditorGUI.BeginChangeCheck();
            shown = (CombatKind)EditorGUILayout.EnumPopup(
                new GUIContent("Attack", "Used when Attacks is empty, and for slam / cast clips."),
                shown);
            if (EditorGUI.EndChangeCheck())
            {
                kindProp.enumValueIndex = (int)shown;
                attackProp.stringValue = PackEnemies.AttackName(shown);
            }

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("authoredCastSeconds"),
                new GUIContent("Cast seconds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cast"), new GUIContent("Cast recipe"), true);
            EditorGUILayout.HelpBox(
                shown == CombatKind.Golem
                    ? "Empty Attacks: slam in close range. Hop or Stoneskin survives it."
                    : shown == CombatKind.Wizard
                        ? "Empty Attacks: they write a long fireball. Empty Cast recipe is Fire · Mercury."
                        : shown == CombatKind.Archer
                            ? "Empty Attacks: they loose a long wood arrow."
                            : "Empty Attacks and Attack None: wander only, unless you add slots or gambits.",
                MessageType.None);
        }

        void AddSlot(CombatSlot slot)
        {
            var attacks = serializedObject.FindProperty("attacks");
            var at = attacks.arraySize;
            attacks.arraySize = at + 1;
            WriteSlot(attacks.GetArrayElementAtIndex(at), slot);
        }

        void AddGambit(CombatGambit gambit)
        {
            var list = serializedObject.FindProperty("gambits");
            var at = list.arraySize;
            list.arraySize = at + 1;
            var row = list.GetArrayElementAtIndex(at);
            row.FindPropertyRelative("Name").stringValue = gambit.Name;
            row.FindPropertyRelative("When").enumValueIndex = (int)gambit.When;
            row.FindPropertyRelative("WhenSpell").intValue = (int)gambit.WhenSpell;
            row.FindPropertyRelative("WhenStatus").enumValueIndex = (int)gambit.WhenStatus;
            row.FindPropertyRelative("ThenStrike").enumValueIndex = (int)gambit.ThenStrike;
            row.FindPropertyRelative("ThenSpell").intValue = (int)gambit.ThenSpell;
            WriteStrings(row.FindPropertyRelative("ThenRecipe"), gambit.ThenRecipe);
            row.FindPropertyRelative("Once").boolValue = gambit.Once;
        }

        static void WriteSlot(SerializedProperty row, CombatSlot slot)
        {
            row.FindPropertyRelative("Name").stringValue = slot.Name;
            row.FindPropertyRelative("Range").enumValueIndex = (int)slot.Range;
            row.FindPropertyRelative("Strike").enumValueIndex = (int)slot.Strike;
            row.FindPropertyRelative("Spell").intValue = (int)slot.Spell;
            WriteStrings(row.FindPropertyRelative("Recipe"), slot.Recipe);
            row.FindPropertyRelative("CastSeconds").floatValue = slot.CastSeconds;
        }

        static int PageIndex(SpellId spell, CombatStrike strike)
        {
            if (strike == CombatStrike.Slam)
            {
                return 1;
            }

            for (var i = 0; i < CombatBook.Pages.Length; i++)
            {
                if (CombatBook.Pages[i].Spell == spell && CombatBook.Pages[i].Strike != CombatStrike.Slam)
                {
                    return i;
                }
            }

            return 0;
        }

        static SpellId SpellPopup(string label, SpellId current)
        {
            var index = 0;
            for (var i = 0; i < SpellIds.Length; i++)
            {
                if (SpellIds[i] == current)
                {
                    index = i;
                    break;
                }
            }

            index = EditorGUILayout.Popup(label, index, SpellLabels);
            return CombatBook.Pages[index].Spell;
        }

        static void WriteStrings(SerializedProperty array, string[] values)
        {
            if (array == null || !array.isArray)
            {
                return;
            }

            var copy = values ?? System.Array.Empty<string>();
            array.arraySize = copy.Length;
            for (var i = 0; i < copy.Length; i++)
            {
                array.GetArrayElementAtIndex(i).stringValue = copy[i];
            }
        }

        static string[] ReadStrings(SerializedProperty array)
        {
            if (array == null || !array.isArray || array.arraySize == 0)
            {
                return System.Array.Empty<string>();
            }

            var names = new string[array.arraySize];
            for (var i = 0; i < array.arraySize; i++)
            {
                names[i] = array.GetArrayElementAtIndex(i).stringValue;
            }

            return names;
        }

        static string[] Labels()
        {
            var labels = new string[CombatBook.Pages.Length];
            for (var i = 0; i < CombatBook.Pages.Length; i++)
            {
                labels[i] = CombatBook.Pages[i].Name;
            }

            return labels;
        }

        static SpellId[] Ids()
        {
            var ids = new SpellId[CombatBook.Pages.Length];
            for (var i = 0; i < CombatBook.Pages.Length; i++)
            {
                ids[i] = CombatBook.Pages[i].Spell;
            }

            return ids;
        }
    }
}
#endif
