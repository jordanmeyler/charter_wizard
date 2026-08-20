using UnityEngine;

namespace RuneMagic
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindFirstObjectByType<SanctumDirector>() != null)
            {
                return;
            }

            PrepareCamera();
            BuildRoom();

            var directorObject = new GameObject("Sanctum");
            var director = directorObject.AddComponent<SanctumDirector>();
            var hud = directorObject.AddComponent<GameHud>();
            hud.Bind(director);

            var player = SpawnPlayer();
            var field = player.AddComponent<RuneField>();
            field.Bind(director.Composer, director.Log);

            var charm = new GameObject("FreeCharm");
            charm.transform.position = new Vector3(-3.2f, -1.4f, 0f);
            charm.AddComponent<FreeCharm>().Bind(director.Grimoire, director.Log);

            var moth = SpawnLock(
                "Cinder Moth",
                "cinder-moth",
                new Vector3(3.4f, 1.6f, 0f),
                new[] { RuneId.Fire, RuneId.Mercury },
                new[] { SpellId.WaterJet, SpellId.IceWall },
                new Color(0.9f, 0.35f, 0.12f),
                ensouled: false);

            var sentinel = SpawnLock(
                "Clay Sentinel",
                "clay-sentinel",
                new Vector3(-1.2f, 3.3f, 0f),
                new[] { RuneId.Earth, RuneId.Salt },
                new[] { SpellId.Gale, SpellId.ScatterDust },
                new Color(0.52f, 0.38f, 0.24f),
                ensouled: false);

            director.Begin(new[] { moth, sentinel });
        }

        static void PrepareCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cam = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.orthographicSize = 6.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.1f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var follow = cam.GetComponent<FollowCamera2D>() ?? cam.gameObject.AddComponent<FollowCamera2D>();
            follow.damp = 7f;
        }

        static void BuildRoom()
        {
            var floor = new GameObject("Floor");
            var floorSprite = floor.AddComponent<SpriteRenderer>();
            floorSprite.sprite = SpriteFactory.Square(new Color(0.12f, 0.13f, 0.18f), 16);
            floorSprite.sortingOrder = 0;
            floor.transform.localScale = new Vector3(18f, 14f, 1f);

            SpawnWall("WallN", new Vector3(0f, 6.6f, 0f), new Vector3(18f, 0.6f, 1f));
            SpawnWall("WallS", new Vector3(0f, -6.6f, 0f), new Vector3(18f, 0.6f, 1f));
            SpawnWall("WallW", new Vector3(-8.8f, 0f, 0f), new Vector3(0.6f, 14f, 1f));
            SpawnWall("WallE", new Vector3(8.8f, 0f, 0f), new Vector3(0.6f, 14f, 1f));
        }

        static void SpawnWall(string name, Vector3 position, Vector3 scale)
        {
            var wall = new GameObject(name);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            var sprite = wall.AddComponent<SpriteRenderer>();
            sprite.sprite = SpriteFactory.Square(new Color(0.18f, 0.16f, 0.24f), 8);
            sprite.sortingOrder = 1;
            wall.AddComponent<BoxCollider2D>();
        }

        static GameObject SpawnPlayer()
        {
            var player = new GameObject("Adept");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var sprite = player.AddComponent<SpriteRenderer>();
            sprite.sprite = SpriteFactory.Circle(new Color(0.55f, 0.42f, 0.82f), 56);
            sprite.sortingOrder = 6;

            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var hit = player.AddComponent<CircleCollider2D>();
            hit.radius = 0.32f;
            player.AddComponent<PlayerMotor2D>();

            var camera = Camera.main;
            if (camera != null)
            {
                var follow = camera.GetComponent<FollowCamera2D>();
                if (follow != null)
                {
                    follow.Target = player.transform;
                }
            }

            return player;
        }

        static EncounterLock SpawnLock(
            string displayName,
            string formulaId,
            Vector3 position,
            RuneId[] formula,
            SpellId[] keys,
            Color color,
            bool ensouled)
        {
            var actor = new GameObject(displayName);
            actor.transform.position = position;
            var encounter = actor.AddComponent<EncounterLock>();
            encounter.Bind(displayName, formulaId, formula, keys, color, ensouled);
            return encounter;
        }
    }
}
