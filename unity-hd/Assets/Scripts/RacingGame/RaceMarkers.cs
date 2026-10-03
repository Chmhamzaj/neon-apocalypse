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

        public void Configure(Transform playerTarget, Transform finishTarget, float raceDistance)
        {
            finish = finishTarget;
            distance = Mathf.Max(1f, raceDistance);
            if (progress) { progress.SetPlayer(playerTarget); progress.SetFinish(finish); progress.SetRaceDistance(distance); }
        }

        private void Update()
        {
            if (!progress) return;
            float travelled = progress.DistanceTravelled;
            float checkpointDistance = distance / (checkpointCount + 1f);
            if (nextCheckpoint <= checkpointCount && travelled >= checkpointDistance * nextCheckpoint)
                events?.Checkpoint(nextCheckpoint++);
            if (progress.Finished)
            {
                events?.FinishRace();
                enabled = false;
            }
        }
    }
}
