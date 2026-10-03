using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceSpawnLayout : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform finishLine;
        [SerializeField] private Transform[] trafficSpawns;
        [SerializeField] private float defaultRaceDistance = 2000f;

        public Transform PlayerSpawn => playerSpawn;
        public Transform FinishLine => finishLine;
        public Transform[] TrafficSpawns => trafficSpawns;
        public float RaceDistance => finishLine && playerSpawn
            ? Mathf.Abs(finishLine.position.z - playerSpawn.position.z)
            : defaultRaceDistance;

        private void OnDrawGizmosSelected()
        {
            if (playerSpawn)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(playerSpawn.position, 0.8f);
            }

            if (finishLine)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(finishLine.position, new Vector3(12f, 2f, 1f));
            }

            if (trafficSpawns == null) return;
            Gizmos.color = Color.red;
            foreach (Transform spawn in trafficSpawns)
                if (spawn) Gizmos.DrawWireSphere(spawn.position, 0.35f);
        }
    }
}
