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
    /// four-state controller. Safe to run again after you re-slice the sheet.
    /// </summary>
    public static class AdeptAnimatorBuilder
    {
        public const string SheetPath = "Assets/ElvGames/Rogue Adventure/Characters/Hero_22.png";
        public const string ClipFolder = "Assets/Animations/Adept";
        public const string ControllerPath = "Assets/Resources/Animations/Adept.controller";

        const string Moving = "Moving";
        const string Casting = "Casting";
        const string Airborne = "Airborne";

        [MenuItem("Window/Rune Magic/Rebuild Adept Animator")]
        public static void Rebuild()
        {
            var sprites = LoadSprites();
            if (sprites.Length < 64)
            {
                Debug.LogError(
                    "Hero_22 needs its 32×32 slices. Select the texture, Sprite Editor, " +
                    "Slice → Grid By Cell Size 32×32, then Apply.");
                return;
            }

            EnsureFolders();
            var idle = WriteClip("Adept-Idle", Slice(sprites, 0, 6), 5f, true);
            var walk = WriteClip("Adept-Walk", Slice(sprites, 6, 6), 8f, true);
            var cast = WriteClip("Adept-Cast", Slice(sprites, 30, 6), 4f, true);
            var hop = WriteClip("Adept-Hop", Slice(sprites, 60, 4), 7f, true);
            WriteController(idle, walk, cast, hop);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller != null)
            {
                EditorGUIUtility.PingObject(controller);
            }

            Debug.Log("Adept Animator rebuilt from Hero_22.");
        }

        [InitializeOnLoadMethod]
        static void EnsureController()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) != null)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath) == null)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) == null)
                {
                    Rebuild();
                }
            };
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
    }
}
#endif
