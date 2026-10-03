using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceComboSystem : MonoBehaviour
    {
        public static RaceComboSystem Instance { get; private set; }
        [SerializeField] private int checkpointPoints = 250;
        [SerializeField] private int driftPointsPerSecond = 35;
        [SerializeField] private int jumpPoints = 350;
        [SerializeField] private int nearMissPoints = 500;
        [SerializeField] private float comboTimeout = 3.5f;

        public int Score { get; private set; }
        public int Combo { get; private set; } = 1;
        public float ComboTimer { get; private set; }

        private void Awake() => Instance = this;

        private void OnEnable()
        {
            RaceStateEvents.CheckpointReached += OnCheckpoint;
            DrivingTrickSystem.DriftStarted += OnDriftStarted;
            DrivingTrickSystem.JumpStarted += OnJumpStarted;
            NearMissDetector.NearMiss += OnNearMiss;
            RaceStateEvents.RaceStarted += OnRaceStarted;
        }

        private void OnDisable()
        {
            RaceStateEvents.CheckpointReached -= OnCheckpoint;
            DrivingTrickSystem.DriftStarted -= OnDriftStarted;
            DrivingTrickSystem.JumpStarted -= OnJumpStarted;
            NearMissDetector.NearMiss -= OnNearMiss;
            RaceStateEvents.RaceStarted -= OnRaceStarted;
        }

        private void Update()
        {
            if (ComboTimer > 0f) ComboTimer -= Time.deltaTime;
            else if (Combo > 1) Combo = 1;
        }

        public void AddScore(int basePoints)
        {
            Combo = Mathf.Clamp(Combo + 1, 1, 12);
            Score += Mathf.Max(0, basePoints) * Combo;
            ComboTimer = comboTimeout;
        }

        public void AddDrift(float seconds) => Score += Mathf.Max(0, Mathf.RoundToInt(seconds * driftPointsPerSecond)) * Combo;
        private void OnRaceStarted() { Score = 0; Combo = 1; ComboTimer = 0f; }
        private void OnCheckpoint(int _) => AddScore(checkpointPoints);
        private void OnDriftStarted() { }
        private void OnJumpStarted() => AddScore(jumpPoints);
        private void OnNearMiss() => AddScore(nearMissPoints);
    }
}
