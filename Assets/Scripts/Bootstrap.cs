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

            var directorObject = new GameObject("Sanctum");
            var director = directorObject.AddComponent<SanctumDirector>();
            var hud = directorObject.AddComponent<GameHud>();
            hud.Bind(director);

            var build = SanctumLayout.Construct();
            director.Begin(build);

            if (build.Charm != null)
            {
                build.Charm.AddComponent<FreeCharm>().Bind(director.Grimoire, director.Log);
            }

            var player = SpawnPlayer(build.Spawn);
            var field = player.AddComponent<RuneField>();
            field.Bind(director.Composer, director.Log);
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
            cam.orthographicSize = 5.6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.045f, 0.07f);
            cam.transform.position = new Vector3(2f, 5f, -10f);

            var follow = cam.GetComponent<FollowCamera2D>() ?? cam.gameObject.AddComponent<FollowCamera2D>();
            follow.damp = 8f;
        }

        static GameObject SpawnPlayer(Vector3 spawn)
        {
            var player = new GameObject("Adept");
            player.tag = "Player";
            player.transform.position = spawn;

            var sprite = player.AddComponent<SpriteRenderer>();
            sprite.sprite = SpriteFactory.Adept();
            sprite.sortingOrder = 8;

            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var hit = player.AddComponent<CircleCollider2D>();
            hit.radius = 0.32f;
            player.AddComponent<PlayerMotor2D>();
            WorldLabel.Attach(player.transform, "You", new Vector3(0f, 0.7f, 0f),
                new Color(0.82f, 0.72f, 1f));

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
    }
}
