using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceProgress : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform finish;
        [SerializeField] private float raceDistance = 2000f;
        public float DistanceTravelled { get; private set; }
        public float Progress => Mathf.Clamp01(DistanceTravelled / Mathf.Max(1f, raceDistance));
        public bool Finished => finish && player && player.position.z >= finish.position.z;
        private float startZ;
        public void SetPlayer(Transform value) { player = value; if (player) startZ = player.position.z; }
        public void SetFinish(Transform value) => finish = value;
        public void SetRaceDistance(float value) => raceDistance = Mathf.Max(1f, value);
        private void Start() { if (player) startZ = player.position.z; }
        private void Update() { if (player) DistanceTravelled = Mathf.Max(0f, player.position.z - startZ); }
    }
}
