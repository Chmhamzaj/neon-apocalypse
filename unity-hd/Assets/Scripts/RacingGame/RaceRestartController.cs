using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroStreetRush.Racing
{
    public sealed class RaceRestartController : MonoBehaviour
    {
        private bool restarting;

        private void Awake()
        {
            if (!FindFirstObjectByType<RacePauseController>())
                gameObject.AddComponent<RacePauseController>();
        }

        public void RestartRace()
        {
            if (restarting) return;
            restarting = true;
            Time.timeScale = 1f;
            var current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }

        public void ReturnToMenu(int buildIndex)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(buildIndex);
        }
    }
}
