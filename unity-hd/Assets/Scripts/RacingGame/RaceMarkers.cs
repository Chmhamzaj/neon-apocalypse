using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceMarkers : MonoBehaviour
    {
        [SerializeField] private RaceProgress progress;
        [SerializeField] private RaceStateEvents events;
        [SerializeField] private Transform finish;
        [SerializeField] private int checkpointCount = 4;
        private Transform player;
        private int nextCheckpoint = 1;
        private float startZ;
        private float distance;

        public void Configure(Transform playerTarget, Transform finishTarget, float raceDistance)
        {
            player = playerTarget; finish = finishTarget; distance = Mathf.Max(1f, raceDistance);
            startZ = player ? player.position.z : 0f;
            if (progress) { progress.SetPlayer(player); progress.SetFinish(finish); progress.SetRaceDistance(distance); }
        }

        private void Update()
        {
            if (!player) return;
            float travelled = Mathf.Max(0f, player.position.z - startZ);
            float checkpointDistance = distance / (checkpointCount + 1f);
            if (nextCheckpoint <= checkpointCount && travelled >= checkpointDistance * nextCheckpoint)
            {
                events?.Checkpoint(nextCheckpoint++);
            }
            if (finish && player.position.z >= finish.position.z)
            {
                events?.FinishRace();
                enabled = false;
            }
        }
    }
}
