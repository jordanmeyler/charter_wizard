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
            var fieldHost = new GameObject("RuneField");
            fieldHost.transform.SetParent(player.transform, false);
            fieldHost.AddComponent<RuneField>().Bind(director);
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
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.045f, 0.07f);
            cam.nearClipPlane = 0.1f;
            cam.transform.position = new Vector3(2.5f, 5.5f, -10f);

            var follow = cam.GetComponent<FollowCamera2D>() ?? cam.gameObject.AddComponent<FollowCamera2D>();
            follow.damp = 8f;
        }

        static GameObject SpawnPlayer(Vector3 spawn)
        {
            var player = new GameObject("Adept");
            player.tag = "Player";
            player.transform.position = spawn;

            var glow = new GameObject("Glow");
            glow.transform.SetParent(player.transform, false);
            glow.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            var halo = glow.AddComponent<SpriteRenderer>();
            halo.sprite = SpriteFactory.Glow(new Color(0.78f, 0.55f, 1f, 0.85f));
            halo.sortingOrder = 7;

            var sprite = player.AddComponent<SpriteRenderer>();
            sprite.sprite = SpriteFactory.Adept();
            sprite.sortingOrder = 20;
            sprite.color = Color.white;

            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var hit = player.AddComponent<CircleCollider2D>();
            hit.radius = 0.38f;
            player.AddComponent<PlayerMotor2D>();
            WorldLabel.Attach(player.transform, "Adept", new Vector3(0f, 1.15f, 0f),
                new Color(0.95f, 0.86f, 1f), 24);

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
