#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Wires ElvGames Enemy_* slices onto EncounterLock prefabs.
    /// A = idle, C = slam / cast, D = resolve.
    /// </summary>
    public static class EnemyArtBind
    {
        const int Row = 6;

        [MenuItem("Window/Rune Magic/Bind Enemy Sprites")]
        public static void MenuBind()
        {
            var count = BindPrefabs(onlyEmpty: true);
            EditorUtility.DisplayDialog(
                "Enemy sprites",
                count + " enemy prefabs took ElvGames slices.\n\n" +
                "A = idle, C = slam or cast, D = resolve.\n" +
                "Drag those same slices onto Portrait / Idle Frames / Attack Frames yourself if you want another facing.",
                "OK");
        }

        public static int BindPrefabs(bool onlyEmpty)
        {
            var changed = 0;
            for (var i = 0; i < PackEnemies.All.Length; i++)
            {
                var spec = PackEnemies.All[i];
                var prefab = LoadEnemyPrefab(spec.Name);
                if (prefab == null)
                {
                    continue;
                }

                var encounter = prefab.GetComponent<EncounterLock>();
                if (Apply(encounter, overwrite: !onlyEmpty, spec))
                {
                    EditorUtility.SetDirty(prefab);
                    changed++;
                }
            }

            AssetDatabase.SaveAssets();
            return changed;
        }

        public static void Apply(EncounterLock encounter, bool overwrite)
        {
            if (encounter == null)
            {
                return;
            }

            Apply(encounter, overwrite, Match(encounter));
            EditorUtility.SetDirty(encounter);
            if (encounter.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                EditorUtility.SetDirty(renderer);
            }
        }

        public static bool Apply(EncounterLock encounter, bool overwrite, PackEnemies.Spec spec)
        {
            if (encounter == null)
            {
                return false;
            }

            var so = new SerializedObject(encounter);
            var spriteId = so.FindProperty("spriteId").stringValue;
            if (string.IsNullOrWhiteSpace(spriteId) && spec != null)
            {
                spriteId = spec.SpriteId;
                so.FindProperty("spriteId").stringValue = spriteId;
            }

            var idle = LoadRow(PackEnemies.SheetPath(spriteId, 'A'));
            var attack = LoadRow(PackEnemies.SheetPath(spriteId, 'C'));
            var resolve = LoadRow(PackEnemies.SheetPath(spriteId, 'D'));
            var wrote = false;
            wrote |= WriteSprites(so.FindProperty("idleFrames"), idle, overwrite);
            wrote |= WriteSprites(so.FindProperty("attackFrames"), attack, overwrite);
            wrote |= WriteSprites(so.FindProperty("resolveFrames"), resolve, overwrite);

            var portrait = so.FindProperty("portrait");
            if (idle.Length > 0 && idle[0] != null && (overwrite || portrait.objectReferenceValue == null))
            {
                portrait.objectReferenceValue = idle[0];
                wrote = true;
            }

            var idleClip = so.FindProperty("idleClip");
            if (string.IsNullOrWhiteSpace(idleClip.stringValue) ||
                (overwrite && idleClip.stringValue != spriteId))
            {
                idleClip.stringValue = spriteId;
                wrote = true;
            }

            if (spec != null)
            {
                var kind = so.FindProperty("authoredAttack");
                if (overwrite || kind.enumValueIndex == (int)CombatKind.None)
                {
                    var named = PackEnemies.KindOf(spec.Attack);
                    if (named != CombatKind.None)
                    {
                        kind.enumValueIndex = (int)named;
                        so.FindProperty("attack").stringValue = spec.Attack;
                        wrote = true;
                    }
                }
            }

            if (!so.ApplyModifiedPropertiesWithoutUndo() && !wrote)
            {
                return false;
            }

            if (encounter.TryGetComponent<SpriteRenderer>(out var renderer) && portrait.objectReferenceValue is Sprite still)
            {
                renderer.sprite = still;
                renderer.sortingOrder = 12;
                renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            }

            return true;
        }

        static PackEnemies.Spec Match(EncounterLock encounter)
        {
            if (encounter == null)
            {
                return null;
            }

            var so = new SerializedObject(encounter);
            var id = so.FindProperty("authoredId").stringValue;
            var sprite = so.FindProperty("spriteId").stringValue;
            for (var i = 0; i < PackEnemies.All.Length; i++)
            {
                var spec = PackEnemies.All[i];
                if (spec.Id == id || spec.SpriteId == sprite || spec.Name == encounter.gameObject.name)
                {
                    return spec;
                }
            }

            return null;
        }

        static GameObject LoadEnemyPrefab(string name)
        {
            var path = "Assets/Prefabs/Enemies/" + name + ".prefab";
            var direct = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (direct != null)
            {
                return direct;
            }

            var guids = AssetDatabase.FindAssets(name + " t:Prefab", new[] { "Assets/Prefabs" });
            for (var i = 0; i < (guids != null ? guids.Length : 0); i++)
            {
                var asset = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (System.IO.Path.GetFileNameWithoutExtension(asset) == name)
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                }
            }

            return null;
        }

        static Sprite[] LoadRow(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return System.Array.Empty<Sprite>();
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var byIndex = new Dictionary<int, Sprite>();
            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not Sprite sprite)
                {
                    continue;
                }

                var index = PackEnemies.FrameIndex(sprite.name);
                if (index >= 0 && index < Row)
                {
                    byIndex[index] = sprite;
                }
            }

            var frames = new Sprite[Row];
            var any = false;
            for (var i = 0; i < Row; i++)
            {
                if (byIndex.TryGetValue(i, out var sprite))
                {
                    frames[i] = sprite;
                    any = true;
                }
            }

            return any ? frames : System.Array.Empty<Sprite>();
        }

        static bool WriteSprites(SerializedProperty property, Sprite[] frames, bool overwrite)
        {
            if (property == null || frames == null || frames.Length == 0)
            {
                return false;
            }

            if (!overwrite && property.arraySize > 0 && property.GetArrayElementAtIndex(0).objectReferenceValue != null)
            {
                return false;
            }

            property.arraySize = frames.Length;
            for (var i = 0; i < frames.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            return true;
        }
    }
}
#endif
