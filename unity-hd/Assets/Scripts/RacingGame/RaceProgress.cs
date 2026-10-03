using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceProgress : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform finish;
        [SerializeField] private RaceSplinePath path;
        [SerializeField] private TrackProgressTracker tracker;
        [SerializeField] private float raceDistance = 2000f;
        public float DistanceTravelled { get; private set; }
        public float Progress => path && path.TotalLength > 0f && tracker ? tracker.NormalizedProgress : Mathf.Clamp01(DistanceTravelled / Mathf.Max(1f, raceDistance));
        public bool Finished => path && tracker ? Progress >= 0.999f : finish && player && player.position.z >= finish.position.z;
        private float startZ;

        public void SetPlayer(Transform value) { player = value; if (player) startZ = player.position.z; if (tracker) tracker.Configure(path, player); }
        public void SetFinish(Transform value) => finish = value;
        public void SetRaceDistance(float value) => raceDistance = Mathf.Max(1f, value);
        public void SetPath(RaceSplinePath value)
        {
            path = value;
            if (!tracker && player) tracker = GetComponent<TrackProgressTracker>();
            if (tracker) tracker.Configure(path, player);
        }
        private void Start() { if (player) startZ = player.position.z; if (tracker && path) tracker.Configure(path, player); }
        private void Update() { if (path && tracker) DistanceTravelled = tracker.Distance; else if (player) DistanceTravelled = Mathf.Max(0f, player.position.z - startZ); }
    }
}
