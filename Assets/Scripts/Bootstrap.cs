using UnityEngine;

namespace RuneMagic
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (AdeptAvatar.Find() != null)
            {
                return;
            }

            var existing = Object.FindFirstObjectByType<SanctumDirector>();
            if (existing != null && AdeptAvatar.Find() == null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
            else if (existing != null)
            {
                return;
            }

            Camera cam = null;
            try
            {
                cam = PrepareCamera();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Camera setup failed: " + exception.Message);
            }

            var directorObject = new GameObject("Sanctum");
            var director = directorObject.AddComponent<SanctumDirector>();
            var hud = directorObject.AddComponent<GameHud>();
            hud.Bind(director);

            SanctumBuild build = null;
            try
            {
                build = SanctumLayout.Construct();
                director.Begin(build);
                if (build.Charm != null)
                {
                    build.Charm.AddComponent<FreeCharm>().Bind(director.Grimoire, director.Log);
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Sanctum layout failed: " + exception.Message);
            }

            var spawn = build != null ? build.Spawn : new Vector3(2.5f, 5.5f, 0f);
            var player = SpawnPlayer(spawn, cam);
            director.BindPlayer(player);
            var fieldHost = new GameObject("RuneField");
            fieldHost.transform.SetParent(player.transform, false);
            fieldHost.AddComponent<RuneField>().Bind(director);
        }

        static Camera PrepareCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cam = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                try
                {
                    cameraObject.tag = "MainCamera";
                }
                catch (System.Exception)
                {
                }
            }

            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.045f, 0.07f);
            cam.nearClipPlane = 0.1f;
            cam.transform.rotation = Quaternion.identity;
            cam.transform.position = new Vector3(2.5f, 5.5f, -10f);

            var follow = cam.GetComponent<FollowCamera2D>() ?? cam.gameObject.AddComponent<FollowCamera2D>();
            follow.damp = 8f;
            return cam;
        }

        static GameObject SpawnPlayer(Vector3 spawn, Camera camera)
        {
            var player = new GameObject("Adept");
            try
            {
                player.tag = "Player";
            }
            catch (System.Exception)
            {
            }

            player.transform.position = spawn;
            player.AddComponent<AdeptAvatar>();

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
