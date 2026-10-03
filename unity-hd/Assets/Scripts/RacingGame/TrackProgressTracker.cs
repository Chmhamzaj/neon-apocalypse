using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class TrackProgressTracker : MonoBehaviour
    {
        [SerializeField] private RaceSplinePath path;
        [SerializeField] private Transform player;
        [SerializeField] private float lookAhead = 15f;
        public float Distance { get; private set; }
        public float NormalizedProgress => path && path.TotalLength > 0f ? Mathf.Clamp01(Distance / path.TotalLength) : 0f;
        public void Configure(RaceSplinePath newPath, Transform target) { path = newPath; player = target; }

        private void Update()
        {
            if (!path || !player || path.TotalLength <= 0f) return;
            Distance = FindNearestDistance(player.position);
        }

        private float FindNearestDistance(Vector3 position)
        {
            float best = Distance;
            float range = Mathf.Max(20f, lookAhead * 4f);
            float min = Mathf.Max(0f, Distance - range);
            float max = Mathf.Min(path.TotalLength, Distance + range);
            const int steps = 28;
            float bestDist = best;
            float bestSq = float.MaxValue;
            for (int i = 0; i <= steps; i++)
            {
                float d = Mathf.Lerp(min, max, i / (float)steps);
                float sq = (path.GetPoint(d) - position).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; bestDist = d; }
            }
            return bestDist;
        }
    }
}
