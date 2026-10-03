using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceObjectiveSystem : MonoBehaviour
    {
        [SerializeField] private RaceProgress progress;
        [SerializeField] private RaceComboSystem combo;
        [SerializeField] private RaceTimer timer;
        [SerializeField] private float targetTimeSeconds = 90f;
        [SerializeField] private int targetScore = 5000;

        public float TargetTimeSeconds => targetTimeSeconds;
        public int TargetScore => targetScore;
        public bool TimeObjectiveComplete => progress && progress.Finished && timer && timer.ElapsedSeconds <= targetTimeSeconds;
        public bool ScoreObjectiveComplete => combo && combo.Score >= targetScore;
        public bool AllObjectivesComplete => TimeObjectiveComplete && ScoreObjectiveComplete;

        public float TimeProgress
        {
            get
            {
                if (!timer) return 0f;
                return Mathf.Clamp01(1f - timer.ElapsedSeconds / Mathf.Max(1f, targetTimeSeconds));
            }
        }

        public float ScoreProgress => combo
            ? Mathf.Clamp01(combo.Score / (float)Mathf.Max(1, targetScore))
            : 0f;

        public string ObjectiveText => $"TIME < {targetTimeSeconds:0}s   •   SCORE > {targetScore:N0}";

        private void Awake()
        {
            if (!progress) progress = FindFirstObjectByType<RaceProgress>();
            if (!combo) combo = FindFirstObjectByType<RaceComboSystem>();
            if (!timer) timer = FindFirstObjectByType<RaceTimer>();
        }
    }
}
