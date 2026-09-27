using System.IO;
using UnobservedRooms.Core;
using UnobservedRooms.Gameplay;
using UnobservedRooms.Generation;
using UnobservedRooms.Quantum;
using UnobservedRooms.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnobservedRooms.Editor
{
    public static class MvpProjectBuilder
    {
        private const string SceneFolder = "Assets/_Game/Scenes";

        [MenuItem("Tools/The Unobserved Rooms/Create MVP Project")]
        public static void CreateMvpProject()
        {
            Directory.CreateDirectory(SceneFolder);
            CreateBootstrap();
            CreateMainMenu();
            CreateGame();
            CreateEmptyScene("ArtTest", new Color(.08f, .08f, .08f));
            CreateEmptyScene("GenerationTest", new Color(.04f, .07f, .10f));
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{SceneFolder}/Bootstrap.unity", true),
                new EditorBuildSettingsScene($"{SceneFolder}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{SceneFolder}/Game.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene($"{SceneFolder}/MainMenu.unity");
            Debug.Log("The Unobserved Rooms MVP scenes and build settings were created.");
        }

        private static void CreateBootstrap()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Bootstrap");
            root.AddComponent<GameStateMachine>();
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/Bootstrap.unity");
        }

        private static void CreateMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera(new Color(.025f, .035f, .045f));
            var controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            var canvas = CreateCanvas();
            var panel = CreateUiObject("Panel", canvas.transform, typeof(Image)).GetComponent<RectTransform>();
            Stretch(panel);
            panel.GetComponent<Image>().color = new Color(.03f, .045f, .06f, 1f);
            CreateText(canvas.transform, "THE UNOBSERVED ROOMS", new Vector2(0, 165), 38, FontStyle.Bold, new Vector2(680, 80));
            CreateText(canvas.transform, "A quantum horror puzzle", new Vector2(0, 112), 18, FontStyle.Italic, new Vector2(500, 45));
            var start = CreateButton(canvas.transform, "Start Offline Run", new Vector2(0, 20));
            UnityEventTools.AddPersistentListener(start.onClick, controller.StartOfflineRun);
            var quit = CreateButton(canvas.transform, "Quit", new Vector2(0, -55));
            UnityEventTools.AddPersistentListener(quit.onClick, controller.Quit);
            AddEventSystem();
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/MainMenu.unity");
        }

        private static void CreateGame()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var systems = new GameObject("Systems");
            var state = systems.AddComponent<GameStateMachine>();
            var runService = systems.AddComponent<RunService>();
            systems.AddComponent<CoherenceSystem>();
            var levelRoot = new GameObject("GeneratedLevel");
            var assembler = levelRoot.AddComponent<LevelAssembler>();
            var runtime = systems.AddComponent<GameRuntime>();
            Assign(runtime, "runService", runService);
            Assign(runtime, "levelAssembler", assembler);
            Assign(runtime, "stateMachine", state);

            var camera = AddCamera(new Color(.035f, .045f, .05f));
            camera.transform.position = new Vector3(0f, 14f, -18f);
            camera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/Game.unity");
        }

        private static void CreateEmptyScene(string name, Color background)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera(background);
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/{name}.unity");
        }

        private static Camera AddCamera(Color color)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = color;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static Canvas CreateCanvas()
        {
            var root = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            return canvas;
        }

        private static void AddEventSystem()
        {
            var root = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            root.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
        {
            var root = new GameObject(name, components); root.transform.SetParent(parent, false); return root;
        }

        private static Text CreateText(Transform parent, string value, Vector2 position, int size, FontStyle style, Vector2 dimensions)
        {
            var root = CreateUiObject("Text", parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = root.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = dimensions; rect.anchoredPosition = position;
            var text = root.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter; text.color = new Color(.88f, .93f, .96f);
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position)
        {
            var root = CreateUiObject(label, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = root.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(320, 56); rect.anchoredPosition = position;
            root.GetComponent<Image>().color = new Color(.10f, .19f, .27f);
            var button = root.GetComponent<Button>();
            var text = CreateText(root.transform, label, Vector2.zero, 20, FontStyle.Normal, rect.sizeDelta);
            Stretch(text.rectTransform);
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Assign(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
