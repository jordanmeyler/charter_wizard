#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Visual picker for every catalog rune, plus click-to-place
    /// floating inscriptions in the Scene view.
    /// </summary>
    public sealed class RunePlaceWindow : EditorWindow
    {
        RuneId _rune = RuneId.Fire;
        RuneStele.Kind _form = RuneStele.Kind.Floor;
        bool _paint = true;
        Vector2 _scroll;
        Vector3 _hover;
        bool _hasHover;

        [MenuItem("Window/Rune Magic/Inscriptions")]
        public static void Open()
        {
            GetWindow<RunePlaceWindow>("Inscriptions");
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnScene;
            SceneView.RepaintAll();
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnScene;
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Every catalog rune can be an inscription. Nothing assigned means a floating mark — no slab or shaft. Turn on paint, then click a tile in the Scene view. Right-click removes. Click an existing mark to change it.",
                MessageType.Info);

            _paint = EditorGUILayout.Toggle("Click Scene to place", _paint);
            _form = (RuneStele.Kind)EditorGUILayout.EnumPopup("Hover", _form);
            EditorGUILayout.LabelField("Selected", RuneCatalog.NameOf(_rune));

            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            RunePicker.Draw(ref _rune);
            EditorGUILayout.EndScrollView();

            if (_hasHover)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Under cursor", AuthoringUtil.CellOf(_hover).x + ", " + AuthoringUtil.CellOf(_hover).y);
            }
        }

        void OnScene(SceneView view)
        {
            var world = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            world.z = 0f;
            _hover = world;
            _hasHover = true;

            var cell = AuthoringUtil.Snap(world);
            Handles.color = new Color(0.92f, 0.78f, 0.28f, 0.95f);
            Handles.DrawWireDisc(cell + Vector3.up * Hover(), Vector3.forward, 0.42f);
            Handles.Label(cell + Vector3.up * (Hover() + 0.35f), RuneCatalog.NameOf(_rune));

            var ev = Event.current;
            if (ev.type == EventType.MouseMove)
            {
                view.Repaint();
                Repaint();
            }

            if (!_paint || ev.alt)
            {
                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (ev.type == EventType.MouseDown && ev.button == 0)
            {
                PlaceAt(cell);
                ev.Use();
            }
            else if (ev.type == EventType.MouseDown && ev.button == 1)
            {
                RemoveAt(cell);
                ev.Use();
            }
        }

        float Hover() => _form == RuneStele.Kind.Pillar ? RuneStele.PillarHover : RuneStele.FloorHover;

        void PlaceAt(Vector3 cell)
        {
            var existing = FindAt(cell);
            if (existing != null)
            {
                Undo.RecordObject(existing, "Set inscription rune");
                existing.Author(_rune, _form);
                EditorUtility.SetDirty(existing);
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var host = new GameObject(RuneStele.NameOf(_rune, _form));
            host.transform.position = cell;
            var stele = host.AddComponent<RuneStele>();
            stele.Author(_rune, _form);
            Undo.RegisterCreatedObjectUndo(host, "Place " + host.name);
            Selection.activeGameObject = host;
        }

        static void RemoveAt(Vector3 cell)
        {
            var existing = FindAt(cell);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }
        }

        static RuneStele FindAt(Vector3 cell)
        {
            var steles = Object.FindObjectsByType<RuneStele>(FindObjectsSortMode.None);
            for (var i = 0; i < steles.Length; i++)
            {
                if (steles[i] != null && (steles[i].transform.position - cell).sqrMagnitude < 0.2f)
                {
                    return steles[i];
                }
            }

            return null;
        }
    }

    [CustomEditor(typeof(RuneStele))]
    public sealed class RuneSteleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var runeProp = serializedObject.FindProperty("authoredRune");
            var formProp = serializedObject.FindProperty("authoredForm");
            EditorGUILayout.HelpBox(
                "Floating mark only — no base. Pick any rune. Portrait / Sprite Id still replace the generated sign.",
                MessageType.None);

            var rune = (RuneId)runeProp.intValue;
            EditorGUILayout.LabelField("Rune", RuneCatalog.NameOf(rune));
            RunePicker.Draw(ref rune);
            if ((int)rune != runeProp.intValue)
            {
                runeProp.intValue = (int)rune;
            }

            EditorGUILayout.PropertyField(formProp, new GUIContent("Hover", "Floor sits lower. Pillar sits a little higher. Neither draws a slab or shaft."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteId"));
            if (serializedObject.ApplyModifiedProperties())
            {
                var stele = (RuneStele)target;
                stele.Author((RuneId)runeProp.intValue, (RuneStele.Kind)formProp.enumValueIndex);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Snap to grid"))
            {
                var stele = (RuneStele)target;
                Undo.RecordObject(stele.transform, "Snap inscription");
                stele.transform.position = AuthoringUtil.Snap(stele.transform.position);
            }

            if (GUILayout.Button("Open inscription placer"))
            {
                RunePlaceWindow.Open();
            }
        }

        void OnSceneGUI()
        {
            var stele = (RuneStele)target;
            var snapped = AuthoringUtil.Snap(stele.transform.position);
            snapped.z = 0f;
            if (snapped != stele.transform.position)
            {
                Undo.RecordObject(stele.transform, "Snap inscription");
                stele.transform.position = snapped;
            }
        }
    }

}
#endif
