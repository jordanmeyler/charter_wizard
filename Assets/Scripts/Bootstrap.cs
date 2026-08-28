using UnityEngine;

namespace RuneMagic
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Schedule()
        {
            var existing = Object.FindFirstObjectByType<SanctumDirector>();
            if (existing != null && existing.Started)
            {
                return;
            }

            if (Object.FindFirstObjectByType<SanctumBoot>() != null)
            {
                return;
            }

            var boot = new GameObject("SanctumBoot");
            boot.AddComponent<SanctumBoot>();
        }

        public static void Run()
        {
            var existing = Object.FindFirstObjectByType<SanctumDirector>();
            if (existing != null && existing.Started)
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
                Debug.LogWarning("Camera setup failed: " + exception);
            }

            var director = existing;
            if (director == null)
            {
                var directorObject = new GameObject("Sanctum");
                director = directorObject.AddComponent<SanctumDirector>();
            }

            if (director.GetComponent<GameHud>() == null)
            {
                director.gameObject.AddComponent<GameHud>().Bind(director);
            }

            if (director.GetComponent<RoomAtmosphere>() == null)
            {
                director.gameObject.AddComponent<RoomAtmosphere>().Bind(director);
            }

            SanctumBuild build = null;
            try
            {
                build = SceneLevel.Construct();
                director.Begin(build);
                if (build != null && build.Charm != null && build.Charm.GetComponent<FreeCharm>() == null)
                {
                    build.Charm.AddComponent<FreeCharm>().Bind(director.Grimoire, director.Log);
                }

                BindWorldItems(director);
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Sanctum layout failed: " + exception);
                director.Log("Sanctum layout failed: " + exception.Message);
                var leftover = GameObject.Find("SanctumGrid");
                if (leftover != null)
                {
                    Object.Destroy(leftover);
                }

                try
                {
                    build = SanctumLayout.FallbackCourt();
                    director.Begin(build);
                }
                catch (System.Exception fallback)
                {
                    Debug.LogError("Fallback court failed: " + fallback);
                }
            }

            var spawn = build != null ? build.Spawn : new Vector3(2.5f, 5.5f, 0f);
            var player = AdeptAvatar.Find() != null
                ? EnsurePlayer(AdeptAvatar.Find().gameObject, spawn, cam)
                : SpawnPlayer(spawn, cam);
            director.BindPlayer(player);
            if (Object.FindFirstObjectByType<RuneTapestry>() == null)
            {
                var tapestryHost = new GameObject("RuneTapestry");
                var tapestry = tapestryHost.AddComponent<RuneTapestry>();
                tapestry.Bind(director, build);
                director.BindTapestry(tapestry);
            }
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

            var follow = cam.GetComponent<FollowCamera2D>();
            if (follow == null)
            {
                follow = cam.gameObject.AddComponent<FollowCamera2D>();
            }
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

            var glow = new GameObject("Glow");
            glow.transform.SetParent(player.transform, false);
            glow.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            var halo = glow.AddComponent<SpriteRenderer>();
            halo.sprite = SpriteFactory.Glow(new Color(0.78f, 0.55f, 1f, 0.85f));
            halo.sortingOrder = 7;

            var sprite = player.AddComponent<SpriteRenderer>();
            sprite.sprite = AdeptStill();
            sprite.sortingOrder = 20;
            sprite.color = Color.white;
            player.AddComponent<AdeptAvatar>();
            BindAdeptAnimator(player);

            var statuses = player.AddComponent<StatusHost>();
            statuses.Bind(CreatureNature.Flesh, new Vector3(0f, 0.92f, 0f));

            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var hit = player.AddComponent<CircleCollider2D>();
            hit.radius = 0.32f;
            player.AddComponent<PlayerMotor2D>();
            WorldLabel.Attach(player.transform, "Adept", new Vector3(0f, 0.72f, 0f),
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

        static GameObject EnsurePlayer(GameObject player, Vector3 spawn, Camera camera)
        {
            if (player.GetComponent<SpriteRenderer>() == null)
            {
                var sprite = player.AddComponent<SpriteRenderer>();
                sprite.sprite = AdeptStill();
                sprite.sortingOrder = 20;
            }

            BindAdeptAnimator(player);

            var body = player.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = player.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (player.GetComponent<CircleCollider2D>() == null)
            {
                var hit = player.AddComponent<CircleCollider2D>();
                hit.radius = 0.32f;
            }

            if (player.GetComponent<PlayerMotor2D>() == null)
            {
                player.AddComponent<PlayerMotor2D>();
            }

            if (player.GetComponent<StatusHost>() == null)
            {
                player.AddComponent<StatusHost>().Bind(CreatureNature.Flesh, new Vector3(0f, 0.92f, 0f));
            }

            if (player.transform.Find("Glow") == null)
            {
                var glow = new GameObject("Glow");
                glow.transform.SetParent(player.transform, false);
                glow.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                var halo = glow.AddComponent<SpriteRenderer>();
                halo.sprite = SpriteFactory.Glow(new Color(0.78f, 0.55f, 1f, 0.85f));
                halo.sortingOrder = 7;
            }

            if (camera != null)
            {
                var follow = camera.GetComponent<FollowCamera2D>() ?? camera.gameObject.AddComponent<FollowCamera2D>();
                follow.Target = player.transform;
                follow.damp = 8f;
            }

            if (player.transform.position.sqrMagnitude < 0.01f)
            {
                player.transform.position = spawn;
            }

            return player;
        }

        static Sprite AdeptStill()
        {
#if UNITY_EDITOR
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(
                "Assets/ElvGames/Rogue Adventure/Characters/Hero_22.png");
            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite && sprite.name == "Hero_22_0")
                {
                    return sprite;
                }
            }
#endif
            return SpriteFactory.Named("adept");
        }

        static void BindAdeptAnimator(GameObject player)
        {
            if (player == null)
            {
                return;
            }

            var controller = Resources.Load<RuntimeAnimatorController>(AdeptAvatar.AnimatorResource);
            if (controller == null)
            {
                return;
            }

            var animator = player.GetComponent<Animator>();
            if (animator == null)
            {
                animator = player.AddComponent<Animator>();
            }

            AdeptAvatar.ApplyController(animator, controller);
        }

        static void BindWorldItems(SanctumDirector director)
        {
            var items = Object.FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
            for (var i = 0; i < items.Length; i++)
            {
                items[i].Bind(director.Grimoire, director.Log, director.Pack);
            }

            var charms = Object.FindObjectsByType<FreeCharm>(FindObjectsSortMode.None);
            for (var i = 0; i < charms.Length; i++)
            {
                charms[i].Bind(director.Grimoire, director.Log);
            }
        }
    }
}
