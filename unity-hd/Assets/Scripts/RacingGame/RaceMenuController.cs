using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject garageMenu;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject resultsMenu;

        private void Awake() => ShowMainMenu();

        public void ShowMainMenu() { Set(mainMenu, true); Set(garageMenu, false); Set(pauseMenu, false); Set(resultsMenu, false); }
        public void ShowGarage() { Set(mainMenu, false); Set(garageMenu, true); Set(pauseMenu, false); Set(resultsMenu, false); }
        public void ShowPause() { Set(pauseMenu, true); }
        public void ShowResults() { Set(resultsMenu, true); }
        public void HideAll() { Set(mainMenu, false); Set(garageMenu, false); Set(pauseMenu, false); Set(resultsMenu, false); }

        private static void Set(GameObject target, bool active)
        {
            if (target) target.SetActive(active);
        }
    }
}