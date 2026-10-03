using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceMarkers : MonoBehaviour
    {
        [SerializeField] private RaceProgress progress;
        [SerializeField] private RaceStateEvents events;
        [SerializeField] private Transform finish;
        [SerializeField] private int checkpointCount = 4;
        private int nextCheckpoint = 1;
        private float distance;

        public int CheckpointCount => checkpointCount;
        public int NextCheckpoint => Mathf.Min(nextCheckpoint, checkpointCount + 1);

        public float NextCheckpointProgress
        {
            get
            {
                float target = distance * nextCheckpoint / (checkpointCount + 1f);
                return Mathf.InverseLerp(progress ? progress.DistanceTravelled : 0f, target, target);
            }
        }

        public void Configure(Transform playerTarget, Transform finishTarget, float raceDistance)
        {
            finish = finishTarget;
            distance = Mathf.Max(1f, raceDistance);
            nextCheckpoint = 1;
            if (!progress) progress = FindFirstObjectByType<RaceProgress>();
            if (!events) events = FindFirstObjectByType<RaceStateEvents>();
            if (progress)
            {
                progress.SetPlayer(playerTarget);
                progress.SetFinish(finish);
                progress.SetRaceDistance(distance);
            }
        }

        private void Update()
        {
            if (!progress) return;

            float travelled = progress.DistanceTravelled;
            float checkpointDistance = distance / (checkpointCount + 1f);

            while (nextCheckpoint <= checkpointCount && travelled >= checkpointDistance * nextCheckpoint)
                events?.Checkpoint(nextCheckpoint++);

            if (progress.Finished) enabled = false;
        }
    }
}
