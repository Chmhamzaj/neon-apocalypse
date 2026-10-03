using UnityEngine;
using UnityEngine.UI;

namespace NitroStreetRush.Racing
{
    public sealed class RaceResultsHUD : MonoBehaviour
    {
        [SerializeField] private SaveGame saveGame;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private Text timeText;
        [SerializeField] private Text rewardText;
        [SerializeField] private Text bestText;

        private void Awake()
        {
            if (!saveGame) saveGame = FindFirstObjectByType<SaveGame>();
            if (resultsPanel) resultsPanel.SetActive(false);
        }

        private void OnEnable() => RaceStateEvents.RaceFinished += ShowResults;
        private void OnDisable() => RaceStateEvents.RaceFinished -= ShowResults;

        private void ShowResults()
        {
            if (!resultsPanel) return;
            resultsPanel.SetActive(true);
            if (!saveGame) return;
            var timer = FindFirstObjectByType<RaceTimer>();
            if (timer) SetRaceTime(Mathf.RoundToInt(timer.ElapsedSeconds * 1000f));
            if (rewardText) rewardText.text = "REWARD  +500";
            if (bestText) bestText.text = saveGame.Data.bestTimeMs == int.MaxValue ? "BEST  --:--.---" : $"BEST  {FormatTime(saveGame.Data.bestTimeMs)}";
        }

        public void SetRaceTime(int milliseconds)
        {
            if (timeText) timeText.text = FormatTime(milliseconds);
        }

        private static string FormatTime(int ms)
        {
            int minutes = ms / 60000;
            int seconds = (ms / 1000) % 60;
            int milli = ms % 1000;
            return $"{minutes:00}:{seconds:00}.{milli:000}";
        }
    }
}