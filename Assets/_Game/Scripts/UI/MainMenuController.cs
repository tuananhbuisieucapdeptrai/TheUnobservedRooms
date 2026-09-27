using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnobservedRooms.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            startButton ??= GameObject.Find("Start Offline Run")?.GetComponent<Button>();
            quitButton ??= GameObject.Find("Quit")?.GetComponent<Button>();
            if (startButton != null) startButton.onClick.AddListener(StartOfflineRun);
            else Debug.LogError("Main menu could not find the Start Offline Run button.");
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
            else Debug.LogError("Main menu could not find the Quit button.");
        }

        private void OnDestroy()
        {
            if (startButton != null) startButton.onClick.RemoveListener(StartOfflineRun);
            if (quitButton != null) quitButton.onClick.RemoveListener(Quit);
        }

        public void StartOfflineRun() => SceneManager.LoadScene("Game");
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
