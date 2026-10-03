using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceResultsController : MonoBehaviour
    {
        [SerializeField] private SaveGame saveGame;
        [SerializeField] private int finishReward = 500;
        private float raceStartTime;
        private bool activeRace;

        private void Awake()
        {
            if (!saveGame) saveGame = FindFirstObjectByType<SaveGame>();
        }

        private void OnEnable()
        {
            RaceStateEvents.RaceStarted += OnRaceStarted;
            RaceStateEvents.RaceFinished += OnRaceFinished;
        }

        private void OnDisable()
        {
            RaceStateEvents.RaceStarted -= OnRaceStarted;
            RaceStateEvents.RaceFinished -= OnRaceFinished;
        }

        private void OnRaceStarted()
        {
            raceStartTime = Time.time;
            activeRace = true;
        }

        private void OnRaceFinished()
        {
            if (!activeRace) return;
            activeRace = false;
            int elapsedMs = Mathf.RoundToInt(Mathf.Max(0f, Time.time - raceStartTime) * 1000f);
            if (saveGame)
            {
                saveGame.Data.credits += finishReward;
                if (elapsedMs < saveGame.Data.bestTimeMs) saveGame.Data.bestTimeMs = elapsedMs;
                saveGame.Commit();
            }
        }
    }
}