using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RacePauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        private bool paused;
        public bool IsPaused => paused;

        public void TogglePause()
        {
            SetPaused(!paused);
        }

        public void SetPaused(bool value)
        {
            paused = value;
            Time.timeScale = paused ? 0f : 1f;
            if (pausePanel) pausePanel.SetActive(paused);
        }

        private void OnDestroy() => Time.timeScale = 1f;
    }
}