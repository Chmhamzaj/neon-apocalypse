using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroStreetRush.Racing
{
    public sealed class RaceRestartController : MonoBehaviour
    {
        [SerializeField] private RacePauseController pause;
        private bool restarting;
        public void RestartRace()
        {
            if (restarting) return;
            restarting = true;
            Time.timeScale = 1f;
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }
        public void ReturnToMenu(int buildIndex) 
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(buildIndex);
        }
    }
}
