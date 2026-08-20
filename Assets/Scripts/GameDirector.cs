using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharterWizard
{
    /// <summary>
    /// Builds the first-game courtyard at runtime and tracks rune collection.
    /// Open this file to change how many runes spawn or how the arena looks.
    /// </summary>
    public class GameDirector : MonoBehaviour
    {
        const int RuneCount = 8;
        const float ArenaRadius = 9f;

        int _collected;
        Transform _camera;
        Transform _wizard;
        bool _won;

        void Awake()
        {
            PrepareCamera();
            EnsureSun();
            BuildCourtyard();
            _wizard = SpawnWizard();
            SpawnRunes();
        }

        void LateUpdate()
        {
            if (_wizard == null)
            {
                return;
            }

            var cameraTarget = _wizard.position + new Vector3(0f, 7.5f, -11f);
            _camera.position = Vector3.Lerp(_camera.position, cameraTarget, 8f * Time.deltaTime);
            _camera.LookAt(_wizard.position + Vector3.up * 1.4f);
        }

        void Update()
        {
            if (_won && Input.GetKeyDown(KeyCode.R))
            {
                var scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        }

        void OnGUI()
        {
            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold
            };
            title.normal.textColor = Color.white;

            var body = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            body.normal.textColor = new Color(0.92f, 0.94f, 1f);

            GUI.Label(new Rect(28, 20, 640, 40), "Charter Wizard", title);
            GUI.Label(new Rect(28, 62, 640, 28), $"Runes collected: {_collected} / {RuneCount}", body);
            GUI.Label(new Rect(28, 90, 720, 28), "WASD to move  ·  Space to hop  ·  Walk into the glowing runes", body);

            if (_won)
            {
                var win = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 34,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                win.normal.textColor = new Color(1f, 0.86f, 0.35f);
                GUI.Label(new Rect(0, Screen.height * 0.38f, Screen.width, 50), "The charter is complete!", win);
                GUI.Label(new Rect(0, Screen.height * 0.38f + 48, Screen.width, 32), "Press R to play again", body);
            }
        }

        public void CollectRune()
        {
            _collected += 1;
            if (_collected >= RuneCount)
            {
                _won = true;
            }
        }

        void PrepareCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cam = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.16f);
            cam.fieldOfView = 55f;
            _camera = cam.transform;
            _camera.position = new Vector3(0f, 8f, -12f);
        }

        static void EnsureSun()
        {
            if (Object.FindFirstObjectByType<Light>() != null)
            {
                return;
            }

            var sun = new GameObject("Directional Light");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.95f, 0.88f);
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        void BuildCourtyard()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Courtyard";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(26f, 1f, 26f);
            ApplyColor(ground, new Color(0.16f, 0.18f, 0.28f));

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "CenterStone";
            ring.transform.position = new Vector3(0f, 0.15f, 0f);
            ring.transform.localScale = new Vector3(3.2f, 0.15f, 3.2f);
            ApplyColor(ring, new Color(0.28f, 0.24f, 0.38f));

            for (var i = 0; i < 12; i++)
            {
                var angle = i * Mathf.PI * 2f / 12f;
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = $"Pillar_{i + 1}";
                pillar.transform.position = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (ArenaRadius + 1.6f);
                pillar.transform.position += Vector3.up * 1.6f;
                pillar.transform.localScale = new Vector3(0.7f, 3.2f, 0.7f);
                ApplyColor(pillar, new Color(0.35f, 0.3f, 0.48f));
            }
        }

        Transform SpawnWizard()
        {
            var wizard = new GameObject("Wizard");
            wizard.tag = "Player";
            wizard.transform.position = new Vector3(0f, 1.1f, 0f);

            var controller = wizard.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 1f, 0f);
            wizard.AddComponent<WizardController>();

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(wizard.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            Object.Destroy(body.GetComponent<Collider>());
            ApplyColor(body, new Color(0.42f, 0.28f, 0.72f));

            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "Hat";
            hat.transform.SetParent(wizard.transform, false);
            hat.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            hat.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            Object.Destroy(hat.GetComponent<Collider>());
            ApplyColor(hat, new Color(0.95f, 0.72f, 0.22f));

            return wizard.transform;
        }

        void SpawnRunes()
        {
            for (var i = 0; i < RuneCount; i++)
            {
                var angle = i * Mathf.PI * 2f / RuneCount;
                var position = new Vector3(Mathf.Cos(angle), 1.15f, Mathf.Sin(angle)) * ArenaRadius;

                var rune = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rune.name = $"Rune_{i + 1}";
                rune.transform.position = position;
                rune.transform.localScale = Vector3.one * 0.7f;
                ApplyColor(rune, new Color(0.35f, 0.85f, 1f));

                var collider = rune.GetComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 0.7f;

                var pickup = rune.AddComponent<RunePickup>();
                pickup.Bind(this);
            }
        }

        static void ApplyColor(GameObject target, Color color)
        {
            var shader = Shader.Find("Standard")
                         ?? Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Unlit/Color");
            var material = new Material(shader);
            if (material.HasProperty("_Color"))
            {
                material.color = color;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.35f);
            }

            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }
    }
}
