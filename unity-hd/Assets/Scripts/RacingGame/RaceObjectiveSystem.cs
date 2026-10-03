using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceObjectiveSystem : MonoBehaviour
    {
        [SerializeField] private RaceProgress progress;
        [SerializeField] private RaceComboSystem combo;
        [SerializeField] private float targetTimeSeconds = 90f;
        [SerializeField] private int targetScore = 5000;
        public bool TimeObjectiveComplete => timer && timer.ElapsedSeconds <= targetTimeSeconds && progress && progress.Finished;
        public bool ScoreObjectiveComplete => combo && combo.Score >= targetScore;
        public string ObjectiveText => $"TIME < {targetTimeSeconds:0}s   SCORE > {targetScore:N0}";
        private RaceTimer timer;

        private void Awake()
        {
            if (!progress) progress = FindFirstObjectByType<RaceProgress>();
            if (!combo) combo = FindFirstObjectByType<RaceComboSystem>();
            timer = FindFirstObjectByType<RaceTimer>();
        }
    }
}