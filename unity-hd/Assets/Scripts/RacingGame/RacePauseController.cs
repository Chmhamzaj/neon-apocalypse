using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NitroStreetRush.Racing
{
    public sealed class RacePauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        private bool paused;

        public bool IsPaused => paused;

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                TogglePause();
#endif
        }

        public void BindPanel(GameObject panel)
        {
            pausePanel = panel;
            if (pausePanel) pausePanel.SetActive(paused);
        }

        public void TogglePause() => SetPaused(!paused);

        public void SetPaused(bool value)
        {
            paused = value;
            Time.timeScale = paused ? 0f : 1f;
            if (pausePanel) pausePanel.SetActive(paused);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
