using UnityEngine;
using UnityEngine.UI;

namespace NitroStreetRush.Racing
{
    public sealed class GaragePresentation : MonoBehaviour
    {
        [SerializeField] private SaveGame saveGame;
        [SerializeField] private Text creditsText;
        [SerializeField] private Text bestTimeText;
        [SerializeField] private Text selectedCarText;

        private void Awake() { if (!saveGame) saveGame = FindFirstObjectByType<SaveGame>(); Refresh(); }

        public void Refresh()
        {
            if (!saveGame) return;
            if (creditsText) creditsText.text = $"CREDITS  {saveGame.Data.credits:N0}";
            if (bestTimeText) bestTimeText.text = saveGame.Data.bestTimeMs == int.MaxValue ? "BEST  --:--.---" : $"BEST  {FormatTime(saveGame.Data.bestTimeMs)}";
            if (selectedCarText) selectedCarText.text = $"CAR  {saveGame.Data.selectedCar + 1}";
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