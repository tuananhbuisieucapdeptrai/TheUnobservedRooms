using UnobservedRooms.Gameplay;
using UnobservedRooms.Data;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnobservedRooms.UI
{
    public sealed class GameHud : MonoBehaviour
    {
        private PlayerInteractor interactor;
        private CoherenceSystem coherence;
        private EntanglementSystem entanglement;
        private Text coherenceText, stateText, objectiveText, promptText, resultText, tutorialText, deltaText;
        private Image coherenceFill, interactionFill;
        private GameObject promptPanel, resultPanel;
        private static string result;
        private static string notice;
        private static float noticeUntil;
        private RunDefinition run;
        private Vector3 startPosition;
        private int tutorialStage;
        private float tutorialUntil;
        private float deltaUntil;

        public void Configure(PlayerInteractor playerInteractor, CoherenceSystem resource, EntanglementSystem pair, RunDefinition runDefinition)
        {
            interactor = playerInteractor; coherence = resource; entanglement = pair; run = runDefinition;
            BuildCanvas();
            startPosition = interactor.transform.position;
            coherence.Changed += OnCoherenceChanged;
            tutorialText.text = "MOVE WITH WASD  •  LOOK WITH THE MOUSE";
            tutorialUntil = Time.time + 12f;
        }

        public static void ShowResult(string value) => result = value;
        public static void ShowNotice(string value, float duration = 5f) { notice = value; noticeUntil = Time.time + duration; }
        private void Awake() => result = null;

        private void Update()
        {
            if (coherenceText == null) return;
            var current = coherence != null ? coherence.Current : 0;
            var maximum = coherence != null ? coherence.Maximum : 100;
            coherenceText.text = $"COHERENCE  {current} / {maximum}";
            coherenceFill.fillAmount = maximum > 0 ? (float)current / maximum : 0f;
            coherenceFill.color = current > 30 ? new Color(.02f, .82f, 1f) : new Color(1f, .22f, .1f);
            stateText.text = entanglement == null ? "" : $"ENTANGLED  A:{entanglement.A}{(entanglement.CalibratedA ? "[OK]" : "[--]")}  B:{entanglement.B}{(entanglement.CalibratedB ? "[OK]" : "[--]")}     TARGET {entanglement.TargetA}/{entanglement.TargetB}";
            objectiveText.text = entanglement != null && entanglement.Solved
                ? "OBJECTIVE: Locate the exit aperture and press E"
                : entanglement != null && !entanglement.BothCalibrated
                    ? "OBJECTIVE: Locate and calibrate BOTH entangled controls"
                    : "OBJECTIVE: Retune the linked controls until A/B match the target";

            UpdateTutorial();
            if (Time.time < noticeUntil)
            {
                tutorialText.enabled = true; tutorialText.text = notice; tutorialText.color = new Color(1f,.42f,.28f);
            }
            else tutorialText.color = Color.white;
            deltaText.enabled = Time.time < deltaUntil;

            var prompt = interactor != null ? interactor.Prompt : string.Empty;
            promptPanel.SetActive(!string.IsNullOrEmpty(prompt));
            promptText.text = prompt;
            interactionFill.fillAmount = interactor != null ? interactor.Progress : 0f;
            resultPanel.SetActive(!string.IsNullOrEmpty(result));
            resultText.text = string.IsNullOrEmpty(result) ? "" : result + BuildQuantumReport();
            if (!string.IsNullOrEmpty(result) && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                SceneManager.LoadScene("Game");
        }

        private void OnDestroy() { if (coherence != null) coherence.Changed -= OnCoherenceChanged; }

        private void OnCoherenceChanged(int before, int after, string reason)
        {
            var delta = after - before;
            deltaText.text = delta >= 0 ? $"+{delta} COHERENCE" : $"{delta} COHERENCE";
            deltaText.color = delta >= 0 ? new Color(.2f, 1f, .65f) : new Color(1f, .35f, .2f);
            deltaText.enabled = true; deltaUntil = Time.time + 2.2f;
        }

        private void UpdateTutorial()
        {
            if (tutorialStage == 0 && Vector3.Distance(startPosition, interactor.transform.position) > 2f)
            { tutorialStage = 1; tutorialText.text = "AIM AT A BLUE THRESHOLD AND HOLD E TO OBSERVE IT"; tutorialUntil = Time.time + 10f; }
            if (tutorialStage == 1 && coherence.Current < coherence.Maximum)
            { tutorialStage = 2; tutorialText.text = "OBSERVATION COSTS COHERENCE  •  CYAN SHARDS RESTORE IT"; tutorialUntil = Time.time + 9f; }
            if (tutorialStage == 2 && Time.time > tutorialUntil)
            { tutorialStage = 3; tutorialText.text = "FIND CONTROLS A AND B  •  MATCH THE TARGET SHOWN LEFT"; tutorialUntil = Time.time + 9f; }
            if (tutorialStage == 3 && entanglement.Solved)
            { tutorialStage = 4; tutorialText.text = "ENTANGLEMENT STABLE  •  THE EXIT IS NOW CYAN"; tutorialUntil = Time.time + 8f; }
            tutorialText.enabled = Time.time < tutorialUntil;
        }

        private string BuildQuantumReport()
        {
            if (run?.Quantum?.Engines == null) return "";
            var text = new StringBuilder("\n\nQUANTUM RUN REPORT\n");
            foreach (var engine in run.Quantum.Engines)
                text.Append(engine.EngineId).Append("  •  ").Append(engine.ExecutionMode).Append("  •  ").Append(engine.JobIdShort).Append('\n').Append(engine.Statement).Append('\n');
            text.Append("Payload: ").Append(run.Quantum.PayloadHash).Append("\n\nR: retry");
            return text.ToString();
        }

        private void BuildCanvas()
        {
            var root = new GameObject("RuntimeHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;

            var status = Panel(root.transform, "Status", new Color(.015f, .035f, .045f, .94f), new Vector2(22, -22), new Vector2(650, 142), new Vector2(0, 1));
            coherenceText = Label(status.transform, "Coherence", 25, TextAnchor.MiddleLeft, new Vector2(20, -12), new Vector2(600, 36), new Vector2(0, 1));
            var barBack = Panel(status.transform, "CoherenceBar", new Color(.12f, .16f, .18f, 1), new Vector2(20, -55), new Vector2(300, 18), new Vector2(0, 1));
            coherenceFill = Fill(barBack.transform, "Fill", new Color(.02f, .82f, 1f));
            stateText = Label(status.transform, "State", 19, TextAnchor.MiddleLeft, new Vector2(20, -78), new Vector2(610, 30), new Vector2(0, 1));
            objectiveText = Label(status.transform, "Objective", 17, TextAnchor.MiddleLeft, new Vector2(20, -108), new Vector2(610, 28), new Vector2(0, 1));
            deltaText = Label(root.transform, "CoherenceDelta", 24, TextAnchor.MiddleLeft, new Vector2(690, -35), new Vector2(360, 42), new Vector2(0, 1));
            deltaText.enabled = false;

            var tutorialPanel = Panel(root.transform, "Tutorial", new Color(.015f, .035f, .045f, .9f), new Vector2(0, -22), new Vector2(860, 58), new Vector2(.5f, 1));
            tutorialText = Label(tutorialPanel.transform, "TutorialText", 21, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(820, 50), new Vector2(.5f, .5f));

            promptPanel = Panel(root.transform, "Prompt", new Color(.015f, .035f, .045f, .94f), new Vector2(0, 88), new Vector2(760, 92), new Vector2(.5f, 0));
            promptText = Label(promptPanel.transform, "PromptText", 25, TextAnchor.MiddleCenter, new Vector2(0, -8), new Vector2(720, 50), new Vector2(.5f, 1));
            var progressBack = Panel(promptPanel.transform, "Progress", new Color(.12f, .16f, .18f, 1), new Vector2(0, 14), new Vector2(500, 12), new Vector2(.5f, 0));
            interactionFill = Fill(progressBack.transform, "Fill", new Color(.02f, .82f, 1f));

            Label(root.transform, "Controls", 18, TextAnchor.MiddleLeft, new Vector2(22, 20), new Vector2(1050, 32), new Vector2(0, 0)).text = "WASD MOVE    MOUSE LOOK    SHIFT SPRINT    HOLD E INTERACT    F FLASHLIGHT    ESC CURSOR";
            Label(root.transform, "Crosshair", 26, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(32, 32), new Vector2(.5f, .5f)).text = "+";
            resultPanel = Panel(root.transform, "Result", new Color(.005f, .015f, .02f, .97f), Vector2.zero, new Vector2(920, 720), new Vector2(.5f, .5f));
            resultText = Label(resultPanel.transform, "ResultText", 23, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(850, 670), new Vector2(.5f, .5f));
            resultPanel.SetActive(false);
        }

        private static Image Fill(Transform parent, string name, Color color)
        {
            var fill = Panel(parent, name, color, Vector2.zero, Vector2.zero, new Vector2(0, .5f)).GetComponent<Image>();
            var rect = fill.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(2, 2); rect.offsetMax = new Vector2(-2, -2);
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; return fill;
        }

        private static GameObject Panel(Transform parent, string name, Color color, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image)); item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>(); rect.anchorMin = anchor; rect.anchorMax = anchor; rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
            item.GetComponent<Image>().color = color; return item;
        }

        private static Text Label(Transform parent, string name, int size, TextAnchor alignment, Vector2 position, Vector2 dimensions, Vector2 anchor)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Text)); item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>(); rect.anchorMin = anchor; rect.anchorMax = anchor; rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var text = item.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.fontStyle = FontStyle.Bold;
            text.alignment = alignment; text.color = Color.white; text.raycastTarget = false; return text;
        }
    }
}
