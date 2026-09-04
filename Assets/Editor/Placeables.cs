#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// GameObject → Rune Magic menu items. Places authored objects
    /// without going through the JSON map file.
    /// </summary>
    public static class Placeables
    {
        [MenuItem("GameObject/Rune Magic/Stones/Fire Stone", false, 20)]
        static void FireStone() => Place("Fire Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Water Stone", false, 21)]
        static void WaterStone() => Place("Water Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Earth Stone", false, 22)]
        static void EarthStone() => Place("Earth Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Air Stone", false, 23)]
        static void AirStone() => Place("Air Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Body Stone", false, 24)]
        static void BodyStone() => Place("Body Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Spirit Stone", false, 25)]
        static void SpiritStone() => Place("Spirit Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Mind Stone", false, 26)]
        static void MindStone() => Place("Mind Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Grove Stone", false, 27)]
        static void GroveStone() => Place("Grove Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Flood Stone", false, 28)]
        static void FloodStone() => Place("Flood Stone");

        [MenuItem("GameObject/Rune Magic/Stones/Spark Stone", false, 29)]
        static void SparkStone() => Place("Spark Stone");

        [MenuItem("GameObject/Rune Magic/Item", false, 30)]
        static void Item() => Spawn("Item", typeof(WorldItem));

        [MenuItem("GameObject/Rune Magic/Decor", false, 21)]
        static void Decor() => Spawn("Decor", typeof(WorldDecor));

        [MenuItem("GameObject/Rune Magic/Mite", false, 22)]
        static void Mite() => Spawn("Mite", typeof(EncounterLock));

        [MenuItem("GameObject/Rune Magic/Enemy", false, 22)]
        static void Enemy() => SpawnEnemy(PackEnemies.All[0]);

        [MenuItem("GameObject/Rune Magic/Enemies/Custom", false, 39)]
        static void EnemyCustom() => SpawnCustom();

        [MenuItem("GameObject/Rune Magic/Enemies/Shade", false, 40)]
        static void EnemyShade() => SpawnEnemy(PackEnemies.All[0]);

        [MenuItem("GameObject/Rune Magic/Enemies/Squire", false, 41)]
        static void EnemySquire() => SpawnEnemy(PackEnemies.All[1]);

        [MenuItem("GameObject/Rune Magic/Enemies/Crawler", false, 42)]
        static void EnemyCrawler() => SpawnEnemy(PackEnemies.All[2]);

        [MenuItem("GameObject/Rune Magic/Enemies/Wisp", false, 43)]
        static void EnemyWisp() => SpawnEnemy(PackEnemies.All[3]);

        [MenuItem("GameObject/Rune Magic/Enemies/Brute", false, 44)]
        static void EnemyBrute() => SpawnEnemy(PackEnemies.All[4]);

        [MenuItem("GameObject/Rune Magic/Enemies/Imp", false, 45)]
        static void EnemyImp() => SpawnEnemy(PackEnemies.All[5]);

        [MenuItem("GameObject/Rune Magic/Enemies/Skeleton", false, 46)]
        static void EnemySkeleton() => SpawnEnemy(PackEnemies.All[6]);

        [MenuItem("GameObject/Rune Magic/Enemies/Cultist", false, 47)]
        static void EnemyCultist() => SpawnEnemy(PackEnemies.All[7]);

        [MenuItem("GameObject/Rune Magic/Enemies/Bat", false, 48)]
        static void EnemyBat() => SpawnEnemy(PackEnemies.All[8]);

        [MenuItem("GameObject/Rune Magic/Enemies/Serpent", false, 49)]
        static void EnemySerpent() => SpawnEnemy(PackEnemies.All[9]);

        [MenuItem("GameObject/Rune Magic/Enemies/Golem", false, 50)]
        static void EnemyGolem() => SpawnEnemy(PackEnemies.All[10]);

        [MenuItem("GameObject/Rune Magic/Enemies/Stone Golem", false, 50)]
        static void EnemyStoneGolem() => SpawnEnemy(PackEnemies.All[10]);

        [MenuItem("GameObject/Rune Magic/Enemies/Warden", false, 51)]
        static void EnemyWarden() => SpawnEnemy(PackEnemies.All[11]);

        [MenuItem("GameObject/Rune Magic/Torch", false, 23)]
        static void Torch() => Spawn("Torch", typeof(TorchFixture));

        [MenuItem("GameObject/Rune Magic/Rod", false, 24)]
        static void Rod() => Spawn("Rod", typeof(LightningConduit));

        [MenuItem("GameObject/Rune Magic/Gate", false, 25)]
        static void Gate() => Spawn("Gate", typeof(SocketGate));

        [MenuItem("GameObject/Rune Magic/Electric Gate", false, 25)]
        static void ElectricGate() => Spawn("Electric Gate", typeof(ChargeGate));

        [MenuItem("GameObject/Rune Magic/Door", false, 25)]
        static void Door() => Spawn("Door", typeof(WorldDoor));

        [MenuItem("GameObject/Rune Magic/Barrier", false, 26)]
        static void Barrier() => Spawn("Barrier", typeof(BarrierLock));

        [MenuItem("GameObject/Rune Magic/Plaque", false, 27)]
        static void Plaque() => Spawn("Plaque", typeof(HintPlaque));

        [MenuItem("GameObject/Rune Magic/Speech", false, 27)]
        static void Speech() => SpawnSpeech("Speech", SpeechCue.Approach, "Read", true, true, string.Empty);

        [MenuItem("GameObject/Rune Magic/Sign", false, 27)]
        static void Sign() => SpawnSpeech("Sign", SpeechCue.Interact, "Read", true, false, "plaque");

        [MenuItem("GameObject/Rune Magic/Talk", false, 27)]
        static void Talk() => SpawnSpeech("Talk", SpeechCue.Interact, "Talk", true, false, string.Empty);

        [MenuItem("GameObject/Rune Magic/Altar", false, 27)]
        static void Altar() => SpawnAltar();

        [MenuItem("GameObject/Rune Magic/Crystal", false, 28)]
        static void Crystal() => Spawn("Crystal", typeof(SpawnCrystal));

        [MenuItem("GameObject/Rune Magic/Charm", false, 29)]
        static void Charm() => Spawn("Charm", typeof(FreeCharm));

        [MenuItem("GameObject/Rune Magic/Rune", false, 30)]
        static void Rune() => Spawn("Rune", typeof(RuneStringSource));

        [MenuItem("GameObject/Rune Magic/Inscription", false, 31)]
        static void Inscription() => SpawnStele(RuneStele.Kind.Floor);

        [MenuItem("GameObject/Rune Magic/Pillar", false, 32)]
        static void Pillar() => SpawnStele(RuneStele.Kind.Pillar);

        [MenuItem("GameObject/Rune Magic/Inscriptions Palette", false, 31)]
        static void InscriptionPalette() => RunePlaceWindow.Open();

        [MenuItem("GameObject/Rune Magic/Arrows", false, 33)]
        static void Arrows() => Spawn("Arrows", typeof(ArrowVolley));

        [MenuItem("GameObject/Rune Magic/Chasm", false, 34)]
        static void Chasm() => Spawn("Chasm", typeof(PitChasm));

        [MenuItem("GameObject/Rune Magic/Fog", false, 35)]
        static void Fog() => Spawn("Fog", typeof(RoomFog));

        [MenuItem("GameObject/Rune Magic/Flame Hall", false, 36)]
        static void Hall() => Spawn("Flame Hall", typeof(FlameHall));

        static void SpawnCustom()
        {
            if (AuthoringWindow.TryPlace("Custom"))
            {
                return;
            }

            var world = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            var host = new GameObject("Custom");
            host.transform.position = world;
            var encounter = host.AddComponent<EncounterLock>();
            encounter.AuthorCustom();
            if (host.GetComponent<SpriteRenderer>() == null)
            {
                host.AddComponent<SpriteRenderer>();
            }

            WorldYSort.On(host);
            Undo.RegisterCreatedObjectUndo(host, "Create Custom");
            Selection.activeGameObject = host;
        }

        static void SpawnEnemy(PackEnemies.Spec spec)
        {
            if (AuthoringWindow.TryPlace(spec.Name))
            {
                return;
            }

            var world = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            var encounter = PackEnemies.Spawn(spec, world);
            Undo.RegisterCreatedObjectUndo(encounter.gameObject, "Create " + spec.Name);
            Selection.activeGameObject = encounter.gameObject;
        }

        static void SpawnStele(RuneStele.Kind form)
        {
            var host = new GameObject(RuneStele.NameOf(RuneId.Fire, form));
            host.transform.position = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            var stele = host.AddComponent<RuneStele>();
            stele.Author(RuneId.Fire, form);
            Undo.RegisterCreatedObjectUndo(host, "Create " + host.name);
            Selection.activeGameObject = host;
        }

        static void Place(string name)
        {
            if (!AuthoringWindow.TryPlace(name))
            {
                Spawn(name, typeof(WorldItem));
            }
        }

        static void SpawnAltar()
        {
            if (AuthoringWindow.TryPlace("Altar"))
            {
                return;
            }

            var host = new GameObject("Altar");
            host.AddComponent<WorldAltar>();
            host.transform.position = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            Undo.RegisterCreatedObjectUndo(host, "Create Altar");
            Selection.activeGameObject = host;
        }

        static void SpawnSpeech(
            string name,
            SpeechCue cue,
            string verb,
            bool approachOnce,
            bool hideLook,
            string spriteId)
        {
            if (AuthoringWindow.TryPlace(name))
            {
                return;
            }

            var host = new GameObject(name);
            var speech = host.AddComponent<WorldSpeech>();
            speech.Author(cue, verb, "The way is shut.", approachOnce, false, hideLook, spriteId);
            if (!hideLook && host.GetComponent<SpriteRenderer>() == null)
            {
                host.AddComponent<SpriteRenderer>();
            }

            host.transform.position = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            Undo.RegisterCreatedObjectUndo(host, "Create " + name);
            Selection.activeGameObject = host;
        }

        static void Spawn(string name, System.Type type)
        {
            if (AuthoringWindow.TryPlace(name))
            {
                return;
            }

            var host = new GameObject(name);
            host.AddComponent(type);
            if (type != typeof(WorldAltar) && host.GetComponent<SpriteRenderer>() == null)
            {
                host.AddComponent<SpriteRenderer>();
            }

            host.transform.position = AuthoringUtil.Snap(SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero);
            Undo.RegisterCreatedObjectUndo(host, "Create " + name);
            Selection.activeGameObject = host;
        }
    }
}
#endif
