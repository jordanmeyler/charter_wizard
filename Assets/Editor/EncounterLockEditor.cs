#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RuneMagic
{
    [CustomEditor(typeof(EncounterLock))]
    public sealed class EncounterLockEditor : Editor
    {
        const string EnemyPrefabFolder = "Assets/Prefabs/Enemies";

        static readonly string[] SpellLabels = Labels();
        static readonly SpellId[] SpellIds = Ids();
        int _packIndex;
        bool _rawAffinities;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Every enemy is a lock. Dress the look, then give them a mind.\n\n" +
                "Attacks — what they do on their own. Add slam, a shot, a pillar, or a wall. Pick Custom and write the runes yourself (Fire · Mercury is a fireball).\n\n" +
                "Gambits — if / then. First match wins. “If they raise a wall, then write flame-pillar” is the Mixed Court lesson. Add that row on any caster.\n\n" +
                "Enemies do not use a Unity Animator. Drag idle / attack slices onto the frames below. The adept is the one with Animator.\n\n" +
                "Resistances are sliders — 0 immune, 1 normal, 5 ruin-weak. Empty rows use Nature. Tweaking a slider saves an override. Customize, then Save as prefab; later drag from Assets/Prefabs/Enemies.\n\n" +
                "See ENEMIES.md.",
                MessageType.Info);

            DrawLock();
            DrawLook();
            DrawNature();
            DrawMind();
            DrawAttacks();
            DrawGambits();
            DrawLegacyAttack();
            DrawSavePrefab();

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
            EditorGUILayout.HelpBox(
                "Not a Unity Animator. Drag ElvGames slices onto Portrait / Idle / Attack. A is idle, C is slam or write, D is the unmake. The adept is the one with an Animator Controller.",
                MessageType.None);
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
            EditorGUILayout.HelpBox(
                "0 immune, 1 normal, 5 ruin-weak. Empty rows use Nature. Tweaking a slider saves an override. A stone golem is defense 4 and shrugs a fireball; set Fire to 0 if it should also ignore hunger.",
                MessageType.None);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("authoredNature"),
                new GUIContent("Nature", "Auto reads the Id. Golem is earth. Warden is mind if Ensouled."));
            if (serializedObject.ApplyModifiedProperties())
            {
                serializedObject.Update();
            }

            var encounter = (EncounterLock)target;
            var nature = encounter.PreviewNature();
            var natureRow = AffinityProfile.Of(nature);
            var profile = encounter.PreviewProfile();
            EditorGUILayout.LabelField("Reads as", nature.ToString());

            DrawDefenseSlider(encounter, profile, natureRow);
            DrawPushSlider(encounter, profile, natureRow);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Strike affinities", EditorStyles.miniBoldLabel);
            for (var i = 0; i < CombatBook.TunableStrikes.Length; i++)
            {
                var kind = CombatBook.TunableStrikes[i];
                DrawAffinitySlider(
                    kind.ToString(),
                    profile.Strike(kind),
                    next => MutateLock("Enemy strike affinity", lockOn => lockOn.SetStrikeAffinity(kind, next)));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status affinities", EditorStyles.miniBoldLabel);
            for (var i = 0; i < CombatBook.TunableStatuses.Length; i++)
            {
                var id = CombatBook.TunableStatuses[i];
                DrawAffinitySlider(
                    id.ToString(),
                    profile.Status(id),
                    next => MutateLock("Enemy status affinity", lockOn => lockOn.SetStatusAffinity(id, next)));
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load nature defaults"))
            {
                MutateLock("Load nature defaults", lockOn => lockOn.LoadNatureDefaults());
            }

            if (GUILayout.Button("Reset to nature"))
            {
                MutateLock("Reset to nature", lockOn => lockOn.ClearAffinityOverrides());
            }

            EditorGUILayout.EndHorizontal();

            _rawAffinities = EditorGUILayout.Foldout(_rawAffinities, "Raw affinity arrays", true);
            if (_rawAffinities)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customDefense"), new GUIContent("Override defense"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredDefense"), new GUIContent("Authored defense"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customPush"), new GUIContent("Override push resist"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("authoredPushResist"), new GUIContent("Authored push resist"));
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("strikeAffinities"),
                    new GUIContent("Strike affinities"),
                    true);
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("statusAffinities"),
                    new GUIContent("Status affinities"),
                    true);
            }
        }

        void DrawDefenseSlider(EncounterLock encounter, AffinityProfile profile, AffinityProfile natureRow)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var next = EditorGUILayout.IntSlider(
                new GUIContent("Defense", "0–10. Power × affinity must beat this."),
                profile.Defense,
                StrikeLaw.DefenseMin,
                StrikeLaw.DefenseMax);
            if (EditorGUI.EndChangeCheck() && next != profile.Defense)
            {
                MutateLock("Enemy defense", lockOn => lockOn.SetDefense(next));
            }

            EditorGUI.BeginDisabledGroup(!encounter.HasDefenseOverride);
            if (GUILayout.Button("Use nature", GUILayout.Width(88)))
            {
                MutateLock("Use nature defense", lockOn => lockOn.ClearDefenseOverride());
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            if (encounter.HasDefenseOverride)
            {
                EditorGUILayout.LabelField("Nature defense", natureRow.Defense.ToString(), EditorStyles.miniLabel);
            }
        }

        void DrawPushSlider(EncounterLock encounter, AffinityProfile profile, AffinityProfile natureRow)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var next = EditorGUILayout.IntSlider(
                new GUIContent("Push resist", "0–6. How hard a shove must be."),
                profile.PushResist,
                0,
                6);
            if (EditorGUI.EndChangeCheck() && next != profile.PushResist)
            {
                MutateLock("Enemy push resist", lockOn => lockOn.SetPushResist(next));
            }

            EditorGUI.BeginDisabledGroup(!encounter.HasPushOverride);
            if (GUILayout.Button("Use nature", GUILayout.Width(88)))
            {
                MutateLock("Use nature push", lockOn => lockOn.ClearPushOverride());
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            if (encounter.HasPushOverride)
            {
                EditorGUILayout.LabelField("Nature push resist", natureRow.PushResist.ToString(), EditorStyles.miniLabel);
            }
        }

        void DrawAffinitySlider(string label, int current, System.Action<int> set)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var next = EditorGUILayout.IntSlider(new GUIContent(label), current, StrikeLaw.AffinityImmune, StrikeLaw.AffinityMax);
            EditorGUILayout.LabelField(StrikeLaw.AffinityWord(next), GUILayout.Width(72));
            if (EditorGUI.EndChangeCheck() && next != current)
            {
                set(next);
            }

            EditorGUILayout.EndHorizontal();
        }

        void MutateLock(string undo, System.Action<EncounterLock> change)
        {
            serializedObject.ApplyModifiedProperties();
            var encounter = (EncounterLock)target;
            Undo.RecordObject(encounter, undo);
            change(encounter);
            PrefabUtility.RecordPrefabInstancePropertyModifications(encounter);
            EditorUtility.SetDirty(encounter);
            serializedObject.Update();
        }

        void DrawSavePrefab()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Prefab", EditorStyles.boldLabel);
            var encounter = (EncounterLock)target;
            var fileName = PrefabFileName(encounter);
            var path = EnemyPrefabFolder + "/" + fileName + ".prefab";
            EditorGUILayout.HelpBox(
                "Customize this body, then save it. Later drag it from Assets/Prefabs/Enemies, or Place it under Saved enemies in Authoring. The file name follows Name — change Name first for a new enemy.",
                MessageType.None);
            EditorGUILayout.LabelField("Writes", path);
            if (GUILayout.Button("Save as prefab"))
            {
                serializedObject.ApplyModifiedProperties();
                SaveEnemyPrefab((EncounterLock)target);
                serializedObject.Update();
            }
        }

        static string PrefabFileName(EncounterLock encounter)
        {
            var stem = encounter != null ? (encounter.AuthoredName ?? string.Empty).Trim() : string.Empty;
            if (string.IsNullOrEmpty(stem) && encounter != null)
            {
                stem = (encounter.AuthoredId ?? string.Empty).Trim();
            }

            if (string.IsNullOrEmpty(stem) && encounter != null)
            {
                stem = encounter.gameObject.name;
            }

            return SanitizeFileName(stem);
        }

        static string SanitizeFileName(string name)
        {
            var raw = (name ?? string.Empty).Trim();
            var chars = Path.GetInvalidFileNameChars();
            var buffer = new char[raw.Length];
            var count = 0;
            for (var i = 0; i < raw.Length; i++)
            {
                var c = raw[i];
                var bad = false;
                for (var j = 0; j < chars.Length; j++)
                {
                    if (c == chars[j])
                    {
                        bad = true;
                        break;
                    }
                }

                buffer[count++] = bad ? '-' : c;
            }

            var file = count == 0 ? string.Empty : new string(buffer, 0, count).Trim();
            return string.IsNullOrEmpty(file) ? "Custom" : file;
        }

        static bool IsPackPrefab(string fileName)
        {
            if (string.Equals(fileName, "Custom", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            for (var i = 0; i < PackEnemies.All.Length; i++)
            {
                if (string.Equals(PackEnemies.All[i].Name, fileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        static bool ConfirmWrite(string path, string fileName)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                return true;
            }

            var body = IsPackPrefab(fileName)
                ? "Overwrite the pack prefab '" + fileName + "'? Rooms that already use it will take this body. Change Name first if you want a new enemy."
                : "Overwrite " + fileName + ".prefab?";
            return EditorUtility.DisplayDialog("Save enemy prefab", body, "Overwrite", "Cancel");
        }

        static void EnsureEnemyFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(EnemyPrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");
            }
        }

        static void PingPrefab(string path)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
            }
        }

        static void SaveEnemyPrefab(EncounterLock encounter)
        {
            if (encounter == null)
            {
                return;
            }

            var go = encounter.gameObject;
            var fileName = PrefabFileName(encounter);
            var path = EnemyPrefabFolder + "/" + fileName + ".prefab";
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.prefabContentsRoot != null
                && go.transform.root == stage.prefabContentsRoot.transform)
            {
                if (string.Equals(stage.assetPath, path, System.StringComparison.OrdinalIgnoreCase))
                {
                    EditorUtility.SetDirty(go);
                    PrefabUtility.SavePrefabAsset(stage.prefabContentsRoot);
                    PingPrefab(path);
                    return;
                }

                EnsureEnemyFolder();
                if (!ConfirmWrite(path, fileName))
                {
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, path);
                PingPrefab(path);
                return;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(go) && !PrefabUtility.IsPartOfNonAssetPrefabInstance(go))
            {
                var assetPath = AssetDatabase.GetAssetPath(go);
                if (string.Equals(assetPath, path, System.StringComparison.OrdinalIgnoreCase))
                {
                    EditorUtility.SetDirty(go);
                    AssetDatabase.SaveAssets();
                    PingPrefab(path);
                    return;
                }

                EnsureEnemyFolder();
                if (!ConfirmWrite(path, fileName))
                {
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(go, path);
                PingPrefab(path);
                return;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                if (!PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    EditorUtility.DisplayDialog(
                        "Save enemy prefab",
                        "This lock sits inside another prefab. Open the enemy prefab, or unpack this instance, then save.",
                        "OK");
                    return;
                }

                var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
                var sourcePath = source != null ? AssetDatabase.GetAssetPath(source) : string.Empty;
                if (string.Equals(sourcePath, path, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (IsPackPrefab(fileName)
                        && !EditorUtility.DisplayDialog(
                            "Save enemy prefab",
                            "Apply these tweaks back to " + fileName + ".prefab? Every instance of that pack body will change.",
                            "Apply",
                            "Cancel"))
                    {
                        return;
                    }

                    PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
                    PingPrefab(path);
                    return;
                }

                EnsureEnemyFolder();
                if (!ConfirmWrite(path, fileName))
                {
                    return;
                }

                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
            }
            else
            {
                EnsureEnemyFolder();
                if (!ConfirmWrite(path, fileName))
                {
                    return;
                }
            }

            go.name = fileName;
            var saved = PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.UserAction);
            if (saved == null)
            {
                EditorUtility.DisplayDialog("Save enemy prefab", "Unity could not write " + path, "OK");
                return;
            }

            PingPrefab(path);
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
            EditorGUILayout.LabelField("Attacks — what they do on their own", EditorStyles.boldLabel);
            var attacks = serializedObject.FindProperty("attacks");
            EditorGUILayout.HelpBox(
                "Each slot is one strike. Close slams. Mid / Long write a sentence (shot, pillar, or wall).\n" +
                "Pick a named attack to fill the runes. Custom keeps Spell empty so you can write any sentence.\n" +
                "A golem with slam + a mid wall will hold the tile, then stand earth if you stay at mid.",
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

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add flame-pillar"))
            {
                AddSlot(CombatBook.SlotFromPage(CombatBook.PageOf(SpellId.FlamePillar, CombatStrike.Pillar)));
            }

            if (GUILayout.Button("Add wall"))
            {
                AddSlot(CombatBook.WallSlot());
            }

            if (GUILayout.Button("Add custom (write runes)"))
            {
                AddSlot(CombatBook.CustomSlot());
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
                strikeProp.enumValueIndex = page.Strike != CombatStrike.None
                    ? (int)page.Strike
                    : (int)CombatStrike.Shot;
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

            EditorGUILayout.PropertyField(strikeProp, new GUIContent("Strike", "Slam, Shot, or Pillar. Pillar also covers walls."));

            EditorGUILayout.PropertyField(slot.FindPropertyRelative("Recipe"), new GUIContent("Recipe"), true);
            var runes = CombatBook.ParseRecipe(ReadStrings(slot.FindPropertyRelative("Recipe")));
            if (runes.Length > 0)
            {
                var matched = CombatBook.SpellFromRecipe(runes);
                var phrase = WorkingNames.RunePhrase(runes);
                EditorGUILayout.LabelField(
                    matched != SpellId.None
                        ? phrase + "  →  " + CombatBook.NameOf(matched, currentStrike)
                        : phrase + "  (custom — set Strike to Slam, Shot, or Pillar)",
                    EditorStyles.miniLabel);
            }
            else if (currentStrike != CombatStrike.Slam)
            {
                EditorGUILayout.LabelField("Write the runes (Fire, Mercury) or pick a named attack above.", EditorStyles.miniLabel);
            }

            EditorGUILayout.PropertyField(slot.FindPropertyRelative("CastSeconds"));
            EditorGUILayout.EndVertical();
        }

        void DrawGambits()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gambits — if / then", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "If the player does this, then they write that. First matching row wins.\n" +
                "If they raise a wall → flame-pillar is the Mixed Court answer. Add it here if you want it on this body everywhere, not only in that room.\n" +
                "Then spell fills the runes. Leave Then spell on None and write Then recipe for a custom sentence.",
                MessageType.None);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("If they raise a wall → flame-pillar"))
            {
                AddGambit(CombatBook.WallToFlamePillar());
            }

            if (GUILayout.Button("If they raise a wall → wall"))
            {
                AddGambit(CombatBook.WallToWall());
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("If close → slam"))
            {
                AddGambit(CombatBook.CloseSlam());
            }

            if (GUILayout.Button("Add empty if / then"))
            {
                AddGambit(CombatBook.EmptyGambit());
            }

            EditorGUILayout.EndHorizontal();

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

            EditorGUILayout.PropertyField(gambit.FindPropertyRelative("ThenStrike"), new GUIContent("Then strike"));
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

            EditorGUILayout.PropertyField(gambit.FindPropertyRelative("ThenRecipe"), new GUIContent("Then recipe (custom if Then spell is None)"), true);
            var thenRunes = CombatBook.ParseRecipe(ReadStrings(gambit.FindPropertyRelative("ThenRecipe")));
            if (thenRunes.Length > 0)
            {
                EditorGUILayout.LabelField(WorkingNames.RunePhrase(thenRunes), EditorStyles.miniLabel);
            }
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
