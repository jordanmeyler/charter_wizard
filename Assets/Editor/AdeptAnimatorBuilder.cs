#if UNITY_EDITOR
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Builds the conventional Hero_22 Animator: sprite clips plus a
    /// four-state controller. Open Window → Rune Magic → Adept Animator.
    /// </summary>
    public sealed class AdeptAnimatorBuilder : EditorWindow
    {
        public const string SheetPath = "Assets/ElvGames/Rogue Adventure/Characters/Hero_22.png";
        public const string ClipFolder = "Assets/Animations/Adept";
        public const string ControllerPath = "Assets/Resources/Animations/Adept.controller";

        const string Moving = "Moving";
        const string Casting = "Casting";
        const string Airborne = "Airborne";

        Vector2 _scroll;

        [MenuItem("Window/Rune Magic/Adept Animator")]
        public static void Open()
        {
            var window = GetWindow<AdeptAnimatorBuilder>("Adept Animator");
            window.minSize = new Vector2(420f, 360f);
            window.Show();
        }

        [MenuItem("Window/Rune Magic/Rebuild Adept Animator")]
        public static void RebuildMenu()
        {
            Rebuild();
            Open();
        }

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.HelpBox(
                "The adept uses a normal Unity Animator. Each state needs a Motion clip that " +
                "animates Sprite Renderer → Sprite.\n\n" +
                "1. Click Build / repair clips below.\n" +
                "2. Open the controller and click Idle, Walk, Cast, Hop — each Motion field should show a clip, not None.\n" +
                "3. If a Motion is None, drag the matching clip from Assets/Animations/Adept onto that state.\n" +
                "4. Press Play. Do not place an Adept in the scene; Play still spawns one.\n\n" +
                "Sprites are 16×16 at 16 PPU, same as the dungeon tiles. " +
                "Do not re-slice Hero_22 as a 16×16 grid — that cuts each pose in four. " +
                "Build / repair crops the 16×16 character out of each 32×32 pack cell.",
                MessageType.Info);

            DrawStatus();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build / repair clips and controller", GUILayout.Height(36)))
            {
                Rebuild();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Select controller (Animator window)"))
            {
                Ping(ControllerPath);
                EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
            }

            if (GUILayout.Button("Select Hero_22 sheet (Sprite Editor)"))
            {
                Ping(SheetPath);
            }

            if (GUILayout.Button("Open Animation window"))
            {
                EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Clip → state", EditorStyles.boldLabel);
            DrawClipRow("Idle", "Adept-Idle", 0, 6);
            DrawClipRow("Walk", "Adept-Walk", 6, 6);
            DrawClipRow("Cast", "Adept-Cast", 30, 6);
            DrawClipRow("Hop", "Adept-Hop", 60, 4);
            EditorGUILayout.EndScrollView();
        }

        void DrawStatus()
        {
            var sprites = LoadSprites();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            var clipsReady = ClipHasSprites("Adept-Idle") && ClipHasSprites("Adept-Walk")
                && ClipHasSprites("Adept-Cast") && ClipHasSprites("Adept-Hop");
            if (sprites.Length < 64)
            {
                EditorGUILayout.HelpBox(
                    "Hero_22 is missing pose slices. Click Build / repair — it crops each pose to 16×16, same as the tiles.",
                    MessageType.Error);
                return;
            }

            if (controller == null || !clipsReady)
            {
                EditorGUILayout.HelpBox(
                    "Clips are empty or the controller is missing. Click Build / repair clips.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox(
                "Clips have sprite keys and the controller is present. If Play still shows no character, " +
                "click a state and confirm Motion is assigned.",
                MessageType.Info);
        }

        void DrawClipRow(string state, string clipName, int start, int count)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{ClipFolder}/{clipName}.anim");
            var ready = ClipHasSprites(clipName);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{state}  ←  {clipName}  (frames {start}–{start + count - 1})",
                ready ? EditorStyles.label : EditorStyles.miniLabel);
            if (clip != null && GUILayout.Button("Select", GUILayout.Width(64)))
            {
                Selection.activeObject = clip;
                EditorGUIUtility.PingObject(clip);
            }

            EditorGUILayout.EndHorizontal();
        }

        [InitializeOnLoadMethod]
        static void EnsureController()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                if (AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath) == null)
                {
                    return;
                }

                var sprites = LoadSprites();
                var oversized = sprites.Length > 0 && sprites[0].rect.width > 16.5f;
                var missing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) == null
                    || !ClipHasSprites("Adept-Idle")
                    || !ClipHasSprites("Adept-Walk")
                    || !ClipHasSprites("Adept-Cast")
                    || !ClipHasSprites("Adept-Hop")
                    || oversized;
                if (missing)
                {
                    Rebuild();
                }
            };
        }

        public static void Rebuild()
        {
            CropSheetToTiles();
            var sprites = LoadSprites();
            if (sprites.Length < 64)
            {
                Debug.LogError(
                    "Hero_22 is missing pose slices. Reimport the texture, then click Build / repair.");
                return;
            }

            EnsureFolders();
            var idle = WriteClip("Adept-Idle", Slice(sprites, 0, 6), 5f, true);
            var walk = WriteClip("Adept-Walk", Slice(sprites, 6, 6), 8f, true);
            var cast = WriteClip("Adept-Cast", Slice(sprites, 30, 6), 4f, true);
            var hop = WriteClip("Adept-Hop", Slice(sprites, 60, 4), 7f, true);
            WriteController(idle, walk, cast, hop);
            AssignPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller != null)
            {
                EditorGUIUtility.PingObject(controller);
                Selection.activeObject = controller;
            }

            Debug.Log("Adept Animator rebuilt from Hero_22. Idle / Walk / Cast / Hop now have sprite keys.");
        }

        static bool ClipHasSprites(string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{ClipFolder}/{name}.anim");
            if (clip == null)
            {
                return false;
            }

            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            for (var i = 0; i < bindings.Length; i++)
            {
                var keys = AnimationUtility.GetObjectReferenceCurve(clip, bindings[i]);
                if (keys != null && keys.Length > 0 && keys[0].value != null)
                {
                    return true;
                }
            }

            return false;
        }

        static void CropSheetToTiles()
        {
            var importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            var sheet = importer.spritesheet;
            if (sheet == null || sheet.Length == 0)
            {
                return;
            }

            var changed = false;
            for (var i = 0; i < sheet.Length; i++)
            {
                var data = sheet[i];
                var rect = data.rect;
                if (Mathf.Approximately(rect.width, 32f) && Mathf.Approximately(rect.height, 32f))
                {
                    data.rect = new Rect(rect.x + 8f, rect.y, 16f, 16f);
                    changed = true;
                }

                data.alignment = SpriteAlignment.Custom;
                data.pivot = new Vector2(0.5f, 0.18f);
                sheet[i] = data;
            }

            importer.spritesheet = sheet;
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        static Sprite[] LoadSprites()
        {
            return AssetDatabase.LoadAllAssetsAtPath(SheetPath)
                .OfType<Sprite>()
                .OrderBy(sprite => IndexOf(sprite.name))
                .ToArray();
        }

        static int IndexOf(string name)
        {
            var match = Regex.Match(name ?? string.Empty, @"(\d+)$");
            return match.Success && int.TryParse(match.Groups[1].Value, out var index) ? index : int.MaxValue;
        }

        static Sprite[] Slice(Sprite[] sprites, int start, int count)
        {
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
            {
                var index = start + i;
                frames[i] = index < sprites.Length ? sprites[index] : sprites[sprites.Length - 1];
            }

            return frames;
        }

        static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            {
                AssetDatabase.CreateFolder("Assets", "Animations");
            }

            if (!AssetDatabase.IsValidFolder(ClipFolder))
            {
                AssetDatabase.CreateFolder("Assets/Animations", "Adept");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Animations"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Animations");
            }
        }

        static AnimationClip WriteClip(string name, Sprite[] frames, float fps, bool loop)
        {
            var path = $"{ClipFolder}/{name}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.name = name;
            clip.frameRate = fps;
            var binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[frames.Length];
            for (var i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / Mathf.Max(1f, fps),
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        static void WriteController(AnimationClip idle, AnimationClip walk, AnimationClip cast, AnimationClip hop)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            foreach (var leftover in controller.parameters.ToArray())
            {
                controller.RemoveParameter(leftover);
            }

            controller.AddParameter(Moving, AnimatorControllerParameterType.Bool);
            controller.AddParameter(Casting, AnimatorControllerParameterType.Bool);
            controller.AddParameter(Airborne, AnimatorControllerParameterType.Bool);

            var machine = controller.layers[0].stateMachine;
            foreach (var old in machine.states.ToArray())
            {
                machine.RemoveState(old.state);
            }

            foreach (var old in machine.anyStateTransitions.ToArray())
            {
                machine.RemoveAnyStateTransition(old);
            }

            var idleState = machine.AddState("Idle", new Vector3(320f, 0f, 0f));
            idleState.motion = idle;
            var walkState = machine.AddState("Walk", new Vector3(320f, 80f, 0f));
            walkState.motion = walk;
            var castState = machine.AddState("Cast", new Vector3(320f, 160f, 0f));
            castState.motion = cast;
            var hopState = machine.AddState("Hop", new Vector3(320f, 240f, 0f));
            hopState.motion = hop;
            machine.defaultState = idleState;

            Any(machine, castState, (Casting, true));
            Any(machine, hopState, (Casting, false), (Airborne, true));
            Any(machine, walkState, (Casting, false), (Airborne, false), (Moving, true));
            Any(machine, idleState, (Casting, false), (Airborne, false), (Moving, false));
            EditorUtility.SetDirty(controller);
        }

        static void AssignPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Adept.prefab");
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            var sprites = LoadSprites();
            if (prefab == null || controller == null || sprites.Length == 0)
            {
                return;
            }

            var animator = prefab.GetComponent<Animator>() ?? prefab.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.applyRootMotion = false;
            var sprite = prefab.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.sprite = sprites[0];
                sprite.sortingOrder = 20;
            }

            EditorUtility.SetDirty(prefab);
        }

        static void Any(AnimatorStateMachine machine, AnimatorState dest, params (string name, bool value)[] conditions)
        {
            var transition = machine.AddAnyStateTransition(dest);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0f;
            transition.canTransitionToSelf = false;
            for (var i = 0; i < conditions.Length; i++)
            {
                var condition = conditions[i];
                transition.AddCondition(
                    condition.value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                    0f,
                    condition.name);
            }
        }

        static void Ping(string assetPath)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Adept Animator", "Missing " + assetPath, "OK");
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
#endif
